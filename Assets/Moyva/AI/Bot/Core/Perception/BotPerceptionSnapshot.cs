namespace Kruty1918.Moyva.AI.Bot
{
    public interface IBotPerceptionSource { BotPerceptionSnapshot Capture(string player); }
    public sealed class BotPerceptionSnapshot
    {
        public float[] Global { get; } = new float[BotDecisionContract.GlobalFeatureCount];
        public float[] Spatial { get; } = new float[BotDecisionContract.SpatialFeatureCount];
    }
    public sealed class EmptyBotPerceptionSource : IBotPerceptionSource
    {
        public BotPerceptionSnapshot Capture(string player) => new BotPerceptionSnapshot();
    }
    /// <summary>Channel indices inside the 8x8 spatial summary block.
    /// Channel-major layout: spatial[channel * 64 + cell].</summary>
    public static class BotSpatialChannel
    {
        public const int Explored = 0, Visible = 1, Frontier = 2;
        public const int OwnUnits = 3, VisibleEnemyUnits = 4, RememberedEnemyUnits = 5;
        public const int OwnBuildings = 6, VisibleEnemyBuildings = 7, RememberedEnemyBuildings = 8;
        public const int TerrainHeight = 9;
    }

    public static class BotSpatialSummaryEncoder
    {
        public static int Cell(int x, int y, int width, int height)
            => width < 1 || height < 1 ? -1 : System.Math.Clamp(y * 8 / height, 0, 7) * 8 + System.Math.Clamp(x * 8 / width, 0, 7);

        public static void Set(BotPerceptionSnapshot snapshot, int channel, int cell, float value)
        {
            if (cell >= 0) snapshot.Spatial[channel * 64 + cell] = value;
        }

        /// <summary>Density accumulate: three entities saturate the cell.</summary>
        public static void AddDensity(BotPerceptionSnapshot snapshot, int channel, int cell)
        {
            if (cell < 0) return;
            int index = channel * 64 + cell;
            snapshot.Spatial[index] = System.Math.Min(1f, snapshot.Spatial[index] + 0.34f);
        }
    }
    public static class BotObservationEncoder
    {
        public static float[] Encode(BotPerceptionSnapshot snapshot, BotCandidateSet candidates, BotTelemetryHub telemetry)
        {
            var result = new float[BotObservationSchema.Size];
            System.Array.Copy(snapshot.Global, result, snapshot.Global.Length);
            System.Array.Copy(snapshot.Spatial, 0, result, BotObservationSchema.SpatialOffset, snapshot.Spatial.Length);
            for (int slot = 0; slot < candidates.Count; slot++)
                for (int f = 0; f < BotDecisionContract.CandidateFeatureCount; f++)
                    result[BotObservationSchema.CandidateOffset + slot * BotDecisionContract.CandidateFeatureCount + f] = candidates[slot].Features[f];
            for (int i = 0; i < result.Length; i++)
                if (!BotRuntimeConfig.Finite(result[i])) { result[i] = 0; telemetry.NonFiniteValues++; }
            return result;
        }
    }
}

