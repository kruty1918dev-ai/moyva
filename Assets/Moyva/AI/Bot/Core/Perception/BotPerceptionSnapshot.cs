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
    public static class BotSpatialSummaryEncoder
    {
        public static int Cell(int x, int y, int width, int height)
            => width < 1 || height < 1 ? -1 : System.Math.Clamp(y * 8 / height, 0, 7) * 8 + System.Math.Clamp(x * 8 / width, 0, 7);
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

