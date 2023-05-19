namespace Sanet.Bots.Telegram.Services;

public interface ITextAnalyticsService
{
    Task<string> DetectLanguage(string text);
}