using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticAgent.Plugins;
using SemanticAgent.Plugins.HumanResources;
using SemanticProcess.Business.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SemanticProcess.Agents.MultiAgent
{
    public class DynamicAgent : BaseAgent
    {
        private readonly AzureOpenAIService svc;
        private List<PluginLookup> plugins;

        public DynamicAgent(AzureOpenAIService svc) : base()
        {
            this.svc = svc;
        }

        // A good question to ask is: send an email from john doe to jane smith subject "Hello" body "Hello"
        // this will lookup the email addresses in the StaffLookup plugin and send the email using the EmailSender plugin.

        public override async Task<string> Ask(string question)
        {
            plugins = DetermineAgents(question, Directory.GetCurrentDirectory());

            AddPlugins();

            OpenAIPromptExecutionSettings executionSettings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

            FunctionResult result = await kernel.InvokePromptAsync(question, new(executionSettings));

            return result.ToString();
        }

        protected override void AddPlugins()
        {
            foreach (var plugin in plugins)
            {
                var assembly = Assembly.LoadFrom(plugin.AssemblyPath);
                var type = assembly.GetType(plugin.Type);

                var plugme = Activator.CreateInstance(type);

                kernel.Plugins.AddFromObject(plugme);
            }
        }

        public List<PluginLookup> DetermineAgents(string message, string dir)
        {
            List<PluginLookup> lookups = new List<PluginLookup>();

            Directory.GetFiles(dir, "*.dll", SearchOption.AllDirectories)
                .ToList()
                .ForEach(file =>
                {
                    var assembly = Assembly.LoadFrom(file);
                    var types = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IPlugin)))
                        .ToList();
                    foreach (var type in types)
                    {
                        var agentInstance = Activator.CreateInstance(type);
                        if (agentInstance != null)
                        {
                            var desc = type.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                .FirstOrDefault();

                            foreach(var method in type.GetMethods())
                            {
                                var description = method.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                    .FirstOrDefault() as DescriptionAttribute;
                                var kernelFunction = method.GetCustomAttributes(typeof(KernelFunctionAttribute), false)
                                    .FirstOrDefault();

                                if (description != null && kernelFunction != null)
                                {
                                    var ass = file;
                                    var thistype = type;

                                    if (!lookups.Any(x => x.Type == thistype.FullName && x.Method == method.Name))
                                    {
                                        lookups.Add(new PluginLookup()
                                        {
                                            AssemblyPath = file,
                                            Description = description.Description,
                                            Type = thistype.FullName,
                                            Method = method.Name
                                        });
                                    }
                                }
                            }
                        }   
                    }
                });

            string prompt = "From the following list of plugins, which ones will I need to fulful this query: '" + message + "'\r\n\r\n";
            prompt += "Plugins:\r\n\r\n";

            foreach (var lookup in lookups) {
                prompt += lookup.Type + ", " + lookup.Description + "\r\n\r\n";
            }

            prompt += "Only give me the Types of the plugins as a comma seperated list";

            var response = svc.SimpleAsk(prompt);
            response = response.Replace(" ", "");

            var filtered = response.Split(',');

            return lookups.Where(x => filtered.Contains(x.Type)).ToList();
        }
    }
}
