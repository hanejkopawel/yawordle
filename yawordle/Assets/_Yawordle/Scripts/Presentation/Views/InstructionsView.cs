using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;
using Yawordle.Core;

namespace Yawordle.Presentation.Views
{
    public class InstructionsView : IStartable
    {
        private readonly ISettingsService _settingsService;
        private readonly UISettings _uiSettings;

        private VisualElement _modalContainer;
        private VisualElement _instructionsOverlayInstance; 
        private VisualElement _instructionsPanel;
        private Button _closeButton;
        private Button _openHelpButton;

        public InstructionsView(ISettingsService settingsService, UISettings uiSettings)
        {
            _settingsService = settingsService;
            _uiSettings = uiSettings;
        }

        public void Start()
        {
            var root = Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            _modalContainer = root.Q<VisualElement>("modal-container");
            _openHelpButton = root.Q<Button>("help-button");
            
            PreparePanel();

            _openHelpButton.clicked += ShowPanel;

            if (!_settingsService.CurrentSettings.HasSeenInstructions) 
                ShowPanel();
        }

        private void PreparePanel()
        {
            if (_uiSettings.InstructionsPanel == null)
            {
                Debug.LogError("InstructionsPanel VisualTreeAsset is not assigned in UISettings.");
                return;
            }
            _instructionsOverlayInstance = _uiSettings.InstructionsPanel.Instantiate();
            _modalContainer.Add(_instructionsOverlayInstance);
            _instructionsPanel = _instructionsOverlayInstance.Q<VisualElement>("instructions-panel");
            _closeButton = _instructionsOverlayInstance.Q<Button>("close-button");
            _closeButton.clicked += ClosePanel;
        }

        private void ShowPanel()
        {
            _modalContainer.style.display = DisplayStyle.Flex;
            _modalContainer.pickingMode = PickingMode.Position;
            _instructionsOverlayInstance.style.display = DisplayStyle.Flex;
            
            _instructionsPanel.schedule.Execute(() => {
                _instructionsPanel.AddToClassList("settings-panel--is-visible");
            });
        }
        
        private void ClosePanel()
        {
            if (!_settingsService.CurrentSettings.HasSeenInstructions)
            {
                var settings = _settingsService.CurrentSettings;
                settings.HasSeenInstructions = true;
                _settingsService.SaveSettings(settings);
            }
            
            _instructionsPanel.RemoveFromClassList("settings-panel--is-visible");
            _instructionsPanel.RegisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
        }

        private void OnCloseTransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target != _instructionsPanel || !evt.stylePropertyNames.Contains("opacity"))
                return;
            _instructionsPanel.UnregisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
            _instructionsOverlayInstance.style.display = DisplayStyle.None;
            _modalContainer.style.display = DisplayStyle.None;
            _modalContainer.pickingMode = PickingMode.Ignore;
        }
    }
}