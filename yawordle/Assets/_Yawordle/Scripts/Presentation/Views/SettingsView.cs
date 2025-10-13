using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;
using Yawordle.Presentation.ViewModels;

namespace Yawordle.Presentation.Views
{
    public class SettingsView : IStartable
    {
        private readonly IObjectResolver _resolver;
        private readonly UISettings _uiSettings;
        private SettingsViewModel _viewModel;

        // Elements from GameScreen.uxml
        private VisualElement _mainContentContainer;
        private VisualElement _modalContainer;
        private Button _openSettingsButton;
        
        // Elements from SettingsPanel.uxml
        private VisualElement _settingsOverlay;
        private DropdownField _languageDropdown;
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

            _mainContentContainer = root.Q<VisualElement>("main-content");
            _modalContainer = root.Q<VisualElement>("modal-container");
            _openSettingsButton = root.Q<Button>("settings-button");
            
            PreparePanel();
            
            _openSettingsButton.clicked += OpenPanel;
        }

        /// <summary>
        /// Instantiates the settings panel from the asset, finds all its controls,
        /// and adds it to the modal container. This is done only once at startup.
        /// </summary>
        private void PreparePanel()
        {
            // Instantiate the panel from our ScriptableObject asset
            _settingsOverlay = _uiSettings.SettingsPanel.Instantiate();
            
            
            // Find all controls within the newly instantiated panel instance
            _languageDropdown = _settingsOverlay.Q<DropdownField>("language-dropdown");
            _wordLengthSlider = _settingsOverlay.Q<SliderInt>("word-length-slider");
            _wordLengthValueLabel = _settingsOverlay.Q<Label>("word-length-value-label");
            _saveButton = _settingsOverlay.Q<Button>("save-button");
            _cancelButton = _settingsOverlay.Q<Button>("cancel-button");

            // Register the close button event
            _cancelButton.clicked += ClosePanel;

            // Add the prepared (but hidden) panel to its container
            _modalContainer.Add(_settingsOverlay);
        }

        private void OpenPanel()
        {
            // Create a fresh ViewModel each time the panel is opened
            _viewModel = _resolver.Resolve<SettingsViewModel>();
            
            // Populate controls with current settings from the new ViewModel
            _languageDropdown.choices = new List<string> { "en", "pl" };
            _languageDropdown.value = _viewModel.TempSettings.Language;
            
            _wordLengthSlider.value = _viewModel.TempSettings.WordLength;
            _wordLengthValueLabel.text = _viewModel.TempSettings.WordLength.ToString();

            // Register callbacks for UI controls
            _languageDropdown.RegisterValueChangedCallback<string>(OnLanguageChanged);
            _wordLengthSlider.RegisterValueChangedCallback<int>(OnWordLengthChanged);
            _saveButton.clicked += OnSaveAndRestart;
            
            // Switch visibility of the main containers
            _mainContentContainer.style.display = DisplayStyle.None;
            _modalContainer.style.display = DisplayStyle.Flex;
        }
        
        private void OnLanguageChanged(ChangeEvent<string> evt) => _viewModel.SetLanguage(evt.newValue);
        private void OnWordLengthChanged(ChangeEvent<int> evt)
        {
            _viewModel.SetWordLength(evt.newValue);
            _wordLengthValueLabel.text = evt.newValue.ToString();
        }
        
        // Use a dedicated method for the save button to allow for clean unsubscription
        private void OnSaveAndRestart() => _viewModel.SaveAndRestart();

        private void ClosePanel()
        {
            // Unregister callbacks to prevent memory leaks
            _languageDropdown.UnregisterValueChangedCallback<string>(OnLanguageChanged);
            _wordLengthSlider.UnregisterValueChangedCallback<int>(OnWordLengthChanged);
            _saveButton.clicked -= OnSaveAndRestart;
            
            // Switch visibility of the main containers
            _modalContainer.style.display = DisplayStyle.None;
            _mainContentContainer.style.display = DisplayStyle.Flex;
        }
    }
}