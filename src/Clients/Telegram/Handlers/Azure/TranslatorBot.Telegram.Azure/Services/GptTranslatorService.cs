using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Sanet.Bots.Telegram.Models;
using Sanet.Bots.Telegram.Services;

public class GptTranslatorService : ITranslatorService
{
    private readonly ILogger _log;
    private readonly OpenAIClient _client;

    public GptTranslatorService(string openAiApiKey, ILogger log)
    {
        _log = log;
        _client = new OpenAIClient(openAiApiKey, new OpenAIClientOptions());
    }

    public async Task<LanguageMessage> TranslateMessage(LanguageMessage message)
    {
        var system = $"You are a translator from {message.OriginalLanguage} to {message.TranslatedLanguage}.";
        var prompt = $"Translate '{message.Text}' from {message.OriginalLanguage} to {message.TranslatedLanguage}. Return only translated text, no comments from you.";
     
        _log.LogInformation("Translating {Prompt}",prompt);
        
        var chatCompletionsOptions = new ChatCompletionsOptions()
        {
            Messages =
            {
                new ChatMessage(ChatRole.System, system),
                new ChatMessage(ChatRole.User, prompt),
            }
        };

        var response = await _client.GetChatCompletionsAsync(
            deploymentOrModelName: "gpt-3.5-turbo",
            chatCompletionsOptions);

        message.Translation = response.Value.Choices[0].Message.Content.Trim();
        return message;
    }
}