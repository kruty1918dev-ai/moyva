namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionInfluenceMeshOverlayRenderer
    {
        void Show(ConstructionInfluenceRadiusOverlayState state, ConstructionInfluenceRadiusOverlayRequest request);
        void Hide(ConstructionInfluenceRadiusOverlayState state);
        void Draw(ConstructionInfluenceRadiusOverlayState state);
    }
}
