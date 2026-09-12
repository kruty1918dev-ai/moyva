using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingInstaller
    {
        public void Install(DiContainer container, TrainingConfig config)
        {
            config.Validate();
            container.Bind<TrainingConfig>().FromInstance(config).AsSingle();
            // Scaffold is an explicit debugging selection, never an exception fallback.
            ITrainingSimulationFactory factory = config.allowScaffoldSimulation
                ? (ITrainingSimulationFactory)new ScaffoldSimulationFactory()
                : new GameplayTrainingSimulationFactory(container.TryResolve<ITrainingGameplayScopeFactory>());
            container.Bind<ITrainingSimulationFactory>().FromInstance(factory).AsSingle();
        }
    }
}
