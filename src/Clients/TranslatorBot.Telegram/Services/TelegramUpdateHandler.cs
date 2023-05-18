using Microsoft.Extensions.Logging;
using Sanet.Bots.Telegram.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sanet.Bots.Telegram.Services;

public class TelegramUpdateHandler
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger _log;
    private readonly TelegramBotClient _bot;

    public TelegramUpdateHandler( string telegramBotToken, IDatabaseService databaseService,ILogger log)
    {
        _databaseService = databaseService;
        _log = log;
        _bot = new TelegramBotClient(telegramBotToken);
    }

    public async Task<UpdateHandlerResult> HandleUpdate(Update update)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.MyChatMember:
                    _log.LogInformation($"New chat member message {update.MyChatMember.NewChatMember.User.Username}");
                    return new UpdateHandlerResult(200);
                case UpdateType.Message:
                    if (update.Message == null)
                    {
                        const string error = "Update.Message is null";
                        return new UpdateHandlerResult(404, error);
                    }

                    if (update.Message.MigrateToChatId != null)
                    {
                        _log.LogInformation($"Migrated to chat {update.Message.MigrateToChatId}");
                        return new UpdateHandlerResult(200);
                    }

                    var chatId = update.Message.Chat.Id;

                    if (chatId < 0)
                    {
                        var groupId = (ulong)Math.Abs(chatId);
                        _log.LogInformation("A message in the group {GroupId}", groupId);

                        if (update.Message.Type == MessageType.ChatMembersAdded)
                        {
                            var member = update.Message?.NewChatMembers?.First();
                            if (member == null)
                            {
                                _log.LogError("Member is null");
                            }

                            _log.LogInformation($"New chat member {member.Username}");

                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithUrl("Subscribe for translations",
                                        "https://t.me/SanetTranslatorBot")
                                }
                            });

                            var welcomeMessage = $"Welcome, {member.FirstName}! "
                                                 + "I can translate messages in this chat for you. Please send me a direct message (DM) if you're interested. "
                                                 + $"Please use '/subscribe {groupId}' in a direct message to me";

                            await _bot.SendTextMessageAsync(chatId, welcomeMessage, replyMarkup: inlineKeyboard);

                            return new UpdateHandlerResult(200);
                        }

                        
                        if (update.Message.Type == MessageType.Text)
                        {
                            var subscriptions = await _databaseService.GetSubscriptionsForGroup(groupId);
                            var tasks = subscriptions
                                .Where(s => s.UserId != update.Message.From.Id)
                                .Select(s => _bot.SendTextMessageAsync(s.ChatId, $"Message from {update.Message.From.FirstName}: {update.Message.Text}"));
                            await Task.WhenAll(tasks);
                        }
                                                 
                        return new UpdateHandlerResult(200);
                    }

                    var text = update.Message.Text;
                    if (text.ToLower().Contains("/subscribe"))
                    {
                        var parts = text.Split(' ');
                        foreach (var part in parts)
                        {
                            if (!ulong.TryParse(part.Trim(), out var id)) continue;
                            _log.LogInformation($"Subscribing to {id}");
                            
                            var isSaved = await _databaseService.SaveSubscription(new SubscriptionEntity(id, chatId, update.Message.From.Id));
                                
                            await _bot.SendTextMessageAsync(chatId, isSaved?"Subscribed!":"Cannot subscribe :( Try Again!");
                            return new UpdateHandlerResult(200);
                        }
                    }

                    await _bot.SendTextMessageAsync(chatId, "Hello World!");
                    return new UpdateHandlerResult(200);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e)
        {
            var error = e.Message ?? "Unknown Error";
            return new UpdateHandlerResult(500,error);
        }
    }
}
