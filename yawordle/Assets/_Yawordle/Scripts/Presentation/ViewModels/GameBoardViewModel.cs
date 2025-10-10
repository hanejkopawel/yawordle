using System;
using System.ComponentModel;
using Yawordle.Core;

namespace Yawordle.Presentation.ViewModels
{
    public class GameBoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<int, LetterState[]> OnRowEvaluatedForAnimation;

        private bool _isGameFinished;
        public bool IsGameFinished
        {
            get => _isGameFinished;
            private set 
            {
                if (_isGameFinished == value) return;
                _isGameFinished = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGameFinished)));
            }
        }
        
        public const int MaxAttempts = 6;
        public TileViewModel[][] Tiles { get; private set; }
        public int WordLength { get; private set; }

        private readonly IGameManager _gameManager;

        public GameBoardViewModel(ISettingsService settingsService, IGameManager gameManager)
        {
            _gameManager = gameManager;

            WordLength = settingsService.CurrentSettings.WordLength;
            InitializeTiles();
            
            _gameManager.OnGuessUpdated += OnGuessUpdated;
            _gameManager.OnGuessEvaluated += OnGuessEvaluated;
            _gameManager.OnGameFinished += OnGameFinished;
            
            _gameManager.StartNewGame();
        }

        private void InitializeTiles()
        {
            Tiles = new TileViewModel[MaxAttempts][];
            for (int i = 0; i < MaxAttempts; i++)
            {
                Tiles[i] = new TileViewModel[WordLength];
                for (int j = 0; j < WordLength; j++)
                    Tiles[i][j] = new TileViewModel();
            }
        }

        private void OnGuessUpdated(int attempt, string guess)
        {
            for (int i = 0; i < WordLength; i++)
            {
                Tiles[attempt][i].Letter = (i < guess.Length) ? guess[i] : ' ';
            }
        }
        
        private void OnGuessEvaluated(int attempt, LetterState[] states)
        {
            for (int i = 0; i < WordLength; i++)
            {
                Tiles[attempt][i].State = states[i];
            }
            OnRowEvaluatedForAnimation?.Invoke(attempt, states);
        }

        private void OnGameFinished(bool isWin)
        {
            IsGameFinished = true;
            // TODO: Wywołać event do pokazania pop-upu z wynikiem
        }
        
        public void TypeLetter(char letter)
        {
            if (IsGameFinished) return;
            _gameManager.TypeLetter(letter);
        }
        public void DeleteLetter()
        {
            if (IsGameFinished) return;
            _gameManager.DeleteLetter();
        }
        public void SubmitGuess()
        {
            if (IsGameFinished) return;
            _gameManager.SubmitGuess();
        }
    }
}