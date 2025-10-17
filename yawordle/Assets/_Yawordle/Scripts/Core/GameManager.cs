using System;
using System.Linq;
using UnityEngine;

namespace Yawordle.Core
{
    public sealed class GameManager : IGameManager
    {
        public event Action<GuessValidationError> OnGuessValidationFailed;
        public event Action<int, string> OnGuessUpdated;
        public event Action<int, LetterState[]> OnGuessEvaluated;
        public event Action<bool> OnGameFinished;

        public string TargetWord => _targetWord;
        
        public int MaxAttempts => MaxAttemptsConst;
        public int CurrentAttempt => _currentAttempt;
        
        private readonly ISettingsService _settingsService;
        private readonly IWordProvider _wordProvider;

        private const int MaxAttemptsConst = 6;
        private int _currentAttempt;
        private string _targetWord;
        private string _currentGuess = "";

        public GameManager(ISettingsService settingsService, IWordProvider wordProvider)
        {
            _settingsService = settingsService;
            _wordProvider = wordProvider;
        }

        public void StartNewGame(string targetWord)
        {
            _currentAttempt = 0;
            _currentGuess = "";
            _targetWord = targetWord.ToUpper();
            Debug.Log($"New game started. Word to guess: {_targetWord}");
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
                OnGuessValidationFailed?.Invoke(GuessValidationError.NotEnoughLetters);
                return;
            }

            if (!_wordProvider.IsValidWord(_currentGuess))
            {
                OnGuessValidationFailed?.Invoke(GuessValidationError.NotInWordList);
                return;
            }

            var result = EvaluateGuess();
            OnGuessEvaluated?.Invoke(_currentAttempt, result);

            if (result.All(s => s == LetterState.Correct))
            {
                OnGameFinished?.Invoke(true);
                return;
            }

            _currentAttempt++;
            _currentGuess = "";

            if (_currentAttempt >= MaxAttempts)
            {
                OnGameFinished?.Invoke(false);
            }
        }
        
        private LetterState[] EvaluateGuess()
        {
            var result = new LetterState[_targetWord.Length];
            if (string.IsNullOrEmpty(_targetWord) || _targetWord == "ERROR")
            {
                Debug.LogError("Cannot evaluate guess: Target word is invalid.");
                return result;
            }
            var targetWordLetters = _targetWord.ToList();
            var guessLetters = _currentGuess.ToList();
            for (var i = 0; i < _targetWord.Length; i++)
            {
                if (guessLetters[i] != targetWordLetters[i]) continue;
                result[i] = LetterState.Correct;
                targetWordLetters[i] = '-';
                guessLetters[i] = '*';
            }
            for (var i = 0; i < _targetWord.Length; i++)
            {
                if (guessLetters[i] == '*') continue;
                var index = targetWordLetters.IndexOf(guessLetters[i]);
                if (index != -1)
                {
                    result[i] = LetterState.Present;
                    targetWordLetters[index] = '-';
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