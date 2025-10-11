using Yawordle.Core;
using UnityEngine;
using System.IO;

namespace Yawordle.Infrastructure
{
    public class JsonSettingsService : ISettingsService
    {
        public GameSettings CurrentSettings { get; private set; }
        private readonly string _savePath;

        public JsonSettingsService()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");
            LoadSettings();
        }

        private void LoadSettings()
        {
            if (File.Exists(_savePath))
                CurrentSettings = JsonUtility.FromJson<GameSettings>(File.ReadAllText(_savePath));
            else
            {
                CurrentSettings = new GameSettings();
                SaveSettings(CurrentSettings);
            }
        }

        public void SaveSettings(GameSettings settings)
        {
            CurrentSettings = settings;
            File.WriteAllText(_savePath, JsonUtility.ToJson(CurrentSettings, true));
            Debug.Log($"Settings saved to: {_savePath}");
        }
    }
}