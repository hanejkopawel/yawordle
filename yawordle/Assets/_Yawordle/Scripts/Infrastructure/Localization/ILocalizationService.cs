using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Yawordle.Infrastructure.Localization
{
    // Simple facade over Unity Localization to keep the rest of the app clean.
    public interface ILocalizationService
    {
        string CurrentLanguage { get; }
        bool IsReady { get; }              
        event Action Initialized; 
        event Action<string> LanguageChanged;
        UniTask InitializeAsync();
        UniTask SetLanguageAsync(string languageCode);
        string GetString(string table, string entry);
        IReadOnlyList<LanguageInfo> GetAvailableLanguages();
    }
    
    public readonly struct LanguageInfo
    {
        public readonly string Code;        // e.g. "en", "pl"
        public readonly string DisplayName; // e.g. "English (en)", "Polski (pl)"

        public LanguageInfo(string code, string displayName)
        {
            Code = code;
            DisplayName = displayName;
        }
        public override string ToString() => $"{DisplayName} [{Code}]";
    }
}