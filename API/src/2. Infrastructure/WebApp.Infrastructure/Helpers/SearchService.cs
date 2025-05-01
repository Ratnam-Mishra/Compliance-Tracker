using System.Text.RegularExpressions;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using WebApp.Infrastructure.Models;

namespace WebApp.Infrastructure.Helpers
{
    public class SearchService
    {
        EmbeddingService _embeddingService;
        private readonly SearchClient _searchClient;
        private readonly SearchIndexClient _indexClient;
        private readonly string? _indexName, _vectorSearchProfileName, _vectorSearchHnswConfig;

        public SearchService()
        {
            _indexName = Configuration.GetAppSetting("AzureSearch:IndexName");
            _vectorSearchProfileName = Configuration.GetAppSetting("AzureSearch:VectorSearchProfileName");
            _vectorSearchHnswConfig = Configuration.GetAppSetting("AzureSearch:VectorSearchHnswConfig");
            var searchApiKey = Configuration.GetAppSetting("AzureSearch:ApiKey");
            var searchEndpoint = Configuration.GetAppSetting("AzureSearch:Endpoint"); 
            var endpoint = new Uri(searchEndpoint);
            var credential = new AzureKeyCredential(searchApiKey);
            _searchClient = new SearchClient(endpoint, _indexName, credential);
            _indexClient = new SearchIndexClient(endpoint, credential);
            _embeddingService = new EmbeddingService();
        }

        public async Task<bool> EnsureIndexExists()
        {
            int modelDimensions = 1536;

            var exists = _indexClient.GetIndexNames().Any(name => name == _indexName);
            if (!exists)
            {
                SearchIndex searchIndex = new(_indexName)
                {
                    Fields =
                    {
                            new SimpleField("id", SearchFieldDataType.String) { IsKey = true },
                            new SearchableField("fileName") { IsFilterable = true },
                            new SimpleField("lastModified", SearchFieldDataType.String) { IsFilterable = true },
                            new SearchField("contentVector", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                                {
                                    IsSearchable = true,
                                    VectorSearchDimensions = 1536,
                                    VectorSearchProfileName = _vectorSearchProfileName
                                },
                            new SearchableField("contentText") { IsFilterable = true },
                            new SimpleField("uploadedDate", SearchFieldDataType.String) { IsFilterable = true }
                        },

                    VectorSearch = new()
                    {
                        Profiles =
                            {
                                new VectorSearchProfile(_vectorSearchProfileName, _vectorSearchHnswConfig)
                            },
                        Algorithms =
                            {
                                new HnswAlgorithmConfiguration(_vectorSearchHnswConfig)
                            }
                    },
                    SemanticSearch = new SemanticSearch()
                    {
                        Configurations =
                            {
                                new SemanticConfiguration(
                                    name: "default",
                                    prioritizedFields: new SemanticPrioritizedFields
                                    {
                                        TitleField = new SemanticField("fileName") { FieldName = "fileName" },
                                        ContentFields = { new SemanticField("contentText") { FieldName = "contentText" } }
                                    }
                                )
                            }
                    }
                };
                try
                {
                    var response = await _indexClient.CreateIndexAsync(searchIndex);
                    if (response == null || response.Value == null)
                    {
                        Console.WriteLine("Index creation failed: response is null.");
                    }
                    else
                    {
                        Console.WriteLine("Index created successfully: " + response.Value.Name);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            return true;
        }

        private List<string> SplitComplianceText(string text)
        {
            var chunks = new List<string>();
            var matches = Regex.Matches(text, @"(Purpose:|[0-9]+\.\s*)");
            if (matches.Count == 0)
            {
                // No match found, return full text
                chunks.Add(text);
                return chunks;
            }

            // Now, split based on matches
            int lastIndex = 0;

            foreach (Match match in matches)
            {
                if (match.Index > lastIndex)
                {
                    for (int i = 0; i < matches.Count - 1; i++)
                    {
                        string chunk = text.Substring(matches[i].Index, matches[i + 1].Index - matches[i].Index).Trim();
                        if (!string.IsNullOrWhiteSpace(chunk))
                            chunks.Add(chunk);
                    }
                }
                lastIndex = match.Index;
            }

            // Add last section
            if (lastIndex < text.Length)
            {
                string chunk = text.Substring(lastIndex).Trim();
                if (!string.IsNullOrWhiteSpace(chunk))
                    chunks.Add(chunk);
            }

            return chunks;
        }

        public async Task UploadDocument(string id, string fileName, string lastModified, string contentText, IReadOnlyList<float> embeddingVector)
        {
            var chunks = SplitComplianceText(contentText);

            var documents = new List<object>();
            int chunkIndex = 0;

            foreach (var chunk in chunks)
            {
                var chunkEmbedding = await _embeddingService.GetEmbedding(chunk); // Create embedding PER chunk
                documents.Add(new
                {
                    id = $"{id}-{chunkIndex}",
                    fileName,
                    lastModified,
                    contentText = chunk,
                    contentVector = chunkEmbedding,
                    uploadedDate = DateTime.UtcNow.ToString("o")
                });
                chunkIndex++;
            }

            await _searchClient.MergeOrUploadDocumentsAsync(documents);
        }


        public async Task<bool> IsDocumentAlreadyIndexed(string fileName, string lastModified)
        {
            var options = new SearchOptions
            {
                Filter = $"fileName eq '{fileName.Replace("'", "''")}' and lastModified eq '{lastModified}'",
                Size = 1
            };
            var response = await _searchClient.SearchAsync<object>("*", options);
            await foreach (var result in response.Value.GetResultsAsync())
            {
                return true; // Found
            }
            return false; // Not found
        }

        public async Task<List<DocumentDto>> SearchSimilarDocuments(string searchText, IReadOnlyList<float> embedding)
        {
            ReadOnlyMemory<float> vectorizedResult = embedding.ToArray();
            var matchedDocuments = new List<DocumentDto>();
            try
            {
                // Perform the search query with the embedding vector
                var response = await _searchClient.SearchAsync<DocumentDto>(
                    searchText: searchText,
                    new SearchOptions
                    {
                        VectorSearch = new()
                        {
                            Queries = { new VectorizedQuery(vectorizedResult) {
                            KNearestNeighborsCount = 5,
                            Fields = { "contentVector" }
                        }}
                        },
                        Size = 5,
                        Select = { "fileName", "contentText", "id" }
                    }
                );
                await foreach (var result in response.Value.GetResultsAsync())
                {
                    if (result.Document != null)
                    {
                        matchedDocuments.Add(result.Document);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return matchedDocuments;
        }
    }
}
