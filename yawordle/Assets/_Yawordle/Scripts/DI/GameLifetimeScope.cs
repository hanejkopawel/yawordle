using UnityEngine;
using Yawordle.Core;
using VContainer;
using VContainer.Unity;
using Yawordle.Infrastructure;
using Yawordle.Infrastructure.Localization;
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
            
            // --- Core Services (Models) ---
            // Register game logic services. VContainer will automatically resolve their dependencies.
            builder.Register<IGameManager, GameManager>(Lifetime.Singleton);
            builder.Register<ISettingsService, JsonSettingsService>(Lifetime.Singleton);
            builder.Register<IWordProvider, ResourceWordProvider>(Lifetime.Singleton);
            
            // --- Infrastructure Services ---
            // Services that interact with external systems (UI, backend, file system).
            builder.Register<IKeyboardLayoutProvider, KeyboardLayoutProvider>(Lifetime.Singleton);
            builder.Register<IUgsService, UgsService>(Lifetime.Singleton);
            builder.Register<UnityLocalizationService>(Lifetime.Singleton).As<ILocalizationService>();
            builder.RegisterInstance(uiSettings);
            
            // --- ViewModels ---
            builder.Register<GameBoardViewModel>(Lifetime.Singleton);
            // SettingsViewModel is created on-demand each time the settings panel is opened.
            builder.Register<SettingsViewModel>(Lifetime.Transient);
            
            // --- Application Entry Point ---
            // It's responsible for the initial asynchronous setup, like fetching the word of the day.
            builder.Register<GameInitializer>(Lifetime.Singleton).As<IAsyncStartable>();

            // --- Views ---
            // Views are registered as IStartable to be initialized after all dependencies are built.
            builder.Register<InstructionsView>(Lifetime.Singleton).As<IStartable>();
            builder.Register<GameScreenView>(Lifetime.Singleton).As<IStartable>();
            builder.Register<SettingsView>(Lifetime.Singleton).As<IStartable>();
            builder.Register<EndGameView>(Lifetime.Singleton).As<IStartable>();

        }
    }
}