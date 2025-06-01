using Microsoft.SemanticKernel.Agents.Orchestration.GroupChat;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable OPENAI001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace SemanticProcess.Business.Services.OrchAgents.Custom
{
    public class CustomRoundRobinGroupChatManager : RoundRobinGroupChatManager
    {
        public override ValueTask<GroupChatManagerResult<bool>> ShouldRequestUserInput(ChatHistory history, CancellationToken cancellationToken = default)
        {
            string? lastAgent = history.LastOrDefault()?.AuthorName;

            //Console.Write(lastAgent + ":" + " " + history.LastOrDefault()?.Content + "\n");

            if (lastAgent is null)
            {
                return ValueTask.FromResult(new GroupChatManagerResult<bool>(false) { Reason = "No agents have spoken yet." });
            }

            if (lastAgent == "Reviewer")
            {
                return ValueTask.FromResult(new GroupChatManagerResult<bool>(true) { Reason = "User input is needed after the reviewer's message." });
            }

            return ValueTask.FromResult(new GroupChatManagerResult<bool>(false) { Reason = "User input is not needed until the reviewer's message." });
        }
    }
}
