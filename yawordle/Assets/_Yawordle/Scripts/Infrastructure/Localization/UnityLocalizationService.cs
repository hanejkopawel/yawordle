using System;
using System.Collections.Generic;
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
        public bool IsReady { get; private set; }
        public event Action Initialized;
        public event Action<string> LanguageChanged;

        private readonly ISettingsService _settings;

        public UnityLocalizationService(ISettingsService settings)
        {
            _settings = settings;
        }

        public async UniTask InitializeAsync()
        {
            if (IsReady) return;

            try
            {
                await LocalizationSettings.InitializationOperation.Task;
            }
            catch (Exception e)
            {
                Debug.LogError($"Localization initialization failed: {e.Message}");
            }

            // Pick saved or system language
            var saved = _settings.CurrentSettings.Language;
            if (!TryFindLocale(saved, out var locale))
            {
                var system = Application.systemLanguage.ToString().ToLowerInvariant();
                if (!TryFindLocale(system, out locale))
                {
                    TryFindLocale("en", out locale); // fallback
                }
            }

            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
                CurrentLanguage = locale.Identifier.Code;
            }

            IsReady = true;
            Initialized?.Invoke();
        }

        public async UniTask SetLanguageAsync(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
                return;

            if (!TryFindLocale(languageCode, out var locale))
            {
                Debug.LogWarning($"Locale not found: '{languageCode}'");
                return;
            }

            LocalizationSettings.SelectedLocale = locale;
            CurrentLanguage = locale.Identifier.Code;

            var s = _settings.CurrentSettings;
            s.Language = CurrentLanguage;
            await _settings.SaveSettingsAsync(s);
            LanguageChanged?.Invoke(CurrentLanguage);
        }

        public string GetString(string table, string entry)
        {
            if (!IsReady) return entry; // avoid mixed UI before init
            if (string.IsNullOrEmpty(table) || string.IsNullOrEmpty(entry))
                return entry ?? string.Empty;

            try
            {
                return LocalizationSettings.StringDatabase.GetLocalizedString(table, entry);
            }
            catch
            {
                return entry;
            }
        }

        public IReadOnlyList<LanguageInfo> GetAvailableLanguages()
        {
            var list = new List<LanguageInfo>();
            var locales = LocalizationSettings.AvailableLocales?.Locales;
            if (locales == null) return list;

            foreach (var loc in locales)
            {
                var code = loc.Identifier.Code;
                var display = string.IsNullOrWhiteSpace(loc.LocaleName) ? code : loc.LocaleName;
                list.Add(new LanguageInfo(code, display));
            }
            return list;
        }

        private static bool TryFindLocale(string code, out Locale locale)
        {
            locale = null;
            if (string.IsNullOrWhiteSpace(code)) return false;

            var id = new LocaleIdentifier(code);
            locale = LocalizationSettings.AvailableLocales.GetLocale(id);
            if (locale != null) return true;

            var shortCode = code.Length >= 2 ? code[..2].ToLowerInvariant() : code.ToLowerInvariant();
            locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(shortCode));
            return locale != null;
        }
    }
}