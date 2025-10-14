using System;

namespace Yawordle.Core
{
    [Serializable] 
    public class GameSettings
    {
        public string Language { get; set; } = "en";
        public GameMode Mode { get; set; } = GameMode.Unlimited;
        public int WordLength { get; set; } = 5;
        public bool HasSeenInstructions { get; set; } = false;
    }
}