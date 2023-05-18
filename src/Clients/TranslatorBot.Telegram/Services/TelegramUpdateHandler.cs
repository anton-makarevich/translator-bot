using Microsoft.Extensions.Logging;
using Sanet.Bots.Telegram.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Sanet.Bots.Telegram.Services;

public class TelegramUpdateHandler
{
    private readonly ILogger _log;
    private readonly string _telegramBotToken;

    public TelegramUpdateHandler(ILogger log, string telegramBotToken)
    {
        _log = log;
        _telegramBotToken = telegramBotToken;
    }

    public async Task<UpdateHandlerResult> HandleUpdate(Update update)
    {
        try
        {
            if (update.Type == UpdateType.MyChatMember)
            {
                _log.LogInformation($"New chat member message {update.MyChatMember.NewChatMember.User.Username}");
                return new UpdateHandlerResult(200);
            }

            if (update.Message == null)
            {
                const string error = "Update.Message is null";
                return new UpdateHandlerResult(404,error);
            }

            if (update.Message.MigrateToChatId != null)
            {
                _log.LogInformation($"Migrated to chat {update.Message.MigrateToChatId}");
                return new UpdateHandlerResult(200);
            }

            var chatId = update.Message.Chat.Id;
            var bot = new TelegramBotClient(_telegramBotToken);
            await bot.SendTextMessageAsync(chatId, "Hello World!");
            
            return new UpdateHandlerResult(200);
        }
        catch (Exception e)
        {
            var error = e.Message ?? "Unknown Error";
            return new UpdateHandlerResult(500,error);
        }
    }
}
