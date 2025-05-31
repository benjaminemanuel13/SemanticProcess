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

        public override async Task<string> Ask(string question)
        {
            OpenAIPromptExecutionSettings executionSettings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

            FunctionResult result = await kernel.InvokePromptAsync(question, new(executionSettings));

            return result.ToString();
        }
        protected override void AddPlugins()
        {
            kernel.ImportPluginFromType<EmailPlugin>();
            kernel.ImportPluginFromType<StaffLookupPlugin>();
        }
    }
}
