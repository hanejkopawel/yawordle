using Yawordle.Core;

namespace Yawordle.Presentation.ViewModels
{
    public class GameBoardViewModel
    {
        public const int MaxAttempts = 6;
        public TileViewModel[][] Tiles { get; private set; }
        public int WordLength { get; private set; }

        private readonly IGameManager _gameManager;

        public GameBoardViewModel(ISettingsService settingsService, IGameManager gameManager)
        {
            _gameManager = gameManager;

            WordLength = settingsService.CurrentSettings.WordLength;
            InitializeTiles();
            
            _gameManager.OnGuessUpdated += OnGuessUpdated;
            _gameManager.OnGuessEvaluated += OnGuessEvaluated;
            
            _gameManager.StartNewGame();
        }

        private void InitializeTiles()
        {
            Tiles = new TileViewModel[MaxAttempts][];
            for (int i = 0; i < MaxAttempts; i++)
            {
                Tiles[i] = new TileViewModel[WordLength];
                for (int j = 0; j < WordLength; j++)
                    Tiles[i][j] = new TileViewModel();
            }
        }

        private void OnGuessUpdated(int attempt, string guess)
        {
            for (int i = 0; i < WordLength; i++)
            {
                Tiles[attempt][i].Letter = (i < guess.Length) ? guess[i] : ' ';
            }
        }
        
        private void OnGuessEvaluated(int attempt, LetterState[] states)
        {
            for (int i = 0; i < WordLength; i++)
            {
                Tiles[attempt][i].State = states[i];
            }
        }
        
        // Komendy wywoływane przez widok
        public void TypeLetter(char letter) => _gameManager.TypeLetter(letter);
        public void DeleteLetter() => _gameManager.DeleteLetter();
        public void SubmitGuess() => _gameManager.SubmitGuess();
    }
}