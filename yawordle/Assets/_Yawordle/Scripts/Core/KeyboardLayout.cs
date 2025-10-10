namespace Yawordle.Core
{
    public class KeyboardLayout
    {
        public string[] KeyRows { get; }

        public KeyboardLayout(string[] keyRows)
        {
            KeyRows = keyRows;
        }
    }
}