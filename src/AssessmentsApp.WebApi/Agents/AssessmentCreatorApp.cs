using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace AssessmentsApp.WebApi.Agents
{
    public class AssessmentCreatorApp
    {
        private readonly Kernel defaultKernel;
        private readonly IConfiguration configuration;
        private PersistentAgentsClient agentsClient;

        public AssessmentCreatorApp(Kernel defaultKernel, IConfiguration configuration)
        {
            this.defaultKernel = defaultKernel;
            this.configuration = configuration;
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            agentsClient = AzureAIAgent.CreateAgentsClient(configuration.GetValue<string>("AIProjectConnectionString")!, new AzureCliCredential());
            // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        }

        internal async Task<SkillsSearchSession> CreateSessionAsync()
        {
            var assessmentTemplate = ReadFileForPromptTemplateConfig("./Agents/Prompts/assessment.yaml");

            var agent = await agentsClient.Administration.GetAgentAsync("asst_YK2flcJLkjtQBgnEC9qkJsiN");
            AzureAIAgent a = new(agent, agentsClient);
            //var assessmentsAgent = await agentsClient.Administration.CreateAgentAsync("gpt-4.1-mini",
            //    name: assessmentTemplate.Name,
            //    description: assessmentTemplate.Description,
            //    instructions: assessmentTemplate.Template);

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            //AzureAIAgent agent = new(assessmentsAgent,
            //                            agentsClient,
            //                            templateFactory: new KernelPromptTemplateFactory(),
            //                            templateFormat: PromptTemplateConfig.SemanticKernelTemplateFormat);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            ChatCompletionAgent chatCompletionAgent = new(assessmentTemplate, templateFactory: new KernelPromptTemplateFactory())
            {
                // Set the chat completion model to use for this agent
                Name = assessmentTemplate.Name,
                Kernel = defaultKernel,
            };

            return new SkillsSearchSession(agentsClient, a, chatCompletionAgent);
        }

        private static PromptTemplateConfig ReadFileForPromptTemplateConfig(string fileName)
        {
            string yaml = File.ReadAllText(fileName);
            return KernelFunctionYaml.ToPromptTemplateConfig(yaml);
        }
    }
}
