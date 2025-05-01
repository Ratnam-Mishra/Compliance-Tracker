using Azure.AI.Projects;
using Azure.Identity;
using Azure.Core;
using WebApp.Infrastructure.Helpers;

namespace WebApp.Infrastructure.Agents
{
    public class ComplainceAgent
    {
        private readonly AIProjectClient _projectClient;
        private readonly AgentsClient _agentsClient;
        private readonly string _indexName, _agentName, _deploymentName;


        public ComplainceAgent()
        {
            var connectionString = Configuration.GetAppSetting("AzureAI:ConnectionString");
            _agentName = Configuration.GetAppSetting("AzureAI:AgentName");
            _deploymentName = Configuration.GetAppSetting("AzureAI:DeploymentName");
            _indexName = Configuration.GetAppSetting("AzureSearch:IndexName");

            var clientOptions = new AIProjectClientOptions();
            clientOptions.AddPolicy(new CustomHeadersPolicy(), HttpPipelinePosition.PerCall);

            _projectClient = new AIProjectClient(connectionString, new DefaultAzureCredential(), clientOptions);
            _agentsClient = _projectClient.GetAgentsClient();

        }

        public async Task<Agent> CreateOrReuseComplianceAgentAsync()
        {
            try
            {
                // Step 1: Check if agent already exists
                var existingAgents = await _agentsClient.GetAgentsAsync();
                var agent = existingAgents.Value.FirstOrDefault(a => a.Name == _agentName);
                if (agent != null)
                {
                    Console.WriteLine($"Reusing existing agent: {agent.Id}");
                    return agent;
                }

                Console.WriteLine("Agent not found. Creating new one...");

                ListConnectionsResponse connections = await _projectClient.GetConnectionsClient().GetConnectionsAsync(ConnectionType.AzureAISearch).ConfigureAwait(false);

                if (connections?.Value == null || connections.Value.Count == 0)
                {
                    throw new InvalidOperationException("No connections found for the Azure AI Search.");
                }

                ConnectionResponse connection = connections.Value[0];


                var searchResource = new ToolResources
                {
                    AzureAISearch = new AzureAISearchResource
                    {
                        IndexList =
                                {
                                    new AISearchIndexResource(connection.Id, _indexName) {
                                        QueryType= AzureAISearchQueryType.Semantic
                                    }
                                }
                    }
                };

                var agentResponse = await _agentsClient.CreateAgentAsync(
                    model: _deploymentName,
                    name: _agentName,
                    instructions: ConfigConstants.agentInstructions,
                    tools: new List<ToolDefinition> { new AzureAISearchToolDefinition() },
                toolResources: searchResource);
                return agentResponse.Value;
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.Message); }

            return null;
        }


        public async Task<string> AskAgentAsync(Agent agent, string userQuestion)
        {
            var threadResponse = await _agentsClient.CreateThreadAsync();
            var thread = threadResponse.Value;

            await _agentsClient.CreateMessageAsync(thread.Id, MessageRole.User, userQuestion);

            var runResponse = await _agentsClient.CreateRunAsync(thread, agent);

            do
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                runResponse = await _agentsClient.GetRunAsync(thread.Id, runResponse.Value.Id);
            }
            while (runResponse.Value.Status == RunStatus.Queued || runResponse.Value.Status == RunStatus.InProgress);

            var afterRunMessagesResponse = await _agentsClient.GetMessagesAsync(thread.Id);
            var messages = afterRunMessagesResponse.Value.Data;

            foreach (var threadMessage in messages)
            {
                Console.WriteLine($"{threadMessage.CreatedAt:HH:mm:ss} [{threadMessage.Role}]");

                foreach (var contentItem in threadMessage.ContentItems)
                {
                    if (contentItem is MessageTextContent textItem)
                        Console.WriteLine(textItem.Text);
                }
            }

            var assistantReply = messages
                                .Where(m => m.Role == MessageRole.Agent)
                                .SelectMany(m => m.ContentItems)
                                .OfType<MessageTextContent>()
                                .FirstOrDefault();

            return assistantReply?.Text ?? string.Empty;
        }

        public Task<string?> AskAgentAsync(string agentInstance, string prompt)
        {
            throw new NotImplementedException();
        }
    }
}
