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

            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            if (string.IsNullOrEmpty(telegramBotToken))
            {
                log.LogError("TELEGRAM_BOT_TOKEN is null");
                return new InternalServerErrorResult();
            }
            var cosmosConnection = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");
            
            if (string.IsNullOrEmpty(cosmosConnection))
            {
                log.LogError("COSMOS_CONNECTION_STRING is null");
                return new InternalServerErrorResult();
            }

            var cosmosService = new CosmosService(cosmosConnection, log);

            var updateHandler = new TelegramUpdateHandler(telegramBotToken,cosmosService,log);
            var updateResult = await updateHandler.HandleUpdate(update);

            return updateResult.ToAzureActionResult(log);
        }
        catch (Exception e)
        {
            log.LogError(e,"{Message}", e.Message);
            return new InternalServerErrorResult();
        }
    }
}