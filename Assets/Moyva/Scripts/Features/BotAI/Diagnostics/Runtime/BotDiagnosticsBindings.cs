using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    public static class BotDiagnosticsBindings
    {
        public static void Install(DiContainer container)
        {
            if (container == null)
                return;

            if (!container.HasBinding(typeof(BotDiagnosticsSettings)))
            {
                var settings = new BotDiagnosticsSettings();
                settings.Normalize();
                container.BindInstance(settings).AsSingle();
            }

            if (!container.HasBinding(typeof(BotDiagnosticsContextProvider)))
                container.Bind<BotDiagnosticsContextProvider>().AsSingle();

            if (!container.HasBinding(typeof(BotDiagnosticsFormatter)))
                container.Bind<BotDiagnosticsFormatter>().AsSingle();

            if (!container.HasBinding(typeof(IBotDiagnosticsBuffer)))
                container.Bind<IBotDiagnosticsBuffer>().To<BotDiagnosticsRingBuffer>().AsSingle();

            if (!container.HasBinding(typeof(BotDiagnosticsConsoleSink)))
                container.Bind<BotDiagnosticsConsoleSink>().AsSingle();

            if (!container.HasBinding(typeof(IBotDiagnosticsLogger)))
                container.Bind<IBotDiagnosticsLogger>().To<BotDiagnosticsLogger>().AsSingle();

            BindObserver<BotDiagnosticsLaunchObserver>(container);
            BindObserver<BotDiagnosticsRecoveryObserver>(container);
            BindObserver<BotDiagnosticsHealthProbe>(container);
            BindObserver<BotDiagnosticsSignalObserver>(container);
            BindObserver<BotDiagnosticsTurnObserver>(container);
            BindObserver<BotDiagnosticsReasoningObserver>(container);
            BindObserver<BotDiagnosticsSnapshotProbe>(container);
        }

        private static void BindObserver<T>(DiContainer container)
            where T : class
        {
            if (!container.HasBinding(typeof(T)))
                container.BindInterfacesAndSelfTo<T>().AsSingle().NonLazy();
        }
    }
}
