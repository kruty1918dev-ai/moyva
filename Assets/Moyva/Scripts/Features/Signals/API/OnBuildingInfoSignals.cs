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

    /// <summary>
    /// Сигнал для фокусування камери на позицію будівлі. Закриває панель замку.
    /// </summary>
    public struct CameraFocusBuildingSignal
    {
        public Vector2Int Position;
        public string BuildingId;
    }

    public struct SettlementStatisticsMenuRequestedSignal
    {
        public string SettlementId;
        public string OwnerId;
    }

    public struct KingdomStatisticsMenuRequestedSignal
    {
        public string OwnerId;
        public string PreferredSettlementId;
    }

    public struct StatisticsMenuClosedSignal
    {
    }

    /// <summary>
    /// Нейтральний провайдер локального playerId. Реалізується мультиплеєрним шаром,
    /// а інші модулі можуть інжектити його як optional dependency.
    /// </summary>
    public interface ILocalPlayerIdentityProvider
    {
        string LocalPlayerId { get; }
    }
}
