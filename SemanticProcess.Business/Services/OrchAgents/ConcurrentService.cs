using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using OpenAI.Assistants;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable OPENAI001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace SemanticProcess.Business.Services.OrchAgents
{
    public class ConcurrentService : BaseAgentService
    {
        public ConcurrentService(): base() { }

        public async Task GoAsync()
        { 
            ChatCompletionAgent physicist = new ChatCompletionAgent
            {
                Name = "PhysicsExpert",
                Description = "An expert in physics.",
                Instructions = "You are an expert in physics. You answer questions from a physics perspective.",
                Kernel = Kernel,
            };
            ChatCompletionAgent chemist = new ChatCompletionAgent
            {
                Name = "ChemistryExpert",
                Description = "An expert in chemistry.",
                Instructions = "You are an expert in chemistry. You answer questions from a chemistry perspective.",
                Kernel = Kernel,
            };


            //---------------------------------------------------
            //  Azure OpenAI Assistant Agent
            //---------------------------------------------------
            ApiKeyCredential credential = new(Key);

            var openClient = OpenAIAssistantAgent.CreateAzureOpenAIClient(credential, new Uri(Endpoint));
            var client = openClient.GetAssistantClient();

            Dictionary<string, string> metadata = new();

            Assistant assistant =
                    await client.CreateAssistantAsync(
                        AzureModel,
                        description: "An assistant that can answer questions about physics and chemistry.",
                        instructions:                                 """
                            Analyze the available data to provide an answer to the user's question.
                            Always format response using markdown.
                            Always include a numerical index that starts at 1 for any lists or tables.
                            Always sort lists in ascending order.
                            """,
                        enableFileSearch: true,
                        metadata: metadata);

            await using Stream stream = File.OpenRead("employees.pdf")!;
            string fileId = await openClient.UploadAssistantFileAsync(stream, "employees.pdf");

            string vectorStoreId =
                await openClient.CreateVectorStoreAsync(
                    [fileId],
                    waitUntilCompleted: true,
                    metadata: metadata);

            //AgentThread thread = new OpenAIAssistantAgentThread(
            //    client,
            //    vectorStoreId: vectorStoreId,
            //    metadata: metadata);

            OpenAIAssistantAgent assistantAgent = new(assistant, client);

            //----------------------------------------------------


            ConcurrentOrchestration orchestration = new(physicist, chemist, assistantAgent);

            InProcessRuntime runtime = new InProcessRuntime();
            await runtime.StartAsync();

            var result = await orchestration.InvokeAsync("What is temperature?", runtime);

            string[] output = await result.GetValueAsync(TimeSpan.FromSeconds(60));

            Console.WriteLine($"# RESULT:\n{string.Join("\n\n", output.Select(text => $"{text}"))}");

            await runtime.RunUntilIdleAsync();

            await client.DeleteAssistantAsync(assistantAgent.Id);
            await openClient.DeleteVectorStoreAsync(vectorStoreId);
            await openClient.DeleteFileAsync(fileId);
        }
    }
}
