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
                    "Q,W,E,R,T,Y,U,I,O,P",
                    "A,S,D,F,G,H,J,K,L",
                    "ENTER,Z,X,C,V,B,N,M,BACKSPACE"
                })
            },
            {
                "pl", new KeyboardLayout(new[]
                {
                    "Ą,Ć,Ę,Ł,Ó,Ś,Ń,Ż,Ź",
                    "Q,W,E,R,T,Y,U,I,O,P",
                    "A,S,D,F,G,H,J,K,L",
                    "ENTER,Z,X,C,V,B,N,M,BACKSPACE"
                })
            }
        };

        private readonly KeyboardLayout _defaultLayout = new KeyboardLayout(new[]
        {
            "Q,W,E,R,T,Y,U,I,O,P",
            "A,S,D,F,G,H,J,K,L",
            "ENTER,Z,X,C,V,B,N,M,BACKSPACE"
        });

        public KeyboardLayout GetLayoutForLanguage(string languageCode) => _layouts.GetValueOrDefault(languageCode, _defaultLayout);
    }
}