using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Infrastructure.Models;

namespace WebApp.Infrastructure.Helpers
{
    public class GraphHelper
    {
        private readonly string _azureTenantId, _azureClientId, _azureClientSecret;
        private GraphServiceClient _graphServiceClient;
        public GraphHelper()
        {
            _azureTenantId = ConfigurationHelper.GetAppSetting(Enumeration.ConfigIdsName.AzureTenantId.ToString());
            _azureClientId = ConfigurationHelper.GetAppSetting(Enumeration.ConfigIdsName.AzureClientId.ToString());
            _azureClientSecret = ConfigurationHelper.GetAppSetting(Enumeration.ConfigIdsName.AzureClientSecret.ToString());
        }

        public void Authenticate()
        {
            try
            {
                string[] scopes = new[] { "https://graph.microsoft.com/.default" };
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

        public async Task<List<TenantUsersDto>> GetUsers()
        {
            if (_graphServiceClient == null)
            {
                Authenticate();
            }

            var users = new List<TenantUsersDto>();

            var usersResponse = await _graphServiceClient.Users
                                .GetAsync(requestConfiguration =>
                                {
                                    requestConfiguration.QueryParameters.Select = new string[] { "id,displayName,givenName,surname,jobTitle,department,mail,createdDateTime" };
                                    requestConfiguration.QueryParameters.Top = 100;
                                });
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

        public async Task<List<Chat>> GetChatsByUserId(string userId)
        {
            if (_graphServiceClient == null)
            {
                Authenticate();
            }

            var chats = new List<Chat>();

            try
            {
                // Retrieve chats for a specific user using their userId
                var chatsResponse = await _graphServiceClient.Users[userId].Chats.GetAsync();

                // Use PageIterator for handling pagination
                var pageIterator = PageIterator<Chat, ChatCollectionResponse>.CreatePageIterator(
                    _graphServiceClient,
                    chatsResponse,
                    chat =>
                    {
                        chats.Add(chat);  // Add each chat to the list
                        return true;
                    });

                await pageIterator.IterateAsync();  // Iterate through all pages of chats
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error retrieving chats: {ex.Message}");
            }

            return chats;
        }
    }
}
