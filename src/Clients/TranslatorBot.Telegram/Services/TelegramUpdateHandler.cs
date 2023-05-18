﻿using Microsoft.Extensions.Logging;
using Sanet.Bots.Telegram.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Sanet.Bots.Telegram.Services;

public class TelegramUpdateHandler
{
    private readonly ILogger _log;
    private readonly TelegramBotClient _bot;

    public TelegramUpdateHandler(ILogger log, string telegramBotToken)
    {
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
                        _log.LogInformation("A message in the group");

                        if (update.Message.Type != MessageType.ChatMembersAdded) return new UpdateHandlerResult(200);

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
                                             + $"Please use '/subscribe {Math.Abs(chatId)}' in a direct message to me";

                        await _bot.SendTextMessageAsync(chatId, welcomeMessage, replyMarkup: inlineKeyboard);

                        return new UpdateHandlerResult(200);
                    }

                    var text = update.Message.Text;
                    if (text.ToLower().Contains("/subscribe"))
                    {
                        var parts = text.Split(' ');
                        foreach (var part in parts)
                        {
                            if (!long.TryParse(part.Trim(), out var id)) continue;
                            _log.LogInformation($"Subscribing to {id}");
                                
                            await _bot.SendTextMessageAsync(chatId, "Subscribed!");
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
