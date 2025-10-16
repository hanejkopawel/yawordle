using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;
using Yawordle.Core;
using Yawordle.Presentation.ViewModels;

namespace Yawordle.Presentation.Views
{
    public class SettingsView : IStartable
    {
        private readonly IObjectResolver _resolver;
        private readonly UISettings _uiSettings;
        private SettingsViewModel _viewModel;

        // Elements from GameScreen.uxml
        private VisualElement _modalContainer;
        private Button _openSettingsButton;
        
        // Elements from SettingsPanel.uxml
        private VisualElement _settingsOverlayInstance;
        private VisualElement _settingsPanel;
        private DropdownField _languageDropdown;
        private DropdownField _gameModeDropdown;
        private SliderInt _wordLengthSlider;
        private Label _wordLengthValueLabel;
        private Button _saveButton;
        private Button _cancelButton;

        public SettingsView(IObjectResolver resolver, UISettings uiSettings)
        {
            _resolver = resolver;
            _uiSettings = uiSettings;
        }

        public void Start()
        {
            var uiDocument = Object.FindAnyObjectByType<UIDocument>();
            var root = uiDocument.rootVisualElement;

            _modalContainer = root.Q<VisualElement>("modal-container");
            _openSettingsButton = root.Q<Button>("settings-button");
            _openSettingsButton.clicked += OpenPanel;
            PreparePanel();
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
            
            _settingsOverlayInstance = _uiSettings.SettingsPanel.Instantiate();
            _settingsOverlayInstance.AddToClassList("panel-container");
            _modalContainer.Add(_settingsOverlayInstance);

            // Find controls once and store their references.
            _settingsPanel = _settingsOverlayInstance.Q<VisualElement>("settings-panel");
            _languageDropdown = _settingsOverlayInstance.Q<DropdownField>("language-dropdown");
            _gameModeDropdown = _settingsOverlayInstance.Q<DropdownField>("game-mode-dropdown");
            _wordLengthSlider = _settingsOverlayInstance.Q<SliderInt>("word-length-slider");
            _wordLengthValueLabel = _settingsOverlayInstance.Q<Label>("word-length-value-label");
            _saveButton = _settingsOverlayInstance.Q<Button>("save-button");
            _cancelButton = _settingsOverlayInstance.Q<Button>("cancel-button");

            _saveButton.clicked += OnSaveAndRestart;
            _cancelButton.clicked += ClosePanel;

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
        
        // Use a dedicated method for the save button to allow for clean unsubscription
        private void OnSaveAndRestart() => _viewModel.SaveAndRestart();
        
        private void OpenPanel()
        {
            // Create a fresh ViewModel to hold temporary settings.
            _viewModel = _resolver.Resolve<SettingsViewModel>();
            
            // Populate controls with current data from the ViewModel.
            _languageDropdown.choices = new List<string> { "en", "pl" };
            _languageDropdown.value = _viewModel.TempSettings.Language;
            
            _gameModeDropdown.choices = new List<string> { "Unlimited", "Daily" };
            _gameModeDropdown.value = _viewModel.TempSettings.Mode.ToString();
            
            _wordLengthSlider.value = _viewModel.TempSettings.WordLength;
            _wordLengthValueLabel.text = _viewModel.TempSettings.WordLength.ToString();

            // Register callbacks for UI controls.
            _languageDropdown.RegisterValueChangedCallback(OnLanguageChanged);
            _gameModeDropdown.RegisterValueChangedCallback(OnGameModeChanged);
            _wordLengthSlider.RegisterValueChangedCallback(OnWordLengthChanged);
            
            // Show the panel by switching the main containers' visibility.
            _modalContainer.style.display = DisplayStyle.Flex;
            _settingsOverlayInstance.style.display = DisplayStyle.Flex;
            _settingsOverlayInstance.schedule.Execute(() => {
                _settingsPanel.AddToClassList("settings-panel--is-visible");
            });
        }
        private void ClosePanel()
        {
            // Unregister callbacks to prevent them from firing when the panel is hidden.
            _languageDropdown.UnregisterValueChangedCallback(OnLanguageChanged);
            _gameModeDropdown.UnregisterValueChangedCallback(OnGameModeChanged);
            _wordLengthSlider.UnregisterValueChangedCallback(OnWordLengthChanged);
            
            _settingsPanel.RemoveFromClassList("settings-panel--is-visible");
            _settingsPanel.RegisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
        }
        
        private void OnCloseTransitionEnd(TransitionEndEvent evt)
        {
            if (evt.target != _settingsPanel) return;
            _settingsPanel.UnregisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
                
            _settingsOverlayInstance.style.display = DisplayStyle.None;
            _modalContainer.style.display = DisplayStyle.None;
        }
    }
}