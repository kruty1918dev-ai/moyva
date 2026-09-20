using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class MoyvaBotTurnAdapter : IBotTurnGateway
    {
        private readonly ITurnService _turns;
        private readonly ITurnAuthorityPolicy _authority;
        public MoyvaBotTurnAdapter(ITurnService turns, ITurnAuthorityPolicy authority = null)
        { _turns = turns; _authority = authority; }
        public BotGameStamp Read(string player) => new BotGameStamp(_turns.ActiveOwnerId, _turns.GlobalTurn,
            (int)_turns.Phase, (_authority?.IsAuthoritative ?? true) && _turns.CanOwnerAct(player, out _));
        public bool CanEndTurn(string player, out string reason)
        {
            reason = "Missing ITurnEndQuery or authority.";
            return (_authority?.IsAuthoritative ?? true) && _turns is ITurnEndQuery query && query.CanEndTurn(player, out reason);
        }
        public bool EndTurn(string player, out string reason)
        {
            if (!CanEndTurn(player, out reason)) return false;
            return _turns.TryEndTurn(player, out reason);
        }
    }

    public sealed class MoyvaBotPerceptionSource : IBotPerceptionSource
    {
        private readonly ITurnService _turns;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _owners;
        private readonly IUnitGameplayProfileService _profiles;
        private readonly IFogOwnerStateReader _fog;
        private readonly IFogIntelReader _intel;
        private readonly IGeneratedTerrainLevelQuery _terrain;
        private readonly IEconomyInfoMediator _economy;
        private readonly IEconomyRuntimeApi _economyApi;
        private readonly IGridService _grid;
        private readonly IConstructionSaveSnapshotSource _placements;
        private readonly IBuildingRegistry _buildingDefs;
        private readonly IUnitRecruitmentQuery _recruitment;
        private readonly Func<string, IReadOnlyDictionary<string, float>> _productionPerTurn;

        public MoyvaBotPerceptionSource(ITurnService turns, IUnitService units, IUnitOwnershipQuery owners,
            IFogOwnerStateReader fog, IUnitGameplayProfileService profiles = null,
            IGeneratedTerrainLevelQuery terrain = null, IEconomyInfoMediator economy = null,
            IFogIntelReader intel = null, IGridService grid = null,
            IConstructionSaveSnapshotSource placements = null, IBuildingRegistry buildingDefs = null,
            IEconomyRuntimeApi economyApi = null, IUnitRecruitmentQuery recruitment = null,
            Func<string, IReadOnlyDictionary<string, float>> productionPerTurn = null)
        {
            _turns = turns; _units = units; _owners = owners; _fog = fog; _profiles = profiles;
            _terrain = terrain; _economy = economy; _intel = intel; _grid = grid;
            _placements = placements; _buildingDefs = buildingDefs; _economyApi = economyApi;
            _recruitment = recruitment; _productionPerTurn = productionPerTurn;
        }

        public BotPerceptionSnapshot Capture(string player)
        {
            var result = new BotPerceptionSnapshot();
            result.Global[BotObservationSchema.Active] = _turns.IsOwnerActive(player) ? 1 : 0;
            result.Global[BotObservationSchema.Round] = _turns.Round / (float)(_turns.Round + 100);
            result.Global[BotObservationSchema.Phase] = (int)_turns.Phase / 5f;
            int own = 0, visibleOther = 0;
            float visionTotal = 0f, attackTotal = 0f, heightTotal = 0f;
            result.Global[BotObservationSchema.UnitsAvailable] = _units != null && _owners != null ? 1 : 0;
            result.Global[BotObservationSchema.VisibilityAvailable] = _fog != null ? 1 : 0;
            int width = _grid?.GridWidth ?? 0, height = _grid?.GridHeight ?? 0;
            bool spatial = width > 0 && height > 0 && _fog != null;
            result.Global[BotObservationSchema.SpatialAvailable] = spatial ? 1 : 0;
            var ownCells = spatial ? new List<Vector2Int>() : null;
            var enemyCells = spatial ? new List<Vector2Int>() : null;
            if (_units != null && _owners != null)
                foreach (string id in _units.GetAllUnitIds())
                {
                    if (_owners.GetUnitOwnerId(id) == player)
                    {
                        own++;
                        var profile = BotUnitTacticalFeatureEncoder.ResolveProfile(_units, _profiles, id);
                        bool positioned = _units.TryGetUnitPosition(id, out var ownPosition);
                        int terrainLevel = positioned
                            ? BotUnitTacticalFeatureEncoder.ResolveTerrainLevel(_terrain, ownPosition)
                            : 0;
                        visionTotal += profile.ResolveVisionRange(terrainLevel, 1, 128);
                        attackTotal += profile.CuttingDamage + profile.PenetratingDamage + profile.CrushingDamage;
                        heightTotal += terrainLevel;
                        if (positioned) ownCells?.Add(ownPosition);
                    }
                    else if (_fog != null && _units.TryGetUnitPosition(id, out var position) && _fog.IsVisible(player, position))
                    {
                        visibleOther++;
                        enemyCells?.Add(position);
                    }
                }
            result.Global[BotObservationSchema.OwnUnits] = own / (float)(own + 100);
            result.Global[BotObservationSchema.VisibleOtherUnits] = visibleOther / (float)(visibleOther + 100);
            result.Global[BotObservationSchema.TacticalVision] = Normalize(visionTotal, own * 12f);
            result.Global[BotObservationSchema.TacticalAttack] = Normalize(attackTotal, own * 30f);
            result.Global[BotObservationSchema.TacticalHeight] = Normalize(heightTotal, own * 4f);
            var rememberedUnitCells = spatial ? new List<Vector2Int>() : null;
            var rememberedBuildingCells = spatial ? new List<Vector2Int>() : null;
            if (_intel != null)
            {
                // Last-known intel: remembered hostiles inform scouting and
                // attack decisions without exposing hidden current state.
                int rememberedUnits = 0, rememberedBuildings = 0;
                var units = _intel.GetRememberedUnits(player);
                if (units != null)
                    foreach (var record in units)
                        if (record != null
                            && !string.Equals(record.OwnerId, player, System.StringComparison.Ordinal))
                        {
                            rememberedUnits++;
                            // Cells visible right now are already counted from
                            // live state; intel only adds not-currently-seen cells.
                            if (spatial && !_fog.IsVisible(player, record.LastKnownPosition))
                                rememberedUnitCells.Add(record.LastKnownPosition);
                        }
                var buildings = _intel.GetRememberedBuildings(player);
                if (buildings != null)
                    foreach (var record in buildings)
                        if (record != null
                            && !string.Equals(record.OwnerId, player, System.StringComparison.Ordinal))
                        {
                            rememberedBuildings++;
                            if (spatial && !_fog.IsVisible(player, record.Position))
                                rememberedBuildingCells.Add(record.Position);
                        }
                result.Global[BotObservationSchema.RememberedEnemyUnits] = rememberedUnits / (float)(rememberedUnits + 100);
                result.Global[BotObservationSchema.RememberedEnemyBuildings] = rememberedBuildings / (float)(rememberedBuildings + 100);
            }
            WriteEconomy(player, result);
            if (spatial)
                WriteSpatial(player, result, width, height, ownCells, enemyCells,
                    rememberedUnitCells, rememberedBuildingCells);
            return result;
        }

        private void WriteEconomy(string player, BotPerceptionSnapshot result)
        {
            int ownSettlements = 0, visibleEnemySettlements = 0;
            if (_placements != null)
                foreach (var placement in _placements.GetSavedPlacements())
                {
                    bool owned = string.Equals(placement.OwnerId, player, StringComparison.Ordinal);
                    bool center = IsSettlementCenter(placement.BuildingId);
                    if (owned)
                    {
                        if (center) ownSettlements++;
                    }
                    else if (center && _fog != null && _fog.IsVisible(player, placement.Position))
                        visibleEnemySettlements++;
                }
            if (_economyApi != null)
            {
                var ids = _economyApi.GetSettlementIdsForOwner(player);
                if (ids != null) ownSettlements = Math.Max(ownSettlements, ids.Count);
                int population = 0;
                var snapshots = _economyApi.GetOwnerSettlementSnapshots(player);
                if (snapshots != null)
                    foreach (var snapshot in snapshots) population += Math.Max(0, snapshot.Population);
                result.Global[BotObservationSchema.PopulationAvailable] = Normalize(population, 40);
            }
            if (_recruitment != null)
            {
                int available = 0;
                var options = _recruitment.GetOptions(player);
                if (options != null)
                    foreach (var option in options) available = Math.Max(available, option.AvailablePopulation);
                if (available > 0)
                    result.Global[BotObservationSchema.PopulationAvailable] =
                        Math.Max(result.Global[BotObservationSchema.PopulationAvailable], Normalize(available, 40));
            }
            if (ownSettlements > 0 || _economyApi != null)
                result.Global[BotObservationSchema.OwnSettlements] = ownSettlements / (float)(ownSettlements + 8);
            result.Global[BotObservationSchema.VisibleEnemySettlements] =
                visibleEnemySettlements / (float)(visibleEnemySettlements + 8);
            if (_productionPerTurn != null)
            {
                var production = _productionPerTurn(player);
                double total = 0;
                if (production != null)
                    foreach (var pair in production)
                        if (!float.IsNaN(pair.Value) && !float.IsInfinity(pair.Value))
                            total += Math.Max(0, pair.Value);
                result.Global[BotObservationSchema.ProductionEstimate] = Normalize(total, 60);
            }
            result.Global[BotObservationSchema.EconomyAvailable] = _economy != null ? 1 : 0;
            if (_economy == null) return;
            var resources = _economy.GetOwnerResourceTotals(player);
            double sum = 0, pool = 0;
            if (resources != null) foreach (var value in resources.Values)
                if (!float.IsNaN(value) && !float.IsInfinity(value)) sum += System.Math.Max(0, value);
            var poolResources = _economy.GetOwnerPoolResourceTotals(player);
            if (poolResources != null) foreach (var value in poolResources.Values)
                if (!float.IsNaN(value) && !float.IsInfinity(value)) pool += System.Math.Max(0, value);
            result.Global[BotObservationSchema.OwnResourcesTotal] = Normalize(sum, 1000);
            result.Global[BotObservationSchema.PoolResourcesTotal] = Normalize(pool, 1000);
            result.Global[BotObservationSchema.ResourceKinds] = (resources?.Count ?? 0) / (float)((resources?.Count ?? 0) + 32);
            result.Global[BotObservationSchema.ResourceFood] = Resource(resources, BotDecisionContract.Spec.Aliases(BotObservationSchema.ResourceFood));
            result.Global[BotObservationSchema.ResourceWood] = Resource(resources, BotDecisionContract.Spec.Aliases(BotObservationSchema.ResourceWood));
            result.Global[BotObservationSchema.ResourceStone] = Resource(resources, BotDecisionContract.Spec.Aliases(BotObservationSchema.ResourceStone));
            result.Global[BotObservationSchema.ResourceIron] = Resource(resources, BotDecisionContract.Spec.Aliases(BotObservationSchema.ResourceIron));
            result.Global[BotObservationSchema.ResourceGold] = Resource(resources, BotDecisionContract.Spec.Aliases(BotObservationSchema.ResourceGold));
        }

        private void WriteSpatial(string player, BotPerceptionSnapshot result, int width, int height,
            List<Vector2Int> ownCells, List<Vector2Int> enemyCells,
            List<Vector2Int> rememberedUnitCells, List<Vector2Int> rememberedBuildingCells)
        {
            // Fog state grid first: explored/visible/frontier drive every other
            // channel's legality (terrain is only "known" where explored).
            var fogState = new FogStateType[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    fogState[y * width + x] = _fog.GetFogState(player, new Vector2Int(x, y));
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var state = fogState[y * width + x];
                    int cell = BotSpatialSummaryEncoder.Cell(x, y, width, height);
                    if (state == FogStateType.Unexplored) continue;
                    BotSpatialSummaryEncoder.Set(result, BotSpatialChannel.Explored, cell, 1f);
                    if (state == FogStateType.Visible)
                        BotSpatialSummaryEncoder.Set(result, BotSpatialChannel.Visible, cell, 1f);
                    else
                        BotSpatialSummaryEncoder.Set(result, BotSpatialChannel.Frontier, cell, 1f);
                    if (_terrain != null && _terrain.TryGetTerrainLevel(new Vector2Int(x, y), out int level))
                        BotSpatialSummaryEncoder.Set(result, BotSpatialChannel.TerrainHeight, cell,
                            Math.Max(result.Spatial[BotSpatialChannel.TerrainHeight * 64 + cell], level / 4f));
                }
            foreach (var position in ownCells)
                BotSpatialSummaryEncoder.AddDensity(result, BotSpatialChannel.OwnUnits,
                    BotSpatialSummaryEncoder.Cell(position.x, position.y, width, height));
            foreach (var position in enemyCells)
                BotSpatialSummaryEncoder.AddDensity(result, BotSpatialChannel.VisibleEnemyUnits,
                    BotSpatialSummaryEncoder.Cell(position.x, position.y, width, height));
            foreach (var position in rememberedUnitCells)
                BotSpatialSummaryEncoder.AddDensity(result, BotSpatialChannel.RememberedEnemyUnits,
                    BotSpatialSummaryEncoder.Cell(position.x, position.y, width, height));
            foreach (var position in rememberedBuildingCells)
                BotSpatialSummaryEncoder.Set(result, BotSpatialChannel.RememberedEnemyBuildings,
                    BotSpatialSummaryEncoder.Cell(position.x, position.y, width, height), 1f);
            if (_placements != null)
                foreach (var placement in _placements.GetSavedPlacements())
                {
                    int px = placement.Position.x, py = placement.Position.y;
                    if (px < 0 || py < 0 || px >= width || py >= height) continue;
                    int cell = BotSpatialSummaryEncoder.Cell(px, py, width, height);
                    if (string.Equals(placement.OwnerId, player, StringComparison.Ordinal))
                        BotSpatialSummaryEncoder.Set(result, BotSpatialChannel.OwnBuildings, cell, 1f);
                    else if (fogState[py * width + px] == FogStateType.Visible)
                        BotSpatialSummaryEncoder.Set(result, BotSpatialChannel.VisibleEnemyBuildings, cell, 1f);
                }
        }

        private bool IsSettlementCenter(string buildingId)
        {
            var definition = _buildingDefs?.GetById(buildingId);
            return definition != null
                && (BuildingDefinitionCapabilities.IsTownHall(definition)
                    || BuildingDefinitionCapabilities.IsCastle(definition));
        }

        private static float Normalize(double value, double scale)
        {
            value = Math.Max(0, value);
            return (float)(value / (value + Math.Max(1, scale)));
        }

        private static float Resource(System.Collections.Generic.IReadOnlyDictionary<string, float> resources, params string[] names)
        {
            if (resources == null || names == null) return 0;
            foreach (var pair in resources)
            {
                if (string.IsNullOrWhiteSpace(pair.Key)) continue;
                string key = pair.Key.ToLowerInvariant();
                if (!names.Any(key.Contains)) continue;
                return Normalize(pair.Value, 250);
            }
            return 0;
        }
    }
}
