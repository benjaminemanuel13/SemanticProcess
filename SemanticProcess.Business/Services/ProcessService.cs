using Microsoft.SemanticKernel;
using SemanticProcess.Common.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS7022
#pragma warning disable CS8321
#pragma warning disable SKEXP0080

namespace SemanticProcess.Business.Services
{
    public class ProcessService
    {
        public static string OpenAIKey { get; set; } = string.Empty;

        public async Task GoAsync()
        {
            ProcessBuilder processBuilder = new("DocumentationGeneration");

            var infoGatheringStep = processBuilder.AddStepFromType<GatherProductInfoStep>();
            var docsGenerationStep = processBuilder.AddStepFromType<GenerateDocumentationStep>();
            var docsPublishStep = processBuilder.AddStepFromType<PublishDocumentationStep>();

            processBuilder
                .OnInputEvent("Start")
                .SendEventTo(new(infoGatheringStep));

            infoGatheringStep
                .OnFunctionResult()
                .SendEventTo(new ProcessFunctionTargetBuilder(docsGenerationStep, "GenerateDocument"));

            docsGenerationStep
                .OnEvent("DocumentationGenerated")
                .SendEventTo(new ProcessFunctionTargetBuilder(docsPublishStep, "Publish"));

            string Model = "gpt-4";

            Kernel kernel = Kernel
                .CreateBuilder()
                .AddOpenAIChatCompletion(Model, OpenAIKey)                
                .Build();

            // Build and run the process
            var process = processBuilder.Build();
            await process.StartAsync(kernel, new KernelProcessEvent { Id = "Start", Data = "Contoso GlowBrew" });
        }
    }
}
