using Yawordle.Core;
using UnityEngine;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace Yawordle.Infrastructure
{
    public class JsonSettingsService : ISettingsService
    {
        public GameSettings CurrentSettings { get; private set; }
        private readonly string _savePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");

        public JsonSettingsService()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(_savePath))
            {
                CurrentSettings = JsonConvert.DeserializeObject<GameSettings>(File.ReadAllText(_savePath));
                Debug.Log($"<color=cyan>Settings loaded from file: Lang={CurrentSettings.Language}, Length={CurrentSettings.WordLength}</color>");
            }
            else
            {
                CurrentSettings = new GameSettings();
                SaveSettings(CurrentSettings);
            }
        }

        public async UniTask SaveSettingsAsync(GameSettings settings, CancellationToken ct = default)
        {
            CurrentSettings = settings;
            await File.WriteAllTextAsync(_savePath, JsonConvert.SerializeObject(CurrentSettings, Formatting.Indented), ct);
            Debug.Log($"<color=yellow>Settings SAVED to file: Lang={CurrentSettings.Language}, Length={CurrentSettings.WordLength}</color>");
        }
        
        public void SaveSettings(GameSettings settings) => SaveSettingsAsync(settings).Forget();
        
    }
}