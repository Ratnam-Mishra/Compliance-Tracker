//using Azure;
//using Azure.AI.OpenAI;
//using Microsoft.Extensions.Configuration;
//using OpenAI.Chat;
//using OpenAI;
//using System.Text.Json;
//using Microsoft.Graph.Models;
//using ChatMessage = OpenAI.Chat.ChatMessage;
//using WebApp.Infrastructure.Helpers;

//namespace Schedulers
//{
//    public class LLMProcessor
//    {
//        private readonly AzureOpenAIClient _azureClient;
//        private readonly string _deploymentName;

//        public LLMProcessor()
//        {
//            var endpoint = ConfigurationHelper.GetAppSetting("AzureOpenAI:Endpoint");
//            var apiKey = ConfigurationHelper.GetAppSetting("AzureOpenAI:ApiKey");
//            _deploymentName = ConfigurationHelper.GetAppSetting("AzureOpenAI:DeploymentName");

//            AzureKeyCredential credential = new AzureKeyCredential(apiKey);

//            _azureClient = new(new Uri(endpoint), credential);
//        }

//        public async Task<bool?> ProcessWithLLM(string message)
//        {
//            // Initialize the ChatClient with the specified deployment name
//            ChatClient chatClient = _azureClient.GetChatClient(_deploymentName);

//            // Create a list of chat messages
//            var messages = new List<ChatMessage>
//            {
//                                new SystemChatMessage("You are an AI assistant that helps people find information."), 
//                                new UserChatMessage($"Clean up this message by removing unnecessary words or filler: {message}")
//                            };


//            // Create chat completion options

//            var options = new ChatCompletionOptions
//            {
//                Temperature = (float)0.7,
//                MaxOutputTokenCount = 800,

//                TopP = (float)0.95,
//                FrequencyPenalty = (float)0,
//                PresencePenalty = (float)0
//            };

//            try
//            {
//                // Create the chat completion request
//                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

//                // Print the response
//                if (completion != null)
//                {
//                    Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));
//                }
//                else
//                {
//                    Console.WriteLine("No response received.");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"An error occurred: {ex.Message}");
//            }
//            return true;
//        }

//    }

//}
