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
            var root = Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            _modalContainer = root.Q<VisualElement>("modal-container");
            
            if (_uiSettings.EndGamePanel == null)
            {
                Debug.LogError("EndGamePanel VisualTreeAsset is not assigned in UISettings.");
                return;
            }
            PreparePanel();

            _gameBoardViewModel.ShowEndGamePanel += ShowPanel;
        }

        private void PreparePanel()
        {
            _endGameOverlayInstance = _uiSettings.EndGamePanel.Instantiate();
            _endGameOverlayInstance.style.display = DisplayStyle.None;
            _modalContainer.Add(_endGameOverlayInstance);
            
            _endGamePanel = _endGameOverlayInstance.Q<VisualElement>("end-game-panel");
            _resultTitle = _endGameOverlayInstance.Q<Label>("result-title");
            _infoText = _endGameOverlayInstance.Q<Label>("info-text");
            _playAgainButton = _endGameOverlayInstance.Q<Button>("play-again-button");
            _playAgainButton.clicked += PlayAgain;

            _endGameOverlayInstance.style.display = DisplayStyle.None; 
        }

        private void ShowPanel(bool isWin, string targetWord) => ShowPanelAsync(isWin, targetWord).Forget();
        
        
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
            _modalContainer.pickingMode = PickingMode.Position;
            _endGameOverlayInstance.style.display = DisplayStyle.Flex; 
            
            await UniTask.Delay(2500); 
            _endGamePanel.schedule.Execute(() => {
                _endGamePanel.AddToClassList("settings-panel--is-visible");
            });
        }

        private void PlayAgain() 
        {
            if (_endGamePanel != null)
            {
                _endGamePanel.RegisterCallback<TransitionEndEvent>(
                    evt => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
                _endGamePanel.RemoveFromClassList("settings-panel--is-visible");    
            }
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }
}