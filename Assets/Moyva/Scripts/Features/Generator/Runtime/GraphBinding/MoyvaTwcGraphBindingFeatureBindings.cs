using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MoyvaTwcGraphBindingFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            if (container.HasBinding<IMoyvaTwcGraphBindingService>())
                return;

            container.Bind<IMoyvaTwcGraphBindingResolver>().To<MoyvaTwcGraphBindingResolver>().AsSingle();
            container.Bind<IMoyvaTwcGraphValidationService>().To<MoyvaTwcGraphValidationService>().AsSingle();
            container.Bind<IMoyvaTwcGraphCompileService>().To<MoyvaTwcGraphCompileService>().AsSingle();
            container.Bind<IMoyvaTwcGraphBindingGenerationService>().To<MoyvaTwcGraphBindingGenerationService>().AsSingle();
            container.Bind<IMoyvaTwcGraphLayerStateService>().To<MoyvaTwcGraphLayerStateService>().AsSingle();
            container.Bind<IMoyvaTwcGraphBindingLayerPreviewService>().To<MoyvaTwcGraphBindingLayerPreviewService>().AsSingle();
            container.Bind<IMoyvaTwcGraphBindingService>().To<MoyvaTwcGraphBindingService>().AsSingle();
        }
    }
}
