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
            _azureTenantId = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.AzureTenantId.ToString()) ?? string.Empty;
            _azureClientId = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.AzureClientId.ToString()) ?? string.Empty;
            _azureClientSecret = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.AzureClientSecret.ToString()) ?? string.Empty;
            _siteId = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.SharePointSiteId.ToString()) ?? string.Empty;
            _siteURL = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.SharePointSiteURL.ToString()) ?? string.Empty;
            _libraryName = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.ComplianceLibraryName.ToString()) ?? string.Empty;
            _libraryId = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.ComplianceLibraryId.ToString()) ?? string.Empty;
            _emailListId = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.EmailListId.ToString()) ?? string.Empty;
            _msgsListId = Configuration.GetAppSetting(ConfigConstants.ConfigIdsName.MessagesListId.ToString()) ?? string.Empty;
        }

        public void Authenticate()
        {
            try
            {
                string[] scopes = { "https://graph.microsoft.com/.default" };
                var options = new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
                var clientSecretCredential = new ClientSecretCredential(_azureTenantId, _azureClientId, _azureClientSecret, options);
                _graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<List<UsersDto>> GetAllUsers()
        {
            var users = new List<UsersDto>();
            if (_graphServiceClient == null) Authenticate();
            var usersResponse = await _graphServiceClient.Users.GetAsync(req =>
            {
                req.QueryParameters.Select = new[] { "id,displayName,givenName,surname,jobTitle,department,mail,createdDateTime" };
                req.QueryParameters.Top = 100;
            });

            if (usersResponse?.Value == null) return users;

            var pageIterator = PageIterator<User, UserCollectionResponse>.CreatePageIterator(
                _graphServiceClient, usersResponse, user =>
                {
                    users.Add(new UsersDto
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
            if (_graphServiceClient == null) Authenticate();
            try
            {
                var chatsResponse = await _graphServiceClient.Users[userId].Chats.GetAsync();
                if (chatsResponse?.Value == null) return cleanedMessages;

                foreach (var chat in chatsResponse.Value)
                {
                    try
                    {
                        var chatMessagesPage = await _graphServiceClient.Chats[chat.Id].Messages.GetAsync();
                        if (chatMessagesPage?.Value == null) continue;

                        var pageIterator = PageIterator<ChatMessage, ChatMessageCollectionResponse>.CreatePageIterator(
                            _graphServiceClient, chatMessagesPage, chatMessage =>
                            {
                                if (!string.IsNullOrEmpty(chatMessage.Body?.Content))
                                {
                                    cleanedMessages.Add(CleanContent(chatMessage.Body.Content));
                                }
                                return true;
                            });
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

        public async Task<List<EmailsDto>> GetEmailsByUserId(string userId)
        {
            var today = DateTime.UtcNow.Date.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var emailDetails = new List<EmailsDto>();
            if (_graphServiceClient == null) Authenticate();
            try
            {
                var allMessages = new List<Message>();
                var messagesResponse = await _graphServiceClient.Users[userId].Messages.GetAsync(req =>
                {
                    req.Headers.Add("Prefer", "outlook.body-content-type=\"text\"");
                    req.QueryParameters.Filter = $"receivedDateTime le {today} and from/emailAddress/address eq 'rahulmittaljuly2@gmail.com'";
                    req.QueryParameters.Select = new[] { "subject", "body", "toRecipients", "ccRecipients" };
                    req.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                });

                var pageIterator = PageIterator<Message, MessageCollectionResponse>.CreatePageIterator(
                    _graphServiceClient, messagesResponse, message =>
                    {
                        allMessages.Add(message);
                        return true;
                    });

                await pageIterator.IterateAsync();

                emailDetails.AddRange(allMessages
                    .Where(m => !string.IsNullOrEmpty(m.Body?.Content))
                    .Select(m => new EmailsDto
                    {
                        BodyContent = CleanContent(m.Body!.Content),
                        ToRecipients = GetValidEmailAddresses(m.ToRecipients!),
                        CcRecipients = GetValidEmailAddresses(m.CcRecipients!)
                    }) ?? Enumerable.Empty<EmailsDto>());
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error retrieving emails: {ex.Message}");
            }
            return emailDetails;
        }

        public async Task<int> CreateEmailDetails(UsersDto user, string emailContent, string matchedPolicyName,
            string matchedPolicyContent, string toRecipients, string ccRecipients, string violationExplanation,
            string detectedRiskLevel, string matchedPolicySection = "")
        {
            // Set flag to true for email list
            return await CreateItemDetails(user, emailContent, matchedPolicyName, matchedPolicyContent, violationExplanation, detectedRiskLevel, matchedPolicySection, _emailListId, toRecipients, ccRecipients);
        }

        public async Task<int> CreateMsgDetails(UsersDto user, string msgContent, string matchedPolicyName,
            string matchedPolicyContent, string violationExplanation, string detectedRiskLevel, string matchedPolicySection = "")
        {
            return await CreateItemDetails(user, msgContent, matchedPolicyName, matchedPolicyContent, violationExplanation, detectedRiskLevel, matchedPolicySection, _msgsListId);
        }


        private async Task<int> CreateItemDetails(UsersDto user, string content, string matchedPolicyName,
            string matchedPolicyContent, string violationExplanation,
            string detectedRiskLevel, string matchedPolicySection, string listId, string toRecipients = "", string ccRecipients = "")
        {
            try
            {
                var fieldValues = new Dictionary<string, object>
                                    {
                                        {"Title", user.Id},
                                        {"UserEmail", user.Mail},
                                        {"UserDisplayName", user.DisplayName},
                                        {"UserDepartment", user.Department},
                                        {"Content", content},
                                        {"MatchedPolicyName", matchedPolicyName},
                                        {"MatchedPolicySection", matchedPolicySection},
                                        {"MatchedPolicyContent", matchedPolicyContent},
                                        {"ViolationExplanation", violationExplanation},
                                        {"DetectedRiskLevel", detectedRiskLevel},
                                        {"DetectedDateTime", DateTime.UtcNow}
                                    };

                if (!string.IsNullOrEmpty(toRecipients) && !string.IsNullOrEmpty(ccRecipients))
                {
                    fieldValues.Add("ToRecipients", toRecipients);
                    fieldValues.Add("CCRecipients", ccRecipients);
                }

                var newItem = new ListItem
                {
                    Fields = new FieldValueSet { AdditionalData = fieldValues }
                };

                var itemCreated = await _graphServiceClient.Sites[_siteId].Lists[listId].Items.PostAsync(newItem);
                return Convert.ToInt32(itemCreated.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 0;
            }
        }

        public async Task<List<DriveItem>?> GetAllFilesFromLibrary()
        {
            List<DriveItem> files = null;
            if (_graphServiceClient == null) Authenticate();
            try
            {
                var folder = await _graphServiceClient.Drives[_libraryId].Items["root"].Children.GetAsync();
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
                fileContent = await _graphServiceClient.Drives[_libraryId].Root.ItemWithPath(fileName).Content.GetAsync();
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
            var tokens = Regex.Split(input.ToLower(), @"\W+").Where(token => !ConfigConstants.StopWords.Contains(token) && token.Length > 1);
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
