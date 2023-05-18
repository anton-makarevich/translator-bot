using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sanet.Bot.Telegram;

public static class TelegramMessageHandler
{

    [FunctionName("TelegramMessageHandler")]
    public static async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var update = JsonConvert.DeserializeObject<Update>(requestBody);
            
            if (update.Type == UpdateType.MyChatMember)
            {
                log.LogInformation($"New chat member message {update.MyChatMember.NewChatMember.User.Username}");
                return new OkResult();
            }
            
            if (update.Message == null)
            {
                log.LogError("Update.Message is null");
                log.LogInformation("Request body: {RequestBody}", requestBody);
                return new BadRequestResult();
            }

            var chatId = update.Message.Chat.Id;
            var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            if (string.IsNullOrEmpty(token))
            {
                log.LogError("TELEGRAM_BOT_TOKEN is null");
                return new InternalServerErrorResult();
            }

            var bot = new TelegramBotClient(token);
            await bot.SendTextMessageAsync(chatId, "Hello World!");

            return new OkResult();
        }
        catch (Exception e)
        {
            log.LogError(e, e.Message ?? "Unknown Error");
            return new InternalServerErrorResult();
        }
    }
}