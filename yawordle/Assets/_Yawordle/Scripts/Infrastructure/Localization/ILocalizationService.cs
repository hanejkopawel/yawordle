using System;
using Cysharp.Threading.Tasks;

namespace Yawordle.Infrastructure.Localization
{
    // Simple facade over Unity Localization to keep the rest of the app clean.
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        event Action<string> LanguageChanged;

        UniTask InitializeAsync();
        UniTask SetLanguageAsync(string languageCode);

        // Returns localized string or the key itself if missing.
        string GetString(string table, string entry);
    }
}