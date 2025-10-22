using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;

namespace Yawordle.Infrastructure
{
    public class UgsService : IUgsService
    {
        public async UniTask InitializeAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("UGS Initialized.");
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log($"Signed in anonymously as: {AuthenticationService.Instance.PlayerId}");
            }
        }

        public async UniTask<string> GetWordOfTheDayAsync(string language, int wordLength)
        {
            await InitializeAsync(); // Ensure we are signed in

            var args = new Dictionary<string, object>
            {
                { "language", language },
                { "wordLength", wordLength }
                // TIP: for debugging you can pass { "date", "YYYY-MM-DD" } temporarily
            };

            try
            {
                var result = await CloudCodeService.Instance.CallEndpointAsync<WordResponse>("getWordOfTheDay", args);

                if (string.IsNullOrEmpty(result?.word))
                {
                    Debug.LogWarning("CloudCode returned empty 'word'. Falling back to null.");
                    return null;
                }

                // Optional: log for verification
                Debug.Log($"CloudCode WOTD: {result.word} (date={result.date}, version={result.dictVersion})");
                return result.word;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Error calling Cloud Code: {e.Message}");
                return null; // Return null on error
            }
        }

        // DTO must include all fields present in the JSON response.
        // Using public fields with exact lower-case names to match payload.
        [System.Serializable]
        private class WordResponse
        {
            public string word;
            public string date;
            public string dictVersion;
        }
    }
}