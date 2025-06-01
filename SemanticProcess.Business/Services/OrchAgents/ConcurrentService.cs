using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration.Concurrent;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                Instructions = "You are an expert in physics. You answer questions from a physics perspective.",
                Kernel = Kernel,
            };
            ChatCompletionAgent chemist = new ChatCompletionAgent
            {
                Name = "ChemistryExpert",
                Instructions = "You are an expert in chemistry. You answer questions from a chemistry perspective.",
                Kernel = Kernel,
            };

            ConcurrentOrchestration orchestration = new(physicist, chemist);

            InProcessRuntime runtime = new InProcessRuntime();
            await runtime.StartAsync();

            var result = await orchestration.InvokeAsync("What is temperature?", runtime);

            string[] output = await result.GetValueAsync(TimeSpan.FromSeconds(20));

            Console.WriteLine($"# RESULT:\n{string.Join("\n\n", output.Select(text => $"{text}"))}");

            await runtime.RunUntilIdleAsync();
        }
    }
}
