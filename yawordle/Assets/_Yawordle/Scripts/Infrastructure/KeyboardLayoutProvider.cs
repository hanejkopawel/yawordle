using System.Collections.Generic;
using Yawordle.Core;

namespace Yawordle.Infrastructure
{
    public interface IKeyboardLayoutProvider
    {
        KeyboardLayout GetLayoutForLanguage(string languageCode);
    }

    public class KeyboardLayoutProvider : IKeyboardLayoutProvider
    {
        private readonly Dictionary<string, KeyboardLayout> _layouts = new()
        {
            { 
                "en", new KeyboardLayout(new[]
                {
                    "QWERTYUIOP",
                    "ASDFGHJKL",
                    "ZXCVBNM"
                })
            },
            {
                "pl", new KeyboardLayout(new[]
                {
                    "QWERTYUIOP",
                    "ASDFGHJKL",
                    "ZXCVBNM",
                    "ĄĆĘŁŃÓŚŻŹ" 
                })
            }
        };

        private readonly KeyboardLayout _defaultLayout = new KeyboardLayout(new[] { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM" });

        public KeyboardLayout GetLayoutForLanguage(string languageCode) => _layouts.GetValueOrDefault(languageCode, _defaultLayout);
    }
}