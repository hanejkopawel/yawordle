using System.Linq;
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
            // Ensure texts are applied when localization is ready
            if (_loc.IsReady) ApplyLocalizedTexts();
            else _loc.Initialized += ApplyLocalizedTexts;
            _loc.LanguageChanged += _ => ApplyLocalizedTexts();
            
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
            _instructionsContent = _uiSettings.InstructionsPanel.Instantiate();
            _instructionsContent.AddToClassList("modal__content");
            _modalContainer.Add(_instructionsContent);

            _instructionsPanel = _instructionsContent.Q<VisualElement>("instructions-panel");
            _instructionsContent.RegisterCallback<ClickEvent>(OnInstructionsBackdropClick);

            _closeButton = _instructionsPanel.Q<Button>("close-button");
            _closeButton.clicked += ClosePanel;
            ApplyLocalizedTexts();
        }

        private void ApplyLocalizedTexts()
        {
            // Title + description
            var title = _instructionsPanel.Q<Label>(className: "instructions-panel__title");
            if (title != null) title.text = _loc.GetString("UI", "instructions_title");

            var desc = _instructionsPanel.Q<Label>(className: "instructions-panel__description");
            if (desc != null) desc.text = _loc.GetString("UI", "instructions_desc");

            // Rules
            var ruleItems = _instructionsPanel.Query<Label>(className: "instructions-panel__rule-item").ToList();
            if (ruleItems.Count >= 1) ruleItems[0].text = _loc.GetString("UI", "instructions_rule_valid_word");
            if (ruleItems.Count >= 2) ruleItems[1].text = _loc.GetString("UI", "instructions_rule_colors_indicate");

            // Examples subtitle
            var subtitle = _instructionsPanel.Q<Label>(className: "instructions-panel__subtitle");
            if (subtitle != null) subtitle.text = _loc.GetString("UI", "instructions_examples_title");

            // Example descriptions
            var exampleDescs = _instructionsPanel.Query<Label>(className: "instructions-panel__example-desc").ToList();
            if (exampleDescs.Count >= 1) exampleDescs[0].text = _loc.GetString("UI", "instructions_example1_desc");
            if (exampleDescs.Count >= 2) exampleDescs[1].text = _loc.GetString("UI", "instructions_example2_desc");
            if (exampleDescs.Count >= 3) exampleDescs[2].text = _loc.GetString("UI", "instructions_example3_desc");

            // Example rows letters (5 labels each row)
            var rows = _instructionsPanel.Query<VisualElement>(className: "instructions-panel__example-row").ToList();
            if (rows.Count >= 1) ApplyLettersToRow(rows[0], _loc.GetString("UI", "instructions_example1_letters"));
            if (rows.Count >= 2) ApplyLettersToRow(rows[1], _loc.GetString("UI", "instructions_example2_letters"));
            if (rows.Count >= 3) ApplyLettersToRow(rows[2], _loc.GetString("UI", "instructions_example3_letters"));

            // Close button (PL phrasing improved)
            _closeButton.text = _loc.GetString("UI", "instructions_got_it");
        }
        
        private static void ApplyLettersToRow(VisualElement row, string csvLetters)
        {
            if (row == null || string.IsNullOrWhiteSpace(csvLetters)) return;
            var letters = csvLetters.Split(',').Select(s => s.Trim()).ToArray();

            var labels = row.Query<Label>(className: "instructions-panel__example-tile").ToList();
            // There are also tiles with state classes (--correct/present/absent) but they share the same base class.
            for (int i = 0; i < labels.Count && i < letters.Length; i++)
            {
                labels[i].text = letters[i];
            }
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