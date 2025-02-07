using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernelPoc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Populate values from your OpenAI deployment
            var modelId = "gpt-4o-mini";
            var localModelId = "deepseek-r1-distill-qwen-7b";
            var endpoint = "";
            var localEndpoints = "http://localhost:1234/v1/";
            var localApiKey = "11";
            HttpClient httpClient1 = new()
            {
                BaseAddress = new Uri(localEndpoints)
            };

            // Create a kernel with Azure OpenAI chat completion
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(modelId: localModelId, apiKey: localApiKey, httpClient: httpClient1);

            // Add enterprise components
            builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Information));

            // Build the kernel
            Kernel kernel = builder.Build();
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            // Add a plugin (the LightsPlugin class is defined below)
            kernel.Plugins.AddFromType<LightsPlugin>("Lights");

            // Enable planning
            OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            // Create a history store the conversation
            var history = new ChatHistory();

            // Initiate a back-and-forth chat
            string? userInput;
            do
            {
                // Collect user input
                Console.Write("User > ");
                userInput = Console.ReadLine();

                // Add user input
                history.AddUserMessage(userInput);

                // Get the response from the AI
                var result = await chatCompletionService.GetChatMessageContentAsync(
                    history,
                    executionSettings: openAIPromptExecutionSettings,
                    kernel: kernel);

                // Print the results
                Console.WriteLine("Assistant > " + result);

                // Add the message from the agent to the chat history
                history.AddMessage(result.Role, result.Content ?? string.Empty);
            } while (userInput is not null);
        }
    }
}
