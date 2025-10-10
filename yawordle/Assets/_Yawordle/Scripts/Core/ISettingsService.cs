namespace Yawordle.Core
{
    public interface ISettingsService
    {
        GameSettings CurrentSettings { get; }
        void SaveSettings(GameSettings settings);
    }
}