using DocumentFormat.OpenXml.Spreadsheet;
using WebApp.Infrastructure.Helpers;

namespace Schedulers
{
    public class UserComplianceAnalyzer
    {
        GraphSharePointHelper? _graphSharePointHelper;
        EmbeddingService _embeddingService;
        SearchService _searchService;

        public UserComplianceAnalyzer()
        {
            _graphSharePointHelper = new GraphSharePointHelper();
            _embeddingService = new EmbeddingService();
            _searchService = new SearchService();
        }

        public async Task<bool?> CleanMessages()
        {
            var allUsers = await _graphSharePointHelper?.GetAllUsers();
            if (allUsers.Count > 0)
            {
                foreach (var user in allUsers)
                {
                    if (user.DisplayName == "Aman Panjwani")
                    {
                        //var userMsgs = await _graphSharePointHelper.GetTeamsMessagesByUserId(user.Id);
                        //if (userMsgs.Count > 0)
                        //{
                        //    foreach (var msgs in userMsgs)
                        //    {
                        //        var msgEmbedding = await _embeddingService.GetEmbedding(msgs);
                        //        var similarDocs = await _searchService.SearchSimilarDocuments(msgEmbedding);
                        //        if (similarDocs.Any())
                        //        {
                        //            foreach (var docName in similarDocs)
                        //            {
                        //                var response = await _graphSharePointHelper.CreateMsgDetails(user, msgs, docName.fileName, docName.contentText);
                        //                Console.WriteLine($"  - {docName}");
                        //            }
                        //        }
                        //        else
                        //        {
                        //            Console.WriteLine("❌ No related documents found.");
                        //        }
                        //    }
                        //}
                        var userEmails = await _graphSharePointHelper.GetEmailsByUserId(user.Id);
                        if (userEmails.Count > 0)
                        {
                            var bodyContents = userEmails.Select(email => email.BodyContent).ToList();
                            foreach (var email in userEmails)
                            {
                                var msgEmbedding = await _embeddingService.GetEmbedding(string.Join(",", bodyContents));
                                var similarDocs = await _searchService.SearchSimilarDocuments(msgEmbedding);
                                if (similarDocs.Any())
                                {
                                    foreach (var docName in similarDocs)
                                    {
                                        var toRecipients = string.Join(";", email.ToRecipients ?? new List<string>());
                                        var ccRecipients = string.Join(";", email.CcRecipients ?? new List<string>());
                                        var response = await _graphSharePointHelper.CreateEmailDetails(user, email.BodyContent, docName.fileName, docName.contentText, toRecipients, ccRecipients);
                                        Console.WriteLine($"  - {docName}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("❌ No related documents found.");
                                }
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return true;
        }


        public async Task<bool> DocumentsHandler()
        {
            var documents = await _graphSharePointHelper.GetAllFilesFromLibrary();

            foreach (var doc in documents)
            {
                Console.WriteLine($"📄 Processing: {doc.Name}");
                var s = await _searchService.EnsureIndexExists();
                bool alreadyExists = await _searchService.IsDocumentAlreadyIndexed(doc.Name, doc.LastModifiedDateTime?.ToString("o"));
                if (alreadyExists)
                {
                    Console.WriteLine("✅ Already indexed. Skipping.");
                    continue;
                }

                string text = string.Empty;
                var content = await _graphSharePointHelper.GetSpecificFileStream(doc.Name);
                if (doc.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {

                    text = FilesTextExtracter.ExtractTextFromPdf(content);
                }
                else if (doc.Name.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    text = FilesTextExtracter.ExtractTextFromWord(content);
                }

                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine("⚠️ No text found. Skipping.");
                    continue;
                }

                var embedding = await _embeddingService.GetEmbedding(text);

                await _searchService.UploadDocument(
                    Guid.NewGuid().ToString(),
                    doc.Name,
                    doc.LastModifiedDateTime?.ToString("o"),
                    text,
                    embedding
                );

                Console.WriteLine("✅ Uploaded to Azure Search.");
            }
            return true;
        }
    }
}
