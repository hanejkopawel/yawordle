using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer.Unity;
using Yawordle.Presentation.ViewModels;

namespace Yawordle.Presentation.Views
{
    public class EndGameView : IStartable
    {
        private readonly GameBoardViewModel _gameBoardViewModel;
        private readonly UISettings _uiSettings;

        private VisualElement _modalContainer;
        private VisualElement _endGameOverlayInstance;
        private VisualElement _endGamePanel;
        private Label _resultTitle;
        private Label _infoText;
        private Button _playAgainButton;

        public EndGameView(GameBoardViewModel gameBoardViewModel, UISettings uiSettings)
        {
            _gameBoardViewModel = gameBoardViewModel;
            _uiSettings = uiSettings;
        }

        public void Start()
        {
            var uiDocument = Object.FindAnyObjectByType<UIDocument>();
            var root = uiDocument.rootVisualElement;
            
            _modalContainer = root.Q<VisualElement>("modal-container");
            PreparePanel();

            _gameBoardViewModel.ShowEndGamePanel += OpenPanel;
        }

        private void PreparePanel()
        {
            
            if (_uiSettings.EndGamePanel == null)
            {
                Debug.LogError("EndGamePanel VisualTreeAsset is not assigned in UISettings.");
                return;
            }
            
            _endGameOverlayInstance = _uiSettings.EndGamePanel.Instantiate();
            _endGameOverlayInstance.AddToClassList("panel-container");
            _modalContainer.Add(_endGameOverlayInstance);
            
            // Find controls once and store their references.
            _endGamePanel = _endGameOverlayInstance.Q<VisualElement>("end-game-panel");
            _resultTitle = _endGameOverlayInstance.Q<Label>("result-title");
            _infoText = _endGameOverlayInstance.Q<Label>("info-text");
            _playAgainButton = _endGameOverlayInstance.Q<Button>("play-again-button");
            
            _playAgainButton.clicked += PlayAgain;

        }

        private void OpenPanel(bool isWin, string targetWord) => ShowPanelAsync(isWin, targetWord).Forget();
        
        
        private async UniTaskVoid ShowPanelAsync(bool isWin, string targetWord)
        {
            
            if (isWin)
            {
                _resultTitle.text = "Congratulations!";
                _infoText.text = "You guessed the word!";
            }
            else
            {
                _resultTitle.text = "Game Over";
                _infoText.text = $"The correct word was: {targetWord.ToUpper()}";
            }
            _modalContainer.style.display = DisplayStyle.Flex;
            _endGameOverlayInstance.style.display = DisplayStyle.Flex; 
            await UniTask.Delay(2500); 
            _endGamePanel.schedule.Execute(() => {
                _endGamePanel.AddToClassList("end-game-panel--is-visible");
            });
        }

        private void PlayAgain() 
        {
            if (_endGamePanel != null)
            {
                _endGamePanel.RegisterCallback<TransitionEndEvent>(
                    evt => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
                _endGamePanel.RemoveFromClassList("end-game-panel--is-visible");    
            }
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }
}