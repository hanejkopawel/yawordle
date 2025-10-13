using System.ComponentModel;
using System.Runtime.CompilerServices;
using Yawordle.Core;

namespace Yawordle.Presentation.ViewModels
{
    public class KeyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public char Key { get; }
        
        private LetterState _state = LetterState.Empty;
        public LetterState State
        {
            get => _state;
            set
            {
                // Enforce state priority: Correct > Present > Absent > Empty
                if (value <= _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public KeyViewModel(char key)
        {
            Key = key;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}