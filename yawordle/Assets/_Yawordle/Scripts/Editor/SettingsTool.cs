#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Yawordle.Editor
{
    public static class SettingsTools
    {
        [MenuItem("Tools/Yawordle/Delete Saved Settings")]
        public static void DeleteSavedSettings()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "gamesettings.json");
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log($"<color=orange>Deleted saved settings file at: {savePath}</color>");
                EditorUtility.DisplayDialog("Success", "Saved settings file has been deleted.", "OK");
            }
            else
            {
                Debug.LogWarning("No saved settings file found to delete.");
                EditorUtility.DisplayDialog("Info", "No saved settings file found to delete.", "OK");
            }
        }
    }
}

#endif