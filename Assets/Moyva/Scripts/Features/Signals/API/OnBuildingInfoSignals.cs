using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>Дані про один ресурс, необхідний для будівництва будівлі. Іконка може бути null.</summary>
    public struct BuildingConstructionCostItemData
    {
        public string ResourceId;
        public string DisplayName;
        public int Amount;
        public Sprite Icon;
    }

    public enum WorldInfoSelectionKind
    {
        None = 0,
        Building = 1,
        Unit = 2,
        MapObject = 3,
    }

    public struct WorldInfoPanelRequestedSignal
    {
        public string Title;
        public string Subtitle;
        public string Content;
        /// <summary>Вартість будівництва. Null або порожній масив — безкоштовно / не актуально.</summary>
        public BuildingConstructionCostItemData[] ConstructionCostItems;
    }

    public struct WorldInfoPanelClosedSignal
    {
    }

    public struct BuildingInfoPanelRequestedSignal
    {
        public string BuildingId;
        public Vector2Int Position;
    }

    public struct BuildingInfoPanelClosedSignal
    {
    }

    public struct UnitInfoPanelRequestedSignal
    {
        public string UnitId;
        public UnityEngine.Vector2Int Position;
    }

    public struct MapObjectInfoPanelRequestedSignal
    {
        public string MapObjectId;
        public Vector2Int Position;
    }

    public struct WorldInfoSelectionChangedSignal
    {
        public WorldInfoSelectionKind Kind;
        public string ObjectId;
        public Vector2Int Position;
    }
}
