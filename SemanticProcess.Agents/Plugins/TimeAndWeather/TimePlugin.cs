using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAgent.Plugins.TimeAndWeather
{
    public class TimePlugin : IPlugin
    {
        [KernelFunction]
        [Description("Retrieves the current time in UTC")]
        public string GetCurrentUtcTime() => DateTime.UtcNow.ToString("R");
    }
}
