using System.Collections.Generic;
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

            _saveButton.text = _loc.GetString("UI", "settings_save");
            _cancelButton.text = _loc.GetString("UI", "settings_cancel");
            
            _cancelButton.clicked += ClosePanel;

        }
        
        private void OnSettingsBackdropClick(ClickEvent e) {
            if (!_settingsContent.ClassListContains("is-active")) return;

            if (e.target is VisualElement target && _settingsPanel.Contains(target)) return;
            ClosePanel();
        }

        private void OpenPanel()
        {
            _viewModel = _resolver.Resolve<SettingsViewModel>();
            
            _languageDropdown.choices = new List<string> { "en", "pl" };
            _languageDropdown.value = _viewModel.TempSettings.Language;
            _gameModeDropdown.choices = new List<string> { nameof(GameMode.Unlimited), nameof(GameMode.Daily) };
            _gameModeDropdown.value = _viewModel.TempSettings.Mode.ToString();
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
        
        private void OnLanguageChanged(ChangeEvent<string> evt) => _viewModel.SetLanguage(evt.newValue);
        private void OnGameModeChanged(ChangeEvent<string> evt)
        {
            if (System.Enum.TryParse<GameMode>(evt.newValue, out var newMode))
            {
                _viewModel.SetGameMode(newMode);
            }
        }
        private void OnWordLengthChanged(ChangeEvent<int> evt)
        {
            _viewModel.SetWordLength(evt.newValue);
            _wordLengthValueLabel.text = evt.newValue.ToString();
        }
        private void OnSaveAndRestart() => _viewModel.SaveAndRestart();
        
        
    }
}