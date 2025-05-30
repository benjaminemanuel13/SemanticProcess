using Microsoft.SemanticKernel;
using SemanticProcess.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SKEXP0080

namespace SemanticProcess.Common.Steps
{
    public class PublishDocumentationStep : KernelProcessStep
    {
        [KernelFunction("Publish")]
        public DocumentInfo PublishDocumentation(DocumentInfo document)
        {
            // For example purposes we just write the generated docs to the console
            Console.WriteLine($"[{nameof(PublishDocumentationStep)}]:\tPublishing product documentation approved by user: \n{document.Title}\n{document.Content}");
            return document;
        }
    }
}
