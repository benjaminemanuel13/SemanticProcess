
using SemanticProcess.Agents.MultiAgent;
using SemanticProcess.Business.Services;

ProcessService.OpenAIKey = Environment.GetEnvironmentVariable("OPENAIKEY");
BaseAgent.OpenAIKey = Environment.GetEnvironmentVariable("OPENAIKEY");

AzureOpenAIService.Endpoint = Environment.GetEnvironmentVariable("AZUREOPENAIENDPOINT");
AzureOpenAIService.Key = Environment.GetEnvironmentVariable("AZUREOPENAIKEY");
AzureOpenAIService.Model = "gpt-4o-smile";

ProcessService processService = new ProcessService();
await processService.GoAsync();

Console.WriteLine("Process completed. Press any key to exit.");
Console.ReadKey();