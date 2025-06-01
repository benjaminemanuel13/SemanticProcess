
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.Agents.Runtime.InProcess;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticProcess.Business.Services.OrchAgents.Custom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable OPENAI001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace SemanticProcess.Business.Services.OrchAgents
{
    public class GroupChatService : BaseAgentService
    {
        ChatHistory history = [];

        public GroupChatService(): base() {}


        public async Task GoAsync()
        {
            ChatCompletionAgent writer = new ChatCompletionAgent
            {
                Name = "CopyWriter",
                Description = "A copy writer",
                Instructions = "You are a copywriter with ten years of experience and are known for brevity and a dry humor. The goal is to refine and decide on the single best copy as an expert in the field. Only provide a single proposal per response. You're laser focused on the goal at hand. Don't waste time with chit chat. Consider suggestions when refining an idea.",
                Kernel = Kernel,
            };

            ChatCompletionAgent editor = new ChatCompletionAgent
            {
                Name = "Reviewer",
                Description = "An editor.",
                Instructions = "You are an art director who has opinions about copywriting born of a love for David Ogilvy. The goal is to determine if the given copy is acceptable to print. If so, state that it is approved. If not, provide insight on how to refine suggested copy without example.",
                Kernel = Kernel,
            };

            // CustomRoundRobinGroupChatManager
            // Replace RoundRobinGroupChatManager with this if user input required.

            GroupChatOrchestration orchestration = new GroupChatOrchestration(
                new CustomRoundRobinGroupChatManager { MaximumInvocationCount = 5, InteractiveCallback = InteractiveCallbackAsync },
                writer,
                editor)
                {
                    ResponseCallback = responseCallbackAsync,
                };

            InProcessRuntime runtime = new InProcessRuntime();
            await runtime.StartAsync();

            var result = await orchestration.InvokeAsync(
                "Create a slogan for a new electric SUV that is affordable and fun to drive.", runtime);

            string output = await result.GetValueAsync(TimeSpan.FromSeconds(60));
            Console.WriteLine($"\n# RESULT: {output}");
            Console.WriteLine("\n\nORCHESTRATION HISTORY");

            // Uncomment to see the history of messages in the orchestration at the end of the run.
            //foreach (ChatMessageContent message in history)
            //{
            //this.WriteAgentChatMessage(message);
            //}

            await runtime.RunUntilIdleAsync();
        }

        ValueTask<ChatMessageContent> InteractiveCallbackAsync()
        {
            ChatMessageContent input = new(AuthorRole.User, "I like it");
            Console.WriteLine($"\n# INPUT: {input.Content}\n");
            return ValueTask.FromResult(input);
        }

        private void WriteAgentChatMessage(ChatMessageContent message)
        {
            if (message.Role == AuthorRole.User )
            {
                Console.WriteLine($"USER: {message.Content}");
            }
            else if (message.Role == AuthorRole.Assistant)
            {
                Console.WriteLine($"{message.AuthorName}: {message.Content}");
            }
            else
            {
                Console.WriteLine($"SYSTEM: {message.Content}");
            }
        }

        ValueTask responseCallbackAsync(ChatMessageContent response)
        {
            history.Add(response);

            // Use this to write the response to the console or any other output.
            WriteAgentChatMessage(response);

            return ValueTask.CompletedTask;
        }
    }
}
