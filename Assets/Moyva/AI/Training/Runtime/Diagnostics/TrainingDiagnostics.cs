namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingDiagnostics
    {
        public long EpisodeNumber { get; private set; }
        public int Decisions { get; internal set; }
        public int Turns { get; internal set; }
        public int ValidActions { get; internal set; }
        public int InvalidActions { get; internal set; }
        public int StaleActions { get; internal set; }
        public int CandidateCount { get; internal set; }
        public int[] CandidateCountsByIntent { get; } = new int[12];
        public float TotalReward { get; internal set; }
        public float ShapingReward { get; internal set; }
        public TrainingEpisodeResult EpisodeResult { get; internal set; }
        public int ResetCount { get; private set; }
        public int LastSeed { get; private set; }
        public string LastError { get; internal set; }
        public double ElapsedSeconds { get; internal set; }

        public void Reset(TrainingResetContext context)
        {
            EpisodeNumber = context.EpisodeId;
            LastSeed = context.Seed;
            ResetCount++;
            Decisions = Turns = ValidActions = InvalidActions = 0;
            StaleActions = 0;
            CandidateCount = 0;
            System.Array.Clear(CandidateCountsByIntent, 0, CandidateCountsByIntent.Length);
            TotalReward = ShapingReward = 0;
            EpisodeResult = TrainingEpisodeResult.None;
            LastError = null;
            ElapsedSeconds = 0;
        }
    }
}
