using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Text.RegularExpressions;
using WebApp.Infrastructure.Models;

namespace WebApp.Infrastructure.Helpers
{
    public class GraphSharePointHelper
    {
        private readonly string _azureTenantId, _azureClientId, _azureClientSecret, _siteId, _siteURL, _libraryName, _libraryId, _emailListId, _msgsListId;
        private GraphServiceClient? _graphServiceClient;
        public GraphSharePointHelper()
        {
            _azureTenantId = Configuration.GetAppSetting(Enumeration.ConfigIdsName.AzureTenantId.ToString()) ?? string.Empty;
            _azureClientId = Configuration.GetAppSetting(Enumeration.ConfigIdsName.AzureClientId.ToString()) ?? string.Empty;
            _azureClientSecret = Configuration.GetAppSetting(Enumeration.ConfigIdsName.AzureClientSecret.ToString()) ?? string.Empty;
            _siteId = Configuration.GetAppSetting(Enumeration.ConfigIdsName.SharePointSiteId.ToString()) ?? string.Empty;
            _siteURL = Configuration.GetAppSetting(Enumeration.ConfigIdsName.SharePointSiteURL.ToString()) ?? string.Empty;
            _libraryName = Configuration.GetAppSetting(Enumeration.ConfigIdsName.ComplianceLibraryName.ToString()) ?? string.Empty;
            _libraryId = Configuration.GetAppSetting(Enumeration.ConfigIdsName.ComplianceLibraryId.ToString()) ?? string.Empty;
            _emailListId = Configuration.GetAppSetting(Enumeration.ConfigIdsName.EmailListId.ToString()) ?? string.Empty;
            _msgsListId = Configuration.GetAppSetting(Enumeration.ConfigIdsName.MessagesListId.ToString()) ?? string.Empty;
        }

        public void Authenticate()
        {
            try
            {
                string[] scopes = ["https://graph.microsoft.com/.default"];
                TokenCredentialOptions options = new TokenCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
                };

                ClientSecretCredential clientSecretCredential = new ClientSecretCredential(_azureTenantId, _azureClientId, _azureClientSecret, options);
                _graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<List<TenantUsersDto>> GetAllUsers()
        {
            var users = new List<TenantUsersDto>();
            if (_graphServiceClient == null)
            {
                Authenticate();
            }
            var usersResponse = await _graphServiceClient.Users
                                .GetAsync(requestConfiguration =>
                                {
                                    requestConfiguration.QueryParameters.Select = new string[] { "id,displayName,givenName,surname,jobTitle,department,mail,createdDateTime" };
                                    requestConfiguration.QueryParameters.Top = 100;
                                });

            if (usersResponse == null || usersResponse.Value == null)
                return users;

            var pageIterator = PageIterator<User, UserCollectionResponse>.CreatePageIterator(
                _graphServiceClient,
                usersResponse,
                user =>
                {
                    users.Add(new TenantUsersDto
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName,
                        GivenName = user.GivenName,
                        Surname = user.Surname,
                        JobTitle = user.JobTitle,
                        Department = user.Department,
                        Mail = user.Mail,
                        CreatedDateTime = user.CreatedDateTime
                    });
                    return true;
                });
            await pageIterator.IterateAsync();
            return users;
        }

        public async Task<List<string>> GetTeamsMessagesByUserId(string userId)
        {
            var cleanedMessages = new List<string>();
            if (_graphServiceClient == null)
            {
                Authenticate();
            }
            try
            {
                var chatsResponse = await _graphServiceClient.Users[userId].Chats.GetAsync();
                if (chatsResponse?.Value == null)
                    return cleanedMessages;

                foreach (var chat in chatsResponse.Value)
                {
                    try
                    {
                        var chatMessagesPage = await _graphServiceClient.Chats[chat.Id].Messages.GetAsync();

                        if (chatMessagesPage?.Value == null)
                            continue;

                        var pageIterator = PageIterator<ChatMessage, ChatMessageCollectionResponse>
                            .CreatePageIterator(
                                _graphServiceClient,
                                chatMessagesPage,
                                (chatMessage) =>
                                {
                                    if (!string.IsNullOrEmpty(chatMessage.Body?.Content))
                                    {
                                        cleanedMessages.Add(CleanContent(chatMessage.Body.Content));
                                    }
                                    return true; // continue paging
                                }
                            );

                        await pageIterator.IterateAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error retrieving messages for chat {chat.Id}: {ex.Message}");
                    }
                }

            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error retrieving chats: {ex.Message}");
            }
            return cleanedMessages;
        }

        public async Task<List<(string BodyContent, List<string> ToRecipients, List<string> CcRecipients)>> GetEmailsByUserId(string userId)
        {
            var today = DateTime.UtcNow.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var emailDetails = new List<(string BodyContent, List<string> ToRecipients, List<string> CcRecipients)>();
            if (_graphServiceClient == null)
            {
                Authenticate();
            }
            try
            {
                var allMessages = new List<Message>();
                var messagesResponse = await _graphServiceClient.Users[userId]
                    .Messages
                    .GetAsync(req =>
                    {
                        req.Headers.Add("Prefer", "outlook.body-content-type=\"text\"");
                        req.QueryParameters.Filter = $"receivedDateTime le {today} and from/emailAddress/address eq 'rahulmittaljuly2@gmail.com'";
                        // req.QueryParameters.Filter = $"from/emailAddress/address eq 'rahulmittaljuly2@gmail.com'";
                        req.QueryParameters.Select = new[] { "subject", "body", "toRecipients", "ccRecipients" };
                        req.QueryParameters.Orderby = new[] { "receivedDateTime desc" };  // Correct: using an array of strings

                    });

                var pageIterator = PageIterator<Message, MessageCollectionResponse>.CreatePageIterator(
                                   _graphServiceClient,
                                   messagesResponse,
                                   message =>
                                   {
                                       // Filter and add message to the list
                                       allMessages.Add(message);
                                       return true;  // Continue processing more pages
                                   });

                // Start iterating over pages
                await pageIterator.IterateAsync();

                emailDetails.AddRange(
                        allMessages
                            .Where(m => !string.IsNullOrEmpty(m.Body?.Content))
                            .Select(m => (
                                BodyContent: CleanContent(m.Body?.Content!),
                                ToRecipients: GetValidEmailAddresses(m.ToRecipients!),
                                CcRecipients: GetValidEmailAddresses(m.CcRecipients!)
                            )) ?? Enumerable.Empty<(string, List<string>, List<string>)>()
                    );
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error retrieving emails: {ex.Message}");
            }
            return emailDetails;
        }

        public async Task<List<DriveItem>?> GetAllFilesFromLibrary()
        {
            List<DriveItem> files = null;
            if (_graphServiceClient == null)
            {
                Authenticate();
            }
            try
            {
                var folder = await _graphServiceClient.Drives[_libraryId]
                       .Items["root"]
                       .Children
                       .GetAsync();
                if (folder != null)
                {
                    files = folder.Value.Where(f => f.File != null).ToList();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.Message == "The resource could not be found.")
                {
                    files = null;
                }
            }
            return files;
        }

        public async Task<Stream?> GetSpecificFileStream(string fileName)
        {
            Stream? fileContent = null;
            try
            {
                fileContent = await _graphServiceClient.Drives[_libraryId]
                               .Root
                               .ItemWithPath(fileName)
                               .Content
                               .GetAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException?.Message == "The resource could not be found.")
                {
                    fileContent = null;
                }
            }
            return fileContent;
        }

        public async Task<int> CreateEmailDetails(TenantUsersDto user, string emailContent, string matchedPolicyName,
            string matchedPolicyContent, string toRecipients, string ccRecipients)
        {
            try
            {
                var fieldValues = new Dictionary<string, object>
                                    {
                                        {"Title", user.Id},
                                        {"UserEmail", user.Mail},
                                        {"UserDisplayName", user.DisplayName},
                                        {"UserDepartment", user.Department},
                                        {"EmailContent", emailContent},
                                        {"MatchedPolicyName", matchedPolicyName},
                                        {"MatchedPolicyContent", matchedPolicyContent},
                                        {"ToRecipients", toRecipients},
                                        {"CCRecipients", ccRecipients}
                                    };
                var newItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = fieldValues
                    }
                };

                ListItem itemCreated = await _graphServiceClient.Sites[_siteId].Lists[_emailListId].Items.PostAsync(newItem);
                return Convert.ToInt32(itemCreated.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }

        public async Task<int> CreateMsgDetails(TenantUsersDto user, string msgContent, string matchedPolicyName,
            string matchedPolicyContent)
        {
            try
            {
                var fieldValues = new Dictionary<string, object>
                {
                                        {"Title", user.Id},
                                        {"UserEmail", user.Mail},
                                        {"UserDisplayName", user.DisplayName},
                                        {"UserDepartment", user.Department},
                                        {"MessageContent", msgContent},
                                        {"MatchedPolicyName", matchedPolicyName},
                                        {"MatchedPolicyContent", matchedPolicyContent}
                                    };
                var newItem = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = fieldValues
                    }
                };

                ListItem itemCreated = await _graphServiceClient.Sites[_siteId].Lists[_msgsListId].Items.PostAsync(newItem);
                return Convert.ToInt32(itemCreated.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }

        public async Task<List<Dictionary<string, object>>> FetchDataFromSharePointList(string listName, string fields)
        {
            var listId = listName == "emails" ? _emailListId : _msgsListId;
            var items = await _graphServiceClient.Sites[_siteId].Lists[listId].Items.GetAsync((requestConfiguration) =>
            {
                requestConfiguration.QueryParameters.Expand = new string[] { "fields($select=" + fields + ")" };
                requestConfiguration.QueryParameters.Top = 5000;
            });

            var itemsList = items?.Value.Select(item =>
            {
                var dict = new Dictionary<string, object>
                    {
                        { "Id", item.Fields.Id }
                    };
                foreach (var kvp in item.Fields.AdditionalData)
                {
                    dict[kvp.Key] = kvp.Value;
                }

                return dict;
            }).ToList();

            return itemsList;
        }

        public async Task<string> UploadFileToSharePoint(string filePath, string fileString)
        {
            try
            {
                byte[] fileBytes = Convert.FromBase64String(fileString);
                var uploadedFile = await _graphServiceClient
                       .Drives[_libraryId]
                       .Root
                       .ItemWithPath(filePath)
                       .Content
                       .PutAsync(new MemoryStream(fileBytes));

                int lastIndex = filePath.LastIndexOf('/');
                string result = lastIndex < 0 ? string.Empty : filePath.Substring(0, lastIndex);
                string fileURL = $"{_siteURL}/{_libraryName}/{filePath}/{uploadedFile?.Name}";
                return fileURL;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static string CleanContent(string input)
        {
            var tokens = Regex.Split(input.ToLower(), @"\W+").Where(token => !Enumeration.stopWords.Contains(token) && token.Length > 1);
            return string.Join(" ", tokens);
        }

        private static List<string> GetValidEmailAddresses(IEnumerable<Recipient> recipients)
        {
            return recipients?
                .Select(r => r.EmailAddress?.Address)
                .Where(address => !string.IsNullOrEmpty(address))
                .Cast<string>()
                .ToList() ?? new List<string>();
        }
    }
}
