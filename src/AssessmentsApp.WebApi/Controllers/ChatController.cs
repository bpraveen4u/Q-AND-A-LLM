using AssessmentsApp.WebApi.Agents;
using Microsoft.AspNetCore.Mvc;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using AssessmentsApp.ServiceDefaults.Models;

namespace AssessmentsApp.WebApi.Controllers
{
    [ApiController, Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AssessmentCreatorApp assessmentCreatorApp;
        private readonly IDeserializer yamlDeserializer;

        public ChatController(AssessmentCreatorApp assessmentCreatorApp)
        {
            this.assessmentCreatorApp = assessmentCreatorApp;

            yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        }

        [HttpPost("agent")]
        [Consumes("application/json")]
        public async Task<string> ProcessMessage(InputModel inputModel)
        {
            //string userInput = "A Large Language Model (LLM) is a type of artificial intelligence system designed to understand and generate human-like text based on vast amounts of data. These models are trained on diverse sources such as books, websites, and articles, enabling them to grasp grammar, context, and even nuanced meanings. LLMs use deep learning, particularly transformer architectures, to process and predict language patterns. They can perform a wide range of tasks including answering questions, summarizing content, translating languages, writing code, and more. The \\\"large\\\" in LLM refers to the billions (or even trillions) of parameters—adjustable weights in the neural network—that help the model make accurate predictions. These models are not conscious or sentient but are capable of producing coherent and contextually relevant responses. Their performance improves with scale, meaning larger models generally understand and generate language more effectively. LLMs are used in applications like chatbots, virtual assistants, search engines, and content creation tools. Despite their capabilities, they can sometimes produce incorrect or biased outputs, so human oversight remains important.";
            var session = await assessmentCreatorApp.CreateSessionAsync();
            try
            {
                var response = await session.ProcessRequest(inputModel.Message!);
                return response;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await session.CleanupSessionAsync();
            }
        }
    }
}
