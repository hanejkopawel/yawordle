using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer.Unity;
using Yawordle.Infrastructure.Localization;
using Yawordle.Presentation.ViewModels;

namespace Yawordle.Presentation.Views
{
    public class EndGameView : IStartable
    {
        private readonly GameBoardViewModel _gameBoardViewModel;
        private readonly UISettings _uiSettings;
        private readonly ILocalizationService _loc;

        private VisualElement _modalContainer;   // overlay z GameScreen (class="modal")
        private VisualElement _endGameContent;   // TemplateContainer (class="modal__content")
        private VisualElement _endGamePanel;     // modal__panel end-game-panel
        private Label _resultTitle;
        private Label _infoText;
        private Button _playAgainButton;
        
        private bool _isPanelVisible;
        private bool _lastIsWin;
        private string _lastTargetWord = string.Empty;

        public EndGameView(GameBoardViewModel gameBoardViewModel, UISettings uiSettings,  ILocalizationService loc)
        {
            _gameBoardViewModel = gameBoardViewModel;
            _uiSettings = uiSettings;
            _loc = loc;
        }

        public void Start()
        {
            var uiDocument = Object.FindAnyObjectByType<UIDocument>();
            var root = uiDocument.rootVisualElement;
            
            _modalContainer = root.Q<VisualElement>("modal-container");
            PreparePanel();

            if (_loc.IsReady) ApplyStaticLocalizedTexts();
            else _loc.Initialized += ApplyStaticLocalizedTexts;

            _loc.LanguageChanged += _ =>
            {
                ApplyStaticLocalizedTexts();
                if (_isPanelVisible) ApplyResultTexts(_lastIsWin, _lastTargetWord);
            };
            
            _gameBoardViewModel.ShowEndGamePanel += OpenPanel;
        }

        private void PreparePanel()
        {
            if (_uiSettings.EndGamePanel == null)
            {
                Debug.LogError("EndGamePanel VisualTreeAsset is not assigned in UISettings.");
                return;
            }

            _endGameContent = _uiSettings.EndGamePanel.Instantiate();
            _endGameContent.AddToClassList("modal__content");
            _endGameContent.RegisterCallback<ClickEvent>(OnEndGameBackdropClick);

            _modalContainer.Add(_endGameContent);

            _endGamePanel = _endGameContent.Q<VisualElement>("end-game-panel");
            _resultTitle = _endGamePanel.Q<Label>("result-title");
            _infoText = _endGamePanel.Q<Label>("info-text");
            _playAgainButton = _endGamePanel.Q<Button>("play-again-button");

            _playAgainButton.clicked += PlayAgain;
        }
        
        private void ApplyStaticLocalizedTexts()
        {
            if (_playAgainButton != null) 
                _playAgainButton.text = _loc.GetString("UI", "end_play_again");
        }

        private void ApplyResultTexts(bool isWin, string targetWord)
        {
            if (_resultTitle == null || _infoText == null) return;

            if (isWin)
            {
                _resultTitle.text = _loc.GetString("UI", "end_win_title");
                _infoText.text = _loc.GetString("UI", "end_win_info");
            }
            else
            {
                _resultTitle.text = _loc.GetString("UI", "end_lose_title");
                // TODO: Consider using Smart String with a placeholder for targetWord in the future.
                var prefix = _loc.GetString("UI", "end_lose_info_prefix");
                _infoText.text = $"{prefix} {targetWord.ToUpper()}";
            }
        }
        
        private void OnEndGameBackdropClick(ClickEvent e) {
            if (!_endGameContent.ClassListContains("is-active")) return;

            // Ignore inside panels clicks
            if (e.target is VisualElement target && _endGamePanel.Contains(target)) return;
            PlayAgain();
        }

        private void OpenPanel(bool isWin, string targetWord) => ShowPanelAsync(isWin, targetWord).Forget();
        
        
        private async UniTaskVoid ShowPanelAsync(bool isWin, string targetWord)
        {
            _lastIsWin = isWin;
            _lastTargetWord = targetWord ?? string.Empty;

            ApplyResultTexts(isWin, _lastTargetWord);

            _endGameContent.BringToFront();
            _endGameContent.AddToClassList("is-active");
            _modalContainer.AddToClassList("modal--is-open");
            _isPanelVisible = true;

            await UniTask.Delay(2500);
            _endGamePanel.schedule.Execute(() => _endGamePanel.AddToClassList("modal__panel--is-visible"));
        }

        private void PlayAgain()
        {
            if (_endGamePanel == null)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            _endGamePanel.RegisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);
            _endGamePanel.RemoveFromClassList("modal__panel--is-visible");
        }
        
        private void OnCloseTransitionEnd(TransitionEndEvent e)
        {
            if (e.target != _endGamePanel) return;
            _endGamePanel.UnregisterCallback<TransitionEndEvent>(OnCloseTransitionEnd);

            _endGameContent.RemoveFromClassList("is-active");
            _modalContainer.RemoveFromClassList("modal--is-open");
            _isPanelVisible = false;

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}