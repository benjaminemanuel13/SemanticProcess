using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticProcess.Agents
{
    public interface IAgent
    {
        Task<string> Ask(string question);
    }
}
