using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI.Assistants;
using OpenAI.Files;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable OPENAI001
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace SemanticProcess.Business.Services
{
    public class CodeService
    {
        public static string Endpoint { get; set; }
        public static string Key { get; set; }
        public static string Model { get; set; }

        public async Task GoAsync()
        {
            ApiKeyCredential credential = new(Key);

            AzureOpenAIClient client = OpenAIAssistantAgent.CreateAzureOpenAIClient(credential, new Uri(Endpoint));

            Console.WriteLine("Uploading files...");
            OpenAIFileClient fileClient = client.GetOpenAIFileClient();
            OpenAIFile fileDataCountryDetail = await fileClient.UploadFileAsync("PopulationByAdmin1.csv", FileUploadPurpose.Assistants);
            OpenAIFile fileDataCountryList = await fileClient.UploadFileAsync("PopulationByCountry.csv", FileUploadPurpose.Assistants);

            Console.WriteLine("Defining agent...");
            AssistantClient assistantClient = client.GetAssistantClient();
            Assistant assistant =
                await assistantClient.CreateAssistantAsync(
                    Model,
                    name: "SampleAssistantAgent",
                    instructions:
                            """
                        Analyze the available data to provide an answer to the user's question.
                        Always format response using markdown.
                        Always include a numerical index that starts at 1 for any lists or tables.
                        Always sort lists in ascending order.
                        """,
                    enableCodeInterpreter: true,
                    codeInterpreterFileIds: [fileDataCountryList.Id, fileDataCountryDetail.Id]);

            // Create agent
            OpenAIAssistantAgent agent = new(assistant, assistantClient);

            Console.WriteLine("Creating thread...");
            OpenAIAssistantAgentThread agentThread = new(assistantClient);

            Console.WriteLine("Ready!");

            try
            {
                bool isComplete = false;
                List<string> fileIds = [];
                do
                {
                    Console.WriteLine();
                    Console.Write("> ");
                    string input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }
                    if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                    {
                        isComplete = true;
                        break;
                    }

                    var message = new ChatMessageContent(AuthorRole.User, input);

                    Console.WriteLine();

                    bool isCode = false;
                    await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(message, agentThread))
                    {
                        if (isCode != (response.Metadata?.ContainsKey(OpenAIAssistantAgent.CodeInterpreterMetadataKey) ?? false))
                        {
                            Console.WriteLine();
                            isCode = !isCode;
                        }

                        // Display response.
                        Console.Write($"{response.Content}");

                        // Capture file IDs for downloading.
                        fileIds.AddRange(response.Items.OfType<StreamingFileReferenceContent>().Select(item => item.FileId));
                    }
                    Console.WriteLine();

                    // Download any files referenced in the response.
                    await DownloadResponseImageAsync(fileClient, fileIds);
                    fileIds.Clear();
                } while (!isComplete);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Cleaning-up...");
                await Task.WhenAll(
                    [
                        agentThread.DeleteAsync(),
                        assistantClient.DeleteAssistantAsync(assistant.Id),
                        fileClient.DeleteFileAsync(fileDataCountryList.Id),
                        fileClient.DeleteFileAsync(fileDataCountryDetail.Id),
                    ]);
            }
        }

        private static async Task DownloadResponseImageAsync(OpenAIFileClient client, ICollection<string> fileIds)
        {
            if (fileIds.Count > 0)
            {
                Console.WriteLine();
                foreach (string fileId in fileIds)
                {
                    await DownloadFileContentAsync(client, fileId, launchViewer: true);
                }
            }
        }

        private static async Task DownloadFileContentAsync(OpenAIFileClient client, string fileId, bool launchViewer = false)
        {
            OpenAIFile fileInfo = await client.GetFileAsync(fileId);
            if (fileInfo.Purpose == FilePurpose.AssistantsOutput)
            {
                string filePath =
                    Path.Combine(
                        Path.GetTempPath(),
                        Path.GetFileName(Path.ChangeExtension(fileInfo.Filename, ".png")));

                BinaryData content = await client.DownloadFileAsync(fileId);
                await using FileStream fileStream = new(filePath, FileMode.CreateNew);
                await content.ToStream().CopyToAsync(fileStream);
                Console.WriteLine($"File saved to: {filePath}.");

                if (launchViewer)
                {
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/C start {filePath}"
                        });
                }
            }
        }
    }
}
