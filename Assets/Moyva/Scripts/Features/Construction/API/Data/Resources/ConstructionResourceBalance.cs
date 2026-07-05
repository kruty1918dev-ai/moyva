namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct ConstructionResourceBalance
    {
        public ConstructionResourceBalance(string resourceId, float available, float reserved)
        {
            ResourceId = resourceId;
            Available = available;
            Reserved = reserved;
            Remaining = available - reserved;
            IsDeficit = Remaining < -0.0001f;
        }

        public string ResourceId { get; }
        public float Available { get; }
        public float Reserved { get; }
        public float Remaining { get; }
        public bool IsDeficit { get; }
    }
}
