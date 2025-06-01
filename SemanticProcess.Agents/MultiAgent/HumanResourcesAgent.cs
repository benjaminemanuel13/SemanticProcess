using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticAgent.Plugins.HumanResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticProcess.Agents.MultiAgent
{
    public class HumanResourcesAgent : BaseAgent
    {
        public HumanResourcesAgent() : base(true) { }

        public override async Task<string> AskAsync(string question, Action<string> del = null)
        {
            OpenAIPromptExecutionSettings executionSettings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

            if (del == null)
            {
                FunctionResult result = await kernel.InvokePromptAsync(question, new(executionSettings));

                return result.ToString();
            }
            else
            {
                var chat = kernel.InvokePromptStreamingAsync(question, new(executionSettings));

                await foreach (StreamingKernelContent completionUpdate in chat)
                {
                    del(completionUpdate.ToString());
                }

                return null;
            }
        }
        protected override void AddPlugins()
        {
            kernel.ImportPluginFromType<EmailPlugin>();
            kernel.ImportPluginFromType<StaffLookupPlugin>();
        }
    }
}
