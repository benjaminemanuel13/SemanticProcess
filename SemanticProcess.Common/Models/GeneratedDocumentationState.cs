using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticProcess.Common.Models
{
    public class GeneratedDocumentationState
    {
        public DocumentInfo LastGeneratedDocument { get; set; } = new();
        public ChatHistory? ChatHistory { get; set; }
    }
}
