#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yawordle.EditorTools
{
    public static class WordlistExporter
    {
        private const string ResourcesRoot = "Assets/_Yawordle/Resources/Dictionaries";

        [MenuItem("Yawordle/Export Dictionaries → docs/ (GitHub Pages)")]
        public static void ExportToDocs()
        {
            // Resolve <project-root>/docs
            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            var outputRoot = Path.Combine(projectRoot, "docs");
            if (!Directory.Exists(outputRoot))
                Directory.CreateDirectory(outputRoot);

            // Find solutions_<lang>_<len>.txt
            var guids = AssetDatabase.FindAssets("t:TextAsset", new[] { ResourcesRoot });
            var solutionAssets = new List<(string path, string lang, int len)>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileName(path);
                if (!fileName.StartsWith("solutions_", StringComparison.OrdinalIgnoreCase) ||
                    !fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    continue;

                var core = fileName.Substring("solutions_".Length);
                core = core.Substring(0, core.Length - ".txt".Length); // "<lang>_<len>"
                var parts = core.Split('_');
                if (parts.Length != 2) continue;
                var lang = parts[0].ToLowerInvariant();
                if (!int.TryParse(parts[1], out var len)) continue;

                solutionAssets.Add((path, lang, len));
            }

            if (solutionAssets.Count == 0)
            {
                Debug.LogWarning("No solutions_* files found in Resources/Dictionaries.");
                return;
            }

            int files = 0;
            foreach (var item in solutionAssets.OrderBy(s => s.lang).ThenBy(s => s.len))
            {
                var text = AssetDatabase.LoadAssetAtPath<TextAsset>(item.path);
                if (text == null) continue;

                var words = text.text
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => w.Trim())
                    .Where(w => !string.IsNullOrWhiteSpace(w))
                    .Distinct()
                    .ToList();

                var dto = new AnswersDto
                {
                    version = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    answers = words
                };

                var dir = Path.Combine(outputRoot, $"{item.lang}_{item.len}");
                Directory.CreateDirectory(dir);

                var jsonPath = Path.Combine(dir, "answers.json");
                var json = JsonUtility.ToJson(dto, true);
                File.WriteAllText(jsonPath, json);

                files++;
                Debug.Log($"Exported {words.Count} answers → {jsonPath}");
            }

            AssetDatabase.Refresh();
            Debug.Log($"Export complete. Files: {files}. Root: {outputRoot}");
        }

        [Serializable]
        private class AnswersDto
        {
            public string version;
            public List<string> answers;
        }
    }
}
#endif