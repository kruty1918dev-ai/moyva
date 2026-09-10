using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
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
using System.Text;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplayHudReadModel
    {
        private readonly ITurnService _turns;
        private readonly ITurnHistoryQuery _turnHistory;
        private readonly IEconomyRuntimeApi _economy;
        private readonly IEconomyInfoMediator _population;
        private readonly IConstructionSessionCommands _construction;
        private readonly IConstructionBootstrapQuery _bootstrap;
        private readonly IConstructionPortfolioQuery _portfolio;
        private readonly IConstructionLifecycle _lifecycle;
        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionSelectionAvailabilityQuery _availability;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _unitOwnership;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly ICombatCommandService _combat;
        private readonly ICombatRemoteCommandRequester _remoteCombat;
        private readonly ISettlementCaptureRemoteCommandRequester _remoteSettlementCapture;
        private readonly IConstructionBuildingCombatTargetQuery _buildingTargets;
        private readonly IHealthRegistry _health;
        private readonly ISettlementCaptureService _settlementCapture;
        private readonly IFogOwnerStateReader _ownerFog;
        private readonly ILocalGameplayRoleResolver _roleResolver;
        private readonly IGameplayProgressClock _progressClock;
        private readonly GameplayCargoPanel _cargoPanel;
        private readonly Dictionary<int, Sprite> _prefabSpriteCache = new();
        private readonly Dictionary<string, Sprite> _icons = new(StringComparer.Ordinal);
        private readonly HashSet<string> _reportedMissingIcons = new(StringComparer.Ordinal);
        private IReadOnlyDictionary<string, Sprite> _publishedIcons;

        private WorldInfoSelectionKind _selectionKind;
        private string _selectionId = string.Empty;
        private Vector2Int _selectionPosition;
        private string _commandUnitId = string.Empty;
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
            [InjectOptional] ICombatCommandService combat = null,
            [InjectOptional] ICombatRemoteCommandRequester remoteCombat = null,
            [InjectOptional] ISettlementCaptureRemoteCommandRequester remoteSettlementCapture = null,
            [InjectOptional] IConstructionBuildingCombatTargetQuery buildingTargets = null,
            [InjectOptional] IHealthRegistry health = null,
            [InjectOptional] ISettlementCaptureService settlementCapture = null,
            [InjectOptional] IFogOwnerStateReader ownerFog = null,
            [InjectOptional] ILocalGameplayRoleResolver roleResolver = null,
            [InjectOptional] IGameplayProgressClock progressClock = null,
            [InjectOptional] EconomyDatabaseSO economyDatabase = null,
            [InjectOptional] GameplayCargoPanel cargoPanel = null,
            [InjectOptional] IConstructionLifecycle lifecycle = null,
            [InjectOptional] IEconomyInfoMediator population = null)
        {
            _turns = turns;
            _economy = economy;
            _construction = construction;
            _buildings = buildings;
            _availability = construction as IConstructionSelectionAvailabilityQuery;
            _turnHistory = turnHistory;
            _bootstrap = bootstrap;
            _portfolio = portfolio;
            _lifecycle = lifecycle;
            _population = population;
            _units = units;
            _unitOwnership = unitOwnership;
            _recruitment = recruitment;
            _unitConfigs = unitConfigs;
            _combat = combat;
            _remoteCombat = remoteCombat;
            _remoteSettlementCapture = remoteSettlementCapture;
            _buildingTargets = buildingTargets;
            _health = health;
            _settlementCapture = settlementCapture;
            _ownerFog = ownerFog;
            _roleResolver = roleResolver;
            _progressClock = progressClock;
            _cargoPanel = cargoPanel;

            foreach (var resource in MoyvaJsonRuntime.GetAll<EconomyResourceDefinition>())
                if (resource != null && resource.Icon != null)
                    _icons[GameplayHtmlIconKeys.Resource(resource.Id)] = resource.Icon;

            if (economyDatabase != null)
                foreach (var resource in economyDatabase.Resources)
                    if (resource != null && resource.Icon != null)
                        _icons[GameplayHtmlIconKeys.Resource(resource.Id)] = resource.Icon;
            if (buildings != null)
                foreach (var building in buildings.GetAll())
                {
                    Sprite icon = ResolveBuildingIcon(building);
                    if (icon != null) _icons[GameplayHtmlIconKeys.Building(building.Id)] = icon;
                }
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
                SandboxRealtime = _progressClock?.IsRealtime ?? false,
                SandboxSpeed = _progressClock?.Speed ?? 1f,
                SandboxElapsedSeconds = _progressClock?.ElapsedGameplaySeconds ?? 0d,
                SandboxSecondsUntilNextProgress = _progressClock?.SecondsUntilNextProgress ?? 0f,
                SandboxRoundSeconds = _progressClock?.SandboxRoundSeconds ?? 10f,
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
            CaptureAttackPreview(snapshot);
            CaptureCapturePreview(snapshot, ownerId);
            if (_selectionKind == WorldInfoSelectionKind.Unit)
                snapshot.Cargo = _cargoPanel?.Capture(_selectionId);
            foreach (var group in snapshot.UnitGroups)
            {
                var config = _unitConfigs?.GetConfig(group.Id);
                var icon = config?.ResolveCustomSprite();
                if (icon != null) _icons[GameplayHtmlIconKeys.Unit(group.Id)] = icon;
            }
            if (_publishedIcons == null || _publishedIcons.Count != _icons.Count)
                _publishedIcons = new System.Collections.ObjectModel.ReadOnlyDictionary<string, Sprite>(
                    new Dictionary<string, Sprite>(_icons, StringComparer.Ordinal));
            snapshot.Icons = _publishedIcons;
            ValidateVisibleIcons(snapshot);
            return snapshot;
        }

        private void ValidateVisibleIcons(GameplayHtmlSnapshot snapshot)
        {
            foreach (var resource in snapshot.Resources)
                CheckIcon(GameplayHtmlIconKeys.Resource(resource.Id), _icons.ContainsKey(GameplayHtmlIconKeys.Resource(resource.Id)));
            foreach (var building in snapshot.BuildingOptions)
                CheckIcon(building.IconGlobalKey, building.HasIcon);
            foreach (var group in snapshot.UnitGroups)
                CheckIcon(GameplayHtmlIconKeys.Unit(group.Id), _icons.ContainsKey(GameplayHtmlIconKeys.Unit(group.Id)));
            foreach (var recipe in snapshot.RecruitmentRecipes)
                CheckIcon(recipe.IconGlobalKey, recipe.HasIcon);
        }

        private void CheckIcon(string key, bool available)
        {
            if (!available && _reportedMissingIcons.Add(key))
                Debug.LogWarning($"[GameplayHTML] Missing catalog sprite '{key}'. Check its JSON asset reference.");
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

        public string ResolveBuildingDisplayName(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return "building";

            BuildingDefinition definition = _buildings?.GetById(buildingId.Trim());
            return string.IsNullOrWhiteSpace(definition?.DisplayName)
                ? Display(buildingId)
                : definition.DisplayName.Trim();
        }

        public Task<CombatCommandResult> AttackSelectionAsync()
        {
            if (string.IsNullOrWhiteSpace(_commandUnitId))
                return Task.FromResult(CombatCommandResult.Rejected(string.Empty, _selectionId, "Select one of your units first."));
            if (_selectionKind == WorldInfoSelectionKind.None || string.IsNullOrWhiteSpace(_selectionId))
                return Task.FromResult(CombatCommandResult.Rejected(_commandUnitId, string.Empty, "Select a target first."));

            string ownerId = ResolveOwnerId();
            string targetId = ResolveCombatTargetId(
                _selectionKind,
                _selectionId,
                _selectionPosition);
            if (!CanInteractWithSelectionThroughFog(ownerId, out string fogReason))
            {
                return Task.FromResult(CombatCommandResult.Rejected(
                    _commandUnitId,
                    targetId,
                    fogReason));
            }
            if (_roleResolver?.Resolve().Role == LocalGameplayRole.Client)
            {
                string remoteReason = null;
                if (_remoteCombat != null
                    && _remoteCombat.TryRequestAttack(
                        ownerId,
                        _commandUnitId,
                        targetId,
                        out remoteReason))
                {
                    return Task.FromResult(new CombatCommandResult(
                        true,
                        _commandUnitId,
                        targetId,
                        0,
                        false,
                        "Attack request sent. Waiting for host."));
                }

                return Task.FromResult(CombatCommandResult.Rejected(
                    _commandUnitId,
                    targetId,
                    string.IsNullOrWhiteSpace(remoteReason)
                        ? "Combat authority is unavailable."
                        : remoteReason));
            }

            if (_combat == null)
                return Task.FromResult(CombatCommandResult.Rejected(_commandUnitId, targetId, "Combat command service is unavailable."));

            return _combat.ExecuteAsync(ownerId, _commandUnitId, targetId);
        }

        public SettlementCaptureResult CaptureSelection()
        {
            string ownerId = ResolveOwnerId();
            if (!TryEvaluateCaptureSelection(ownerId, out ConstructionBuildingCombatTarget target, out string reason))
                return SettlementCaptureResult.Rejected(string.Empty, reason);

            if (_roleResolver?.Resolve().Role == LocalGameplayRole.Client)
            {
                string remoteReason = null;
                if (_remoteSettlementCapture != null
                    && _remoteSettlementCapture.TryRequestCapture(
                        ownerId,
                        _commandUnitId,
                        target.EntityId,
                        target.Position,
                        out remoteReason))
                {
                    return new SettlementCaptureResult(
                        true,
                        string.Empty,
                        target.OwnerId,
                        ownerId,
                        "Capture request sent. Waiting for host.");
                }

                return SettlementCaptureResult.Rejected(
                    string.Empty,
                    string.IsNullOrWhiteSpace(remoteReason)
                        ? "Settlement capture authority is unavailable."
                        : remoteReason);
            }

            if (_settlementCapture == null)
                return SettlementCaptureResult.Rejected(string.Empty, "Settlement capture service is unavailable.");

            return _settlementCapture.CaptureSettlementAtPosition(
                target.Position,
                target.OwnerId,
                ownerId,
                "captured-by-unit");
        }

        private static string ResolveCombatTargetId(
            WorldInfoSelectionKind kind,
            string selectionId,
            Vector2Int position)
            => kind == WorldInfoSelectionKind.Building
                ? $"{selectionId}@{position.x},{position.y}"
                : selectionId;

        private bool TryEvaluateCaptureSelection(
            string ownerId,
            out ConstructionBuildingCombatTarget target,
            out string reason)
        {
            target = default;
            reason = string.Empty;

            if (string.IsNullOrWhiteSpace(_commandUnitId))
            {
                reason = "Select one of your units first.";
                return false;
            }

            if (_selectionKind != WorldInfoSelectionKind.Building
                || string.IsNullOrWhiteSpace(_selectionId))
            {
                reason = "Select a settlement center to capture.";
                return false;
            }

            if (!string.Equals(_unitOwnership?.GetUnitOwnerId(_commandUnitId), ownerId, StringComparison.Ordinal))
            {
                reason = "Only your own unit can capture a settlement.";
                return false;
            }

            if (_units == null || !_units.TryGetUnitPosition(_commandUnitId, out Vector2Int unitPosition))
            {
                reason = "Selected unit position is unavailable.";
                return false;
            }

            string targetId = ResolveCombatTargetId(
                _selectionKind,
                _selectionId,
                _selectionPosition);
            if (_buildingTargets == null || !_buildingTargets.TryGetCombatTarget(targetId, out target))
            {
                reason = "Selected building cannot be captured.";
                return false;
            }

            if (string.Equals(target.OwnerId, ownerId, StringComparison.Ordinal))
            {
                reason = "You already control this settlement.";
                return false;
            }

            if (!CanInteractWithSelectionThroughFog(ownerId, out string fogReason))
            {
                reason = fogReason;
                return false;
            }

            BuildingDefinition definition = _buildings?.GetById(target.BuildingId);
            if (!BuildingDefinitionCapabilities.IsCastle(definition)
                && !BuildingDefinitionCapabilities.IsTownHall(definition))
            {
                reason = "Only castles and town halls can be captured.";
                return false;
            }

            int dx = Math.Abs(unitPosition.x - target.Position.x);
            int dy = Math.Abs(unitPosition.y - target.Position.y);
            if (Math.Max(dx, dy) > 1)
            {
                reason = "Move a unit next to the settlement center first.";
                return false;
            }

            if (_health == null || !_health.TryGet(target.EntityId, out IHealth health))
            {
                reason = "Settlement defenses are unavailable.";
                return false;
            }

            int captureThreshold = Math.Max(1, Mathf.CeilToInt(health.MaxHp * 0.25f));
            if (health.CurrentHp > captureThreshold)
            {
                reason = $"Reduce defenses to {captureThreshold} HP or less before capture.";
                return false;
            }

            reason = "Settlement center can be captured.";
            return true;
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
                .OrderBy(pair => GetResourceSortIndex(pair.Key))
                .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => new GameplayResourceSnapshot(pair.Key, pair.Value))
                .ToArray();
        }

        private GameplayBuildingOptionSnapshot[] CaptureBuildingOptions(string ownerId)
        {
            BuildingDefinition[] definitions = _buildings?.GetAll();
            if (definitions == null)
                return Array.Empty<GameplayBuildingOptionSnapshot>();

            string requiredCastleId = string.Empty;
            bool requiresInitialCastle = _bootstrap != null
                && _bootstrap.RequiresInitialCastle(ownerId, out requiredCastleId);

            return definitions
                .Where(definition => definition != null && !string.IsNullOrWhiteSpace(definition.Id))
                .Where(definition => !BuildingDefinitionCapabilities.IsCastle(definition)
                    || (requiresInitialCastle
                        && string.Equals(definition.Id, requiredCastleId, StringComparison.Ordinal)))
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
                            : availability.Reason,
                        definition.BuildTurns);
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
            if (!_recruitment.TryEnqueue(ownerId, _selectionPosition, unitTypeId.Trim(), out string reason))
            {
                return UiActionResult.Rejected(
                    UiActionReason.ActionUnavailable,
                    string.IsNullOrWhiteSpace(reason) ? "Recruitment could not be started." : reason);
            }

            return UiActionResult.Performed();
        }

        public UiActionResult TryCancelRecruitment(string queueId)
        {
            if (_selectionKind != WorldInfoSelectionKind.Building || _recruitment == null
                || !long.TryParse(queueId, out long id))
                return UiActionResult.Rejected(UiActionReason.WrongContext);
            return _recruitment.TryCancel(ResolveOwnerId(), _selectionPosition, id, out string reason)
                ? UiActionResult.Performed()
                : UiActionResult.Rejected(UiActionReason.ActionUnavailable, reason);
        }

        public bool TryResolveNotificationBuilding(Vector2Int position, out string buildingId)
        {
            buildingId = null;
            return _population != null && _population.TryGetBuildingContext(position, out buildingId, out _);
        }

        public bool TryGetReadyRecruitment(string queueId, out UnitRecruitmentReadyIndicatorClickedSignal signal)
        {
            signal = default;
            if (_selectionKind != WorldInfoSelectionKind.Building || _recruitment == null
                || !long.TryParse(queueId, out long id))
                return false;
            foreach (var item in _recruitment.GetQueue(ResolveOwnerId(), _selectionPosition))
            {
                if (item.QueueId != id || !item.IsReady)
                    continue;
                signal = new UnitRecruitmentReadyIndicatorClickedSignal
                {
                    OwnerId = item.OwnerId,
                    QueueId = item.QueueId,
                    UnitTypeId = item.UnitTypeId,
                    RecruitingBuildingId = item.RecruitingBuildingId,
                    RecruitingBuildingPosition = item.RecruitingBuildingPosition,
                };
                return true;
            }
            return false;
        }

        public bool TryGetUnitPosition(string unitId, out Vector2Int position)
        {
            position = default;
            return _units != null && _units.TryGetUnitPosition(unitId, out position);
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
            snapshot.SelectionOperational = _lifecycle?.IsOperational(_selectionPosition) ?? true;

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
                            canRecruit && (_population?.GetRecruitmentPopulation(ownerId, _selectionPosition).Available ?? 0) >= Math.Max(1, recipe.PopulationCost),
                            !canRecruit ? unavailableReason
                                : $"Requires {Math.Max(1, recipe.PopulationCost)} available residents.",
                            recipe.TrainingSeconds > 0f ? recipe.TrainingSeconds
                                : recipe.TrainingTurns * (_progressClock?.SandboxRoundSeconds ?? 10f),
                            Math.Max(1, recipe.PopulationCost));
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
                    item.IsReady,
                    item.Status == UnitRecruitmentQueueStatus.Waiting,
                    item.TrainingSeconds > 0f ? item.RemainingSeconds
                        : item.RemainingTurns * (_progressClock?.SandboxRoundSeconds ?? 10f));
            }
            snapshot.RecruitmentQueue = queueSnapshots;
        }

        private void CaptureSelectionDetails(GameplayHtmlSnapshot snapshot, string ownerId)
        {
            if (_selectionKind == WorldInfoSelectionKind.None)
                return;

            var facts = new List<GameplayFactSnapshot>();
            bool ownedByLocal = IsSelectionOwnedByOwner(ownerId);
            if (!IsSelectionKnownThroughFog(ownerId, ownedByLocal))
            {
                snapshot.SelectionOwnedByLocalPlayer = false;
                snapshot.SelectionTitle = "Unknown contact";
                snapshot.SelectionSubtitle = "This tile is outside your kingdom's known view.";
                facts.Add(new GameplayFactSnapshot(
                    "Visibility",
                    "Unexplored",
                    "Scout this area to identify the target"));
                snapshot.SelectionFacts = facts.ToArray();
                return;
            }

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
                    facts.Add(new GameplayFactSnapshot(
                        "Build time",
                        GameplayProgressTimeText.BuildDuration(definition?.BuildTurns ?? 0, _progressClock),
                        "Construction duration"));
                    if (_lifecycle != null
                        && _lifecycle.TryGetProgress(_selectionPosition, out int completed, out int required)
                        && completed < required)
                    {
                        string progressLabel = $"Under construction {completed}/{required}";
                        string remainingLabel = GameplayProgressTimeText.Remaining(required - completed, _progressClock);
                        if (_lifecycle is IConstructionRealtimeProgress realtime
                            && realtime.TryGetRealtimeProgress(_selectionPosition, out float progress, out float seconds))
                        {
                            progressLabel = $"Under construction {progress:P0}";
                            remainingLabel = GameplayProgressTimeText.Duration(seconds);
                        }
                        facts.Add(new GameplayFactSnapshot("Status", progressLabel, remainingLabel + " remaining"));
                    }
                    else
                    {
                        facts.Add(new GameplayFactSnapshot(
                            "Status",
                            snapshot.SelectionOperational ? "Operational" : "Unavailable",
                            "Current building state"));
                    }
                    if (snapshot.SelectionOwnedByLocalPlayer && _population != null)
                    {
                        var residents = _population.GetRecruitmentPopulation(ownerId, _selectionPosition);
                        facts.Add(new GameplayFactSnapshot("Population", residents.Total.ToString(),
                            $"{residents.Available} available · {residents.Training} training/ready · {residents.Military} military"));
                        facts.Add(new GameplayFactSnapshot("Construction workforce", $"{residents.ConstructionSpeed:P0}",
                            "Speed updates with available adult population"));
                    }
                    break;
                }
                case WorldInfoSelectionKind.Unit:
                {
                    string typeId = _units?.GetUnitTypeId(_selectionId);
                    if (_units != null && _units.TryGetUnitPosition(_selectionId, out var currentPosition))
                        snapshot.SelectionPosition = currentPosition;
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
                    if (snapshot.SelectionOwnedByLocalPlayer)
                        _commandUnitId = _selectionId;
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

        private void CaptureAttackPreview(GameplayHtmlSnapshot snapshot)
        {
            snapshot.AttackSourceId = _commandUnitId ?? string.Empty;
            snapshot.CanAttackSelection = false;
            snapshot.AttackPreview = string.Empty;
            snapshot.AttackUnavailableReason = string.Empty;

            if (string.IsNullOrWhiteSpace(snapshot.AttackSourceId)
                || string.IsNullOrWhiteSpace(snapshot.SelectionId)
                || snapshot.SelectionOwnedByLocalPlayer
                || !snapshot.CanIssueLocalCommands)
            {
                return;
            }

            if (_combat == null)
            {
                snapshot.AttackUnavailableReason = "Combat command service is unavailable.";
                return;
            }

            if (!CanInteractWithSelectionThroughFog(snapshot.OwnerId, out string fogReason))
            {
                snapshot.AttackUnavailableReason = fogReason;
                return;
            }

            if (_combat.TryPreview(
                    snapshot.AttackSourceId,
                    ResolveCombatTargetId(snapshot),
                    out CombatCommandPreview preview,
                    out string reason))
            {
                snapshot.CanAttackSelection = true;
                snapshot.AttackPreview = preview.TargetWouldDie
                    ? $"{preview.ExpectedDamage} damage, target will fall"
                    : $"{preview.ExpectedDamage} damage";
                return;
            }

            snapshot.AttackUnavailableReason = string.IsNullOrWhiteSpace(reason)
                ? "Selected target cannot be attacked."
                : reason;
        }

        private void CaptureCapturePreview(GameplayHtmlSnapshot snapshot, string ownerId)
        {
            snapshot.CanCaptureSelection = false;
            snapshot.CapturePreview = string.Empty;
            snapshot.CaptureUnavailableReason = string.Empty;

            if (string.IsNullOrWhiteSpace(snapshot.AttackSourceId)
                || !string.Equals(snapshot.SelectionKind, WorldInfoSelectionKind.Building.ToString(), StringComparison.Ordinal)
                || snapshot.SelectionOwnedByLocalPlayer
                || !snapshot.CanIssueLocalCommands)
            {
                return;
            }

            if (_roleResolver?.Resolve().Role == LocalGameplayRole.Client)
            {
                if (_remoteSettlementCapture == null)
                {
                    snapshot.CaptureUnavailableReason = "Settlement capture authority is unavailable.";
                    return;
                }
            }
            else if (_settlementCapture == null)
            {
                snapshot.CaptureUnavailableReason = "Settlement capture service is unavailable.";
                return;
            }

            if (TryEvaluateCaptureSelection(ownerId, out _, out string reason))
            {
                snapshot.CanCaptureSelection = true;
                snapshot.CapturePreview = "Settlement center can be captured";
                return;
            }

            snapshot.CaptureUnavailableReason = string.IsNullOrWhiteSpace(reason)
                ? "Selected building cannot be captured."
                : reason;
        }

        private static string ResolveCombatTargetId(GameplayHtmlSnapshot snapshot)
            => ResolveCombatTargetId(
                string.Equals(snapshot.SelectionKind, WorldInfoSelectionKind.Building.ToString(), StringComparison.Ordinal)
                    ? WorldInfoSelectionKind.Building
                    : WorldInfoSelectionKind.Unit,
                snapshot.SelectionId,
                snapshot.SelectionPosition);

        private bool CanInteractWithSelectionThroughFog(string ownerId, out string reason)
        {
            reason = string.Empty;
            if (_ownerFog == null || IsSelectionOwnedByOwner(ownerId))
                return true;

            if (_ownerFog.IsVisible(ownerId, _selectionPosition))
                return true;

            reason = "Target is outside your current vision.";
            return false;
        }

        private bool IsSelectionKnownThroughFog(string ownerId, bool ownedByLocal)
        {
            if (_ownerFog == null || ownedByLocal)
                return true;

            return _selectionKind switch
            {
                WorldInfoSelectionKind.Unit => _ownerFog.IsVisible(ownerId, _selectionPosition),
                WorldInfoSelectionKind.Building => _ownerFog.IsExplored(ownerId, _selectionPosition),
                WorldInfoSelectionKind.MapObject => _ownerFog.IsExplored(ownerId, _selectionPosition),
                _ => true,
            };
        }

        private bool IsSelectionOwnedByOwner(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId)
                || _selectionKind == WorldInfoSelectionKind.None
                || string.IsNullOrWhiteSpace(_selectionId))
            {
                return false;
            }

            string normalizedOwner = ownerId.Trim();
            if (_selectionKind == WorldInfoSelectionKind.Unit)
            {
                return string.Equals(
                    _unitOwnership?.GetUnitOwnerId(_selectionId)?.Trim(),
                    normalizedOwner,
                    StringComparison.Ordinal);
            }

            if (_selectionKind == WorldInfoSelectionKind.Building
                && _construction is IConstructionBuildingOwnershipQuery ownership
                && ownership.TryGetPlacedBuildingOwner(_selectionPosition, out string buildingOwner))
            {
                return string.Equals(
                    buildingOwner?.Trim(),
                    normalizedOwner,
                    StringComparison.Ordinal);
            }

            return false;
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
                    .OrderBy(pair => GetResourceSortIndex(pair.Key))
                    .ThenBy(pair => pair.Key, StringComparer.Ordinal)
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
                        .OrderBy(pair => GetResourceSortIndex(pair.Key))
                        .ThenBy(pair => pair.Key, StringComparer.Ordinal)
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
                    item.IsLocal,
                    item.IsEliminated);
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
            if (CanonicalResourceLabels.TryGetValue(normalized, out string label))
                return label;

            normalized = TrimSuffix(normalized, "-materials-resources");
            normalized = TrimSuffix(normalized, "_materials_resources");
            normalized = TrimSuffix(normalized, "-resources");
            normalized = TrimSuffix(normalized, "_resources");
            normalized = TrimSuffix(normalized, "-resource");
            normalized = TrimSuffix(normalized, "_resource");
            return Display(normalized);
        }

        private static int GetResourceSortIndex(string resourceId)
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                return int.MaxValue;

            string normalized = resourceId.Trim();
            for (int index = 0; index < CanonicalResourceOrder.Length; index++)
            {
                if (string.Equals(CanonicalResourceOrder[index], normalized, StringComparison.Ordinal))
                    return index;
            }

            return CanonicalResourceOrder.Length;
        }

        private static string TrimSuffix(string value, string suffix)
        {
            return value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? value.Substring(0, value.Length - suffix.Length)
                : value;
        }

        private static readonly string[] CanonicalResourceOrder =
        {
            "steak-food-resources",
            "walnut-wood-materials-resources",
            "stone-materials-resources",
            "iron-ore-materials-resources",
            "iron-ingot-materials-resources",
            "gold-coins-materials-resources",
        };

        private static readonly Dictionary<string, string> CanonicalResourceLabels =
            new(StringComparer.Ordinal)
            {
                ["steak-food-resources"] = "Food",
                ["walnut-wood-materials-resources"] = "Wood",
                ["stone-materials-resources"] = "Stone",
                ["iron-ore-materials-resources"] = "Iron Ore",
                ["iron-ingot-materials-resources"] = "Iron",
                ["gold-coins-materials-resources"] = "Gold",
            };
    }
}
