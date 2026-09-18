using System;

namespace Kruty1918.Moyva.AI.Training
{
    public interface ITrainingEpisodeOutcomeSource
    {
        bool IsConnected { get; }
        string Limitation { get; }
        event Action<TrainingEpisodeResult> Completed;
    }
}
