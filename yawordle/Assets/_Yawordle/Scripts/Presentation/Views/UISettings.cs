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
        [field:SerializeField] public VisualTreeAsset EndGamePanel { get; set; }
        [field:SerializeField] public VisualTreeAsset InstructionsPanel { get; set; }

    }
}