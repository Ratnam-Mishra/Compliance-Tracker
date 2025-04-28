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
        private readonly string? _indexName;

        public SearchService()
        {
            _indexName = Configuration.GetAppSetting("AzureSearch:IndexName");
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
            string vectorSearchProfileName = "compliance-documents";
            string vectorSearchHnswConfig = "compliance-documents-hsnw-vector-config";
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
                                    VectorSearchDimensions = 1536, // set your vector dimension size here
                                    VectorSearchProfileName = vectorSearchProfileName
                                },
                            new SearchableField("contentText") { IsFilterable = true },
                            new SimpleField("uploadedDate", SearchFieldDataType.String) { IsFilterable = true }
                        },

                    VectorSearch = new()
                    {
                        Profiles =
                            {
                                new VectorSearchProfile(vectorSearchProfileName, vectorSearchHnswConfig)
                            },
                        Algorithms =
                            {
                                new HnswAlgorithmConfiguration(vectorSearchHnswConfig)
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

        public async Task UploadDocument(string id, string fileName, string lastModified, string contentText, IReadOnlyList<float> embeddingVector)
        {
            var doc = new
            {
                id,
                fileName,
                lastModified,
                contentText,
                contentVector = embeddingVector,
                uploadedDate = DateTime.UtcNow.ToString("o")
            };

            await _searchClient.MergeOrUploadDocumentsAsync(new[] { doc });
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

        public async Task<List<DocumentDto>> SearchSimilarDocuments(IReadOnlyList<float> embedding)
        {
            ReadOnlyMemory<float> vectorizedResult = embedding.ToArray();
            var matchedDocuments = new List<DocumentDto>();
            try
            {
                // Perform the search query with the embedding vector
                var response = await _searchClient.SearchAsync<DocumentDto>(
                    searchText: null,
                    new SearchOptions
                    {
                        VectorSearch = new()
                        {
                            Queries = { new VectorizedQuery(vectorizedResult) {
                            KNearestNeighborsCount = 3,
                            Fields = { "contentVector" }
                } }
                        },
                        Size = 3, // Limit number of results
                        Select = { "fileName", "contentText", "id" } // Choose fields you want back
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
