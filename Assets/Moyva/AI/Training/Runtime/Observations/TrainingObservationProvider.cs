using System;
using Unity.MLAgents.Sensors;

namespace Kruty1918.Moyva.AI.Training
{
    public interface ITrainingObservationProvider
    {
        void Collect(VectorSensor sensor);
    }

    // Future adapters must supply only player-visible state; never raw world/enemy registries.
    public interface ITrainingVisibleObservationSource
    {
        void CopyVisibleState(string playerId, TrainingObservationSnapshot target);
    }

    public sealed class TrainingObservationProvider : ITrainingObservationProvider
    {
        private readonly string _playerId;
        private readonly ITrainingVisibleObservationSource _source;
        private readonly TrainingObservationSnapshot _snapshot = new TrainingObservationSnapshot();

        public TrainingObservationProvider(string playerId, ITrainingVisibleObservationSource source = null)
        {
            _playerId = playerId;
            _source = source;
        }

        public void Collect(VectorSensor sensor)
        {
            Array.Clear(_snapshot.Values, 0, _snapshot.Values.Length);
            _source?.CopyVisibleState(_playerId, _snapshot);
            foreach (float value in _snapshot.Values)
                sensor.AddObservation(TrainingConfig.Finite(value) ? value : 0f);
        }
    }
}
