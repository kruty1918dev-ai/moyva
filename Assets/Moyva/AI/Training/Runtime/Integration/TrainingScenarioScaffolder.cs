using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Combat.Runtime;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    /// <summary>
    /// Builds an independent, authoritative starting state for a scenario.
    /// Every asset is committed through the real gameplay pipeline: placement
    /// legality is evaluated by the construction query, committed through the
    /// confirmed-placement applier, timed buildings are completed through the
    /// lifecycle restore seam, units come from the unit factory, and weakening
    /// goes through the health registry. Nothing here grants reward, progress
    /// or mastery — it runs while the episode is still in setup phase.
    /// </summary>
    internal static class TrainingScenarioScaffolder
    {
        private const int DefaultOpeningRevealRadius = 6;
        private const int MaxPlacementScanRadius = 10;
        private const int FootprintMargin = 4;
        private const int VisibilityProbeRange = 2;

        // Failures that depend on the generated world are retryable with a new seed.
        internal const string PlacementFailurePrefix = "Scenario setup placement:";
        private const string ServiceFailurePrefix = "Scenario setup service:";

        internal static void Apply(
            DiContainer container,
            MenuWorldPreviewData world,
            IReadOnlyDictionary<string, Vector2Int> anchors,
            TrainingScenarioDefinition scenario,
            bool learnerMustPlaceCastle,
            string castleBuildingId,
            string defaultUnitTypeId)
        {
            if (container == null || world == null)
                throw new InvalidOperationException(ServiceFailurePrefix + " missing container or world.");
            if (string.IsNullOrWhiteSpace(castleBuildingId))
                throw new InvalidOperationException(ServiceFailurePrefix + " castle building type id is required.");
            if (string.IsNullOrWhiteSpace(defaultUnitTypeId))
                throw new InvalidOperationException(ServiceFailurePrefix + " default unit type id is required.");

            var conditions = scenario?.startingConditions ?? new TrainingScenarioStartingConditions();
            if (scenario == null) conditions.learnerMustPlaceCastle = learnerMustPlaceCastle;
            var opponent = conditions.opponent ?? new TrainingScenarioOpponentSetup { enabled = false };

            var buildings = container.TryResolve<IBuildingRegistry>();
            var placementQuery = container.TryResolve<IConstructionPlacementQuery>();
            var setupApplier = container.TryResolve<IConstructionSetupPlacementApplier>();
            var lifecycleRestore = container.TryResolve<IConstructionOperationalRestore>();
            var economyApi = container.TryResolve<IEconomyRuntimeApi>();
            var economyManager = container.TryResolve<EconomyManager>();
            var unitFactory = container.TryResolve<IUnitFactory>();
            var unitPlacement = container.TryResolve<IUnitPlacementValidator>();
            var health = container.TryResolve<IHealthRegistry>();
            var fogSources = container.TryResolve<IFogOwnerVisionSourceRegistry>();
            var fogLocal = container.TryResolve<IFogVisionSourceRegistry>();
            var signalBus = container.TryResolve<SignalBus>();
            var grid = container.TryResolve<IGridService>();
            var traversalCosts = container.TryResolve<ITraversalCostResolver>();
            var objectsMap = container.TryResolve<IObjectsMapService>();
            var visibilityResolver = container.TryResolve<IFogVisibilityResolver>();
            bool routeValidation = grid != null && traversalCosts != null && objectsMap != null;

            if (buildings == null || placementQuery == null || setupApplier == null)
                throw new InvalidOperationException(ServiceFailurePrefix + " construction services are unavailable.");
            if (economyApi == null || economyManager == null)
                throw new InvalidOperationException(ServiceFailurePrefix + " economy services are unavailable.");
            if (unitFactory == null || unitPlacement == null || health == null)
                throw new InvalidOperationException(ServiceFailurePrefix + " unit/health services are unavailable.");

            string learner = TrainingGameplayScope.LearnerId;
            string enemy = TrainingGameplayScope.OpponentId;
            var objectivePositions = new List<Vector2Int>();
            var castlePositions = new Dictionary<string, Vector2Int>(StringComparer.Ordinal);

            // 1. Opening fog reveal happens before placement so authoritative
            //    placement rules that reject fogged cells see a valid state.
            //    Placement legality reads the shared local fog grid, while the
            //    learner/opponent perception reads per-owner state — reveal
            //    both. The local reveal must cover the whole placement scan
            //    area plus the largest building footprint margin.
            int revealRadius = conditions.openingRevealRadius >= 0
                ? conditions.openingRevealRadius
                : DefaultOpeningRevealRadius;
            if (fogLocal != null)
            {
                int localRadius = Mathf.Max(revealRadius, MaxPlacementScanRadius + FootprintMargin);
                foreach (var pair in anchors)
                    fogLocal.RevealArea(pair.Value, localRadius,
                        FogRevealShape.Square, true, "training-opening-local:" + pair.Key);
            }
            if (fogSources != null && revealRadius > 0)
            {
                foreach (var pair in anchors)
                    fogSources.RevealArea(pair.Key, pair.Value, revealRadius,
                        FogRevealShape.Square, true, "training-opening-spawn:" + pair.Key);
            }

            // 2. Settlement centers first: every other structure depends on them.
            bool learnerCastle = !conditions.learnerMustPlaceCastle
                && (conditions.learnerStartsWithCastle || (scenario?.fullGame ?? true) || scenario == null);
            if (learnerCastle)
                castlePositions[learner] = PlaceCastle(world, anchors, learner, castleBuildingId,
                    buildings, placementQuery, setupApplier, lifecycleRestore);

            // Connectivity: enemy-side settlement centers must stay reachable
            // for the learner's units under the real traversal rules (terrain
            // passability, movement profile costs and occupancy), otherwise
            // capture/combat goals can never be completed.
            HashSet<Vector2Int> reachable = null;
            if (routeValidation)
            {
                Vector2Int learnerAnchor = castlePositions.TryGetValue(learner, out var learnerCastleCell)
                    ? learnerCastleCell
                    : anchors[learner];
                reachable = ComputeTraversalReachable(world, learnerAnchor,
                    unitPlacement, grid, traversalCosts, objectsMap);
            }
            bool opponentActive = opponent.enabled || scenario == null || (scenario?.fullGame ?? false);
            bool opponentCastle = opponentActive
                && (opponent.startingCastle || scenario == null || (scenario?.fullGame ?? false));
            if (opponentCastle)
                castlePositions[enemy] = PlaceCastle(world, anchors, enemy, castleBuildingId,
                    buildings, placementQuery, setupApplier, lifecycleRestore,
                    reachable != null && reachable.Count > 0 ? reachable : null);

            // 3. Resources go through the canonical starter-pack signal: an
            //    existing settlement receives them directly, a settlement-less
            //    owner uses the owner resource pool, which transfers into the
            //    first warehouse once the learner's castle is built.
            if (conditions.startingResources != null && conditions.startingResources.Length > 0)
            {
                if (signalBus == null)
                    throw new InvalidOperationException(ServiceFailurePrefix + " signal bus is unavailable.");
                var byOwner = new Dictionary<string, List<StarterPackResourceEntrySignal>>(StringComparer.Ordinal);
                foreach (var resource in conditions.startingResources)
                {
                    string ownerId = ResolveOwner(resource.owner, learner, enemy);
                    if (!byOwner.TryGetValue(ownerId, out var list))
                        byOwner[ownerId] = list = new List<StarterPackResourceEntrySignal>();
                    list.Add(new StarterPackResourceEntrySignal
                    {
                        ResourceId = resource.resourceId,
                        Amount = resource.amount
                    });
                }
                foreach (var pair in byOwner)
                    signalBus.Fire(new GrantStarterPackResourcesSignal
                    {
                        SettlementId = FirstSettlementId(economyApi, pair.Key) ?? string.Empty,
                        OwnerId = pair.Key,
                        Entries = pair.Value.ToArray()
                    });
            }

            // 4. Workforce: adult civilians are the worker pool for production
            //    and recruitment upkeep.
            SeedResidents(economyApi, economyManager, learner, conditions.residents);
            SeedResidents(economyApi, economyManager, enemy, opponent.residents);

            // 5. Scenario buildings through the same legality-checked path.
            if (conditions.startingBuildings != null)
                foreach (var building in conditions.startingBuildings)
                {
                    string ownerId = ResolveOwner(building.owner, learner, enemy);
                    for (int i = 0; i < building.count; i++)
                    {
                        Vector2Int anchor = castlePositions.TryGetValue(ownerId, out var castle)
                            ? castle
                            : anchors[ownerId];
                        Vector2Int cell = FindBuildingCell(buildings, placementQuery, world,
                            building.buildingTypeId, ownerId, anchor);
                        if (!setupApplier.TryApplySetupPlacement(building.buildingTypeId, cell, ownerId))
                            throw new InvalidOperationException(PlacementFailurePrefix
                                + $" confirmed commit failed for '{building.buildingTypeId}' at {cell} owner '{ownerId}'.");
                        if (building.operational)
                            lifecycleRestore?.TryRestoreOperational(cell);
                        if (building.healthFraction < 1f)
                            Weaken(health, EntityId(building.buildingTypeId, cell), building.healthFraction,
                                building.buildingTypeId);
                    }
                }

            // 6. Objectives: settlement objectives need a real capturable
            //    settlement owned by the declared side.
            if (conditions.objectives != null)
                foreach (var objective in conditions.objectives)
                {
                    if (objective == null) continue;
                    string ownerId = ResolveOwner(objective.owner, learner, enemy);
                    if (string.Equals(objective.objectiveType, "settlement", StringComparison.OrdinalIgnoreCase))
                    {
                        string buildingId = string.IsNullOrWhiteSpace(objective.buildingTypeId)
                            ? castleBuildingId : objective.buildingTypeId;
                        if (!castlePositions.TryGetValue(ownerId, out var cell))
                        {
                            Vector2Int anchor = anchors.TryGetValue(ownerId, out var a)
                                ? a : anchors[enemy];
                            var constraint = string.Equals(ownerId, learner, StringComparison.Ordinal)
                                || reachable == null || reachable.Count == 0 ? null : reachable;
                            cell = FindBuildingCell(buildings, placementQuery, world,
                                buildingId, ownerId, anchor, constraint);
                            if (!setupApplier.TryApplySetupPlacement(buildingId, cell, ownerId))
                                throw new InvalidOperationException(PlacementFailurePrefix
                                    + $" objective '{buildingId}' commit failed at {cell} owner '{ownerId}'.");
                            lifecycleRestore?.TryRestoreOperational(cell);
                            castlePositions[ownerId] = cell;
                        }
                        if (objective.healthFraction < 1f)
                            Weaken(health, EntityId(buildingId, cell), objective.healthFraction, buildingId);
                        objectivePositions.Add(cell);
                    }
                }

            // 7. Units through the real factory so ownership, occupancy and fog
            //    vision sources all register through the canonical pipeline.
            //    Learner units must spawn inside the learner-reachable region
            //    so scenario objectives are actually attainable.
            if (routeValidation)
                reachable = ComputeTraversalReachable(world,
                    castlePositions.TryGetValue(learner, out var learnerCastleCell2)
                        ? learnerCastleCell2 : anchors[learner],
                    unitPlacement, grid, traversalCosts, objectsMap);
            var learnerUnitArea = reachable != null && reachable.Count > 0 ? reachable : null;
            var learnerUnitCells = new List<Vector2Int>();
            if (conditions.startingUnits != null)
                foreach (var unit in conditions.startingUnits)
                {
                    string ownerId = ResolveOwner(unit.owner, learner, enemy);
                    Vector2Int near = ResolveNear(unit.near, ownerId, learner, enemy, anchors, castlePositions, objectivePositions);
                    var allowed = string.Equals(ownerId, learner, StringComparison.Ordinal) ? learnerUnitArea : null;
                    for (int i = 0; i < unit.count; i++)
                    {
                        Vector2Int cell = FindUnitCell(unitPlacement, world, unit.unitTypeId, near, allowed);
                        string unitId = unitFactory.CreateUnit(unit.unitTypeId, cell, ownerId);
                        if (string.IsNullOrWhiteSpace(unitId))
                            throw new InvalidOperationException(PlacementFailurePrefix
                                + $" unit '{unit.unitTypeId}' rejected at {cell} owner '{ownerId}'.");
                        if (string.Equals(ownerId, learner, StringComparison.Ordinal))
                            learnerUnitCells.Add(cell);
                        if (unit.healthFraction < 1f)
                            Weaken(health, unitId, unit.healthFraction, unit.unitTypeId);
                    }
                }

            // Legacy default (no scenario): canonical two-sided start — castle,
            // a recruitment source and one warrior per side.
            if (scenario == null)
            {
                string recruitmentSource = FindRecruitmentBuildingId(buildings);
                foreach (string owner in new[] { learner, enemy })
                {
                    if (recruitmentSource != null && castlePositions.TryGetValue(owner, out var castle))
                    {
                        Vector2Int cell = FindBuildingCell(buildings, placementQuery, world,
                            recruitmentSource, owner, castle);
                        if (!setupApplier.TryApplySetupPlacement(recruitmentSource, cell, owner))
                            throw new InvalidOperationException(PlacementFailurePrefix
                                + $" recruitment source '{recruitmentSource}' commit failed at {cell} owner '{owner}'.");
                        lifecycleRestore?.TryRestoreOperational(cell);
                    }
                    Vector2Int near = anchors[owner];
                    var allowed = string.Equals(owner, learner, StringComparison.Ordinal) ? learnerUnitArea : null;
                    Vector2Int unitCell = FindUnitCell(unitPlacement, world, defaultUnitTypeId, near, allowed);
                    string created = unitFactory.CreateUnit(defaultUnitTypeId, unitCell, owner);
                    if (string.Equals(owner, learner, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(created))
                        learnerUnitCells.Add(unitCell);
                }
            }

            // 8. Final route validation: every settlement objective and enemy
            //    settlement center must have a learner-reachable adjacent cell,
            //    and every learner unit must be able to leave its spawn cell.
            if (routeValidation)
                reachable = ComputeTraversalReachable(world,
                    castlePositions.TryGetValue(learner, out var finalLearnerCell)
                        ? finalLearnerCell : anchors[learner],
                    unitPlacement, grid, traversalCosts, objectsMap);
            if (reachable != null && reachable.Count > 0)
            {
                foreach (var target in objectivePositions)
                    ValidateReachableObjective(target, reachable, visibilityResolver, world, "objective");
                foreach (var pair in castlePositions)
                    if (!string.Equals(pair.Key, learner, StringComparison.Ordinal)
                        && !objectivePositions.Contains(pair.Value))
                        ValidateReachableObjective(pair.Value, reachable, visibilityResolver, world,
                            $"settlement owner '{pair.Key}'");
                foreach (var cell in learnerUnitCells)
                    if (!reachable.Contains(cell) && !HasAdjacentInSet(cell, reachable))
                        throw new InvalidOperationException(PlacementFailurePrefix
                            + $" learner unit at {cell} is isolated from the reachable region.");
            }
        }

        private static string ResolveOwner(string declared, string learner, string enemy)
            => string.Equals(declared, "opponent", StringComparison.OrdinalIgnoreCase) ? enemy
                : string.Equals(declared, "neutral", StringComparison.OrdinalIgnoreCase) ? "training_neutral"
                : learner;

        private static Vector2Int ResolveNear(
            string near, string ownerId, string learner, string enemy,
            IReadOnlyDictionary<string, Vector2Int> anchors,
            Dictionary<string, Vector2Int> castlePositions,
            List<Vector2Int> objectivePositions)
        {
            if (string.Equals(near, "enemy", StringComparison.OrdinalIgnoreCase))
            {
                string other = string.Equals(ownerId, learner, StringComparison.Ordinal) ? enemy : learner;
                if (anchors.TryGetValue(other, out var enemyAnchor)) return enemyAnchor;
            }
            if (string.Equals(near, "objective", StringComparison.OrdinalIgnoreCase) && objectivePositions.Count > 0)
                return objectivePositions[0];
            if (castlePositions.TryGetValue(ownerId, out var castle)) return castle;
            return anchors.TryGetValue(ownerId, out var own) ? own : anchors[learner];
        }

        private static Vector2Int PlaceCastle(
            MenuWorldPreviewData world,
            IReadOnlyDictionary<string, Vector2Int> anchors,
            string ownerId,
            string castleBuildingId,
            IBuildingRegistry buildings,
            IConstructionPlacementQuery placementQuery,
            IConstructionSetupPlacementApplier setupApplier,
            IConstructionOperationalRestore lifecycleRestore,
            HashSet<Vector2Int> requiredNeighborSet = null)
        {
            if (!anchors.TryGetValue(ownerId, out var anchor))
                throw new InvalidOperationException(PlacementFailurePrefix + " no spawn anchor for owner '" + ownerId + "'.");
            Vector2Int cell = FindBuildingCell(buildings, placementQuery, world, castleBuildingId, ownerId, anchor,
                requiredNeighborSet);
            if (!setupApplier.TryApplySetupPlacement(castleBuildingId, cell, ownerId))
                throw new InvalidOperationException(PlacementFailurePrefix
                    + $" castle commit failed at {cell} owner '{ownerId}'.");
            lifecycleRestore?.TryRestoreOperational(cell);
            return cell;
        }

        private static Vector2Int FindBuildingCell(
            IBuildingRegistry buildings,
            IConstructionPlacementQuery placementQuery,
            MenuWorldPreviewData world,
            string buildingId,
            string ownerId,
            Vector2Int anchor,
            HashSet<Vector2Int> requiredNeighborSet = null)
        {
            if (buildings.GetById(buildingId) == null)
                throw new InvalidOperationException(ServiceFailurePrefix + " unknown building '" + buildingId + "'.");
            Vector2Int best = default;
            float bestScore = float.NegativeInfinity;
            bool found = false;
            foreach (var cell in CellsAround(anchor, MaxPlacementScanRadius))
            {
                if (!InBounds(world, cell)) continue;
                var result = placementQuery.EvaluatePlacement(new ConstructionPlacementQueryRequest(
                    buildingId, cell, includeResources: false, includeDetails: false,
                    ownerId: ownerId, includePendingPlacements: false));
                if (!result.CanCommit) continue;
                if (requiredNeighborSet != null && !HasAdjacentInSet(cell, requiredNeighborSet)) continue;
                float score = ResourceScore(world, cell) - (cell - anchor).magnitude * 0.01f;
                if (!found || score > bestScore)
                {
                    found = true;
                    bestScore = score;
                    best = cell;
                }
            }
            if (!found)
            {
                string detail = DescribePlacementBlockers(buildings, placementQuery, buildingId, ownerId, anchor);
                throw new InvalidOperationException(PlacementFailurePrefix
                    + $" no legal cell for '{buildingId}' near {anchor} owner '{ownerId}'.{detail}");
            }
            return best;
        }

        private static string DescribePlacementBlockers(
            IBuildingRegistry buildings,
            IConstructionPlacementQuery placementQuery,
            string buildingId,
            string ownerId,
            Vector2Int anchor)
        {
            var result = placementQuery.EvaluatePlacement(new ConstructionPlacementQueryRequest(
                buildingId, anchor, includeResources: false, includeDetails: true,
                ownerId: ownerId, includePendingPlacements: false));
            var evaluation = result.EvaluationResult;
            if (evaluation == null || evaluation.Blockers == null || evaluation.Blockers.Count == 0)
                return " Reason: " + (result.Reason ?? "none");
            var first = evaluation.Blockers[0];
            return $" Reason: {result.Reason ?? "none"}; first blocker: kind={first.Kind} msg='{first.Message}' at {first.Position} blockers={evaluation.Blockers.Count}";
        }

        private static Vector2Int FindUnitCell(
            IUnitPlacementValidator placement,
            MenuWorldPreviewData world,
            string unitTypeId,
            Vector2Int near,
            HashSet<Vector2Int> allowed = null)
        {
            for (int radius = 0; radius <= MaxPlacementScanRadius; radius++)
                foreach (var cell in CellsAround(near, radius))
                {
                    if (!InBounds(world, cell)) continue;
                    if (allowed != null && !allowed.Contains(cell)) continue;
                    if (placement.CanDeployUnit(unitTypeId, cell, out _)) return cell;
                }
            throw new InvalidOperationException(PlacementFailurePrefix
                + $" no legal unit cell for '{unitTypeId}' near {near}.");
        }

        private static void SeedResidents(IEconomyRuntimeApi economyApi, EconomyManager manager, string ownerId, int count)
        {
            if (count <= 0 || economyApi == null || manager == null) return;
            var ids = economyApi.GetSettlementIdsForOwner(ownerId);
            if (ids == null) return;
            foreach (var settlementId in ids)
            {
                var state = manager.GetSettlement(settlementId);
                if (state == null) continue;
                for (int i = 0; i < count; i++)
                    state.Residents.Add(new EconomyResidentState(age: 25, hp: 100f, comfort: 50f, houseCollapsed: false));
            }
        }

        private static string FirstSettlementId(IEconomyRuntimeApi api, string ownerId)
        {
            var ids = api?.GetSettlementIdsForOwner(ownerId);
            return ids != null && ids.Count > 0 ? ids[0] : null;
        }

        private static void Weaken(IHealthRegistry health, string entityId, float fraction, string label)
        {
            if (!health.TryGet(entityId, out var target) || target == null)
                throw new InvalidOperationException(ServiceFailurePrefix
                    + $" no health entity '{entityId}' for '{label}'.");
            int hp = Math.Max(1, (int)Math.Round(target.MaxHp * fraction));
            if (target is HealthComponent component)
                component.RestoreCurrentHp(hp);
            else
                target.TakeDamage(Math.Max(0, target.MaxHp - hp));
        }

        internal static string EntityId(string buildingId, Vector2Int position)
            => $"{buildingId}@{position.x},{position.y}";

        private static string FindRecruitmentBuildingId(IBuildingRegistry buildings)
        {
            foreach (var definition in buildings.GetAll())
                if (definition != null && BuildingDefinitionCapabilities.TryGetEnabledModule(
                        definition, out UnitRecruitmentBuildingModule _))
                    return definition.Id;
            return null;
        }

        // Flood-fills the cells a ground unit can actually enter: terrain the
        // placement validator allows, a resolvable ground movement cost and no
        // occupancy, with the same diagonal side-blockage rule the traversal
        // policy applies.
        private static HashSet<Vector2Int> ComputeTraversalReachable(
            MenuWorldPreviewData world,
            Vector2Int seed,
            IUnitPlacementValidator placementValidator,
            IGridService grid,
            ITraversalCostResolver traversalCosts,
            IObjectsMapService objectsMap)
        {
            var result = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            void TryAdd(Vector2Int cell)
            {
                if (IsPassable(cell, world, placementValidator, grid, traversalCosts, objectsMap)
                    && result.Add(cell))
                    queue.Enqueue(cell);
            }
            TryAdd(seed);
            foreach (var neighbor in CellsAround(seed, 1)) TryAdd(neighbor);
            while (queue.Count > 0)
            {
                var from = queue.Dequeue();
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        var next = from + new Vector2Int(dx, dy);
                        if (dx != 0 && dy != 0
                            && (!IsPassable(new Vector2Int(next.x, from.y), world, placementValidator, grid, traversalCosts, objectsMap)
                                || !IsPassable(new Vector2Int(from.x, next.y), world, placementValidator, grid, traversalCosts, objectsMap)))
                            continue;
                        TryAdd(next);
                    }
            }
            return result;
        }

        private static bool IsPassable(
            Vector2Int cell,
            MenuWorldPreviewData world,
            IUnitPlacementValidator placementValidator,
            IGridService grid,
            ITraversalCostResolver traversalCosts,
            IObjectsMapService objectsMap)
        {
            if (!InBounds(world, cell) || objectsMap.IsOccupied(cell)) return false;
            if (!placementValidator.IsTerrainAllowed(cell, out _)) return false;
            return grid.TryGetTileData(cell, out string tileTypeId)
                && !string.IsNullOrWhiteSpace(tileTypeId)
                && traversalCosts.TryResolve(MovementProfileIds.GroundDefault, tileTypeId, out _, out _);
        }

        private static bool HasAdjacentInSet(Vector2Int cell, HashSet<Vector2Int> set)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (set.Contains(cell + new Vector2Int(dx, dy))) return true;
                }
            return false;
        }

        // A reachable target is not enough: height-aware LOS can hide a
        // settlement behind a level wall or crest even from an adjacent cell.
        // Without a reachable adjacent cell that actually observes the target
        // the learner can never attack or capture it, so the world is
        // rejected and retried with a new seed.
        private static void ValidateReachableObjective(
            Vector2Int target,
            HashSet<Vector2Int> reachable,
            IFogVisibilityResolver visibilityResolver,
            MenuWorldPreviewData world,
            string label)
        {
            if (!HasAdjacentInSet(target, reachable))
                throw new InvalidOperationException(PlacementFailurePrefix
                    + $" {label} at {target} is not reachable for learner units.");
            if (visibilityResolver != null
                && !HasVisibleAdjacentCell(target, reachable, visibilityResolver, world))
                throw new InvalidOperationException(PlacementFailurePrefix
                    + $" {label} at {target} is not observable from any reachable adjacent cell.");
        }

        private static bool HasVisibleAdjacentCell(
            Vector2Int target,
            HashSet<Vector2Int> reachable,
            IFogVisibilityResolver visibilityResolver,
            MenuWorldPreviewData world)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var cell = target + new Vector2Int(dx, dy);
                    if (!reachable.Contains(cell)) continue;
                    var visible = visibilityResolver.ComputeVisibleTiles(
                        cell, VisibilityProbeRange, world.Width, world.Height);
                    for (int i = 0; i < visible.Count; i++)
                        if (visible[i] == target) return true;
                }
            return false;
        }

        private static IEnumerable<Vector2Int> CellsAround(Vector2Int center, int radius)
        {
            var cells = new List<Vector2Int>((radius + 1) * (radius + 1));
            for (int y = center.y - radius; y <= center.y + radius; y++)
                for (int x = center.x - radius; x <= center.x + radius; x++)
                    cells.Add(new Vector2Int(x, y));
            cells.Sort((a, b) =>
            {
                int da = Mathf.Abs(a.x - center.x) + Mathf.Abs(a.y - center.y);
                int db = Mathf.Abs(b.x - center.x) + Mathf.Abs(b.y - center.y);
                int byDistance = da.CompareTo(db);
                if (byDistance != 0) return byDistance;
                int byX = a.x.CompareTo(b.x);
                return byX != 0 ? byX : a.y.CompareTo(b.y);
            });
            return cells;
        }

        private static bool InBounds(MenuWorldPreviewData world, Vector2Int cell)
            => cell.x >= 0 && cell.y >= 0 && cell.x < world.Width && cell.y < world.Height;

        private static int ResourceScore(MenuWorldPreviewData world, Vector2Int center)
        {
            int score = 0;
            foreach (var cell in CellsAround(center, 5))
            {
                if (!InBounds(world, cell)) continue;
                string tile = (world.BiomeMap[cell.x, cell.y] ?? string.Empty).ToLowerInvariant();
                if (tile.Contains("forest") || tile.Contains("wood")) score += 3;
                else if (tile.Contains("mountain") || tile.Contains("hill") || tile.Contains("rock")) score += 3;
                else if (tile.Contains("grass") || tile.Contains("lowland")) score += 1;
            }
            return score;
        }
    }
}
