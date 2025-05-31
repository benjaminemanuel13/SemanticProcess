
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticProcess.Common.Steps;
using System.Reflection;

#pragma warning disable CS7022
#pragma warning disable CS8321
#pragma warning disable SKEXP0080

static void Main(string[] args)
{
}

// Create the process builder
ProcessBuilder processBuilder = new("DocumentationGeneration");

// Add the steps
var infoGatheringStep = processBuilder.AddStepFromType<GatherProductInfoStep>();
var docsGenerationStep = processBuilder.AddStepFromType<GenerateDocumentationStep>();
var docsPublishStep = processBuilder.AddStepFromType<PublishDocumentationStep>();


// Orchestrate the events
processBuilder
    .OnInputEvent("Start")
    .SendEventTo(new(infoGatheringStep));

infoGatheringStep
    .OnFunctionResult()
    .SendEventTo(new ProcessFunctionTargetBuilder(docsGenerationStep, "GenerateDocument"));

docsGenerationStep
    .OnEvent("DocumentationGenerated")
    .SendEventTo(new ProcessFunctionTargetBuilder(docsPublishStep, "Publish"));

string OpenAIKey = Environment.GetEnvironmentVariable("OPENAIKEY");
string Model = "gpt-4";

Kernel kernel = Kernel
    .CreateBuilder()
    .AddOpenAIChatCompletion(Model, OpenAIKey)
    .Build();

// Build and run the process
var process = processBuilder.Build();
await process.StartAsync(kernel, new KernelProcessEvent { Id = "Start", Data = "Contoso GlowBrew" });

Console.WriteLine("Process completed. Press any key to exit.");
Console.ReadKey();