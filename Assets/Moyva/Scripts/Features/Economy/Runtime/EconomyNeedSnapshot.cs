namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct EconomyNeedSnapshot
    {
        public EconomyNeedSnapshot(float foodSeverity, float coldSeverity, float diseaseSeverity, float warSeverity)
        {
            FoodSeverity = foodSeverity;
            ColdSeverity = coldSeverity;
            DiseaseSeverity = diseaseSeverity;
            WarSeverity = warSeverity;
        }

        public float FoodSeverity { get; }
        public float ColdSeverity { get; }
        public float DiseaseSeverity { get; }
        public float WarSeverity { get; }
    }
}
