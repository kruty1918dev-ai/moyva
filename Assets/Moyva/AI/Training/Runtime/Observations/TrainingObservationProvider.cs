using Kruty1918.Moyva.AI.Bot;
using Unity.MLAgents.Sensors;
namespace Kruty1918.Moyva.AI.Training
{
    public interface ITrainingObservationProvider { void Collect(VectorSensor sensor); }
    public interface ITrainingVisibleObservationSource
    {
        void CopyVisibleState(string playerId, TrainingObservationSnapshot target);
    }
    public sealed class TrainingObservationProvider : ITrainingObservationProvider
    {
        private readonly TrainingBotBridge _bridge;
        public TrainingObservationProvider(TrainingBotBridge bridge) { _bridge = bridge; }
        public void Collect(VectorSensor sensor) => BotMlFrameWriter.Observe(_bridge.Frame, sensor);
    }
}
