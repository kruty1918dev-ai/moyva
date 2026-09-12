namespace Kruty1918.Moyva.AI.Training
{
    public static class TrainingObservationLayout
    {
        public const int Version = 1;
        public const int GlobalOffset = 0;
        public const int GlobalSize = 12;
        public const int PlayerOffset = GlobalOffset + GlobalSize;
        public const int PlayerSize = 2;
        public const int UnitsOffset = PlayerOffset + PlayerSize;
        public const int UnitsSize = 8;
        public const int BuildingsOffset = UnitsOffset + UnitsSize;
        public const int BuildingsSize = 6;
        public const int MapOffset = BuildingsOffset + BuildingsSize;
        public const int MapSize = 6;
        public const int Size = MapOffset + MapSize;
    }

    public sealed class TrainingObservationSnapshot
    {
        public float[] Values { get; } = new float[TrainingObservationLayout.Size];
    }
}
