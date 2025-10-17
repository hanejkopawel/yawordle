using System;

namespace Yawordle.Core
{
    public interface IGameManager
    {
        event Action<GuessValidationError> OnGuessValidationFailed;
        event Action<int, string> OnGuessUpdated;
        event Action<int, LetterState[]> OnGuessEvaluated;
        event Action<bool> OnGameFinished; // bool: isWin

        int MaxAttempts { get; }
        int CurrentAttempt { get; }
        
        string TargetWord { get; }
        void StartNewGame(string targetWord);
        void TypeLetter(char letter);
        void DeleteLetter();
        void SubmitGuess();
    }
}