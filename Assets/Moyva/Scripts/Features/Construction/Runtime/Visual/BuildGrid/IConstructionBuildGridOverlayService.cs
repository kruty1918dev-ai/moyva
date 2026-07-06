namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridOverlayService
    {
        void Initialize();
        void SetConstructionModeActive(bool active);
        void MarkDirty();
        void Tick();
        void Hide();
    }
}
