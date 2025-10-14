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
            };

            try
            {
                var result = await CloudCodeService.Instance.CallEndpointAsync<WordResponse>("getWordOfTheDay", args);
                return result.word;
            }
            catch (CloudCodeException e)
            {
                Debug.LogError($"Error calling Cloud Code: {e.Message}");
                return null; // Return null on error
            }
        }
        
        // Struct to match the JSON response from Cloud Code
        private struct WordResponse { public string word; }
    }
}