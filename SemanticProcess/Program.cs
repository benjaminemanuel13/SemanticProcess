
using SemanticProcess.Agents.MultiAgent;
using SemanticProcess.Business.Services;

#pragma warning disable CS8601 // Possible null reference assignment.
ProcessService.OpenAIKey = Environment.GetEnvironmentVariable("OPENAIKEY");
BaseAgent.OpenAIKey = Environment.GetEnvironmentVariable("OPENAIKEY");

CodeService.Endpoint = Environment.GetEnvironmentVariable("AZUREOPENAIENDPOINT");
CodeService.Key = Environment.GetEnvironmentVariable("AZUREOPENAIKEY");
CodeService.Model = "gpt-4o-smile";

AzureOpenAIService.Endpoint = Environment.GetEnvironmentVariable("AZUREOPENAIENDPOINT");
AzureOpenAIService.Key = Environment.GetEnvironmentVariable("AZUREOPENAIKEY");
AzureOpenAIService.Model = "gpt-4o-smile";
#pragma warning restore CS8601 // Possible null reference assignment.

ProcessService processService = new ProcessService();
await processService.GoAsync();

Console.WriteLine("Process completed. Press any key to exit.");
Console.ReadKey();