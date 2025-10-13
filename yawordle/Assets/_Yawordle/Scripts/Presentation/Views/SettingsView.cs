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
        private VisualElement _modalContainer;
        private Button _openSettingsButton;
        
        // Elements from SettingsPanel.uxml
        private VisualElement _settingsPanelInstance;
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

            _modalContainer = root.Q<VisualElement>("modal-container");
            _openSettingsButton = root.Q<Button>("settings-button");
            _openSettingsButton.clicked += OpenPanel;
        }

        private void OnLanguageChanged(ChangeEvent<string> evt) => _viewModel.SetLanguage(evt.newValue);
        private void OnWordLengthChanged(ChangeEvent<int> evt)
        {
            _viewModel.SetWordLength(evt.newValue);
            _wordLengthValueLabel.text = evt.newValue.ToString();
        }
        
        // Use a dedicated method for the save button to allow for clean unsubscription
        private void OnSaveAndRestart() => _viewModel.SaveAndRestart();
        
        private void OpenPanel()
        {
            _modalContainer.Clear();
            
            _settingsPanelInstance = _uiSettings.SettingsPanel.Instantiate();
            _modalContainer.Add(_settingsPanelInstance);

            _languageDropdown = _settingsPanelInstance.Q<DropdownField>("language-dropdown");
            _wordLengthSlider = _settingsPanelInstance.Q<SliderInt>("word-length-slider");
            _wordLengthValueLabel = _settingsPanelInstance.Q<Label>("word-length-value-label");
            _saveButton = _settingsPanelInstance.Q<Button>("save-button");
            _cancelButton = _settingsPanelInstance.Q<Button>("cancel-button");

            _cancelButton.clicked += ClosePanel;
            
            _viewModel = _resolver.Resolve<SettingsViewModel>();
            _languageDropdown.choices = new List<string> { "en", "pl" };
            _languageDropdown.value = _viewModel.TempSettings.Language;
            _wordLengthSlider.value = _viewModel.TempSettings.WordLength;
            _wordLengthValueLabel.text = _viewModel.TempSettings.WordLength.ToString();
            _languageDropdown.RegisterValueChangedCallback<string>(OnLanguageChanged);
            _wordLengthSlider.RegisterValueChangedCallback<int>(OnWordLengthChanged);
            _saveButton.clicked += OnSaveAndRestart;
            
            _modalContainer.style.display = DisplayStyle.Flex;
            _modalContainer.pickingMode = PickingMode.Position;

        }
        private void ClosePanel()
        {
            _modalContainer.style.display = DisplayStyle.None;
            _modalContainer.pickingMode = PickingMode.Ignore;
            _modalContainer.Clear();
        }
    }
}