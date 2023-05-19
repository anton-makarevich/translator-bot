using Sanet.Bots.Telegram.Models;

namespace Sanet.Bots.Telegram.Services;

public interface ITranslatorService
{
    Task<LanguageMessage> TranslateMessage(LanguageMessage message);
}