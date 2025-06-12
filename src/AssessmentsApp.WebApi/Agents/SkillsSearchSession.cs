using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace AssessmentsApp.WebApi.Agents
{
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class SkillsSearchSession(PersistentAgentsClient agentsClient, AzureAIAgent agent, ChatCompletionAgent chatCompletionAgent)
    {
        // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        internal async Task<string> ProcessRequest(string userInput)
        // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        {
            string? result = string.Empty;
            AgentThread thread = null;
            //https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/azure-ai-agent?pivots=programming-language-csharp

             // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            AzureAIAgentThread agentThread = new(agent.Client);
             // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            try
            {
                //AgentGroupChat chat = new()
                //{
                //    ExecutionSettings = new AgentGroupChatSettings
                //    {
                //        SelectionStrategy = new SequentialSelectionStrategy() { InitialAgent = null },
                //        TerminationStrategy = new NoFeedbackLeftTerminationStrategy()
                //    }
                //};

                StringBuilder sb = new StringBuilder();
                ChatMessageContent message = new(AuthorRole.User, "What are the learning resources available for the new manager in my team");
                await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread))
                {
                    sb.AppendLine(response.Content);
                }

                //Simple Chat Completn using the chat completion agent
                await foreach (var response in chatCompletionAgent.InvokeAsync(userInput))
                {
                    thread = response.Thread;                  
                    sb.AppendLine(response.Message.Content);
                }
                
                result = sb.ToString();
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed, e.g., log them or rethrow
                throw new InvalidOperationException("An error occurred while processing the request.", ex);
            }
            finally
            {
                await thread!.DeleteAsync();
                await agentThread.DeleteAsync();
            }

            return result;
        }

        private sealed class NoFeedbackLeftTerminationStrategy : TerminationStrategy
        {
            // Terminate when the final message contains the term "Article accepted, no further rework necessary." - all done
            protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
            {
                //if (agent.Name != CreativeWriterApp.EditorName)
                //    return Task.FromResult(false);

                return Task.FromResult(history[history.Count - 1].Content?.Contains("Article accepted", StringComparison.OrdinalIgnoreCase) ?? false);
            }
        }

        public async Task CleanupSessionAsync()
        {
            // delete all Agents from the session, otherwise they will not be deleted on the service/backend of Azure AI Agents Service
            //await agentsClient.Administration.DeleteAgentAsync(agent.Id);
            
        }
    }
}