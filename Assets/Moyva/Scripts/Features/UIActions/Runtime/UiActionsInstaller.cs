using System.Collections.Generic;
using Kruty1918.InputRouting.API;
using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.UIActions.API;
using Kruty1918.UIActions.Runtime;
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
            {
                container.Bind<IUiActionRouter>()
                    .FromMethod(ctx => (IUiActionRouter)new UiActionRouter(
                        ctx.Container.Resolve<List<IUiActionHandler>>(),
                        ctx.Container.Resolve<IUiActionJournal>(),
                        ctx.Container.TryResolve<IUiActionFeedbackSink>()))
                    .AsSingle();
            }

            if (!container.HasBinding<IUiEscapeRouter>())
            {
                if (!container.HasBinding<UiEscapeRoutingOptions>())
                {
                    container.Bind<UiEscapeRoutingOptions>()
                        .FromInstance(MoyvaUiActionCatalog.CreateEscapeRoutingOptions());
                }

                container.Bind<IUiEscapeRouter>()
                    .FromMethod(ctx => (IUiEscapeRouter)new UiEscapeRouter(
                        ctx.Container.Resolve<IUiContextStack>(),
                        ctx.Container.Resolve<IUiActionRouter>(),
                        ctx.Container.Resolve<IUiActionJournal>(),
                        ctx.Container.Resolve<UiEscapeRoutingOptions>()))
                    .AsSingle();
            }

            if (!container.HasBinding<IUiHotkeyService>())
            {
                container.BindInterfacesAndSelfTo<UiHotkeyService>()
                    .FromMethod(ctx =>
                    {
                        var controls = ctx.Container.TryResolve<IPlayerControlSettingsService>();
                        return new UiHotkeyService(
                            ctx.Container.Resolve<IUiActionRouter>(),
                            ctx.Container.Resolve<IUiContextStack>(),
                            ctx.Container.TryResolve<IGameplayInputPolicy>(),
                            MoyvaUiActionCatalog.CreateDefaultHotkeys(),
                            controls == null ? (System.Func<int>)null : () => (int)controls.SprintModifierMask);
                    })
                    .AsSingle().NonLazy();

                container.Bind<ITickable>().To<UiHotkeyServiceTickAdapter>().AsSingle().NonLazy();
            }
        }

        private sealed class UiHotkeyServiceTickAdapter : ITickable
        {
            private readonly UiHotkeyService _service;

            public UiHotkeyServiceTickAdapter(UiHotkeyService service)
            {
                _service = service;
            }

            public void Tick() => _service.Tick();
        }
    }
}
