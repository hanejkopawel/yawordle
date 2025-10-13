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

            _gameBoardViewModel.ShowEndGamePanel += ShowPanel;
        }

        private void ShowPanel(bool isWin, string targetWord)
        {
            _modalContainer.Clear();

            var endGamePanelInstance = _uiSettings.EndGamePanel.Instantiate();
            _modalContainer.Add(endGamePanelInstance);

            _resultTitle = endGamePanelInstance.Q<Label>("result-title");
            _infoText = endGamePanelInstance.Q<Label>("info-text");
            _playAgainButton = endGamePanelInstance.Q<Button>("play-again-button");
            
            _playAgainButton.clicked += PlayAgain;
            
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
        }

        private static void PlayAgain() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}