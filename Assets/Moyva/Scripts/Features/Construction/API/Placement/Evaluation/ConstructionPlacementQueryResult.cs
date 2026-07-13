namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct ConstructionPlacementQueryResult
    {
        public ConstructionPlacementQueryResult(
            bool isSpatiallyValid,
            bool resourcesValid,
            bool isGateReplacement,
            string reason = null,
            BuildingPlacementEvaluationResult evaluationResult = null)
        {
            IsSpatiallyValid = isSpatiallyValid;
            ResourcesValid = resourcesValid;
            IsGateReplacement = isGateReplacement;
            Reason = reason;
            EvaluationResult = evaluationResult;
        }

        public bool IsSpatiallyValid { get; }
        public bool ResourcesValid { get; }
        public bool IsGateReplacement { get; }
        public string Reason { get; }
        public BuildingPlacementEvaluationResult EvaluationResult { get; }
        public bool IsValid => IsSpatiallyValid && ResourcesValid;
    }
}
