using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
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
