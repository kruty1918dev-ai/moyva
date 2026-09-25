using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    // Викликається, коли фабрика створила юніта
    /// <summary>UnitCreatedSignal — struct: юніта Created сигнал.</summary>
    public struct UnitCreatedSignal
    {
        /// <summary>юніта ID — string.</summary>
        public string UnitId;       // "warrior-01_1"
        /// <summary>юніта Type ID — string.</summary>
        public string UnitTypeId;   // "warrior" для пошуку в SO
        /// <summary>позицію — UnityEngine.Vector2Int.</summary>
        public UnityEngine.Vector2Int Position;
        /// <summary>огляду дальності — int.</summary>
        public int VisionRange;
        /// <summary>Чи користувацького огляду Modifiers — HasCustomVisionModifiers.</summary>
        public bool HasCustomVisionModifiers;
        /// <summary>Чи See гребеня — CanSeeCrest.</summary>
        public bool CanSeeCrest;
        /// <summary>гребеня Visibility Factor — float.</summary>
        public float CrestVisibilityFactor;
        /// <summary>Down схилу огляду Bonus — float.</summary>
        public float DownSlopeVisionBonus;
        /// <summary>Штраф до огляду за силует на тлі неба.</summary>
        public float SilhouettePenalty;
        /// <summary>юніта обʼєкта — UnityEngine.GameObject.</summary>
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

    /// <summary>UnitGarrisonStateChangedSignal — struct: юніта гарнізону стану Changed сигнал.</summary>
    public struct UnitGarrisonStateChangedSignal
    {
        /// <summary>юніта ID — string.</summary>
        public string UnitId;
        /// <summary>Чи Garrisoned — IsGarrisoned.</summary>
        public bool IsGarrisoned;
        /// <summary>будівлі позицію — Vector2Int.</summary>
        public Vector2Int BuildingPosition;
        /// <summary>юніта позицію — Vector2Int.</summary>
        public Vector2Int UnitPosition;
        /// <summary>огляду дальності — int.</summary>
        public int VisionRange;
        /// <summary>власника ID — string.</summary>
        public string OwnerId;
    }

    /// <summary>BuildingOperationalSignal — struct: будівлі робочого сигнал.</summary>
    public struct BuildingOperationalSignal
    {
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>власника ID — string.</summary>
        public string OwnerId;

        /// <summary>миттєвого візуалу — bool.</summary>
        public bool InstantVisual;
    }

    /// <summary>BuildingMotionPhase — enum: будівлі руху фази.</summary>
    public enum BuildingMotionPhase
    {
        /// <summary>Варіант PlacementEmerge.</summary>
        PlacementEmerge,
        /// <summary>Варіант Settle.</summary>
        Settle,
        /// <summary>Варіант RelocationStart.</summary>
        RelocationStart,
        /// <summary>Варіант RelocationSettle.</summary>
        RelocationSettle,
        /// <summary>Варіант ConstructionComplete.</summary>
        ConstructionComplete,
        /// <summary>Варіант DemolitionStart.</summary>
        DemolitionStart,
        /// <summary>Варіант DemolitionComplete.</summary>
        DemolitionComplete,
        /// <summary>Варіант BlockedShake.</summary>
        BlockedShake
    }

    /// <summary>BuildingMotionPhaseSignal — struct: будівлі руху фази сигнал.</summary>
    public struct BuildingMotionPhaseSignal
    {
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>фази — BuildingMotionPhase.</summary>
        public BuildingMotionPhase Phase;
    }

    // Викликається, коли юніт перемістився
    /// <summary>UnitMovedSignal — struct: юніта Moved сигнал.</summary>
    public struct UnitMovedSignal
    {
        /// <summary>юніта ID — string.</summary>
        public string UnitId;
        /// <summary>нового позицію — UnityEngine.Vector2Int.</summary>
        public UnityEngine.Vector2Int NewPosition;
        /// <summary>вартості — float.</summary>
        public float Cost;
        /// <summary>джерела Faction ID — string.</summary>
        public string SourceFactionId;

        /// <summary>Чи спільної Occupancy — AllowSharedOccupancy.</summary>
        public bool AllowSharedOccupancy;
    }

    // Викликається при смерті/видаленні
    /// <summary>UnitDestroyedSignal — struct: юніта Destroyed сигнал.</summary>
    public struct UnitDestroyedSignal
    {
        /// <summary>юніта ID — string.</summary>
        public string UnitId;
    }

    /// <summary>InterruptMovementSignal — struct: Interrupt руху сигнал.</summary>
    public struct InterruptMovementSignal
    {
        /// <summary>юніта ID — string.</summary>
        public string UnitId;
    }

    /// <summary>LocalUnitSelectionChangedSignal — struct: локального юніта вибору Changed сигнал.</summary>
    public struct LocalUnitSelectionChangedSignal
    {
        /// <summary>юніта ID — string.</summary>
        public string UnitId;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>Чи вибраного — IsSelected.</summary>
        public bool IsSelected;
    }

    /// <summary>
    /// Надсилається TileInteractionService, коли гравець наказує юніту рухатись до тайлу.
    /// MultiplayerAuthorityService перехоплює і або виконує MoveUnitAsync локально (хост/офлайн),
    /// або надсилає запит до хоста (клієнт).
    /// </summary>
    public struct MoveUnitRequestSignal
    {
        /// <summary>юніта ID — string.</summary>
        public string UnitId;
        /// <summary>цілі позицію — Vector2Int.</summary>
        public Vector2Int TargetPosition;
        /// <summary>Requester власника ID — string.</summary>
        public string RequesterOwnerId;
    }

    /// <summary>
    /// Надсилається коли гравець наказує групі юнітів рухатись до тайлу.
    /// MultiplayerAuthorityService маршрутизує так само, як MoveUnitRequestSignal.
    /// </summary>
    public struct MoveGroupRequestSignal
    {
        /// <summary>групи ID — string.</summary>
        public string GroupId;
        /// <summary>цілі позицію — Vector2Int.</summary>
        public Vector2Int TargetPosition;
        /// <summary>Requester власника ID — string.</summary>
        public string RequesterOwnerId;
    }

    /// <summary>
    /// Надсилається UnitMovementService, коли канонічний рух юніта відхилено.
    /// UI підписується, щоб показати причину гравцю.
    /// </summary>
    public struct UnitMoveRejectedSignal
    {
        /// <summary>юніта ID — string.</summary>
        public string UnitId;
        /// <summary>цілі позицію — Vector2Int.</summary>
        public Vector2Int TargetPosition;
        /// <summary>причини — string.</summary>
        public string Reason;
    }

    /// <summary>Тип зміни тайла.</summary>
    public enum UnitGroupChangeKind : byte
    {
        /// <summary>Варіант Formed.</summary>
        Formed = 0,
        /// <summary>Варіант MembersChanged.</summary>
        MembersChanged = 1,
        /// <summary>Варіант Disbanded.</summary>
        Disbanded = 2,
    }

    /// <summary>
    /// Надсилається IUnitGroupService після кожної зміни складу групи.
    /// MemberUnitIds містить актуальний впорядкований склад (порожній для Disbanded).
    /// </summary>
    public struct UnitGroupChangedSignal
    {
        /// <summary>групи ID — string.</summary>
        public string GroupId;
        /// <summary>власника ID — string.</summary>
        public string OwnerId;
        /// <summary>Тип зміни тайла.</summary>
        public UnitGroupChangeKind Kind;
        /// <summary>Member юніта Ids — string[].</summary>
        public string[] MemberUnitIds;
    }

    /// <summary>
    /// Надсилається, коли команду групи відхилено авторитетною стороною
    /// (локально або хостом у мультиплеєрі). UI показує причину гравцю.
    /// </summary>
    public struct UnitGroupCommandRejectedSignal
    {
        /// <summary>причини — string.</summary>
        public string Reason;
    }

    /// <summary>
    /// Надсилається MapVisualInstantiator після спавну статичного обʼєкта карти (гора, річка, ліс…)
    /// </summary>
    public struct OnMapObjectSpawnedSignal
    {
        /// <summary>обʼєкта ID — string.</summary>
        public string ObjectId;        // TileTypeId, наприклад "river", "mountain"
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
    }

    /// <summary>
    /// Надсилається реалізацією IObjectsMapService після зміни карти обʼєктів.
    /// </summary>
    public struct OnObjectsMapChangedSignal
    {
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>ID юніта-окупанта клітинки.</summary>
        public string OccupantId;      // null якщо тайл звільнено
    }

    /// <summary>GridTileChangedSignal — struct: сітки тайла Changed сигнал.</summary>
    public struct GridTileChangedSignal
    {
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>Previous тайла ID — string.</summary>
        public string PreviousTileId;
        /// <summary>поточного тайла ID — string.</summary>
        public string CurrentTileId;
    }

    /// <summary>FogStateChangedSignal — struct: туману стану Changed сигнал.</summary>
    public struct FogStateChangedSignal
    {
        /// <summary>Changed тайлів кількості — int.</summary>
        public int ChangedTilesCount;
    }

    /// <summary>WorldBuiltSignal — struct: світу Built сигнал.</summary>
    public struct WorldBuiltSignal
    {
    }

    /// <summary>WorldGeneratedDataSource — enum: світу Generated даних джерела.</summary>
    public enum WorldGeneratedDataSource
    {
        /// <summary>Варіант Unknown.</summary>
        Unknown = 0,
        /// <summary>Варіант GeneratedHost.</summary>
        GeneratedHost = 1,
        /// <summary>Варіант LoadedSave.</summary>
        LoadedSave = 2,
        /// <summary>Варіант DirectGameplayTest.</summary>
        DirectGameplayTest = 3,
    }

    /// <summary>WorldSpawnPositionsSource — enum: світу спавну Positions джерела.</summary>
    public enum WorldSpawnPositionsSource
    {
        /// <summary>Варіант Unknown.</summary>
        Unknown = 0,
        /// <summary>Варіант GeneratedHost.</summary>
        GeneratedHost = 1,
        /// <summary>Варіант DirectGameplayTest.</summary>
        DirectGameplayTest = 2,
        /// <summary>Варіант MultiplayerSync.</summary>
        MultiplayerSync = 3,
        /// <summary>Варіант SavedGame.</summary>
        SavedGame = 4,
    }

    /// <summary>
    /// Надсилається після завершення побудови світу і містить згенеровані мапи.
    /// </summary>
    public struct WorldGeneratedDataSignal
    {
        /// <summary>запуску Sequence — long.</summary>
        public long StartupSequence;
        /// <summary>знімка ревізії — int.</summary>
        public int SnapshotRevision;
        /// <summary>запуску сесії ID — string.</summary>
        public string StartupSessionId;
        /// <summary>джерела — WorldGeneratedDataSource.</summary>
        public WorldGeneratedDataSource Source;
        /// <summary>опублікованої кадру — int.</summary>
        public int PublishedFrame;
        /// <summary>опублікованої At Utc Ticks — long.</summary>
        public long PublishedAtUtcTicks;
        /// <summary>ширини — int.</summary>
        public int Width;
        /// <summary>висоти — int.</summary>
        public int Height;
        /// <summary>сітки Topology — int.</summary>
        public int GridTopology;
        /// <summary>проекції режим — int.</summary>
        public int ProjectionMode;
        /// <summary>рендер режим — int.</summary>
        public int RenderMode;
        /// <summary>Neighborhood режим — int.</summary>
        public int NeighborhoodMode;
        /// <summary>клітинки розміру — float.</summary>
        public float CellSize;
        /// <summary>Чи карти світу межі — HasMapWorldBounds.</summary>
        public bool HasMapWorldBounds;
        /// <summary>карти світу межі центру — Vector3.</summary>
        public Vector3 MapWorldBoundsCenter;
        /// <summary>карти світу межі розміру — Vector3.</summary>
        public Vector3 MapWorldBoundsSize;
        /// <summary>тайла карти — string[,].</summary>
        public string[,] TileMap;
        /// <summary>обʼєкта карти — string[,].</summary>
        public string[,] ObjectMap;
        /// <summary>висоти карти — float[,].</summary>
        public float[,] HeightMap;
        /// <summary>терейну рівня карти — int[,].</summary>
        public int[,] TerrainLevelMap;
        /// <summary>Авторитетна rendered surface height map (meters) — float[,].</summary>
        public float[,] SurfaceHeightMap;
        /// <summary>спавну Hints — Vector2Int[].</summary>
        public Vector2Int[] SpawnHints;
    }

    /// <summary>SpawnPositionAssignment — struct: спавну позицію Assignment.</summary>
    public struct SpawnPositionAssignment
    {
        /// <summary>слота індексу — int.</summary>
        public int SlotIndex;
        /// <summary>ID учасника події.</summary>
        public string ParticipantId;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
    }

    /// <summary>WorldSpawnPositionsSignal — struct: світу спавну Positions сигнал.</summary>
    public struct WorldSpawnPositionsSignal
    {
        /// <summary>запуску Sequence — long.</summary>
        public long StartupSequence;
        /// <summary>знімка ревізії — int.</summary>
        public int SnapshotRevision;
        /// <summary>запуску сесії ID — string.</summary>
        public string StartupSessionId;
        /// <summary>джерела — WorldSpawnPositionsSource.</summary>
        public WorldSpawnPositionsSource Source;
        /// <summary>опублікованої кадру — int.</summary>
        public int PublishedFrame;
        /// <summary>опублікованої At Utc Ticks — long.</summary>
        public long PublishedAtUtcTicks;
        /// <summary>Призначення юнітів на зміни.</summary>
        public SpawnPositionAssignment[] Assignments;
    }
}
