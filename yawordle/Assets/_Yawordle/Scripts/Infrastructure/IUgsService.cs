using Cysharp.Threading.Tasks;

namespace Yawordle.Infrastructure
{
    public interface IUgsService
    {
        UniTask InitializeAsync();
        UniTask<string> GetWordOfTheDayAsync(string language, int wordLength);
    }
}