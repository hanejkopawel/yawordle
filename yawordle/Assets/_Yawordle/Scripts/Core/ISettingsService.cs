using System.Threading;
using Cysharp.Threading.Tasks;

namespace Yawordle.Core
{
    public interface ISettingsService
    {
        GameSettings CurrentSettings { get; }
        void SaveSettings(GameSettings settings);
        UniTask SaveSettingsAsync(GameSettings settings, CancellationToken ct = default);
    }
}