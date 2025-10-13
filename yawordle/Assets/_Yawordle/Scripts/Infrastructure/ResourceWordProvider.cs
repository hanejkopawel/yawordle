using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yawordle.Core;

namespace Yawordle.Infrastructure
{
    /// <summary>
    /// An IWordProvider implementation that loads dictionaries from text files
    /// located within a Resources/Dictionaries folder.
    /// It caches loaded dictionaries to prevent redundant file I/O operations.
    /// </summary>
    public class ResourceWordProvider : IWordProvider
    {
        // Cache for words that can be the solution. Key format: "en_5", "pl_6", etc.
        private readonly Dictionary<string, List<string>> _solutionWordsCache = new();
        
        // Cache for all valid words. For now, it's the same as solution words.
        private readonly Dictionary<string, HashSet<string>> _validWordsCache = new();

        private readonly ISettingsService _settingsService;

        public ResourceWordProvider(ISettingsService settingsService)
        {
            _settingsService = settingsService;
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

            Debug.LogError($"No solution words found for language '{language}' and length {length}. " +
                           $"Returning fallback word. Ensure 'Dictionaries/words_{cacheKey}.txt' exists in Resources.");
            return "ERROR"; // Fallback word to indicate an issue
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

        /// <summary>
        /// Loads the word list for a given language and length if it's not already in the cache.
        /// </summary>
        private void LoadWordsIfRequired(string language, int length)
        {
            string cacheKey = $"{language}_{length}";
            if (_validWordsCache.ContainsKey(cacheKey))
            {
                return; // Already cached
            }

            string resourcePath = $"Dictionaries/words_{cacheKey}";
            var textAsset = Resources.Load<TextAsset>(resourcePath);

            if (textAsset == null)
            {
                Debug.LogError($"Could not find dictionary file at 'Resources/{resourcePath}'.");
                // Add empty collections to the cache to prevent future load attempts for this key.
                _solutionWordsCache[cacheKey] = new List<string>();
                _validWordsCache[cacheKey] = new HashSet<string>();
                return;
            }

            // Split by newline characters, remove empty entries, trim whitespace, and convert to uppercase.
            var words = textAsset.text
                .Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Select(word => word.Trim().ToUpper())
                .ToList();
            
            _solutionWordsCache[cacheKey] = words;
            _validWordsCache[cacheKey] = new HashSet<string>(words);
            
            Debug.Log($"Loaded and cached {words.Count} words from '{resourcePath}'.");
        }
    }
}