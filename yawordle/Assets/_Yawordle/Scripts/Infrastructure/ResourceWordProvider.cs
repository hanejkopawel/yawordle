using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Yawordle.Core;

namespace Yawordle.Infrastructure
{
    public class ResourceWordProvider : IWordProvider
    {
        private readonly Dictionary<string, List<string>> _solutionWordsCache = new();
        private readonly Dictionary<string, HashSet<string>> _validWordsCache = new();

        private readonly ISettingsService _settingsService;
        private readonly IUgsService _ugsService;

        public ResourceWordProvider(ISettingsService settingsService, IUgsService ugsService)
        {
            _settingsService = settingsService;
            _ugsService = ugsService;
        }

        public string GetRandomSolutionWord()
        {
            string language = _settingsService.CurrentSettings.Language;
            int length = _settingsService.CurrentSettings.WordLength;
            
            LoadWordsIfRequired(language, length);

            string cacheKey = $"{language}_{length}";
            if (_solutionWordsCache.TryGetValue(cacheKey, out var wordList) && wordList.Count > 0)
            {
                return wordList[Random.Range(0, wordList.Count)];
            }

            Debug.LogError($"No solution words found for language '{language}' and length {length}.");
            return "ERROR";
        }

        public bool IsValidWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;

            string language = _settingsService.CurrentSettings.Language;
            int length = word.Length;
            
            LoadWordsIfRequired(language, length);
            
            string cacheKey = $"{language}_{length}";
            return _validWordsCache.TryGetValue(cacheKey, out var wordSet) && wordSet.Contains(word.ToUpper());
        }
        
        public async UniTask<string> GetWordOfTheDayAsync()
        {
            var settings = _settingsService.CurrentSettings;
            LoadWordsIfRequired(settings.Language, settings.WordLength);
            return await _ugsService.GetWordOfTheDayAsync(settings.Language, settings.WordLength);
        }

        private void LoadWordsIfRequired(string language, int length)
        {
            string cacheKey = $"{language}_{length}";
            if (_validWordsCache.ContainsKey(cacheKey))
            {
                return;
            }

            var solutions = LoadWordList($"Dictionaries/solutions_{cacheKey}");
            _solutionWordsCache[cacheKey] = solutions;

            var allowedGuesses = LoadWordList($"Dictionaries/guesses_{cacheKey}");
            var validWordSet = new HashSet<string>(allowedGuesses);
            
            validWordSet.UnionWith(solutions);
            _validWordsCache[cacheKey] = validWordSet;

            Debug.Log($"Loaded {solutions.Count} solution words and {validWordSet.Count} total valid guesses for '{cacheKey}'.");
        }
        
        private List<string> LoadWordList(string resourcePath)
        {
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogWarning($"Could not find dictionary file at 'Resources/{resourcePath}'. An empty list will be used.");
                return new List<string>();
            }
            
            return textAsset.text
                .Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Select(word => word.Trim().ToUpper())
                .ToList();
        }
    }
}