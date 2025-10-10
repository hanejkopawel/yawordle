using System;

namespace Yawordle.Core
{
    public interface IGameManager
    {
        event Action<int, string> OnGuessUpdated;
        event Action<int, LetterState[]> OnGuessEvaluated;
        event Action<bool> OnGameFinished; // bool: isWin

        void StartNewGame();
        void TypeLetter(char letter);
        void DeleteLetter();
        void SubmitGuess();
    }
}