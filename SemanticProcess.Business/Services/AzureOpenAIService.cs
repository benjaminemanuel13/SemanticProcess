using System.ClientModel;
using OpenAI.Assistants;
using Azure.AI.OpenAI;
using OpenAI.VectorStores;
using OpenAI.Chat;

#pragma warning disable OPENAI001
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace SemanticProcess.Business.Services
{
    public class AzureOpenAIService
    {
        public static string Endpoint { get; set; } = string.Empty;
        public static string Key { get; set; } = string.Empty;
        public static string Model { get; set; } = string.Empty;

        private readonly AzureOpenAIClient _client;

        public AzureOpenAIService() {
            ApiKeyCredential credential = new ApiKeyCredential(Key);
            _client = new AzureOpenAIClient(new Uri(Endpoint), credential);
        }

        public string SimpleAsk(string text, List<ChatMessage> existingMessages = null)
        {
            var client = _client.GetChatClient(Model);

            var message = ChatMessage.CreateUserMessage(text);

            List<ChatMessage> messages = new List<ChatMessage>() {
                message
            };

            var chat = client.CompleteChat(messages);

            return chat.Value.Content[0].Text;
        }

        public void SimpleAskStreamed(string text, List<ChatMessage> existingMessages = null, Action<string> del = null)
        {
            var client = _client.GetChatClient(Model);

            var message = ChatMessage.CreateUserMessage(text);

            List<ChatMessage> messages = new List<ChatMessage>() {
                message
            };

            var chat = client.CompleteChatStreaming(messages);

            foreach (StreamingChatCompletionUpdate completionUpdate in chat)
            {
                foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                {
                    if (del != null)
                    {
                        del(contentPart.Text);
                    }
                    else
                    {
                        Console.Write(contentPart.Text);
                    }
                }
            }
        }
    }
}
