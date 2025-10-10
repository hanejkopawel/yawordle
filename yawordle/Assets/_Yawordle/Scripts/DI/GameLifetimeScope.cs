using Yawordle.Core;
using VContainer;
using VContainer.Unity;
using Yawordle.Infrastructure;
using Yawordle.Presentation.Views;
using Yawordle.Presentation.ViewModels;

namespace Yawordle.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IGameManager, GameManager>(Lifetime.Singleton);
            
            builder.Register<ISettingsService, JsonSettingsService>(Lifetime.Singleton);
            
            // builder.Register<IWordProvider, ResourceWordProvider>(Lifetime.Singleton);

            builder.Register<GameBoardViewModel>(Lifetime.Singleton);
            
            // As<IStartable>()aby VContainer automatycznie wywołał metodę Start() po utworzeniu wszystkich obiektów.
            builder.Register<GameScreenView>(Lifetime.Singleton).As<IStartable>();
        }
    }
}