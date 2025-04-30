
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpenAI;

namespace WebApp.Infrastructure.Helpers
{
    public class EmbeddingService
    {
        private readonly AzureOpenAIClient _azureClient;
        private readonly string? _deploymentName;
        private readonly string? _embeddingModel;
        private const int MaxTokens = 8192;  // Max tokens allowed by the model (including prompt and response)

        public EmbeddingService()
        {
            var endpoint = Configuration.GetAppSetting("AzureAI:Endpoint");
            var apiKey = Configuration.GetAppSetting("AzureAI:ApiKey");
            _deploymentName = Configuration.GetAppSetting("AzureAI:DeploymentName");
            _embeddingModel = Configuration.GetAppSetting("AzureAI:EmbeddingModelName");

            AzureKeyCredential credential = new AzureKeyCredential(apiKey);
            _azureClient = new(new Uri(endpoint), credential);
        }

        public async Task<IReadOnlyList<float>> GetEmbedding(string input)
        {
            var response = await _azureClient.GetEmbeddingClient(_embeddingModel).GenerateEmbeddingAsync(input);
            return response.Value.ToFloats().ToArray();
        }


        public async Task<IReadOnlyList<float>> ProcessEmailsInBatchesAsync(List<string> userEmails)
        {
            var chunks = SplitIntoChunks(userEmails);
            var allEmbeddings = new List<float>();

            foreach (var chunk in chunks)
            {
                var chunkText = string.Join(", ", chunk);
                var embedding = await GetEmbeddingChunk(chunkText);
                allEmbeddings.AddRange(embedding);  // Add all elements of the embedding array
            }

            return allEmbeddings.AsReadOnly();  // Return a read-only list
        }


        private List<List<string>> SplitIntoChunks(List<string> userEmails)
        {
            var chunks = new List<List<string>>();
            var currentChunk = new List<string>();
            int currentTokenCount = 0;

            foreach (var email in userEmails)
            {
                // Estimate token count for the email text (this is an approximation)
                int emailTokenCount = EstimateTokenCount(email);

                if (currentTokenCount + emailTokenCount > MaxTokens)
                {
                    chunks.Add(currentChunk);
                    currentChunk = new List<string>();
                    currentTokenCount = 0;
                }

                currentChunk.Add(email);
                currentTokenCount += emailTokenCount;
            }

            if (currentChunk.Any())  // Add the last chunk if it has any emails
            {
                chunks.Add(currentChunk);
            }

            return chunks;
        }

        private int EstimateTokenCount(string text)
        {
            // Rough approximation: assume each word is 1 token (you can refine this based on actual tokenization)
            return text.Split(' ').Length;
        }

        private async Task<float[]> GetEmbeddingChunk(string text)
        {
            // Simulate calling the embedding service and getting the embedding
            // Replace this with your actual embedding service call
            Console.WriteLine($"Getting embedding for text chunk: {text}");
            await Task.Delay(100); // Simulating a delay for the embedding service

            return new float[100];  // Simulating an embedding (replace with actual service response)
        }
    }
}
