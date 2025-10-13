using UnityEngine;
using Yawordle.Core;
using VContainer;
using VContainer.Unity;
using Yawordle.Infrastructure;
using Yawordle.Presentation;
using Yawordle.Presentation.Views;
using Yawordle.Presentation.ViewModels;

namespace Yawordle.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private UISettings uiSettings;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IGameManager, GameManager>(Lifetime.Singleton);
            builder.Register<ISettingsService, JsonSettingsService>(Lifetime.Singleton);
            builder.Register<IWordProvider, ResourceWordProvider>(Lifetime.Singleton);
            builder.Register<IKeyboardLayoutProvider, KeyboardLayoutProvider>(Lifetime.Singleton);
            
            builder.RegisterInstance(uiSettings);
            
            builder.Register<GameBoardViewModel>(Lifetime.Singleton);
            // Register SettingsViewModel with a Transient lifetime,
            // so a new instance is created every time it's requested.
            builder.Register<SettingsViewModel>(Lifetime.Transient);

            
            // As<IStartable>() aby VContainer automatycznie wywołał metodę Start() po utworzeniu wszystkich obiektów.
            builder.Register<GameScreenView>(Lifetime.Singleton).As<IStartable>();
            builder.Register<SettingsView>(Lifetime.Singleton).As<IStartable>();

        }
    }
}