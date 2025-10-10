using System;
using System.Collections.Generic;
using System.Linq;

namespace Yawordle.Core
{
    public sealed class GameManager : IGameManager
    {
        public event Action<int, string> OnGuessUpdated;
        public event Action<int, LetterState[]> OnGuessEvaluated;
        public event Action<bool> OnGameFinished;

        private readonly ISettingsService _settingsService;
        // private readonly IWordProvider _wordProvider; // Dodamy później

        private const int MaxAttempts = 6;
        private int _currentAttempt;
        private string _targetWord = "UNITY"; // Na razie na sztywno
        private string _currentGuess = "";

        public GameManager(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void StartNewGame()
        {
            _currentAttempt = 0;
            _currentGuess = "";
            // _targetWord = _wordProvider.GetWord(_settingsService.CurrentSettings.WordLength);
            // Na razie twardo kodujemy dla testów
            Console.WriteLine($"New game started. Word to guess: {_targetWord}");
        }

        public void TypeLetter(char letter)
        {
            if (_currentGuess.Length >= _settingsService.CurrentSettings.WordLength) return;
            _currentGuess += char.ToUpper(letter);
            OnGuessUpdated?.Invoke(_currentAttempt, _currentGuess);
        }

        public void DeleteLetter()
        {
            if (_currentGuess.Length <= 0) return;
            _currentGuess = _currentGuess[..^1];
            OnGuessUpdated?.Invoke(_currentAttempt, _currentGuess);
        }

        public void SubmitGuess()
        {
            if (_currentGuess.Length != _settingsService.CurrentSettings.WordLength)
            {
                // TODO: Poinformuj gracza, że słowo jest za krótkie
                return;
            }

            // if (!_wordProvider.IsValidWord(_currentGuess))
            // {
            //     // TODO: Poinformuj gracza, że słowa nie ma w słowniku
            //     return;
            // }

            var result = EvaluateGuess();
            OnGuessEvaluated?.Invoke(_currentAttempt, result);

            if (result.All(s => s == LetterState.Correct))
            {
                OnGameFinished?.Invoke(true); // Wygrana
                return;
            }

            _currentAttempt++;
            _currentGuess = "";

            if (_currentAttempt >= MaxAttempts)
            {
                OnGameFinished?.Invoke(false); // Przegrana
            }
        }
        
        private LetterState[] EvaluateGuess()
        {
            var result = new LetterState[_targetWord.Length];
            var targetWordLetters = _targetWord.ToList();
            var guessLetters = _currentGuess.ToList();

            // Krok 1: Znajdź idealne trafienia (Correct)
            for (int i = 0; i < _targetWord.Length; i++)
            {
                if (guessLetters[i] == targetWordLetters[i])
                {
                    result[i] = LetterState.Correct;
                    targetWordLetters[i] = '-'; // Zaznacz jako zużytą
                    guessLetters[i] = '*';
                }
            }

            // Krok 2: Znajdź litery obecne, ale w złym miejscu (Present)
            for (int i = 0; i < _targetWord.Length; i++)
            {
                if(guessLetters[i] == '*') continue;
                
                int index = targetWordLetters.IndexOf(guessLetters[i]);
                if (index != -1)
                {
                    result[i] = LetterState.Present;
                    targetWordLetters[index] = '-'; // Zaznacz jako zużytą
                }
                else
                {
                    result[i] = LetterState.Absent;
                }
            }
            return result;
        }
    }
}