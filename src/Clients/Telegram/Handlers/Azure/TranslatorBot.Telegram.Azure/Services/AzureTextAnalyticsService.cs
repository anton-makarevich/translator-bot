using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Logging;
using Sanet.Bots.Telegram.Services;

namespace Sanet.Bots.Telegram.Azure.Services;

public class AzureTextAnalyticsService : ITextAnalyticsService
{
    private readonly ILogger _log;
    private readonly TextAnalyticsClient _client;

    public AzureTextAnalyticsService(string azureKey, string azureEndpoint, ILogger log)
    {
        _log = log;
        var credential = new AzureKeyCredential(azureKey);
        var endpoint = new Uri(azureEndpoint);
        _client = new TextAnalyticsClient(endpoint, credential);
    }

    public async Task<string> DetectLanguage(string text)
    {
        var response = await _client.DetectLanguageAsync(text);
        _log.LogInformation("Language: {Name}",response.Value.Name);
        _log.LogInformation("ISO6391 Language: {Name}",response.Value.Iso6391Name);
        return response.Value.Iso6391Name;
    }
}               