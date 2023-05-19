namespace Sanet.Bots.Telegram.Models;

public record LanguageMessage(
    string Author,
    string OriginalLanguage,
    string TranslatedLanguage,
    string Text
)
{
    public string? Translation { get; set; }
}