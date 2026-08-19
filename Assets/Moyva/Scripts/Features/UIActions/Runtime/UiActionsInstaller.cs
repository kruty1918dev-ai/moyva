using Kruty1918.Moyva.UIActions.API;
using Zenject;

namespace Kruty1918.Moyva.UIActions.Runtime
{
    public static class UiActionsInstaller
    {
        public static void Install(DiContainer container)
        {
            if (container == null)
                return;

            if (!container.HasBinding<IUiActionJournal>())
                container.Bind<IUiActionJournal>().To<UiActionJournal>().AsSingle();

            if (!container.HasBinding<IUiContextStack>())
                container.Bind<IUiContextStack>().To<UiContextStack>().AsSingle();

            if (!container.HasBinding<IUiActionRouter>())
                container.Bind<IUiActionRouter>().To<UiActionRouter>().AsSingle();

            if (!container.HasBinding<IUiEscapeRouter>())
                container.Bind<IUiEscapeRouter>().To<UiEscapeRouter>().AsSingle();

            if (!container.HasBinding<IUiHotkeyService>())
                container.BindInterfacesTo<UiHotkeyService>().AsSingle().NonLazy();
        }
    }
}
