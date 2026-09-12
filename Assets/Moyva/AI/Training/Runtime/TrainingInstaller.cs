using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingInstaller
    {
        public void Install(DiContainer container, TrainingConfig config)
        {
            container.Bind<TrainingConfig>().FromInstance(config).AsSingle();
            container.Bind<ITrainingSimulationFactory>().To<ScaffoldSimulationFactory>().AsSingle();
        }
    }
}
