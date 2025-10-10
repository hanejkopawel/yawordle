using System.ComponentModel;
using System.Runtime.CompilerServices;
using Yawordle.Core;

namespace Yawordle.Presentation.ViewModels
{
    public class TileViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private char _letter;
        public char Letter
        {
            get => _letter;
            set { _letter = value; OnPropertyChanged(); }
        }

        private LetterState _state = LetterState.Empty;
        public LetterState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}