using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.Agents.Chat;
using System.Text;

namespace AssessmentsApp.WebApi.Agents
{
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    public class SkillsSearchSession(Kernel kernel, Azure.AI.Agents.Persistent.PersistentAgentsClient agentsClient, AzureAIAgent researcherAgent, ChatCompletionAgent marketingAgent, ChatCompletionAgent writerAgent, ChatCompletionAgent editorAgent)
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    {
        internal async IAsyncEnumerable<AIChatCompletionDelta> ProcessStreamingRequest(CreateWriterRequest createWriterRequest)
        {
            // create an conversation Thread with the Researcher agent
            var threadResponse = await agentsClient.Threads.CreateThreadAsync();
            var thread = threadResponse.Value;

            StringBuilder sbResearchResults = new();
            await foreach (ChatMessageContent response in researcherAgent.InvokeAsync(thread.Id, new KernelArguments() { { "research_context", createWriterRequest.Research } }))
            {
                sbResearchResults.AppendLine(response.Content);
                yield return new AIChatCompletionDelta(Delta: new AIChatMessageDelta
                {
                    Role = AIChatRole.Assistant,
                    Context = new AIChatAgentInfo(CreativeWriterApp.ResearcherName),
                    Content = response.Content,
                });
            }

            StringBuilder sbProductResults = new();
            await foreach (ChatMessageContent response in marketingAgent.InvokeAsync([], new() { { "product_context", createWriterRequest.Products } }))
            {
                sbProductResults.AppendLine(response.Content);
                yield return new AIChatCompletionDelta(Delta: new AIChatMessageDelta
                {
                    Role = AIChatRole.Assistant,
                    Context = new AIChatAgentInfo(CreativeWriterApp.MarketingName),
                    Content = response.Content,
                });
            }

            writerAgent.Arguments["research_context"] = createWriterRequest.Research;
            writerAgent.Arguments["research_results"] = sbResearchResults.ToString();
            writerAgent.Arguments["product_context"] = createWriterRequest.Products;
            writerAgent.Arguments["product_results"] = sbProductResults.ToString();
            writerAgent.Arguments["assignment"] = createWriterRequest.Writing;

            AgentGroupChat chat = new(writerAgent, editorAgent)
            {
                LoggerFactory = kernel.LoggerFactory,
                ExecutionSettings = new AgentGroupChatSettings
                {
                    SelectionStrategy = new SequentialSelectionStrategy() { InitialAgent = writerAgent },
                    TerminationStrategy = new NoFeedbackLeftTerminationStrategy()
                }
            };

            await foreach (ChatMessageContent response in chat.InvokeAsync())
            {
                yield return new AIChatCompletionDelta(Delta: new AIChatMessageDelta
                {
                    Role = AIChatRole.Assistant,
                    Context = new AIChatAgentInfo(response.AuthorName ?? ""),
                    Content = response.Content,
                });
            }
        }

    }
}
