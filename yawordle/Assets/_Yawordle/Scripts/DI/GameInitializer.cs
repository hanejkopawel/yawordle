using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using Yawordle.Core;
using Yawordle.Infrastructure.Localization;
using Yawordle.Presentation.ViewModels; 

namespace Yawordle.DI
{
    public class GameInitializer : IAsyncStartable
    {
        private readonly ISettingsService _settingsService;
        private readonly IWordProvider _wordProvider;
        private readonly IGameManager _gameManager;
        private readonly ILocalizationService _localization; 

        public GameInitializer(
            ISettingsService settingsService, 
            IWordProvider wordProvider, 
            IGameManager gameManager,
            ILocalizationService localization) 
        {
            _settingsService = settingsService;
            _wordProvider = wordProvider;
            _gameManager = gameManager;
            _localization = localization;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            
            await _localization.InitializeAsync();
            
            string targetWord;
            if (_settingsService.CurrentSettings.Mode == GameMode.Daily)
            {
                targetWord = await _wordProvider.GetWordOfTheDayAsync();
                if (string.IsNullOrEmpty(targetWord))
                {
                    Debug.LogError("Failed to get word of the day. Falling back to random word.");
                    targetWord = _wordProvider.GetRandomSolutionWord();
                }
            }
            else
            {
                targetWord = _wordProvider.GetRandomSolutionWord();
            }
            
            _gameManager.StartNewGame(targetWord);
        }
    }
}