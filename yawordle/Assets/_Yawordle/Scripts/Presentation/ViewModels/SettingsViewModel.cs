using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Yawordle.Core;

namespace Yawordle.Presentation.ViewModels
{
    public class SettingsViewModel
    {
        public GameSettings TempSettings { get; private set; }
        
        private readonly ISettingsService _settingsService;

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            // Create a temporary copy of the settings to modify in the UI.
            // This allows the user to cancel changes.
            TempSettings = new GameSettings
            {
                Language = _settingsService.CurrentSettings.Language,
                Mode = _settingsService.CurrentSettings.Mode,
                WordLength = _settingsService.CurrentSettings.WordLength
            };
        }

        public void SetLanguage(string language) => TempSettings.Language = language;
        public void SetWordLength(int length) => TempSettings.WordLength = length;

        
        public void SaveAndRestart() => SaveAndRestartAsync().Forget();
        
        private async UniTaskVoid SaveAndRestartAsync()
        {
            try
            {
                await _settingsService.SaveSettingsAsync(TempSettings);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            catch (System.Exception ex)
            {
                
                UnityEngine.Debug.LogError($"Failed to save settings and restart: {ex.Message}");
            }
        }
    }
}