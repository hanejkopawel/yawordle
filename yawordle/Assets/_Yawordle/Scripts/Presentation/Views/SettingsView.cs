using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;
using Yawordle.Core;
using Yawordle.Infrastructure.Localization;
using Yawordle.Presentation.ViewModels;

namespace Yawordle.Presentation.Views
{
    public class SettingsView : IStartable
    {
        private readonly IObjectResolver _resolver;
        private readonly UISettings _uiSettings;
        private readonly ILocalizationService _loc;
        private SettingsViewModel _viewModel;

        // Elements from GameScreen.uxml
        private VisualElement _modalContainer;
        private VisualElement _settingsContent;
        private Button _openSettingsButton;
        
        // Elements from SettingsPanel.uxml
        private VisualElement _settingsPanel;
        private DropdownField _languageDropdown;
        private DropdownField _gameModeDropdown;
        private SliderInt _wordLengthSlider;
        private Label _wordLengthValueLabel;
        private Button _saveButton;
        private Button _cancelButton;
        
        // Maps for localized UI <-> domain values
        private readonly Dictionary<string, string> _langDisplayToCode = new();
        private readonly Dictionary<GameMode, string> _modeEnumToDisplay = new();
        private readonly Dictionary<string, GameMode> _modeDisplayToEnum = new();

        public SettingsView(IObjectResolver resolver, UISettings uiSettings, ILocalizationService loc)
        {
            _resolver = resolver;
            _uiSettings = uiSettings;
            _loc = loc;
        }

        public void Start()
        {
            var uiDocument = Object.FindAnyObjectByType<UIDocument>();
            var root = uiDocument.rootVisualElement;

            _modalContainer = root.Q<VisualElement>("modal-container");
            _openSettingsButton = root.Q<Button>("settings-button");
            PreparePanel();
            _openSettingsButton.clicked += OpenPanel;

            // Ensure texts are applied after localization init to avoid mixed UI
            if (_loc.IsReady) ApplyLocalizedTexts();
            else _loc.Initialized += ApplyLocalizedTexts;

            _loc.LanguageChanged += _ => ApplyLocalizedTexts();
        }
        
        /// <summary>
        /// Instantiates the settings panel from the asset, finds all its controls,
        /// and adds it to the modal container. This is done only once.
        /// </summary>
        private void PreparePanel()
        {
            if (_uiSettings.SettingsPanel == null)
            {
                Debug.LogError("SettingsPanel VisualTreeAsset is not assigned in UISettings.");
                return;
            }

            var panelInstance = _uiSettings.SettingsPanel.Instantiate();
            _settingsContent = panelInstance;
            _settingsContent.AddToClassList("modal__content");
            _modalContainer.Add(_settingsContent);

            _settingsPanel = _settingsContent.Q<VisualElement>("settings-panel");
            _settingsContent.RegisterCallback<ClickEvent>(OnSettingsBackdropClick);

            _languageDropdown = _settingsPanel.Q<DropdownField>("language-dropdown");
            _gameModeDropdown = _settingsPanel.Q<DropdownField>("game-mode-dropdown");
            _wordLengthSlider = _settingsPanel.Q<SliderInt>("word-length-slider");
            _wordLengthValueLabel = _settingsPanel.Q<Label>("word-length-value-label");
            _saveButton = _settingsPanel.Q<Button>("save-button");
            _cancelButton = _settingsPanel.Q<Button>("cancel-button");

            _cancelButton.clicked += ClosePanel;
        }

        private void ApplyLocalizedTexts()
        {
            // Title
            var titleLabel = _settingsPanel.Q<Label>(className: "settings-panel__title");
            if (titleLabel != null)
                titleLabel.text = _loc.GetString("UI", "settings_title");

            // Row labels
            var labels = _settingsPanel.Query<Label>(className: "settings-panel__label").ToList();
            if (labels.Count >= 1) labels[0].text = _loc.GetString("UI", "settings_language_label");
            if (labels.Count >= 2) labels[1].text = _loc.GetString("UI", "settings_game_mode_label");
            if (labels.Count >= 3) labels[2].text = _loc.GetString("UI", "settings_word_length_label");

            // Buttons
            _saveButton.text = _loc.GetString("UI", "settings_save");
            _cancelButton.text = _loc.GetString("UI", "settings_cancel");

            // Localized mode maps
            _modeEnumToDisplay.Clear();
            _modeDisplayToEnum.Clear();
            var modeUnlimited = _loc.GetString("UI", "mode_unlimited");
            var modeDaily = _loc.GetString("UI", "mode_daily");
            _modeEnumToDisplay[GameMode.Unlimited] = modeUnlimited;
            _modeEnumToDisplay[GameMode.Daily] = modeDaily;
            _modeDisplayToEnum[modeUnlimited] = GameMode.Unlimited;
            _modeDisplayToEnum[modeDaily] = GameMode.Daily;

            // Languages
            _langDisplayToCode.Clear();
            foreach (var li in _loc.GetAvailableLanguages())
                _langDisplayToCode[li.DisplayName] = li.Code;
        }

        
        private void OnSettingsBackdropClick(ClickEvent e) {
            if (!_settingsContent.ClassListContains("is-active")) return;

            if (e.target is VisualElement target && _settingsPanel.Contains(target)) return;
            ClosePanel();
        }

        private void OpenPanel()
        {
            _viewModel = _resolver.Resolve<SettingsViewModel>();
            _languageDropdown.choices = _langDisplayToCode.Keys.ToList();
            var currentCode = _viewModel.TempSettings.Language;
            var currentDisplay = _langDisplayToCode.FirstOrDefault(kv => kv.Value == currentCode).Key;
            _languageDropdown.value = string.IsNullOrEmpty(currentDisplay)
                ? _languageDropdown.choices.FirstOrDefault()
                : currentDisplay;

            _gameModeDropdown.choices = _modeEnumToDisplay.Values.ToList();
            _gameModeDropdown.value = _modeEnumToDisplay[_viewModel.TempSettings.Mode];

            _wordLengthSlider.value = _viewModel.TempSettings.WordLength;
            _wordLengthValueLabel.text = _viewModel.TempSettings.WordLength.ToString();

            _languageDropdown.RegisterValueChangedCallback(OnLanguageChanged);
            _gameModeDropdown.RegisterValueChangedCallback(OnGameModeChanged);
            _wordLengthSlider.RegisterValueChangedCallback(OnWordLengthChanged);
            _saveButton.clicked += OnSaveAndRestart;

            UIUtils.OpenModal(_modalContainer, _settingsContent, _settingsPanel);
        }

        private void ClosePanel()
        {
            _languageDropdown.UnregisterValueChangedCallback(OnLanguageChanged);
            _gameModeDropdown.UnregisterValueChangedCallback(OnGameModeChanged);
            _wordLengthSlider.UnregisterValueChangedCallback(OnWordLengthChanged);
            _saveButton.clicked -= OnSaveAndRestart;
            UIUtils.CloseModal(_modalContainer, _settingsContent, _settingsPanel);
        }
        
        private void OnLanguageChanged(ChangeEvent<string> evt)
        {
            if (string.IsNullOrEmpty(evt.newValue)) return;
            if (_langDisplayToCode.TryGetValue(evt.newValue, out var code)) 
                _viewModel.SetLanguage(code);
        }
        private void OnGameModeChanged(ChangeEvent<string> evt)
        {
            if (string.IsNullOrEmpty(evt.newValue)) return;
            if (_modeDisplayToEnum.TryGetValue(evt.newValue, out var mode)) 
                _viewModel.SetGameMode(mode);
        }

        private void OnWordLengthChanged(ChangeEvent<int> evt)
        {
            _viewModel.SetWordLength(evt.newValue);
            _wordLengthValueLabel.text = evt.newValue.ToString();
        }

        private void OnSaveAndRestart() => _viewModel.SaveAndRestart();
        
        
    }
}