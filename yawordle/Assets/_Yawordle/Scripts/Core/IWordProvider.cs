using Cysharp.Threading.Tasks;

namespace Yawordle.Core
{
    /// <summary>
    /// Defines a contract for services that provide words for the game.
    /// </summary>
    public interface IWordProvider
    {
        /// <summary>
        /// Gets a random word to be used as the solution for a new game,
        /// based on the current game settings.
        /// </summary>
        string GetRandomSolutionWord();
        
        /// <summary>
        /// Checks if a given word exists in the valid words dictionary for the current settings.
        /// </summary>
        bool IsValidWord(string word);
        
        UniTask<string> GetWordOfTheDayAsync();
    }
}