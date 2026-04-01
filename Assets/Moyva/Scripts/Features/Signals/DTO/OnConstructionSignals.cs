using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>Запускається при переході в режим розміщення будівлі.</summary>
    public struct BuildingModeStartedSignal
    {
        public string TypeId;
    }

    /// <summary>Запускається при переміщенні попереднього перегляду будівлі.</summary>
    public struct BuildingPreviewMovedSignal
    {
        public Vector2Int Position;
        public string TypeId;
        /// <summary>True — позиція заблокована (червоний колір).</summary>
        public bool IsBlocked;
    }

    /// <summary>Запускається при розміщенні pending-будівлі на тайлі.</summary>
    public struct BuildingPlacedSignal
    {
        public string TempId;
        public string TypeId;
        public Vector2Int Position;
    }

    /// <summary>Запускається при скасуванні останнього розміщення (Ctrl+Z).</summary>
    public struct BuildingUndoneSignal
    {
        public string TempId;
        public Vector2Int Position;
    }

    /// <summary>Запускається при повторі скасованого розміщення (Ctrl+Y).</summary>
    public struct BuildingRedoneSignal
    {
        public string TempId;
        public Vector2Int Position;
    }

    /// <summary>Запускається при скасуванні всіх pending-будівель.</summary>
    public struct BuildingCancelledSignal
    {
        public string[] TempIds;
        public Vector2Int[] Positions;
    }

    /// <summary>Запускається при підтвердженні всіх pending-будівель.</summary>
    public struct BuildingConfirmedSignal
    {
        public string[] TempIds;
        public string[] TypeIds;
        public Vector2Int[] Positions;
    }

    /// <summary>Запускається при показі точок з'єднання навколо стіни.</summary>
    public struct WallConnectionPointsShownSignal
    {
        public Vector2Int WallPosition;
        /// <summary>До 8 сусідніх позицій, на яких можна продовжити стіну.</summary>
        public Vector2Int[] ConnectionPoints;
    }

    /// <summary>Запускається при приховуванні точок з'єднання стіни.</summary>
    public struct WallConnectionPointsHiddenSignal { }
}
