using Azure.AI.Agents.Persistent;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable OPENAI001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace SemanticProcess.Business.Services
{
    public class AssistantOpenService
    {
        public static string Endpoint { get; set; }
        public static string Key { get; set; }
        public static string Model { get; set; }

        private OpenAIAssistantAgent agent;
        private AgentThread thread;

        public async Task GoAsync()
        {
            ApiKeyCredential credential = new(Key);

            AssistantClient client = OpenAIAssistantAgent.CreateAzureOpenAIClient(credential, new Uri(Endpoint)).GetAssistantClient();

            var openClient = OpenAIAssistantAgent.CreateAzureOpenAIClient(credential, new Uri(Endpoint));

            Dictionary<string, string> metadata = new();

            Assistant assistant =
                    await client.CreateAssistantAsync(
                        Model,
                        enableFileSearch: true,
                        metadata: metadata);

            // Create the agent
            agent = new(assistant, client);

            // Upload file - Using a table of fictional employees.
            await using Stream stream = File.OpenRead("employees.pdf")!;
            string fileId = await openClient.UploadAssistantFileAsync(stream, "employees.pdf");

            // Download File
            // var file = (openClient.GetOpenAIFileClient().GetFile(fileId)).Value;

            // var downloaded = openClient.GetOpenAIFileClient().DownloadFile(fileId);
            // Stream fileStream = downloaded.Value.ToStream();
            // End Download File


            // Create a vector-store
            string vectorStoreId =
                await openClient.CreateVectorStoreAsync(
                    [fileId],
                    waitUntilCompleted: true,
                    metadata: metadata);

            // Create a thread associated with a vector-store for the agent conversation.
            thread = new OpenAIAssistantAgentThread(
                client,
                vectorStoreId: vectorStoreId,
                metadata: metadata);

            // Respond to user input
            try
            {
                await InvokeAgentAsync("Who is the youngest employee?");
                await InvokeAgentAsync("Who works in sales?");
                await InvokeAgentAsync("I have a customer request, who can help me?");
            }
            finally
            {
                await thread.DeleteAsync();
                await client.DeleteAssistantAsync(agent.Id);
                await openClient.DeleteVectorStoreAsync(vectorStoreId);
                await openClient.DeleteFileAsync(fileId);
            }
        }

        // Local function to invoke agent and display the conversation messages.
        async Task InvokeAgentAsync(string input, Action<string> del = null)
        {
            Microsoft.SemanticKernel.ChatMessageContent message = new(AuthorRole.User, input);
            WriteAgentChatMessage(message);

            List<Microsoft.SemanticKernel.ChatMessageContent> messages = new() { message };

            if (del == null)
            {
                await foreach (Microsoft.SemanticKernel.ChatMessageContent response in agent.InvokeAsync(messages, thread))
                {
                    WriteAgentChatMessage(response);
                }
            }
            else
            {
                var chat = agent.InvokeStreamingAsync(messages, thread);
                await foreach (StreamingChatMessageContent response in chat)
                {
                    del(response.ToString());
                }
            }
        }

        void WriteAgentChatMessage(Microsoft.SemanticKernel.ChatMessageContent message)
        {
            // Include ChatMessageContent.AuthorName in output, if present.
            string authorExpression = message.Role == AuthorRole.User ? string.Empty : $" - {message.AuthorName ?? "*"}";
            // Include TextContent (via ChatMessageContent.Content), if present.
            string contentExpression = string.IsNullOrWhiteSpace(message.Content) ? string.Empty : message.Content;
            bool isCode = message.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false;
            string codeMarker = isCode ? "\n  [CODE]\n" : " ";
            Console.WriteLine($"\n# {message.Role}{authorExpression}:{codeMarker}{contentExpression}");

            // Provide visibility for inner content (that isn't TextContent).
            foreach (KernelContent item in message.Items)
            {
                if (item is AnnotationContent annotation)
                {
                    if (annotation.Kind == AnnotationKind.UrlCitation)
                    {
                        Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: {annotation.ReferenceId} - {annotation.Title}");
                    }
                    else
                    {
                        Console.WriteLine($"  [{item.GetType().Name}] {annotation.Label}: File #{annotation.ReferenceId}");
                    }
                }
                else if (item is FileReferenceContent fileReference)
                {
                    Console.WriteLine($"  [{item.GetType().Name}] File #{fileReference.FileId}");
                }
                else if (item is ImageContent image)
                {
                    Console.WriteLine($"  [{item.GetType().Name}] {image.Uri?.ToString() ?? image.DataUri ?? $"{image.Data?.Length} bytes"}");
                }
                else if (item is FunctionCallContent functionCall)
                {
                    Console.WriteLine($"  [{item.GetType().Name}] {functionCall.Id}");
                }
                else if (item is FunctionResultContent functionResult)
                {
                    //Console.WriteLine($"  [{item.GetType().Name}] {functionResult.CallId} - {functionResult.Result?.AsJson() ?? "*"}");
                }
            }

            if (message.Metadata?.TryGetValue("Usage", out object? usage) ?? false)
            {
                if (usage is RunStepTokenUsage assistantUsage)
                {
                    WriteUsage(assistantUsage.TotalTokenCount, assistantUsage.InputTokenCount, assistantUsage.OutputTokenCount);
                }
                else if (usage is RunStepCompletionUsage agentUsage)
                {
                    WriteUsage(agentUsage.TotalTokens, agentUsage.PromptTokens, agentUsage.CompletionTokens);
                }
                else if (usage is ChatTokenUsage chatUsage)
                {
                    WriteUsage(chatUsage.TotalTokenCount, chatUsage.InputTokenCount, chatUsage.OutputTokenCount);
                }
            }

            void WriteUsage(long totalTokens, long inputTokens, long outputTokens)
            {
                Console.WriteLine($"  [Usage] Tokens: {totalTokens}, Input: {inputTokens}, Output: {outputTokens}");
            }
        }
    }
}
