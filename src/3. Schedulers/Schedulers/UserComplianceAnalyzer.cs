using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using WebApp.Infrastructure.Agents;
using WebApp.Infrastructure.Helpers;
using WebApp.Infrastructure.Models;

namespace Schedulers
{
    /// <summary>
    /// Analyzes user communication and SharePoint documents for compliance breaches.
    /// </summary>
    public class UserComplianceAnalyzer
    {
        private readonly GraphSharePointHelper _graphSharePointHelper;
        private readonly EmbeddingService _embeddingService;
        private readonly SearchService _searchService;
        private readonly ComplainceAgent _complianceAgent;

        public UserComplianceAnalyzer()
        {
            _graphSharePointHelper = new GraphSharePointHelper();
            _embeddingService = new EmbeddingService();
            _searchService = new SearchService();
            _complianceAgent = new ComplainceAgent();
        }

        /// <summary>
        /// Analyzes user emails and Teams messages for compliance breaches and logs them to SharePoint if found.
        /// </summary>
        public async Task<bool?> ProcessAndValidateUserCommunications()
        {
            var allUsers = await _graphSharePointHelper.GetAllUsers();
            foreach (var user in allUsers.Where(u => u.DisplayName == "Aman Panjwani"))
            {
                await ProcessUserEmailsAndTeamsMessages(user);
            }
            return true;
        }

        private async Task ProcessUserEmailsAndTeamsMessages(UsersDto user)
        {
            // Process Emails
            var userEmails = await _graphSharePointHelper.GetEmailsByUserId(user.Id);
            if (userEmails.Count > 0)
            {
                await AnalyzeUserCommunications(user, userEmails.Cast<object>().ToList(), "Email");
            }

            // Process Teams Messages
            var userTeamsMessages = await _graphSharePointHelper.GetTeamsMessagesByUserId(user.Id);
            if (userTeamsMessages.Count > 0)
            {
                await AnalyzeUserCommunications(user, userTeamsMessages.Cast<object>().ToList(), "Teams Chat");
            }
        }

        private async Task AnalyzeUserCommunications(UsersDto user, List<object> communications, string sourceType)
        {
            var agentInstance = await _complianceAgent.CreateOrReuseComplianceAgentAsync();

            foreach (var communication in communications)
            {
                IReadOnlyList<float>? embedding = null;
                string bodyContent = string.Empty;
                List<string> toRecipients = new List<string>();
                List<string> ccRecipients = new List<string>();

                if (sourceType == "Email")
                {
                    var email = (EmailsDto)communication;
                    embedding = await _embeddingService.GetEmbedding(email.BodyContent);
                    bodyContent = email.BodyContent;
                    toRecipients = email.ToRecipients ?? new List<string>();
                    ccRecipients = email.CcRecipients ?? new List<string>();
                }
                else if (sourceType == "Teams Chat")
                {
                    var teamsMessage = (string)communication;
                    embedding = await _embeddingService.GetEmbedding(teamsMessage);
                    bodyContent = teamsMessage;
                }

                var similarDocs = await _searchService.SearchSimilarDocuments(bodyContent, embedding);
                if (!similarDocs.Any()) continue;

                var prompt = BuildCompliancePrompt(bodyContent, sourceType);
                var agentResponse = await _complianceAgent.AskAgentAsync(agentInstance, prompt);

                if (string.IsNullOrWhiteSpace(agentResponse)) continue;

                try
                {
                    var breaches = JsonConvert.DeserializeObject<List<ComplianceBreachDto>>(agentResponse);
                    if (breaches == null || breaches.Count == 0) continue;

                    foreach (var breach in breaches)
                    {
                        if (sourceType == "Email")
                        {
                            var toRecipientsString = string.Join(";", toRecipients);
                            var ccRecipientsString = string.Join(";", ccRecipients);
                            await _graphSharePointHelper.CreateEmailDetails(
                                user,
                                bodyContent,
                                breach.DocumentTitle,
                                breach.PolicySentence,
                                toRecipientsString,
                                ccRecipientsString,
                                breach.ViolationExplanation,
                                breach.DetectedRiskLevel
                            );
                        }
                        else if (sourceType == "Teams Chat")
                        {
                            await _graphSharePointHelper.CreateMsgDetails(
                                user,
                                bodyContent,
                                breach.DocumentTitle,
                                breach.PolicySentence,
                                breach.ViolationExplanation,
                                breach.DetectedRiskLevel
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing agent response for {sourceType}: {ex.Message}");
                }
            }
        }

        private string BuildCompliancePrompt(string content, string sourceType)
        {
            var escapedContent = content.Replace("\"", "\\\"").Replace("\r", "").Replace("\n", " ");
            return string.Format(ConfigConstants.AIPrompt, sourceType, escapedContent);
        }

        /// <summary>
        /// Indexes new SharePoint documents in Azure AI Search if they aren't already vectorized.
        /// </summary>
        public async Task<bool> RunVectorCheckOnNewDocuments()
        {
            var documents = await _graphSharePointHelper.GetAllFilesFromLibrary();
            await _searchService.EnsureIndexExists();

            foreach (var doc in documents)
            {
                if (await _searchService.IsDocumentAlreadyIndexed(doc.Name, doc.LastModifiedDateTime?.ToString("o")))
                    continue;

                var content = await _graphSharePointHelper.GetSpecificFileStream(doc.Name);
                var text = FilesTextExtracter.ExtractTextFromPdf(content);
                if (string.IsNullOrWhiteSpace(text)) continue;

                var embedding = await _embeddingService.GetEmbedding(text);
                await _searchService.UploadDocument(
                    Guid.NewGuid().ToString(),
                    doc.Name,
                    doc.LastModifiedDateTime?.ToString("o"),
                    text,
                    embedding
                );
            }
            return true;
        }
    }
}
