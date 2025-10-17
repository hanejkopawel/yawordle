using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;
using Yawordle.Core;
using Yawordle.Infrastructure.Localization;

namespace Yawordle.Presentation.Views
{
    public class InstructionsView : IStartable
    {
        private readonly ISettingsService _settingsService;
        private readonly UISettings _uiSettings;
        private readonly ILocalizationService _loc;

        private VisualElement _modalContainer;
        private VisualElement _instructionsContent;
        private VisualElement _instructionsPanel;
        private Button _closeButton;
        private Button _openHelpButton;

        public InstructionsView(ISettingsService settingsService, UISettings uiSettings, ILocalizationService loc)
        {
            _settingsService = settingsService;
            _uiSettings = uiSettings;
            _loc = loc;
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
            _instructionsContent = _uiSettings.InstructionsPanel.Instantiate(); // TemplateContainer
            _instructionsContent.AddToClassList("modal__content");
            _modalContainer.Add(_instructionsContent);

            // Find controls once and store their references.
            _instructionsPanel = _instructionsContent.Q<VisualElement>("instructions-panel");
            _instructionsContent.RegisterCallback<ClickEvent>(OnInstructionsBackdropClick);

            _closeButton = _instructionsPanel.Q<Button>("close-button");
            _closeButton.text = _loc.GetString("UI", "instructions_got_it");
            _closeButton.clicked += ClosePanel;
            
            var title = _instructionsPanel.Q<Label>(className: "instructions-panel__title");
            if (title != null) title.text = _loc.GetString("UI", "instructions_title");

            var desc = _instructionsPanel.Q<Label>(className: "instructions-panel__description");
            if (desc != null) desc.text = _loc.GetString("UI", "instructions_desc");
        }
        
        private void OnInstructionsBackdropClick(ClickEvent e) {
            if (!_instructionsContent.ClassListContains("is-active")) return;

            // Ignore inside panels clicks
            if (e.target is VisualElement target && _instructionsPanel.Contains(target)) return;
            ClosePanel();
        }

        private void OpenPanel() => UIUtils.OpenModal(_modalContainer, _instructionsContent, _instructionsPanel);

        private void ClosePanel()
        {
            if (!_settingsService.CurrentSettings.HasSeenInstructions)
            {
                var settings = _settingsService.CurrentSettings;
                settings.HasSeenInstructions = true;
                _settingsService.SaveSettings(settings);
            }
            UIUtils.CloseModal(_modalContainer, _instructionsContent, _instructionsPanel);
        }
    }
}