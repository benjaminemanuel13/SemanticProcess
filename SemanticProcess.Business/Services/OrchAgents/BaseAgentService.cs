using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SemanticProcess.Business.Services.OrchAgents
{
    public class BaseAgentService
    {
        public static string OpenAIKey { get; set; } = string.Empty;
        public static string Model { get; set; } = "gpt-4";

        protected Kernel Kernel { get; private set; }

        public BaseAgentService() {
             Kernel = Kernel
                .CreateBuilder()
                .AddOpenAIChatCompletion(Model, OpenAIKey)
                .Build();
        }
    }
}
