using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;

namespace AssessmentsApp.WebApi.Agents
{
    public class AssessmentCreatorApp
    {
        private readonly IConfiguration configuration;
        private PersistentAgentsClient agentsClient;

        public AssessmentCreatorApp(IConfiguration configuration)
        {
            this.configuration = configuration;
#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            agentsClient = AzureAIAgent.CreateAgentsClient(configuration.GetValue<string>("AIProjectConnectionString")!, new AzureCliCredential());
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        }

        internal async Task<SkillsSearchSession> CreateSessionAsync()
        {
            var assessmentTemplate = ReadFileForPromptTemplateConfig("./Agents/Prompts/assessment.yaml");
            var assessmentsAgent = await agentsClient.Administration.CreateAgentAsync("gpt-4.1-mini",
                name: assessmentTemplate.Name,
                description: assessmentTemplate.Description,
                instructions: assessmentTemplate.Template);

#pragma warning disable SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            AzureAIAgent agent = new(assessmentsAgent,
                                        agentsClient,
                                        templateFactory: new KernelPromptTemplateFactory(),
                                        templateFormat: PromptTemplateConfig.SemanticKernelTemplateFormat);
#pragma warning restore SKEXP0110 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


            return new SkillsSearchSession(agentsClient, agent);
        }

        private static PromptTemplateConfig ReadFileForPromptTemplateConfig(string fileName)
        {
            string yaml = File.ReadAllText(fileName);
            return KernelFunctionYaml.ToPromptTemplateConfig(yaml);
        }
    }
}
