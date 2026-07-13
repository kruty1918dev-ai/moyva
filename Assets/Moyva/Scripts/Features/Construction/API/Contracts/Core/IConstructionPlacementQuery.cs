namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionPlacementQuery
    {
        ConstructionPlacementQueryResult EvaluatePlacement(ConstructionPlacementQueryRequest request);
    }
}
