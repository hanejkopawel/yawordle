using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using Yawordle.Core;

namespace Yawordle.Infrastructure.Localization
{
    public sealed class UnityLocalizationService : ILocalizationService
    {
        public string CurrentLanguage { get; private set; } = "en";
        public event Action<string> LanguageChanged;

        private readonly ISettingsService _settings;

        public UnityLocalizationService(ISettingsService settings)
        {
            _settings = settings;
        }

        public async UniTask InitializeAsync()
        {
            // Ensure Localization system is initialized before use.
            try
            {
                await LocalizationSettings.InitializationOperation.Task;
            }
            catch (Exception e)
            {
                Debug.LogError($"Localization initialization failed: {e.Message}");
            }

            // Pick saved or system language.
            var saved = _settings.CurrentSettings.Language;
            if (!TryFindLocale(saved, out var locale))
            {
                var system = Application.systemLanguage.ToString().ToLowerInvariant();
                if (!TryFindLocale(system, out locale))
                {
                    TryFindLocale("en", out locale); // ultimate fallback
                }
            }

            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
                CurrentLanguage = locale.Identifier.Code; // e.g. "en", "pl"
            }
            else
            {
                CurrentLanguage = "en";
            }
        }

        public async UniTask SetLanguageAsync(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return;

            if (TryFindLocale(languageCode, out var locale))
            {
                LocalizationSettings.SelectedLocale = locale;
                CurrentLanguage = locale.Identifier.Code;

                // Persist in settings
                var s = _settings.CurrentSettings;
                s.Language = CurrentLanguage;
                await _settings.SaveSettingsAsync(s);

                LanguageChanged?.Invoke(CurrentLanguage);
            }
            else
            {
                Debug.LogWarning($"Locale not found for code '{languageCode}'.");
            }
        }

        public string GetString(string table, string entry)
        {
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(entry))
                return entry ?? string.Empty;

            try
            {
                return LocalizationSettings.StringDatabase.GetLocalizedString(table, entry);
            }
            catch
            {
                // When table/entry missing or not yet ready, return key to avoid blank UI.
                return entry;
            }
        }

        private static bool TryFindLocale(string code, out Locale locale)
        {
            locale = null;
            if (string.IsNullOrWhiteSpace(code)) return false;

            // Normalize common variants, e.g. "pl-PL" => "pl"
            var id = new LocaleIdentifier(code);
            locale = LocalizationSettings.AvailableLocales.GetLocale(id);
            if (locale != null) return true;

            // Try two-letter language without region
            var shortCode = code.Length >= 2 ? code[..2].ToLowerInvariant() : code.ToLowerInvariant();
            locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(shortCode));
            return locale != null;
        }
    }
}