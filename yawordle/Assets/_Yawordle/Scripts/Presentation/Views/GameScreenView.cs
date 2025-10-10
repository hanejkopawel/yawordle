using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;
using Yawordle.Core;
using Yawordle.Presentation.ViewModels;

namespace Yawordle.Presentation.Views
{
    /// <summary>
    /// Klasa Widoku (View) dla głównego ekranu gry.
    /// Jej jedynym zadaniem jest renderowanie stanu dostarczonego przez GameBoardViewModel
    /// i przekazywanie akcji użytkownika (np. naciśnięcia klawiszy) do ViewModelu.
    /// Jest "głupia" - nie zawiera żadnej logiki gry.
    /// </summary>
    public class GameScreenView : IStartable
    {
        private readonly GameBoardViewModel _viewModel;

        // Referencje do dynamicznie tworzonych elementów UI, abyśmy mogli je aktualizować.
        private VisualElement _boardContainer;
        private VisualElement[][] _tileElements;
        private Label[][] _tileLabels;

        /// <summary>
        /// Konstruktor jest wywoływany przez kontener VContainer.
        /// Wstrzykuje ViewModel, od którego ten widok jest zależny.
        /// </summary>
        public GameScreenView(GameBoardViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Metoda wywoływana przez VContainer po zainicjowaniu wszystkich zależności.
        /// To tutaj inicjujemy nasz widok.
        /// </summary>
        public void Start()
        {
            // Znajdujemy główny dokument UI w scenie.
            // W bardziej złożonym projekcie można by przekazać referencję w inny sposób.
            var uiDocument = Object.FindObjectOfType<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("Nie znaleziono UIDocument w scenie!");
                return;
            }
            var root = uiDocument.rootVisualElement;

            _boardContainer = root.Q<VisualElement>("game-board-container");
            if (_boardContainer == null)
            {
                Debug.LogError("Nie znaleziono 'game-board-container' w UXML!");
                return;
            }

            // Ustawiamy focus na kontenerze z kodu - to pewniejsze niż w UXML.
            _boardContainer.focusable = true;
            _boardContainer.Focus();

            // Uruchamiamy proces tworzenia i bindowania UI.
            GenerateGrid();
            BindToViewModel();
            BindKeyboardInput();

            // Rejestrujemy callback, który zadba o kwadratowy kształt kafelków,
            // gdy UI zostanie narysowane i jego wymiary będą znane.
            _boardContainer.RegisterCallback<GeometryChangedEvent>(OnBoardGeometryChanged);
        }

        /// <summary>
        /// Dynamicznie tworzy siatkę kafelków wewnątrz _boardContainer na podstawie danych z ViewModelu.
        /// </summary>
        private void GenerateGrid()
        {
            _boardContainer.Clear(); // Wyczyść na wypadek ponownego startu lub zmiany ustawień.

            _tileElements = new VisualElement[GameBoardViewModel.MaxAttempts][];
            _tileLabels = new Label[GameBoardViewModel.MaxAttempts][];

            for (int i = 0; i < GameBoardViewModel.MaxAttempts; i++)
            {
                _tileElements[i] = new VisualElement[_viewModel.WordLength];
                _tileLabels[i] = new Label[_viewModel.WordLength];
                
                var rowElement = new VisualElement();
                rowElement.AddToClassList("row");

                for (int j = 0; j < _viewModel.WordLength; j++)
                {
                    var tileElement = new VisualElement();
                    tileElement.AddToClassList("tile");
                    
                    var labelElement = new Label();
                    labelElement.AddToClassList("tile-label");
                    
                    tileElement.Add(labelElement);
                    rowElement.Add(tileElement);
                    
                    // Zapisujemy referencje do nowo utworzonych elementów.
                    _tileElements[i][j] = tileElement;
                    _tileLabels[i][j] = labelElement;
                }
                _boardContainer.Add(rowElement);
            }
        }
        
        /// <summary>
        /// Łączy (binduje) stan ViewModelu z elementami UI.
        /// Rejestruje nasłuchiwanie na zmiany w każdym TileViewModel.
        /// </summary>
        private void BindToViewModel()
        {
            for (int i = 0; i < GameBoardViewModel.MaxAttempts; i++)
            {
                for (int j = 0; j < _viewModel.WordLength; j++)
                {
                    // Pobieramy konkretny ViewModel i odpowiadające mu elementy UI.
                    TileViewModel tileVM = _viewModel.Tiles[i][j];
                    Label tileLabel = _tileLabels[i][j];
                    VisualElement tileElement = _tileElements[i][j];

                    // Reagujemy na zmiany właściwości w ViewModelu.
                    tileVM.PropertyChanged += (sender, args) =>
                    {
                        var vm = (TileViewModel)sender;
                        if (args.PropertyName == nameof(TileViewModel.Letter))
                        {
                            tileLabel.text = vm.Letter.ToString().ToUpper();
                        }
                        else if (args.PropertyName == nameof(TileViewModel.State))
                        {
                            UpdateTileState(tileElement, vm.State);
                        }
                    };
                }
            }
        }

        /// <summary>
        /// Aktualizuje klasy USS dla kafelka na podstawie jego nowego stanu.
        /// </summary>
        private void UpdateTileState(VisualElement tileElement, LetterState state)
        {
            // Usuń wszystkie stare klasy stanu, aby uniknąć konfliktów.
            tileElement.RemoveFromClassList("tile--correct");
            tileElement.RemoveFromClassList("tile--present");
            tileElement.RemoveFromClassList("tile--absent");

            switch (state)
            {
                case LetterState.Correct:
                    tileElement.AddToClassList("tile--correct");
                    break;
                case LetterState.Present:
                    tileElement.AddToClassList("tile--present");
                    break;
                case LetterState.Absent:
                    tileElement.AddToClassList("tile--absent");
                    break;
            }
        }

        /// <summary>
        /// Prosty input z klawiatury PC do celów testowych.
        /// Przekazuje akcje do ViewModelu.
        /// </summary>
        private void BindKeyboardInput()
        {
            _boardContainer.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (char.IsLetter(evt.character))
                {
                    _viewModel.TypeLetter(evt.character);
                }
                else if (evt.keyCode == KeyCode.Backspace)
                {
                    _viewModel.DeleteLetter();
                }
                else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                {
                    _viewModel.SubmitGuess();
                }
            });
        }
        
        /// <summary>
        /// Callback wywoływany, gdy geometria kontenera planszy się zmienia.
        /// Używamy go, aby wymusić kwadratowy kształt kafelków, ustawiając ich wysokość
        /// na taką samą wartość jak obliczona przez Flexbox szerokość.
        /// </summary>
        private void OnBoardGeometryChanged(GeometryChangedEvent evt)
        {
            if (_tileElements == null || _tileElements.Length == 0 || _tileElements[0].Length == 0)
                return;

            var firstTile = _tileElements[0][0];
            if (firstTile == null) return;
            
            float tileWidth = firstTile.resolvedStyle.width;
            if (tileWidth <= 0) return;

            // Ustaw wysokość wszystkich kafelków na podstawie szerokości pierwszego.
            for (int i = 0; i < GameBoardViewModel.MaxAttempts; i++)
            {
                for (int j = 0; j < _viewModel.WordLength; j++)
                {
                    if (_tileElements[i][j] != null)
                    {
                        _tileElements[i][j].style.height = tileWidth;
                    }
                }
            }
        }
    }
}