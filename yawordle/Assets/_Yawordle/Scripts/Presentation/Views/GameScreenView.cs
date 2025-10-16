using System;
using System.Collections.Generic;
using Bitbebop;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer.Unity;
using Yawordle.Core;
using Yawordle.Presentation.ViewModels;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Yawordle.Infrastructure;
using Object = UnityEngine.Object;

namespace Yawordle.Presentation.Views
{
    public class GameScreenView : IStartable
    {
        private readonly GameBoardViewModel _viewModel;
        private readonly ISettingsService _settingsService;
        private readonly IKeyboardLayoutProvider _keyboardLayoutProvider;

        private SafeArea _screenContainer;
        private VisualElement _header;
        private VisualElement _boardContainer;
        private VisualElement[][] _tileElements;
        private Label[][] _tileLabels;
        private VisualElement _keyboardContainer;
        private readonly Dictionary<char, Button> _keyButtons = new();
        
        private Label _toastNotification; 
        private Sequence _currentToastSequence;
        private Tween _currentShakeAnimation;

        public GameScreenView(
            GameBoardViewModel viewModel, 
            ISettingsService settingsService, 
            IKeyboardLayoutProvider keyboardLayoutProvider)
        {
            _viewModel = viewModel;
            _settingsService = settingsService;
            _keyboardLayoutProvider = keyboardLayoutProvider;
        }

        public void Start()
        {
            var uiDocument = Object.FindAnyObjectByType<UIDocument>();
            var root = uiDocument.rootVisualElement;

            // Find all root containers from UXML
            _screenContainer = root.Q<SafeArea>("screen-container");
            _header = root.Q<VisualElement>("header");
            _boardContainer = root.Q<VisualElement>("game-board-container");
            _keyboardContainer = root.Q<VisualElement>("keyboard-container");
            _toastNotification = root.Q<Label>("toast-notification");

            _boardContainer.focusable = true;
            _boardContainer.Focus();

            // Register a single callback on the root container to handle all geometry changes
            _screenContainer.RegisterCallback<GeometryChangedEvent>(OnScreenGeometryChanged);
            
            GenerateGrid();
            GenerateKeyboard();
            BindToViewModel();
            BindKeyboardInput();
        }

        private void OnScreenGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateTileSize();
        }


        /// <summary>
        /// Enforces a square aspect ratio for the tiles by setting their height
        /// based on their calculated width.
        /// </summary>
        private void UpdateTileSize()
        {
            if (_tileElements == null || _tileElements.Length == 0 || _tileElements[0] == null || _tileElements[0].Length == 0) return;
            var firstTile = _tileElements[0][0];
            if (firstTile == null) return;
            float tileWidth = firstTile.resolvedStyle.width;
            if (tileWidth <= 0) return;

            for (int i = 0; i < GameBoardViewModel.MaxAttempts; i++)
            {
                if (_tileElements[i] == null) continue;
                for (int j = 0; j < _viewModel.WordLength; j++)
                {
                    if (_tileElements[i][j] != null)
                    {
                        _tileElements[i][j].style.height = tileWidth;
                    }
                }
            }
        }

        private void GenerateGrid()
        {
            _boardContainer.Clear();
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
                    
                    _tileElements[i][j] = tileElement;
                    _tileLabels[i][j] = labelElement;
                }
                _boardContainer.Add(rowElement);
            }
        }
        
        private void GenerateKeyboard()
        {
            _keyboardContainer.Clear();
            _keyButtons.Clear();
            var layout = _keyboardLayoutProvider.GetLayoutForLanguage(_settingsService.CurrentSettings.Language);

            int rowIndex = -1;
            int lastRowIndex = layout.KeyRows.Length - 1;
            foreach (var rowString in layout.KeyRows)
            {
                rowIndex++;
                var rowElement = new VisualElement();
                rowElement.AddToClassList("keyboard-row");
                if(rowIndex == lastRowIndex)
                    rowElement.AddToClassList("keyboard-row--last");
                
                var keys = rowString.Split(',');
                foreach (var key in keys)
                {
                    
                    if (key.Length > 1) // To jest klawisz funkcyjny (ENTER, BACKSPACE)
                    {
                        var button = new Button { text = key };
                        button.AddToClassList("key");
                        button.AddToClassList("key--functional");
                        switch (key)
                        {
                            case "ENTER":
                                button.clicked += _viewModel.SubmitGuess;
                                break;
                            case "BACKSPACE":
                                button.text = string.Empty;
                                button.AddToClassList("key--backspace");
                                button.clicked += _viewModel.DeleteLetter;
                                break;
                        }

                        rowElement.Add(button);
                    }
                    else // To jest rząd liter
                    {
                        foreach (var keyChar in key)
                        {
                            var keyButton = new Button { text = keyChar.ToString() };
                            keyButton.AddToClassList("key");
                            keyButton.clicked += () => _viewModel.TypeLetter(keyChar);
                            rowElement.Add(keyButton);
                            _keyButtons[keyChar] = keyButton;
                        }
                    }
                }
                _keyboardContainer.Add(rowElement);
            }
        }
        
        private void BindToViewModel()
        {
            for (int i = 0; i < GameBoardViewModel.MaxAttempts; i++)
            {
                for (int j = 0; j < _viewModel.WordLength; j++)
                {
                    TileViewModel tileVM = _viewModel.Tiles[i][j];
                    Label tileLabel = _tileLabels[i][j];
                    VisualElement tileElement = _tileElements[i][j];

                    tileVM.PropertyChanged += (sender, args) =>
                    {
                        var vm = (TileViewModel)sender;
                        if (args.PropertyName == nameof(TileViewModel.Letter))
                        {
                            tileLabel.text = vm.Letter.ToString().ToUpper();
                        }
                        else if (args.PropertyName == nameof(TileViewModel.State))
                        {
                            // UWAGA: Już nie zmieniamy stanu bezpośrednio tutaj,
                            // animacja się tym zajmie. Można to zakomentować lub usunąć.
                            // UpdateTileState(tileElement, vm.State);
                        }
                    };
                }
            }
            
            foreach (var keyVM in _viewModel.Keys.Values)
            {
                if (_keyButtons.TryGetValue(keyVM.Key, out var keyButton))
                {
                    keyVM.PropertyChanged += (sender, args) =>
                    {
                        var changedKeyVM = (KeyViewModel)sender;
                        if (args.PropertyName == nameof(KeyViewModel.State))
                        {
                            UpdateKeyState(keyButton, changedKeyVM.State);
                        }
                    };
                }
            }
            
            // Subskrybuj event do animacji
            _viewModel.OnRowEvaluatedForAnimation += (rowIndex, states) => AnimateRowAsync(rowIndex, states).Forget();
            _viewModel.OnInvalidGuess += OnInvalidGuess;
        }

        private void UpdateTileState(VisualElement tileElement, LetterState state)
        {
            tileElement.RemoveFromClassList("tile--correct");
            tileElement.RemoveFromClassList("tile--present");
            tileElement.RemoveFromClassList("tile--absent");
            switch (state)
            {
                case LetterState.Correct: tileElement.AddToClassList("tile--correct"); break;
                case LetterState.Present: tileElement.AddToClassList("tile--present"); break;
                case LetterState.Absent: tileElement.AddToClassList("tile--absent"); break;
            }
        }
        
        /// <summary>
        /// Adds a new method to update the USS class of a key button.
        /// </summary>
        private void UpdateKeyState(Button keyButton, LetterState state)
        {
            keyButton.EnableInClassList("key--present", state == LetterState.Present);
            keyButton.EnableInClassList("key--absent",state == LetterState.Absent);
            keyButton.EnableInClassList("key--correct",state == LetterState.Correct);
        }
        
        private async UniTaskVoid AnimateRowAsync(int rowIndex, LetterState[] states)
        {
            var sequence = Sequence.Create();
            for (int i = 0; i < _viewModel.WordLength; i++)
            {
                VisualElement tile = _tileElements[rowIndex][i];
                LetterState state = states[i];

                var tweenPart1 = Tween.Custom(
                    startValue: new Vector2(1, 1),
                    endValue: new Vector2(0, 1),
                    duration: 0.25f,
                    onValueChange: newScale => tile.style.scale = newScale
                );
                
                var tweenPart2 = Tween.Custom(
                    startValue: new Vector2(0, 1),
                    endValue: new Vector2(1, 1),
                    duration: 0.25f,
                    onValueChange: newScale => tile.style.scale = newScale
                );
                
                var fullTileTween = Sequence.Create()
                    .Chain(tweenPart1)
                    .ChainCallback(() => UpdateTileState(tile, state))
                    .Chain(tweenPart2);
            
                sequence = sequence.Chain(fullTileTween);
            }
        
            await sequence;
        }
        
        private void OnInvalidGuess(GuessValidationError error, int attemptIndex)
        {
            ShakeRow(attemptIndex);

            string message = error switch
            {
                GuessValidationError.NotEnoughLetters => "Not enough letters",
                GuessValidationError.NotInWordList => "Not in word list",
                _ => "Unknown error"
            };
            ShowToast(message);
        }
        
        private void ShakeRow(int rowIndex)
        {
            if (rowIndex >= _tileElements.Length || _tileElements[rowIndex].Length == 0) return;

            _currentShakeAnimation.Stop();

            var rowToShake = _tileElements[rowIndex][0].parent;
            
            const float shakeStrength = 10f; 
            const float shakeDuration = 0.4f;

            _currentShakeAnimation = Tween.Custom(
                target: rowToShake,
                startValue: 0f,
                endValue: 1f,
                duration: shakeDuration,
                onValueChange: (target, progress) =>
                {
                    float strength = shakeStrength * (1 - progress);
                    float xOffset = Mathf.Sin(progress * 10 * Mathf.PI * 2) * strength;
                    target.style.translate = new Vector2(xOffset, 0);
                }
            ).OnComplete(rowToShake, (target) => target.style.translate = Vector2.zero);
        }

        private void ShowToast(string message)
        {
            _currentToastSequence.Stop();
            
            _toastNotification.text = message;

            const float fadeInDuration = 0.25f;
            const float fadeOutDuration = 0.3f;
            const float visibleDuration = 1.5f;

            
            _currentToastSequence = Sequence.Create()
                .ChainCallback(() => {
                    _toastNotification.style.opacity = 0;
                    _toastNotification.style.scale = new Vector3(0.9f, 0.9f, 1f);
                    _toastNotification.style.display = DisplayStyle.Flex;
                })
                .Chain(Tween.Custom(
                    startValue: 0.9f, 
                    endValue: 1f, 
                    duration: fadeInDuration, 
                    ease: Ease.OutQuad, 
                    onValueChange: newScale => _toastNotification.style.scale = new Vector3(newScale, newScale, 1f)
                ))
                .Group(Tween.Custom(
                    startValue: 0f, 
                    endValue: 1f, 
                    duration: fadeInDuration, 
                    onValueChange: newOpacity => _toastNotification.style.opacity = newOpacity
                ))
                .ChainDelay(visibleDuration)
                .Chain(Tween.Custom(
                    startValue: 1f, 
                    endValue: 0.9f, 
                    duration: fadeOutDuration, 
                    ease: Ease.InQuad, 
                    onValueChange: newScale => _toastNotification.style.scale = new Vector3(newScale, newScale, 1f)
                ))
                .Group(Tween.Custom(
                    startValue: 1f, 
                    endValue: 0f, 
                    duration: fadeOutDuration, 
                    onValueChange: newOpacity => _toastNotification.style.opacity = newOpacity
                ))
                .OnComplete(() => {
                    _toastNotification.style.display = DisplayStyle.None;
                });

        }

        private void BindKeyboardInput()
        {
            _boardContainer.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (char.IsLetter(evt.character)) _viewModel.TypeLetter(evt.character);
                else if (evt.keyCode == KeyCode.Backspace) _viewModel.DeleteLetter();
                else if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) _viewModel.SubmitGuess();
            });
        }
        
    }
}