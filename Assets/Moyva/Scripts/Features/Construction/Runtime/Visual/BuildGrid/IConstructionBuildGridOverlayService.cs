namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridOverlayService
    {
        void Initialize();
        void SetConstructionModeActive(bool active);
        void SetSelectedBuilding(string buildingId, bool isDemolishMode);
        void MarkDirty();
        void MarkDirty(UnityEngine.Vector2Int position, int radius);
        void ResetWorld();
        void Tick();
        void Hide();
        void Dispose();
    }
}
