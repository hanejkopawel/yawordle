using System;
using System.Collections.Generic;
using System.ComponentModel;
using Yawordle.Core;
using Yawordle.Infrastructure;

namespace Yawordle.Presentation.ViewModels
{
    public class GameBoardViewModel : INotifyPropertyChanged
    {
        public event Action<GuessValidationError, int> OnInvalidGuess;
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<int, LetterState[]> OnRowEvaluatedForAnimation;
        public event Action<bool, string> ShowEndGamePanel;

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
        public Dictionary<char, KeyViewModel> Keys { get; } = new();
        private readonly IGameManager _gameManager;
        private readonly string _targetWord;
        private int _currentAttempt;

        public GameBoardViewModel(
            ISettingsService settingsService, 
            IGameManager gameManager, 
            IKeyboardLayoutProvider keyboardLayoutProvider,
            IWordProvider wordProvider)
        {
            _gameManager = gameManager;
            WordLength = settingsService.CurrentSettings.WordLength;
            
            var layout = keyboardLayoutProvider.GetLayoutForLanguage(settingsService.CurrentSettings.Language);
            foreach (var row in layout.KeyRows)
            {
                foreach (var key in row)
                {
                    if (!Keys.ContainsKey(key))
                    {
                        Keys.Add(key, new KeyViewModel(key));
                    }
                }
            }
            
            InitializeTiles();

            // Get the word for this session.
            _targetWord = wordProvider.GetRandomSolutionWord();
            
            // Subscribe to game logic events.
            _gameManager.OnGuessValidationFailed += OnGuessValidationFailed;
            _gameManager.OnGuessUpdated += OnGuessUpdated;
            _gameManager.OnGuessEvaluated += OnGuessEvaluated;
            _gameManager.OnGameFinished += OnGameFinished;
            
            // Start the game in the GameManager with the chosen word.
            _gameManager.StartNewGame(_targetWord);
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

        private void OnGuessValidationFailed(GuessValidationError error)
        {
            OnInvalidGuess?.Invoke(error, _currentAttempt);
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
            
            _currentAttempt = attempt + 1;
            
            for (int i = 0; i < WordLength; i++)
            {
                Tiles[attempt][i].State = states[i];
            }
            
            OnRowEvaluatedForAnimation?.Invoke(attempt, states);
            
            string currentGuess = ""; 
            foreach (var tileVM in Tiles[attempt])
            {
                currentGuess += tileVM.Letter;
            }
            currentGuess = currentGuess.Trim();

            for (int i = 0; i < currentGuess.Length; i++)
            {
                char letter = currentGuess[i];
                LetterState state = states[i];
                if (Keys.TryGetValue(letter, out var keyVM))
                {
                    keyVM.State = state;
                }
            }
        }

        private void OnGameFinished(bool isWin)
        {
            IsGameFinished = true;
            ShowEndGamePanel?.Invoke(isWin, _targetWord);
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