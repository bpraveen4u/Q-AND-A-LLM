using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace AssessmentsApp.WebApi.Agents
{
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class SkillsSearchSession(PersistentAgentsClient agentsClient, AzureAIAgent agent)
#pragma warning restore SKEXP0110
    {
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        internal async Task<string> ProcessRequest(string userInput)
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        {
            string? result = string.Empty;
            //https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/azure-ai-agent?pivots=programming-language-csharp

            //var assessmentTemplate = ReadFileForPromptTemplateConfig("./Agents/Prompts/assessment.yaml");

            
            //var assessmentsAgent = await agentsClient.Administration.GetAgentAsync("asst_32IkQqw7tCO9aGBOxBezivXA");
//#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//            AzureAIAgent agent = new(assessmentsAgent, 
//                                        agentsClient,
//                                        templateFactory: new KernelPromptTemplateFactory(),
//                                        templateFormat: PromptTemplateConfig.SemanticKernelTemplateFormat);
//#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
//            // create an conversation Thread with the Researcher agent
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            AzureAIAgentThread agentThread = new(agent.Client);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            try
            {
                ChatMessageContent message = new(AuthorRole.User, userInput);
                await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread))
                {
                    result = response.Content;
                }
            }
            finally
            {
                await agentThread.DeleteAsync();
            }

            return result;
        }

        public async Task CleanupSessionAsync()
        {
            // delete all Agents from the session, otherwise they will not be deleted on the service/backend of Azure AI Agents Service
            await agentsClient.Administration.DeleteAgentAsync(agent.Id);
        }
    }
}