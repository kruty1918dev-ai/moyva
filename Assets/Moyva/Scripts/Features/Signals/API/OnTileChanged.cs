using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    // Викликається, коли фабрика створила юніта
    public struct UnitCreatedSignal
    {
        public string UnitId;       // "warrior-01_1"
        public string UnitTypeId;   // "warrior" для пошуку в SO
        public UnityEngine.Vector2Int Position;
        public int VisionRange;
        public bool HasCustomVisionModifiers;
        public bool CanSeeCrest;
        public float CrestVisibilityFactor;
        public float DownSlopeVisionBonus;
        public float SilhouettePenalty;
        public UnityEngine.GameObject UnitObject;

        /// <summary>
        /// Ідентифікатор фракції-власника (FactionId.Value).
        /// Null або порожній рядок — юніт не прив'язаний до жодної фракції.
        /// FactionOwnershipService підхопить це значення автоматично.
        /// </summary>
        public string OwnerId;
    }

    /// <summary>
    /// Надсилається коли фракцію вважають переможеною (всі юніти знищені або
    /// досягнуто умову поразки).
    /// Отримується: UI, GameOverService тощо.
    /// </summary>
    public struct FactionEliminatedSignal
    {
        /// <summary>FactionId.Value переможеної фракції.</summary>
        public string FactionId;
    }

    // Викликається, коли юніт перемістився
    public struct UnitMovedSignal
    {
        public string UnitId;
        public UnityEngine.Vector2Int NewPosition;
        public float Cost;
        public string SourceFactionId;
    }

    // Викликається при смерті/видаленні
    public struct UnitDestroyedSignal
    {
        public string UnitId;
    }

    public struct InterruptMovementSignal
    {
        public string UnitId;
    }

    /// <summary>
    /// Надсилається TileInteractionService, коли гравець наказує юніту рухатись до тайлу.
    /// MultiplayerAuthorityService перехоплює і або виконує MoveUnitAsync локально (хост/офлайн),
    /// або надсилає запит до хоста (клієнт).
    /// </summary>
    public struct MoveUnitRequestSignal
    {
        public string UnitId;
        public Vector2Int TargetPosition;
    }

    /// <summary>
    /// Надсилається MapVisualInstantiator після спавну статичного обʼєкта карти (гора, річка, ліс…)
    /// </summary>
    public struct OnMapObjectSpawnedSignal
    {
        public string ObjectId;        // TileTypeId, наприклад "river", "mountain"
        public Vector2Int Position;
    }

    /// <summary>
    /// Надсилається ObjectsMapService після будь-якої зміни карти обʼєктів
    /// </summary>
    public struct OnObjectsMapChangedSignal
    {
        public Vector2Int Position;
        public string OccupantId;      // null якщо тайл звільнено
    }

    public struct FogStateChangedSignal
    {
        public int ChangedTilesCount;
    }

    public struct WorldBuiltSignal
    {
    }

    /// <summary>
    /// Надсилається після завершення побудови світу і містить згенеровані мапи.
    /// </summary>
    public struct WorldGeneratedDataSignal
    {
        public int Width;
        public int Height;
        public string[,] TileMap;
        public string[,] ObjectMap;
        public float[,] HeightMap;
        public int[,] TerrainLevelMap;
    }

    public struct SpawnPositionAssignment
    {
        public int SlotIndex;
        public string ParticipantId;
        public bool IsBot;
        public Vector2Int Position;
    }

    public struct WorldSpawnPositionsSignal
    {
        public SpawnPositionAssignment[] Assignments;
    }
}
