using Kruty1918.Moyva.AI.Bot;
namespace Kruty1918.Moyva.AI.Training
{
    public static class TrainingObservationLayout
    {
        public const int Version = BotDecisionContract.ObservationSchemaVersion;
        public const int Size = BotObservationSchema.Size;
    }
    public sealed class TrainingObservationSnapshot
    {
        public float[] Values { get; } = new float[TrainingObservationLayout.Size];
    }
}
