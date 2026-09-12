using System;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.AI.Training
{
    public enum TrainingEpisodeResult { None, Victory, Defeat, Draw, Timeout, InvalidState }

    public interface ITrainingEnvironment
    {
        int EnvironmentId { get; }
        long EpisodeId { get; }
        bool IsReady { get; }
        TrainingEpisodeResult Result { get; }
        TrainingCurriculumStage Stage { get; }
        void ResetEnvironment();
        void BeginEpisode();
        void EndEpisode(TrainingEpisodeResult result);
    }

    public interface ITrainingSimulation : IDisposable
    {
        ITurnService Turns { get; }
        string PlayerId { get; }
        bool IsReady { get; }
        string Limitation { get; }
        bool Reset(TrainingResetContext context);
        bool CanEndTurn();
    }

    public interface ITrainingSimulationFactory
    {
        bool SupportsIndependentEnvironments { get; }
        ITrainingSimulation Create(int environmentId);
    }

    public sealed class ScaffoldSimulationFactory : ITrainingSimulationFactory
    {
        public bool SupportsIndependentEnvironments => true;
        public ITrainingSimulation Create(int environmentId) => new ScaffoldSimulation();

        private sealed class ScaffoldSimulation : ITrainingSimulation
        {
            public ITurnService Turns => null;
            public string PlayerId => string.Empty;
            public bool IsReady { get; private set; }
            public string Limitation => "SCAFFOLD / NOT REAL GAMEPLAY. No world, units, perception or gameplay reset. Only Wait is available.";
            public bool Reset(TrainingResetContext context) { IsReady = true; return true; }
            public bool CanEndTurn() => false;
            public void Dispose() { IsReady = false; }
        }
    }
}
