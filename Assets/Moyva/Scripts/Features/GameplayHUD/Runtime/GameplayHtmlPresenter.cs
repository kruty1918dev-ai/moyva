using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Notifications.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using UnityHTML.Runtime;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplayHudReadModel
    {
        private readonly ITurnService _turns;
        private readonly ITurnHistoryQuery _turnHistory;
        private readonly IEconomyRuntimeApi _economy;
        private readonly IConstructionSessionCommands _construction;
        private readonly IConstructionBootstrapQuery _bootstrap;
        private readonly IConstructionPortfolioQuery _portfolio;
        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionSelectionAvailabilityQuery _availability;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _unitOwnership;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly ILocalGameplayRoleResolver _roleResolver;
        private readonly Dictionary<int, Sprite> _prefabSpriteCache = new();

        private WorldInfoSelectionKind _selectionKind;
        private string _selectionId = string.Empty;
        private Vector2Int _selectionPosition;
        private BuildingPreviewState _lastPreviewState;
        private string _lastPreviewMessage = string.Empty;

        public bool HasSelection => _selectionKind != WorldInfoSelectionKind.None;

        public GameplayHudReadModel(
            ITurnService turns,
            IEconomyRuntimeApi economy,
            IConstructionSessionCommands construction,
            IBuildingRegistry buildings,
            [InjectOptional] ITurnHistoryQuery turnHistory = null,
            [InjectOptional] IConstructionBootstrapQuery bootstrap = null,
            [InjectOptional] IConstructionPortfolioQuery portfolio = null,
            [InjectOptional] IUnitService units = null,
            [InjectOptional] IUnitOwnershipQuery unitOwnership = null,
            [InjectOptional] IUnitRecruitmentService recruitment = null,
            [InjectOptional] IUnitClassConfig unitConfigs = null,
            [InjectOptional] ILocalGameplayRoleResolver roleResolver = null)
        {
            _turns = turns;
            _economy = economy;
            _construction = construction;
            _buildings = buildings;
            _availability = construction as IConstructionSelectionAvailabilityQuery;
            _turnHistory = turnHistory;
            _bootstrap = bootstrap;
            _portfolio = portfolio;
            _units = units;
            _unitOwnership = unitOwnership;
            _recruitment = recruitment;
            _unitConfigs = unitConfigs;
            _roleResolver = roleResolver;
        }

        public void SetSelection(WorldInfoSelectionChangedSignal signal)
        {
            _selectionKind = signal.Kind;
            _selectionId = signal.ObjectId ?? string.Empty;
            _selectionPosition = signal.Position;
        }

        public bool SetPreview(BuildingPreviewChangedSignal signal)
        {
            BuildingPreviewState previousState = _lastPreviewState;
            string previousMessage = _lastPreviewMessage;
            _lastPreviewState = signal.PreviewState;
            _lastPreviewMessage = signal.PreviewState switch
            {
                BuildingPreviewState.Valid => "Valid location. Ready to confirm.",
                BuildingPreviewState.Blocked => "This location is blocked.",
                BuildingPreviewState.Unaffordable => "The kingdom cannot afford this placement.",
                _ => "Choose a location on the map.",
            };
            return previousState != _lastPreviewState
                || !string.Equals(previousMessage, _lastPreviewMessage, StringComparison.Ordinal);
        }

        public GameplayHtmlSnapshot Capture(GameplayHtmlState state)
        {
            string ownerId = ResolveOwnerId();
            var snapshot = new GameplayHtmlSnapshot
            {
                OwnerId = ownerId,
                KingdomName = ResolveKingdomName(ownerId),
                Round = _turns?.Round ?? 1,
                GlobalTurn = _turns?.GlobalTurn ?? 1,
                ActionsThisTurn = _turns?.ActionsThisTurn ?? 0,
                ActiveOwnerId = _turns?.ActiveOwnerId ?? ownerId,
                IsLocalTurn = _turns == null
                    || string.Equals(_turns.ActiveOwnerId, ownerId, StringComparison.Ordinal),
                TurnUiEnabled = IsTurnUiEnabled(),
                SelectedBuildingId = _construction?.GetSelectedBuildingId() ?? string.Empty,
                SelectionKind = _selectionKind == WorldInfoSelectionKind.None
                    ? string.Empty
                    : _selectionKind.ToString(),
                SelectionId = _selectionId,
                SelectionPosition = _selectionPosition,
            };

            IReadOnlyDictionary<Vector2Int, string> pending =
                _construction?.GetPendingPlacements();
            snapshot.PendingPlacementCount = pending?.Count ?? 0;
            snapshot.PlacementValid = _lastPreviewState == BuildingPreviewState.Valid;
            snapshot.PlacementStatus = ResolvePlacementStatus(pending);
            string castleId = string.Empty;
            snapshot.RequiresFirstCastle = _bootstrap != null
                && _bootstrap.RequiresInitialCastle(ownerId, out castleId);
            if (snapshot.RequiresFirstCastle)
                snapshot.CastleBuildingId = castleId;

            snapshot.Resources = CaptureResources(ownerId);
            if (state?.OpenPanelId == GameplayHtmlPanel.Construction)
                snapshot.BuildingOptions = CaptureBuildingOptions(ownerId);
            if (state?.OpenPanelId == GameplayHtmlPanel.Kingdom)
            {
                snapshot.BuildingGroups = CaptureBuildingGroups(ownerId, out int buildingCount);
                snapshot.BuildingCount = buildingCount;
                snapshot.UnitGroups = CaptureUnitGroups(ownerId, out int unitCount);
                snapshot.UnitCount = unitCount;
                snapshot.Warehouses = CaptureWarehouses(ownerId);
                snapshot.Settlements = CaptureSettlements(ownerId, out int settlements, out int population);
                snapshot.SettlementCount = settlements;
                snapshot.Population = population;
                snapshot.TurnHistory = CaptureTurnHistory();
            }
            CaptureRecruitment(snapshot, ownerId);
            CaptureSelectionDetails(snapshot, ownerId);
            return snapshot;
        }

        public bool IsTurnUiEnabled()
        {
            GameLaunchContext.EnsureNotExpired();
            if (GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest
                || GameLaunchContext.Source == GameLaunchSource.DirectGameplayTest)
            {
                return false;
            }

            if (GameLaunchContext.HasWorldSettings && GameLaunchContext.MaxPlayers <= 1)
                return false;

            IReadOnlyList<TurnFaction> factions = _turns?.Factions;
            return factions == null || factions.Count > 1;
        }

        private string ResolveOwnerId()
        {
            string ownerId = _turns?.LocalOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                ownerId = _roleResolver?.Resolve().PlayerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                ownerId = _construction?.GetActiveOwner();
            if (string.IsNullOrWhiteSpace(ownerId))
                ownerId = _turns?.ActiveOwnerId;
            return string.IsNullOrWhiteSpace(ownerId) ? "player_0" : ownerId.Trim();
        }

        private string ResolvePlacementStatus(IReadOnlyDictionary<Vector2Int, string> pending)
        {
            if (pending != null)
            {
                foreach (Vector2Int position in pending.Keys)
                {
                    if (_construction.TryGetPendingPlacementStatus(position, out var status))
                    {
                        if (!string.IsNullOrWhiteSpace(status.ErrorMessage))
                            return status.ErrorMessage;
                        if (!status.IsAffordable)
                            return "The kingdom cannot afford this placement.";
                        return "Valid location. Ready to confirm.";
                    }
                }
            }

            string actionMessage = _construction?.GetLastActionMessage();
            if (!string.IsNullOrWhiteSpace(actionMessage))
                return actionMessage;
            return string.IsNullOrWhiteSpace(_lastPreviewMessage)
                ? "Choose a location on the map."
                : _lastPreviewMessage;
        }

        private GameplayResourceSnapshot[] CaptureResources(string ownerId)
        {
            Dictionary<string, float> totals = _economy?.GetOwnerResourceTotals(ownerId);
            if (totals == null || totals.Count == 0)
                return Array.Empty<GameplayResourceSnapshot>();

            return totals
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new GameplayResourceSnapshot(pair.Key, pair.Value))
                .ToArray();
        }

        private GameplayBuildingOptionSnapshot[] CaptureBuildingOptions(string ownerId)
        {
            BuildingDefinition[] definitions = _buildings?.GetAll();
            if (definitions == null)
                return Array.Empty<GameplayBuildingOptionSnapshot>();

            return definitions
                .Where(definition => definition != null && !string.IsNullOrWhiteSpace(definition.Id))
                .OrderBy(definition => definition.Category)
                .ThenBy(definition => definition.DisplayName, StringComparer.Ordinal)
                .Select(definition =>
                {
                    ConstructionSelectionAvailabilityResult availability =
                        _availability?.EvaluateSelectionAvailability(
                            definition.Id,
                            ownerId,
                            preferredFundingPosition: null,
                            includePendingPlacements: true)
                        ?? new ConstructionSelectionAvailabilityResult(true, true, false);
                    return new GameplayBuildingOptionSnapshot(
                        definition.Id,
                        definition.DisplayName,
                        definition.Category.ToString(),
                        definition.Description,
                        FormatCost(definition.ConstructionCost),
                        ResolveBuildingIcon(definition),
                        availability.CanSelect,
                        string.IsNullOrWhiteSpace(availability.Reason)
                            ? "Unavailable under current construction rules."
                            : availability.Reason);
                })
                .ToArray();
        }

        public UiActionResult TryRecruitSelected(string unitTypeId)
        {
            if (_selectionKind != WorldInfoSelectionKind.Building)
                return UiActionResult.Rejected(UiActionReason.WrongContext, "Select a recruitment building first.");
            if (string.IsNullOrWhiteSpace(unitTypeId))
                return UiActionResult.Rejected(UiActionReason.NoSelection, "Select a unit to recruit.");
            if (_recruitment == null)
                return UiActionResult.Rejected(UiActionReason.ActionUnavailable, "Recruitment service is unavailable.");

            string ownerId = ResolveOwnerId();
            if (_turns != null
                && !string.Equals(_turns.ActiveOwnerId, ownerId, StringComparison.Ordinal))
            {
                return UiActionResult.Rejected(UiActionReason.ActionUnavailable, "Recruitment is available only during your turn.");
            }

            if (!_recruitment.TryEnqueue(ownerId, _selectionPosition, unitTypeId.Trim(), out string reason))
            {
                return UiActionResult.Rejected(
                    UiActionReason.ActionUnavailable,
                    string.IsNullOrWhiteSpace(reason) ? "Recruitment could not be started." : reason);
            }

            return UiActionResult.Performed();
        }

        private void CaptureRecruitment(GameplayHtmlSnapshot snapshot, string ownerId)
        {
            if (_selectionKind != WorldInfoSelectionKind.Building
                || string.IsNullOrWhiteSpace(_selectionId))
            {
                return;
            }

            IConstructionBuildingOwnershipQuery ownership =
                _construction as IConstructionBuildingOwnershipQuery;
            snapshot.SelectionOwnedByLocalPlayer = ownership != null
                && ownership.TryGetPlacedBuildingOwner(_selectionPosition, out string buildingOwner)
                && string.Equals(buildingOwner?.Trim(), ownerId, StringComparison.Ordinal);
            snapshot.SelectionOperational = _construction is not IConstructionLifecycle lifecycle
                || lifecycle.IsOperational(_selectionPosition);

            BuildingDefinition definition = _buildings?.GetById(_selectionId);
            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out UnitRecruitmentBuildingModule module))
            {
                return;
            }

            snapshot.SupportsRecruitment = true;
            snapshot.RecruitmentQueueCapacity = Math.Max(1, module.QueueCapacity);
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                _recruitment?.GetQueue(ownerId, _selectionPosition);
            int queueCount = queue?.Count ?? 0;
            bool queueFull = queueCount >= snapshot.RecruitmentQueueCapacity;
            bool canRecruit = snapshot.CanIssueLocalCommands
                && snapshot.SelectionOwnedByLocalPlayer
                && snapshot.SelectionOperational
                && !queueFull;
            string unavailableReason = ResolveRecruitmentUnavailableReason(
                snapshot,
                queueFull);

            if (module.Recipes != null)
            {
                snapshot.RecruitmentRecipes = module.Recipes
                    .Where(recipe => recipe != null && !string.IsNullOrWhiteSpace(recipe.UnitTypeId))
                    .Select(recipe =>
                    {
                        string unitTypeId = recipe.UnitTypeId.Trim();
                        UnitClassConfig config = _unitConfigs?.GetConfig(unitTypeId);
                        return new GameplayRecruitmentRecipeSnapshot(
                            unitTypeId,
                            string.IsNullOrWhiteSpace(config?.DisplayName) ? Display(unitTypeId) : config.DisplayName,
                            config?.Role.ToString() ?? "Unit",
                            config?.CombatType.ToString() ?? string.Empty,
                            FormatResourceCost(recipe.Costs),
                            recipe.TrainingTurns,
                            config?.HitPoints ?? 0,
                            config?.MovementPointsPerTurn ?? 0f,
                            config?.ResolveCustomSprite(),
                            canRecruit,
                            unavailableReason);
                    })
                    .ToArray();
            }

            if (queue == null || queue.Count == 0)
                return;
            var queueSnapshots = new GameplayRecruitmentQueueSnapshot[queue.Count];
            for (int index = 0; index < queue.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot item = queue[index];
                UnitClassConfig config = _unitConfigs?.GetConfig(item.UnitTypeId);
                queueSnapshots[index] = new GameplayRecruitmentQueueSnapshot(
                    item.QueueId,
                    item.UnitTypeId,
                    string.IsNullOrWhiteSpace(config?.DisplayName) ? Display(item.UnitTypeId) : config.DisplayName,
                    item.CompletedTurns,
                    item.TrainingTurns,
                    item.IsReady);
            }
            snapshot.RecruitmentQueue = queueSnapshots;
        }

        private void CaptureSelectionDetails(GameplayHtmlSnapshot snapshot, string ownerId)
        {
            if (_selectionKind == WorldInfoSelectionKind.None)
                return;

            var facts = new List<GameplayFactSnapshot>();
            switch (_selectionKind)
            {
                case WorldInfoSelectionKind.Building:
                {
                    BuildingDefinition definition = _buildings?.GetById(_selectionId);
                    snapshot.SelectionTitle = string.IsNullOrWhiteSpace(definition?.DisplayName)
                        ? Display(_selectionId)
                        : definition.DisplayName.Trim();
                    snapshot.SelectionSubtitle = definition?.Description ?? string.Empty;
                    facts.Add(new GameplayFactSnapshot(
                        "Ownership",
                        snapshot.SelectionOwnedByLocalPlayer ? "Your kingdom" : "Another kingdom",
                        "Command authority"));
                    if (_construction is IConstructionLifecycle lifecycle
                        && lifecycle.TryGetProgress(_selectionPosition, out int completed, out int required)
                        && completed < required)
                    {
                        facts.Add(new GameplayFactSnapshot(
                            "Status",
                            $"Under construction {completed}/{required}",
                            $"{Math.Max(0, required - completed)} turns remaining"));
                    }
                    else
                    {
                        facts.Add(new GameplayFactSnapshot(
                            "Status",
                            snapshot.SelectionOperational ? "Operational" : "Unavailable",
                            "Current building state"));
                    }
                    break;
                }
                case WorldInfoSelectionKind.Unit:
                {
                    string typeId = _units?.GetUnitTypeId(_selectionId);
                    UnitClassConfig config = string.IsNullOrWhiteSpace(typeId)
                        ? null
                        : _unitConfigs?.GetConfig(typeId);
                    snapshot.SelectionTitle = string.IsNullOrWhiteSpace(config?.DisplayName)
                        ? Display(typeId ?? _selectionId)
                        : config.DisplayName.Trim();
                    string unitOwner = _unitOwnership?.GetUnitOwnerId(_selectionId);
                    snapshot.SelectionOwnedByLocalPlayer = string.Equals(
                        unitOwner?.Trim(),
                        ownerId,
                        StringComparison.Ordinal);
                    snapshot.SelectionSubtitle = config == null
                        ? "Selected unit"
                        : $"{config.Role} / {config.CombatType}";
                    facts.Add(new GameplayFactSnapshot(
                        "Ownership",
                        snapshot.SelectionOwnedByLocalPlayer ? "Your kingdom" : "Another kingdom",
                        "Unit command authority"));
                    if (_units != null)
                    {
                        facts.Add(new GameplayFactSnapshot(
                            "Stamina",
                            $"{_units.GetStamina(_selectionId):0.#} / {config?.BaseStamina ?? 0f:0.#}",
                            "Movement resource"));
                    }
                    if (config != null)
                    {
                        facts.Add(new GameplayFactSnapshot("Hit points", config.HitPoints.ToString(CultureInfo.InvariantCulture), "Base unit profile"));
                        facts.Add(new GameplayFactSnapshot("Movement", config.MovementPointsPerTurn.ToString("0.#", CultureInfo.InvariantCulture), "Points per turn"));
                        facts.Add(new GameplayFactSnapshot("Attack range", config.AttackRange.ToString(CultureInfo.InvariantCulture), "Grid tiles"));
                    }
                    break;
                }
                default:
                    snapshot.SelectionTitle = Display(_selectionId);
                    snapshot.SelectionSubtitle = "Selected map object";
                    break;
            }

            snapshot.SelectionFacts = facts.ToArray();
        }

        private static string ResolveRecruitmentUnavailableReason(
            GameplayHtmlSnapshot snapshot,
            bool queueFull)
        {
            if (!snapshot.SelectionOwnedByLocalPlayer)
                return "Recruitment is available only in your own building.";
            if (!snapshot.SelectionOperational)
                return "This building is still under construction.";
            if (!snapshot.CanIssueLocalCommands)
                return "Recruitment is available only during your turn.";
            if (queueFull)
                return $"Recruitment queue is full ({snapshot.RecruitmentQueueCapacity}/{snapshot.RecruitmentQueueCapacity}).";
            return string.Empty;
        }

        private Sprite ResolveBuildingIcon(BuildingDefinition building)
        {
            if (building == null)
                return null;

            if (building.Icon != null)
                return building.Icon;

            GameObject fallbackPrefab = building.ResolvePreviewPrefab();
            if (building.Category == BuildingCategory.Walls)
            {
                var collection = _buildings?.GetWallCollectionByBuildingId(building.Id);
                if (collection != null)
                {
                    if (collection.IsGate(building.Id))
                    {
                        Sprite gateSprite = ExtractSpriteFromPrefabCached(collection.GatePrefab)
                            ?? ExtractSpriteFromPrefabCached(fallbackPrefab)
                            ?? ExtractSpriteFromPrefabCached(collection.HorizontalPrefab);
                        if (gateSprite != null)
                            return gateSprite;
                    }
                    else
                    {
                        Sprite wallSprite = ExtractSpriteFromPrefabCached(collection.HorizontalPrefab);
                        if (wallSprite != null)
                            return wallSprite;
                    }
                }
            }

            return ExtractSpriteFromPrefabCached(fallbackPrefab);
        }

        private Sprite ExtractSpriteFromPrefabCached(GameObject prefab)
        {
            if (prefab == null)
                return null;

            int prefabId = prefab.GetInstanceID();
            if (_prefabSpriteCache.TryGetValue(prefabId, out Sprite cached))
                return cached;

            Sprite sprite = ExtractSpriteFromPrefab(prefab);
            _prefabSpriteCache[prefabId] = sprite;
            return sprite;
        }

        private static Sprite ExtractSpriteFromPrefab(GameObject prefab)
        {
            if (prefab == null)
                return null;

            var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            for (int index = 0; index < renderers.Length; index++)
            {
                SpriteRenderer renderer = renderers[index];
                if (renderer != null && renderer.sprite != null)
                    return renderer.sprite;
            }

            return null;
        }

        private GameplayGroupSnapshot[] CaptureBuildingGroups(string ownerId, out int count)
        {
            var grouped = new Dictionary<string, int>(StringComparer.Ordinal);
            IReadOnlyList<ConstructionSavedPlacement> placements = _portfolio?.GetOwnerPlacements(ownerId);
            if (placements != null)
            {
                for (int index = 0; index < placements.Count; index++)
                {
                    ConstructionSavedPlacement placement = placements[index];
                    string id = placement.BuildingId ?? "building";
                    grouped[id] = grouped.TryGetValue(id, out int current) ? current + 1 : 1;
                }
            }

            count = grouped.Values.Sum();
            return grouped
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new GameplayGroupSnapshot(
                    pair.Key,
                    _buildings?.GetById(pair.Key)?.DisplayName ?? Display(pair.Key),
                    pair.Value,
                    "Kingdom total"))
                .ToArray();
        }

        private GameplayGroupSnapshot[] CaptureUnitGroups(string ownerId, out int count)
        {
            var grouped = new Dictionary<string, int>(StringComparer.Ordinal);
            IReadOnlyCollection<string> unitIds = _units?.GetAllUnitIds();
            if (unitIds != null)
            {
                foreach (string unitId in unitIds)
                {
                    if (_unitOwnership != null
                        && !string.Equals(_unitOwnership.GetUnitOwnerId(unitId), ownerId, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string typeId = _units.GetUnitTypeId(unitId) ?? "unit";
                    grouped[typeId] = grouped.TryGetValue(typeId, out int current) ? current + 1 : 1;
                }
            }

            count = grouped.Values.Sum();
            return grouped
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new GameplayGroupSnapshot(pair.Key, Display(pair.Key), pair.Value, "Active units"))
                .ToArray();
        }

        private GameplayWarehouseViewSnapshot[] CaptureWarehouses(string ownerId)
        {
            IReadOnlyList<EconomyWarehouseSnapshot> warehouses =
                _economy?.GetOwnerWarehouseSnapshots(ownerId);
            if (warehouses == null)
                return Array.Empty<GameplayWarehouseViewSnapshot>();

            var result = new GameplayWarehouseViewSnapshot[warehouses.Count];
            for (int index = 0; index < warehouses.Count; index++)
            {
                EconomyWarehouseSnapshot warehouse = warehouses[index];
                GameplayResourceSnapshot[] resources = warehouse.Resources
                    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .Select(pair => new GameplayResourceSnapshot(pair.Key, pair.Value))
                    .ToArray();
                result[index] = new GameplayWarehouseViewSnapshot(
                    warehouse.WarehouseKey,
                    warehouse.BuildingId,
                    warehouse.SettlementName,
                    warehouse.GridPosition,
                    warehouse.UsedCapacity,
                    warehouse.Capacity,
                    resources);
            }
            return result;
        }

        private GameplaySettlementViewSnapshot[] CaptureSettlements(
            string ownerId,
            out int count,
            out int population)
        {
            IReadOnlyList<EconomySettlementSnapshot> settlements =
                _economy?.GetOwnerSettlementSnapshots(ownerId);
            count = settlements?.Count ?? 0;
            population = 0;
            if (settlements == null)
                return Array.Empty<GameplaySettlementViewSnapshot>();
            var result = new GameplaySettlementViewSnapshot[settlements.Count];
            for (int index = 0; index < settlements.Count; index++)
            {
                EconomySettlementSnapshot settlement = settlements[index];
                population += settlement.Population;
                result[index] = new GameplaySettlementViewSnapshot(
                    settlement.SettlementId,
                    settlement.Name,
                    settlement.Population,
                    settlement.BuildingCount,
                    settlement.Resources
                        .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                        .Select(pair => new GameplayResourceSnapshot(pair.Key, pair.Value))
                        .ToArray());
            }
            return result;
        }

        private GameplayTurnHistoryViewSnapshot[] CaptureTurnHistory()
        {
            IReadOnlyList<TurnParticipantHistorySnapshot> history =
                _turnHistory?.GetParticipantHistory();
            if (history == null)
                return Array.Empty<GameplayTurnHistoryViewSnapshot>();
            var result = new GameplayTurnHistoryViewSnapshot[history.Count];
            for (int index = 0; index < history.Count; index++)
            {
                TurnParticipantHistorySnapshot item = history[index];
                result[index] = new GameplayTurnHistoryViewSnapshot(
                    item.OwnerId,
                    item.CompletedTurns,
                    item.IsActive,
                    item.IsLocal);
            }
            return result;
        }

        private static string FormatCost(IReadOnlyList<BuildingDefinition.BuildingConstructionCostEntry> cost)
        {
            if (cost == null || cost.Count == 0)
                return string.Empty;
            return string.Join(" / ", cost
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.ResourceId))
                .Select(entry => $"{DisplayResource(entry.ResourceId)} {entry.Amount}"));
        }

        private static string FormatResourceCost(IReadOnlyList<BuildingResourceAmount> cost)
        {
            if (cost == null || cost.Count == 0)
                return string.Empty;
            return string.Join(" / ", cost
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.ResourceId))
                .Select(entry => $"{DisplayResource(entry.ResourceId)} {entry.Amount}"));
        }

        private static string ResolveKingdomName(string ownerId)
            => string.Equals(ownerId, "player_0", StringComparison.OrdinalIgnoreCase)
                ? "Your Kingdom"
                : Display(ownerId);

        private static string Display(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                value.Replace('-', ' ').Replace('_', ' ').ToLowerInvariant());
        }

        private static string DisplayResource(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";

            string normalized = value.Trim();
            normalized = TrimSuffix(normalized, "-materials-resources");
            normalized = TrimSuffix(normalized, "_materials_resources");
            normalized = TrimSuffix(normalized, "-resources");
            normalized = TrimSuffix(normalized, "_resources");
            normalized = TrimSuffix(normalized, "-resource");
            normalized = TrimSuffix(normalized, "_resource");
            return Display(normalized);
        }

        private static string TrimSuffix(string value, string suffix)
        {
            return value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? value.Substring(0, value.Length - suffix.Length)
                : value;
        }
    }

    internal sealed class GameplayHtmlBridge
    {
        private readonly GameplayHtmlState _state;
        private readonly LazyInject<IUiActionRouter> _actions;
        private readonly IConstructionSessionCommands _construction;
        private readonly ILocalGameplayRoleResolver _roles;
        private readonly IGameplayCameraFocusService _cameraFocus;
        private readonly IExitMatchCoordinator _exit;

        public GameplayHtmlBridge(
            GameplayHtmlState state,
            LazyInject<IUiActionRouter> actions,
            IConstructionSessionCommands construction,
            ILocalGameplayRoleResolver roles,
            IGameplayCameraFocusService cameraFocus,
            IExitMatchCoordinator exit)
        {
            _state = state;
            _actions = actions;
            _construction = construction;
            _roles = roles;
            _cameraFocus = cameraFocus;
            _exit = exit;
        }

        public void Kingdom() => OpenOverlayPanel(GameplayHtmlPanel.Kingdom);
        public void Construction()
        {
            UiActionResult result = Execute(UiActionIds.Construction.Open, "GameplayHTML");
            if (result.Status == UiActionStatus.Performed)
                _state.OpenPanel(GameplayHtmlPanel.Construction);
            else
                SetResult(result, "Construction is unavailable.");
        }

        public void ClosePanel()
        {
            if (_state.OpenPanelId == GameplayHtmlPanel.Construction)
            {
                UiActionResult closeResult = Execute(UiActionIds.Construction.Close, "GameplayHTML");
                if (closeResult.Status == UiActionStatus.Rejected)
                {
                    SetResult(closeResult, string.Empty);
                    return;
                }
            }
            _state.ClosePanel();
        }

        public void SelectBuilding(object value)
        {
            string buildingId = value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(buildingId))
                return;
            UiActionResult result = _actions.Value.Execute(
                UiActionIds.Construction.SelectBuilding,
                UiActionSource.Button,
                "GameplayHTML",
                buildingId);
            SetResult(result, $"Selected {buildingId}.");
        }

        public void ConfirmPlacement()
        {
            int before = _construction?.GetPendingPlacements()?.Count ?? 0;
            UiActionResult action = Execute(UiActionIds.Construction.ConfirmPlacement, "GameplayHTML/Placement");
            int after = _construction?.GetPendingPlacements()?.Count ?? 0;
            int confirmed = Math.Max(0, before - after);

            if (confirmed > 0)
            {
                _state.SetFeedback($"Placed {confirmed} building(s).");
                return;
            }

            if (action.Status == UiActionStatus.Performed
                && _roles?.Resolve().Role == LocalGameplayRole.Client)
            {
                _state.SetFeedback("Placement request sent. Waiting for the host.");
                return;
            }

            string reason = action.Details;
            if (string.IsNullOrWhiteSpace(reason))
                reason = _construction?.GetLastActionMessage();
            if (string.IsNullOrWhiteSpace(reason))
                reason = before == 0
                    ? "Choose a location on the map before confirming."
                    : "The placement was rejected by gameplay rules.";
            _state.SetFeedback(reason);
        }

        public void CancelPlacement()
        {
            if (IsInitialCastleRequired())
            {
                _state.SetFeedback("Place your first castle before leaving construction mode.");
                return;
            }

            SetResult(
                Execute(UiActionIds.Construction.CancelPlacement, "GameplayHTML/Placement"),
                "Placement cancelled.");
        }
        public void RotatePlacement() => SetResult(
            Execute(UiActionIds.Construction.RotatePlacement, "GameplayHTML/Placement"),
            "Placement rotated.");
        public void UndoPlacement() => SetResult(
            Execute(UiActionIds.Construction.UndoPlacement, "GameplayHTML/Placement"),
            "Placement undone.");
        public void RedoPlacement() => SetResult(
            Execute(UiActionIds.Construction.RedoPlacement, "GameplayHTML/Placement"),
            "Placement restored.");
        public void ClearSelection() => SetResult(
            Execute(UiActionIds.ClearSelection, "GameplayHTML"),
            "Selection cleared.");
        public void EndTurn() => SetResult(
            Execute(UiActionIds.EndTurn, "GameplayHTML"),
            "Turn ended.");
        public void Pause() => Execute(UiActionIds.Pause.Open, "GameplayHTML");
        public void Resume() => Execute(UiActionIds.Pause.Close, "GameplayHTML");
        public void Notifications() => OpenOverlayPanel(GameplayHtmlPanel.Notifications);
        public void SetConstructionCategory(object value)
            => _state.SetConstructionCategory(value?.ToString());
        public void SetConstructionSearch(string value)
            => _state.SetConstructionSearch(value);
        public void PreviousConstructionPage() => _state.MoveConstructionPage(-1);
        public void NextConstructionPage() => _state.MoveConstructionPage(1);

        public void ShowOverview() => _state.SetDashboardTab(KingdomDashboardTab.Overview);
        public void ShowResources() => _state.SetDashboardTab(KingdomDashboardTab.Resources);
        public void ShowStorage() => _state.SetDashboardTab(KingdomDashboardTab.Storage);
        public void ShowBuildings() => _state.SetDashboardTab(KingdomDashboardTab.Buildings);
        public void ShowUnits() => _state.SetDashboardTab(KingdomDashboardTab.Units);
        public void ShowTurns() => _state.SetDashboardTab(KingdomDashboardTab.Turns);
        public void ShowSelectionDetails() => _state.SetSelectionTab(GameplaySelectionTab.Details);
        public void ShowRecruitment() => _state.SetSelectionTab(GameplaySelectionTab.Recruit);
        public void ShowRecruitmentQueue() => _state.SetSelectionTab(GameplaySelectionTab.Queue);

        public void Recruit(object value)
        {
            string unitTypeId = value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(unitTypeId))
                return;
            UiActionResult result = _actions.Value.Execute(
                UiActionIds.Recruitment.Enqueue,
                UiActionSource.Button,
                "GameplayHTML/Recruitment",
                unitTypeId);
            SetResult(result, $"{unitTypeId} added to the recruitment queue.");
        }

        public void FocusWarehouse(object x, object y, object targetId)
        {
            _cameraFocus?.FocusGridPosition(
                new Vector2Int(ToInt(x), ToInt(y)),
                targetId?.ToString());
            _state.ClosePanel();
        }

        public async void ExitToMenu()
        {
            if (_exit == null || _exit.IsExiting)
                return;
            try
            {
                ExitMatchResult result = await _exit.ExitToMenuAsync();
                if (!result.Succeeded && !result.Cancelled)
                    _state.SetFeedback(string.IsNullOrWhiteSpace(result.Error) ? "Could not leave the session." : result.Error);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameplayHTML] Exit failed: {exception.Message}");
                _state.SetFeedback(exception.Message);
            }
        }

        private void OpenOverlayPanel(GameplayHtmlPanel panel)
        {
            if (_state.OpenPanelId == GameplayHtmlPanel.Construction)
            {
                UiActionResult closeResult = Execute(UiActionIds.Construction.Close, "GameplayHTML");
                if (closeResult.Status == UiActionStatus.Rejected)
                {
                    SetResult(closeResult, string.Empty);
                    return;
                }
            }
            _state.OpenPanel(panel);
        }

        private UiActionResult Execute(UiActionId id, string context)
            => _actions?.Value?.Execute(id, UiActionSource.Button, context)
               ?? UiActionResult.Rejected(UiActionReason.ActionUnavailable, "UI action router is unavailable.");

        private bool IsInitialCastleRequired()
        {
            if (_construction is not IConstructionBootstrapQuery bootstrap)
                return false;

            string ownerId = _roles?.Resolve().PlayerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                ownerId = _construction.GetActiveOwner();

            return !string.IsNullOrWhiteSpace(ownerId)
                   && bootstrap.RequiresInitialCastle(ownerId.Trim(), out _);
        }

        private void SetResult(UiActionResult result, string success)
        {
            _state.SetFeedback(result.Status == UiActionStatus.Performed
                ? success
                : string.IsNullOrWhiteSpace(result.Details)
                    ? result.Reason.ToString()
                    : result.Details);
        }

        private static int ToInt(object value)
        {
            if (value == null)
                return 0;
            if (value is int integer)
                return integer;
            if (value is double number)
                return Mathf.RoundToInt((float)number);
            return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : 0;
        }
    }

    internal sealed class GameplayHtmlPresenter :
        IInitializable,
        ITickable,
        IDisposable,
        IUiActionHandler
    {
        private static readonly UiActionId[] HandledActions =
        {
            UiActionIds.Diagnostics.PanelClose,
            UiActionIds.Recruitment.Enqueue,
            UiActionIds.EndTurn,
            UiActionIds.ClearSelection,
        };

        private readonly SignalBus _signals;
        private readonly IUnityHtmlHost _host;
        private readonly GameplayHtmlState _state;
        private readonly GameplayHudReadModel _readModel;
        private readonly GameplayHtmlBridge _bridge;
        private readonly ITurnService _turns;
        private readonly IUiContextStack _contexts;
        private readonly GameplayHtmlAnchor[] _anchors;
        private readonly LazyInject<IUiActionRouter> _actions;
        private readonly IConstructionSessionCommands _construction;
        private readonly IConstructionBootstrapQuery _bootstrap;

        private GameplayHtmlAnchor _anchor;
        private IDisposable _panelContext;
        private IDisposable _initialCastleContext;
        private int _dirtyFrame = -1;
        private string _viewportClass = string.Empty;
        private bool _initialCastleModeActive;

        public GameplayHtmlPresenter(
            SignalBus signals,
            IUnityHtmlHost host,
            GameplayHtmlState state,
            GameplayHudReadModel readModel,
            LazyInject<IUiActionRouter> actions,
            IConstructionSessionCommands construction,
            ITurnService turns,
            [InjectOptional] IUiContextStack contexts = null,
            [InjectOptional] ILocalGameplayRoleResolver roles = null,
            [InjectOptional] IGameplayCameraFocusService cameraFocus = null,
            [InjectOptional] IExitMatchCoordinator exit = null,
            [InjectOptional] GameplayHtmlAnchor[] anchors = null)
        {
            _signals = signals;
            _host = host;
            _state = state;
            _readModel = readModel;
            _turns = turns;
            _contexts = contexts;
            _anchors = anchors ?? Array.Empty<GameplayHtmlAnchor>();
            _actions = actions;
            _construction = construction;
            _bootstrap = construction as IConstructionBootstrapQuery;
            _bridge = new GameplayHtmlBridge(state, actions, construction, roles, cameraFocus, exit);
        }

        public IReadOnlyCollection<UiActionId> ActionIds => HandledActions;

        public void Initialize()
        {
            _anchor = _anchors.FirstOrDefault(anchor => anchor != null);
            if (_anchor == null || _anchor.MountRoot == null || _anchor.CssAsset == null)
            {
                Debug.LogError("[GameplayHTML] GameplayHtmlAnchor or CSS is missing. Legacy HUD remains disabled.");
                return;
            }

            _anchor.StopEditorPreview();
            _anchor.PrepareForMount();
            _anchor.SetLegacyUiVisible(false);
            _state.Changed += MarkDirty;
            GameplayNotificationStream.Published += OnNotificationPublished;
            if (_turns != null)
                _turns.StateChanged += MarkDirty;
            SubscribeSignals();
            _panelContext = _contexts?.Push(new UiContextRegistration(
                "GameplayHTML/Panel",
                UiContextLayer.Panel,
                300,
                () => _state.OpenPanelId != GameplayHtmlPanel.None || _readModel.HasSelection,
                UiActionIds.Diagnostics.PanelClose,
                true));
            _initialCastleContext = _contexts?.Push(new UiContextRegistration(
                "InitialCastlePlacement",
                UiContextLayer.Modal,
                250,
                IsInitialCastleRequired,
                UiActionIds.Construction.CancelPlacement,
                blocksLowerHotkeys: true,
                allowedHotkeyActionIds: new[]
                {
                    UiActionIds.Construction.ConfirmPlacement,
                    UiActionIds.Construction.RotatePlacement,
                    UiActionIds.Construction.UndoPlacement,
                    UiActionIds.Construction.RedoPlacement,
                }));
            Render(true);
        }

        public void Tick()
        {
            if (_anchor == null)
                return;

            _state.ExpireFeedbackIfNeeded();
            EnsureInitialCastlePlacement();
            string viewport = _anchor.ViewportClass;
            if (!string.Equals(viewport, _viewportClass, StringComparison.Ordinal))
                _state.MarkDirty();

            if (_dirtyFrame == Time.frameCount)
                return;
            Render(false);
        }

        public void Dispose()
        {
            _state.Changed -= MarkDirty;
            GameplayNotificationStream.Published -= OnNotificationPublished;
            if (_turns != null)
                _turns.StateChanged -= MarkDirty;
            UnsubscribeSignals();
            _panelContext?.Dispose();
            _panelContext = null;
            _initialCastleContext?.Dispose();
            _initialCastleContext = null;
            _host?.Dispose();
        }

        public UiActionResult Execute(in UiActionRequest request)
        {
            if (request.ActionId == UiActionIds.Recruitment.Enqueue)
                return _readModel.TryRecruitSelected(request.TargetId);

            if (request.ActionId == UiActionIds.EndTurn)
            {
                if (!_readModel.IsTurnUiEnabled())
                    return UiActionResult.Performed();

                if (_turns == null || string.IsNullOrWhiteSpace(_turns.LocalOwnerId))
                    return UiActionResult.Rejected(UiActionReason.ActionUnavailable, "Local turn owner is unavailable.");
                return _turns.TryEndTurn(_turns.LocalOwnerId, out string reason)
                    ? UiActionResult.Performed()
                    : UiActionResult.Rejected(
                        UiActionReason.ActionUnavailable,
                        string.IsNullOrWhiteSpace(reason) ? "The turn cannot be ended yet." : reason);
            }

            if (request.ActionId == UiActionIds.ClearSelection)
            {
                _signals.Fire(new WorldInfoPanelClosedSignal());
                return UiActionResult.Performed();
            }

            if (request.ActionId != UiActionIds.Diagnostics.PanelClose
                || (_state.OpenPanelId == GameplayHtmlPanel.None && !_readModel.HasSelection))
            {
                return UiActionResult.Ignored(UiActionReason.WrongContext);
            }

            if (_state.OpenPanelId != GameplayHtmlPanel.None)
                _bridge.ClosePanel();
            else
                _signals.Fire(new WorldInfoPanelClosedSignal());
            return UiActionResult.Performed();
        }

        private void Render(bool force)
        {
            if (!force && !_state.ConsumeDirty())
                return;
            if (force)
                _state.ConsumeDirty();

            _dirtyFrame = Time.frameCount;
            _viewportClass = _anchor.ViewportClass;
            GameplayHtmlSnapshot snapshot = _readModel.Capture(_state);
            var globals = new Dictionary<string, object>
            {
                ["gameplay"] = _bridge,
            };
            if (_anchor.FontAsset != null)
                globals["moyvaFont"] = _anchor.FontAsset;
            AddIconGlobals(globals, snapshot);

            UnityHtmlMountResult result = _host.Mount(
                _anchor.MountRoot,
                new UnityHtmlDocument(
                    GameplayHtmlMarkup.Build(snapshot, _state, _viewportClass),
                    _anchor.CssAsset.text,
                    "Moyva Gameplay"),
                globals);
            _anchor.SyncInputShields(snapshot, _state);
            if (!result.Succeeded)
                Debug.LogError($"[GameplayHTML] Mount failed: {result.ErrorMessage}");
        }

        private static void AddIconGlobals(
            IDictionary<string, object> globals,
            GameplayHtmlSnapshot snapshot)
        {
            if (globals == null || snapshot == null)
                return;

            for (int index = 0; index < snapshot.BuildingOptions.Length; index++)
            {
                GameplayBuildingOptionSnapshot option = snapshot.BuildingOptions[index];
                if (option.Icon != null && !string.IsNullOrWhiteSpace(option.IconGlobalKey))
                    globals[option.IconGlobalKey] = option.Icon;
            }

            for (int index = 0; index < snapshot.RecruitmentRecipes.Length; index++)
            {
                GameplayRecruitmentRecipeSnapshot recipe = snapshot.RecruitmentRecipes[index];
                if (recipe.Icon != null && !string.IsNullOrWhiteSpace(recipe.IconGlobalKey))
                    globals[recipe.IconGlobalKey] = recipe.Icon;
            }
        }

        private bool IsInitialCastleRequired()
        {
            if (_turns == null
                || string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
                || _bootstrap == null)
            {
                return false;
            }

            return _bootstrap.RequiresInitialCastle(_turns.LocalOwnerId, out _);
        }

        private void EnsureInitialCastlePlacement()
        {
            if (_turns == null
                || _turns.Phase != TurnPhase.AwaitingInput
                || string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
                || !string.Equals(_turns.ActiveOwnerId, _turns.LocalOwnerId, StringComparison.Ordinal)
                || _bootstrap == null)
            {
                return;
            }

            bool required = _bootstrap.RequiresInitialCastle(
                _turns.LocalOwnerId,
                out string castleBuildingId);
            if (!required)
            {
                if (_initialCastleModeActive)
                {
                    _actions.Value.Execute(
                        UiActionIds.Construction.Close,
                        UiActionSource.Programmatic,
                        "GameplayHTML/Onboarding");
                    _initialCastleModeActive = false;
                }
                return;
            }

            if (!_initialCastleModeActive)
            {
                UiActionResult opened = _actions.Value.Execute(
                    UiActionIds.Construction.Open,
                    UiActionSource.Programmatic,
                    "GameplayHTML/Onboarding");
                _initialCastleModeActive = opened.Status == UiActionStatus.Performed;
            }

            if (_initialCastleModeActive
                && !string.Equals(
                    _construction.GetSelectedBuildingId(),
                    castleBuildingId,
                    StringComparison.Ordinal))
            {
                _actions.Value.Execute(
                    UiActionIds.Construction.SelectBuilding,
                    UiActionSource.Programmatic,
                    "GameplayHTML/Onboarding",
                    castleBuildingId);
            }
        }

        private void MarkDirty()
        {
            _dirtyFrame = Time.frameCount;
            if (!_state.Dirty)
                _state.MarkDirty();
        }

        private void SubscribeSignals()
        {
            _signals.Subscribe<BuildingPlacedSignal>(OnGameplayChanged);
            _signals.Subscribe<BuildingCancelledSignal>(OnGameplayChanged);
            _signals.Subscribe<BuildingPreviewChangedSignal>(OnPreviewChanged);
            _signals.Subscribe<BuildingSelectionChangedSignal>(OnGameplayChanged);
            _signals.Subscribe<BuildingDemolishedSignal>(OnGameplayChanged);
            _signals.Subscribe<EconomyTickCompletedSignal>(OnGameplayChanged);
            _signals.Subscribe<SettlementCreatedSignal>(OnGameplayChanged);
            _signals.Subscribe<SettlementDeactivatedSignal>(OnGameplayChanged);
            _signals.Subscribe<SettlementResourceChangedSignal>(OnGameplayChanged);
            _signals.Subscribe<UnitCreatedSignal>(OnGameplayChanged);
            _signals.Subscribe<UnitDestroyedSignal>(OnGameplayChanged);
            _signals.Subscribe<UnitRecruitmentQueueChangedSignal>(OnGameplayChanged);
            _signals.Subscribe<UnitRecruitmentReadySignal>(OnGameplayChanged);
            _signals.Subscribe<UnitRecruitmentDeployedSignal>(OnGameplayChanged);
            _signals.Subscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
            _signals.Subscribe<GamePausedSignal>(OnPauseChanged);
        }

        private void UnsubscribeSignals()
        {
            _signals.TryUnsubscribe<BuildingPlacedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<BuildingCancelledSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<BuildingPreviewChangedSignal>(OnPreviewChanged);
            _signals.TryUnsubscribe<BuildingSelectionChangedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<BuildingDemolishedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<EconomyTickCompletedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<SettlementCreatedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<SettlementDeactivatedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<SettlementResourceChangedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitCreatedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitDestroyedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitRecruitmentQueueChangedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitRecruitmentReadySignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitRecruitmentDeployedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
            _signals.TryUnsubscribe<GamePausedSignal>(OnPauseChanged);
        }

        private void OnPreviewChanged(BuildingPreviewChangedSignal signal)
        {
            if (_readModel.SetPreview(signal))
                _state.MarkDirty();
        }

        private void OnSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            _readModel.SetSelection(signal);
            _state.ResetSelectionTab();
            _state.MarkDirty();
        }

        private void OnPauseChanged(GamePausedSignal signal) => _state.SetPaused(signal.IsPaused);
        private void OnNotificationPublished(GameplayNotificationRequest request)
            => _state.AddNotification(request.Message, request.Kind.ToString());
        private void OnGameplayChanged(BuildingPlacedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(BuildingCancelledSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(BuildingSelectionChangedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(BuildingDemolishedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(EconomyTickCompletedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(SettlementCreatedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(SettlementDeactivatedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(SettlementResourceChangedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(UnitCreatedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(UnitDestroyedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(UnitRecruitmentQueueChangedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(UnitRecruitmentReadySignal _) => _state.MarkDirty();
        private void OnGameplayChanged(UnitRecruitmentDeployedSignal _) => _state.MarkDirty();
    }

    internal sealed class GameplayCameraFocusService : IGameplayCameraFocusService
    {
        private readonly ICameraMovement _camera;
        private readonly IGridProjection _grid;
        private readonly SignalBus _signals;

        public GameplayCameraFocusService(ICameraMovement camera, IGridProjection grid, SignalBus signals)
        {
            _camera = camera;
            _grid = grid;
            _signals = signals;
        }

        public void FocusGridPosition(Vector2Int gridPosition, string targetId = null)
        {
            _camera.MoveCameraFocusToWorldPoint(_grid.GridToWorld(gridPosition), false);
            _signals.Fire(new WorldFocusPingRequestedSignal
            {
                TargetId = targetId ?? string.Empty,
                Position = gridPosition,
                DurationSeconds = 1.2f,
            });
        }
    }

    internal sealed class GameplayWorldFocusPingPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus _signals;
        private readonly IGridProjection _grid;
        private GameObject _root;
        private LineRenderer _line;
        private float _startedAt = -1f;
        private float _duration = 1.2f;
        private Vector3 _center;

        public GameplayWorldFocusPingPresenter(SignalBus signals, IGridProjection grid)
        {
            _signals = signals;
            _grid = grid;
        }

        public void Initialize() => _signals.Subscribe<WorldFocusPingRequestedSignal>(OnPing);

        public void Tick()
        {
            if (_line == null || _startedAt < 0f)
                return;
            float progress = Mathf.Clamp01((Time.unscaledTime - _startedAt) / _duration);
            if (progress >= 1f)
            {
                _line.enabled = false;
                _startedAt = -1f;
                return;
            }

            float radius = Mathf.Lerp(0.35f, 1.65f, progress);
            float alpha = 1f - progress;
            Color color = new Color(0.94f, 0.73f, 0.25f, alpha);
            _line.startColor = color;
            _line.endColor = color;
            SetCircle(radius);
        }

        public void Dispose()
        {
            _signals.TryUnsubscribe<WorldFocusPingRequestedSignal>(OnPing);
            if (_root != null)
                UnityEngine.Object.Destroy(_root);
        }

        private void OnPing(WorldFocusPingRequestedSignal signal)
        {
            EnsureView();
            _center = _grid.GridToWorld(signal.Position);
            _duration = Mathf.Max(0.2f, signal.DurationSeconds);
            _startedAt = Time.unscaledTime;
            _line.enabled = true;
            SetCircle(0.35f);
        }

        private void EnsureView()
        {
            if (_line != null)
                return;
            _root = new GameObject("Presentation/WorldFocusPing");
            _line = _root.AddComponent<LineRenderer>();
            _line.loop = true;
            _line.useWorldSpace = true;
            _line.positionCount = 49;
            _line.widthMultiplier = 0.08f;
            _line.numCornerVertices = 2;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.sortingOrder = 32000;
        }

        private void SetCircle(float radius)
        {
            for (int index = 0; index < _line.positionCount; index++)
            {
                float angle = index / (float)(_line.positionCount - 1) * Mathf.PI * 2f;
                Vector3 offset = _grid.WorldPlane == GridWorldPlane.XZ
                    ? new Vector3(Mathf.Cos(angle) * radius, 0.08f, Mathf.Sin(angle) * radius)
                    : new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, -0.08f);
                _line.SetPosition(index, _center + offset);
            }
        }
    }
}
