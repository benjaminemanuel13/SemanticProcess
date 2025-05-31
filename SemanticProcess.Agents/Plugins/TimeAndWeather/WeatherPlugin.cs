using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticAgent.Plugins.TimeAndWeather
{
    public class WeatherPlugin : IPlugin
    {
        [KernelFunction]
        [Description("Gets the current weather for the specified city")]
        public string GetWeatherForCity(string cityName) =>
        cityName.ToLower() switch
        {
            "boston" => "61 and rainy",
            "london" => "55 and cloudy",
            "miami" => "80 and sunny",
            "paris" => "60 and rainy",
            "tokyo" => "50 and sunny",
            "sydney" => "75 and sunny",
            "tel aviv" => "80 and sunny",
            _ => "31 and snowing",
        };
    }
}
