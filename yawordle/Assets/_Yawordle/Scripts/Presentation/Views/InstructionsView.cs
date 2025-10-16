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
            var uiDocument = Object.FindAnyObjectByType<UIDocument>();
            var root = uiDocument.rootVisualElement;
            
            _modalContainer = root.Q<VisualElement>("modal-container");
            _openHelpButton = root.Q<Button>("help-button");
            _openHelpButton.clicked += OpenPanel;
            PreparePanel();

            if (!_settingsService.CurrentSettings.HasSeenInstructions) 
                OpenPanel();
        }

        private void PreparePanel()
        {
            if (_uiSettings.InstructionsPanel == null)
            {
                Debug.LogError("InstructionsPanel VisualTreeAsset is not assigned in UISettings.");
                return;
            }
            _instructionsOverlayInstance = _uiSettings.InstructionsPanel.Instantiate();
            _instructionsOverlayInstance.AddToClassList("panel-container");
            _modalContainer.Add(_instructionsOverlayInstance);
            
            // Find controls once and store their references.
            _instructionsPanel = _instructionsOverlayInstance.Q<VisualElement>("instructions-panel");
            _closeButton = _instructionsOverlayInstance.Q<Button>("close-button");
            
            _closeButton.clicked += ClosePanel;
        }

        private void OpenPanel()
        {
            _modalContainer.style.display = DisplayStyle.Flex;
            _instructionsOverlayInstance.style.display = DisplayStyle.Flex;
            
            _instructionsPanel.schedule.Execute(() => {
                _instructionsPanel.AddToClassList("instructions-panel--is-visible");
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
            
            _instructionsPanel.RemoveFromClassList("instructions-panel--is-visible");
            _instructionsPanel.RegisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
        }

        private void OnCloseTransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target != _instructionsPanel)
                return;
            _instructionsPanel.UnregisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
            _instructionsOverlayInstance.style.display = DisplayStyle.None;
            _modalContainer.style.display = DisplayStyle.None;
        }
    }
}