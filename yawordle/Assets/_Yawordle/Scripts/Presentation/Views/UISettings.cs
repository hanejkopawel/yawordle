using UnityEngine;
using UnityEngine.UIElements;

namespace Yawordle.Presentation
{
    /// <summary>
    /// A ScriptableObject that holds references to global UI assets (VisualTreeAssets).
    /// This allows us to keep UI asset references organized and independent of any scene.
    /// </summary>
    [CreateAssetMenu(fileName = "UISettings", menuName = "Yawordle/UI Settings")]
    public class UISettings : ScriptableObject
    {
        [field:SerializeField] public VisualTreeAsset SettingsPanel { get; set; }
        // public VisualTreeAsset WinPanel { get; set; }
        // public VisualTreeAsset LosePanel { get; set; }
    }
}