using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sanet.Bots.Telegram.Azure.Extensions;
using Sanet.Bots.Telegram.Azure.Services;
using Sanet.Bots.Telegram.Services;
using Telegram.Bot.Types;

namespace Sanet.Bots.Telegram.Azure;

public static class TelegramMessageHandler
{

    [FunctionName("TelegramMessageHandler")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        log.LogInformation("Request body: {RequestBody}", requestBody);

        try
        {
            var update = JsonConvert.DeserializeObject<Update>(requestBody);

            var telegramBotToken =GetEnvironmentVariable("TELEGRAM_BOT_TOKEN",log);
            var cosmosConnection = GetEnvironmentVariable("COSMOS_CONNECTION_STRING",log);
            var openAiToken = GetEnvironmentVariable("OPENAI_TOKEN",log);
            var azureLanguageServiceKey = GetEnvironmentVariable("LANGUAGE_SERVICE_KEY",log);
            var azureLanguageServiceEndpoint = GetEnvironmentVariable("LANGUAGE_SERVICE_ENDPOINT",log);
            
            var translatorService = new GptTranslatorService(openAiToken, log);
            var cosmosService = new CosmosService(cosmosConnection, log);
            var azureLanguageService =
                new AzureTextAnalyticsService(azureLanguageServiceKey, azureLanguageServiceEndpoint, log);

            var updateHandler = new TelegramUpdateHandler(telegramBotToken,cosmosService, translatorService, azureLanguageService,log);
            var updateResult = await updateHandler.HandleUpdate(update);

            return updateResult.ToAzureActionResult(log);
        }
        catch (Exception e)
        {
            log.LogError(e,"{Message}", e.Message);
            return new InternalServerErrorResult();
        }
    }
    
    public static string GetEnvironmentVariable(string variableName, ILogger log)
    {
        var value = Environment.GetEnvironmentVariable(variableName);

        if (!string.IsNullOrEmpty(value)) return value;
        log.LogError($"{variableName} is null");
        throw  new ArgumentNullException(variableName);
    }
}