using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal enum GuidanceGoalKind
    {
        None = 0,
        Placement = 1,
        Recruitment = 2,
    }

    internal enum GuidanceBlockerKind
    {
        Resource = 0,
        Population = 1,
        Placement = 2,
        Eligibility = 3,
        Generic = 4,
    }

    internal enum GuidanceOptionKind
    {
        Unobtainable = 0,
        /// <summary>Open construction focused on a building that produces the
        /// missing resource (or its achievable prerequisite).</summary>
        BuildProducer = 1,
        /// <summary>A placed producer already exists — focus it on the map and
        /// open its panel.</summary>
        FocusProducer = 2,
        /// <summary>A placed producer exists but is still being built.</summary>
        ProducerConstructing = 3,
        /// <summary>Send the missing resource to the funding settlement by
        /// supply wagon.</summary>
        OpenSupply = 4,
        /// <summary>Resources are reserved by queued training — open the queue
        /// so the player can cancel entries and free them.</summary>
        OpenQueue = 5,
        /// <summary>Population is short because beds are full — build a
        /// housing building.</summary>
        BuildHousing = 6,
        /// <summary>Population is short because food stock is empty — build a
        /// food producer.</summary>
        ProduceFood = 7,
    }

    /// <summary>What the player originally tried to do, kept so guidance can
    /// return them to it once blockers clear.</summary>
    internal sealed class GuidanceGoal
    {
        public GuidanceGoalKind Kind = GuidanceGoalKind.None;
        /// <summary>Building id for Placement goals.</summary>
        public string BuildingId = string.Empty;
        /// <summary>Unit type id for Recruitment goals.</summary>
        public string UnitTypeId = string.Empty;
        /// <summary>Placement tile, or the recruiting building's tile.</summary>
        public Vector2Int Position;
        /// <summary>Funding settlement for Placement goals (may be null for
        /// owner-pool funding).</summary>
        public string SettlementId;
        /// <summary>How many pending placements the goal covers.</summary>
        public int PlacementCount = 1;
    }

    internal readonly struct GuidanceOption
    {
        public GuidanceOption(GuidanceOptionKind kind, string label,
            string detail = null, string buildingId = null,
            string resourceId = null, Vector2Int? position = null)
        {
            Kind = kind;
            Label = label ?? string.Empty;
            Detail = detail ?? string.Empty;
            BuildingId = buildingId ?? string.Empty;
            ResourceId = resourceId ?? string.Empty;
            Position = position;
        }

        public GuidanceOptionKind Kind { get; }
        public string Label { get; }
        public string Detail { get; }
        public string BuildingId { get; }
        public string ResourceId { get; }
        public Vector2Int? Position { get; }
    }

    internal sealed class GuidanceBlocker
    {
        public GuidanceBlockerKind Kind;
        public string Title = string.Empty;
        public string Detail = string.Empty;
        /// <summary>Resource id for Resource/Population-food blockers.</summary>
        public string ResourceId = string.Empty;
        public float Required;
        public float Available;
        public float Reserved;
        /// <summary>Whether the blocking condition is satisfied right now —
        /// recomputed on every capture so the popup marks completed items.</summary>
        public bool Resolved;
        public List<GuidanceOption> Options = new List<GuidanceOption>();
        public float Missing => Required > Available ? Required - Available : 0f;
    }

    /// <summary>Snapshot-independent guidance model; the read model rebuilds
    /// the blocker states each capture so changes in economy/live state are
    /// reflected without extra bookkeeping.</summary>
    internal sealed class GuidanceModel
    {
        public GuidanceGoal Goal;
        public List<GuidanceBlocker> Blockers = new List<GuidanceBlocker>();
        public int PendingCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Blockers.Count; i++)
                    if (!Blockers[i].Resolved) count++;
                return count;
            }
        }
        public bool AllResolved => Blockers.Count > 0 && PendingCount == 0;
    }

    /// <summary>
    /// Builds the shared "why can't I do this and how do I fix it" model from
    /// canonical gameplay queries only: construction resource projection,
    /// pending-placement status, selection availability, recruitment
    /// shortages, producer feasibility and placed-building state. It never
    /// mutates gameplay state and never validates an action twice — the
    /// authoritative services remain the only mutation path.
    /// </summary>
    internal sealed class GameplayGuidanceResolver
    {
        private const float Epsilon = 0.0001f;

        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionSessionCommands _construction;
        private readonly IConstructionSelectionAvailabilityQuery _availability;
        private readonly IConstructionPortfolioQuery _portfolio;
        private readonly IConstructionLifecycle _lifecycle;
        private readonly IEconomyInfoMediator _economyInfo;
        private readonly IEconomyRuntimeApi _economy;
        private readonly EconomyDatabaseSO _database;
        private readonly IConstructionSupplyService _supply;
        private readonly IUnitRecruitmentService _recruitment;

        public GameplayGuidanceResolver(
            IBuildingRegistry buildings,
            IConstructionSessionCommands construction,
            IConstructionSelectionAvailabilityQuery availability,
            IConstructionPortfolioQuery portfolio,
            IConstructionLifecycle lifecycle,
            IEconomyInfoMediator economyInfo,
            IEconomyRuntimeApi economy,
            EconomyDatabaseSO database,
            IConstructionSupplyService supply,
            IUnitRecruitmentService recruitment = null)
        {
            _buildings = buildings;
            _construction = construction;
            _availability = availability;
            _portfolio = portfolio;
            _lifecycle = lifecycle;
            _economyInfo = economyInfo;
            _economy = economy;
            _database = database;
            _supply = supply;
            _recruitment = recruitment;
        }

        /// <summary>Blockers for a rejected placement confirm: per pending
        /// placement — spatial/status reason plus every deficit resource from
        /// its canonical resource projection (which already includes other
        /// pending placements' reservations).</summary>
        public GuidanceModel BuildPlacement(string ownerId,
            IReadOnlyDictionary<Vector2Int, string> pending)
        {
            var model = new GuidanceModel
            {
                Goal = new GuidanceGoal
                {
                    Kind = GuidanceGoalKind.Placement,
                    PlacementCount = pending?.Count ?? 0,
                },
            };
            if (pending == null || pending.Count == 0 || _construction == null)
                return model;

            var feasibility = new ProducerFeasibilityResolver(
                _buildings, _availability, _construction, _economyInfo);
            bool first = true;

            foreach (var pair in pending)
            {
                Vector2Int position = pair.Key;
                string buildingId = pair.Value;
                if (first)
                {
                    model.Goal.Position = position;
                    model.Goal.BuildingId = buildingId ?? string.Empty;
                    first = false;
                }

                _construction.TryGetPendingPlacementStatus(position, out var status);
                ConstructionResourceProjection projection =
                    _construction.GetResourceProjection(position)
                    ?? ConstructionResourceProjection.Empty;
                if (string.IsNullOrWhiteSpace(model.Goal.SettlementId)
                    && projection.HasSettlement)
                    model.Goal.SettlementId = projection.SettlementId;

                // Spatial/status failure that is not a resource deficit.
                bool hasResourceDeficit = projection.HasDeficit;
                if (!string.IsNullOrWhiteSpace(status.ErrorMessage)
                    && !hasResourceDeficit)
                {
                    model.Blockers.Add(new GuidanceBlocker
                    {
                        Kind = GuidanceBlockerKind.Placement,
                        Title = BuildBlockerTitle(buildingId, position),
                        Detail = status.ErrorMessage,
                        Required = 1f,
                    });
                }

                if (!projection.HasSettlement)
                {
                    model.Blockers.Add(new GuidanceBlocker
                    {
                        Kind = GuidanceBlockerKind.Eligibility,
                        Title = BuildBlockerTitle(buildingId, position),
                        Detail = string.IsNullOrWhiteSpace(projection.Message)
                            ? "No settlement funds this placement."
                            : projection.Message,
                        Required = 1f,
                    });
                }

                if (!hasResourceDeficit || projection.Balances == null)
                    continue;

                bool supplyUseful = status.HasSettlement && !status.IsAffordable;
                for (int i = 0; i < projection.Balances.Count; i++)
                {
                    var balance = projection.Balances[i];
                    if (!balance.IsDeficit)
                        continue;
                    var blocker = new GuidanceBlocker
                    {
                        Kind = GuidanceBlockerKind.Resource,
                        ResourceId = balance.ResourceId,
                        Title = BuildBlockerTitle(buildingId, position),
                        Required = balance.Reserved,
                        Available = balance.Available,
                        Reserved = balance.Reserved,
                    };
                    blocker.Options.AddRange(ResolveResourceOptions(
                        ownerId, projection.SettlementId, balance.ResourceId,
                        position, supplyUseful));
                    model.Blockers.Add(blocker);
                }
            }

            Refresh(model, ownerId);
            return model;
        }

        /// <summary>Blockers for a rejected recruitment enqueue: every
        /// resource/population shortage from the canonical query; a non-shortage
        /// rejection reason becomes a Generic blocker.</summary>
        public GuidanceModel BuildRecruitment(string ownerId,
            Vector2Int buildingPosition, string unitTypeId)
        {
            var model = new GuidanceModel
            {
                Goal = new GuidanceGoal
                {
                    Kind = GuidanceGoalKind.Recruitment,
                    UnitTypeId = unitTypeId ?? string.Empty,
                    Position = buildingPosition,
                },
            };
            var query = _recruitment as IUnitRecruitmentQuery;
            if (query == null)
                return model;

            string settlementId = null;
            if (_economyInfo != null
                && _economyInfo.TryGetSettlementContext(buildingPosition, out var ctx))
                settlementId = ctx.SettlementId;

            query.TryGetEnqueueShortages(ownerId, buildingPosition, unitTypeId,
                out IReadOnlyList<UnitRecruitmentShortage> shortages,
                out string reason);

            var feasibility = new ProducerFeasibilityResolver(
                _buildings, _availability, _construction, _economyInfo);

            if (shortages != null)
            {
                for (int i = 0; i < shortages.Count; i++)
                {
                    var shortage = shortages[i];
                    if (shortage.IsPopulation)
                    {
                        var blocker = new GuidanceBlocker
                        {
                            Kind = GuidanceBlockerKind.Population,
                            Title = "Population",
                            Required = shortage.Required,
                            Available = shortage.Available,
                            Reserved = shortage.Reserved,
                        };
                        if (shortage.PopulationBlocker == PopulationGrowthBlocker.Housing)
                        {
                            blocker.Detail = "Housing is full — residents cannot grow.";
                            string housing = HousingBuildingId(ownerId);
                            if (!string.IsNullOrWhiteSpace(housing))
                                blocker.Options.Add(new GuidanceOption(
                                    GuidanceOptionKind.BuildHousing,
                                    string.Empty, null, housing));
                        }
                        else if (shortage.PopulationBlocker == PopulationGrowthBlocker.Food)
                        {
                            blocker.Detail = "Food stock is empty — residents starve.";
                            AddFoodProducerOption(blocker, ownerId, settlementId, feasibility);
                        }
                        else
                        {
                            blocker.Detail = "No free residents in this settlement.";
                            AddFoodProducerOption(blocker, ownerId, settlementId, feasibility);
                        }
                        model.Blockers.Add(blocker);
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(shortage.ResourceId))
                        continue;

                    var resourceBlocker = new GuidanceBlocker
                    {
                        Kind = GuidanceBlockerKind.Resource,
                        ResourceId = shortage.ResourceId,
                        Title = shortage.ResourceId,
                        Required = shortage.Required,
                        Available = shortage.Available,
                        Reserved = shortage.Reserved,
                    };
                    resourceBlocker.Options.AddRange(ResolveResourceOptions(
                        ownerId, settlementId, shortage.ResourceId,
                        null, supplyUseful: false));
                    if (shortage.Reserved > Epsilon)
                        resourceBlocker.Options.Add(new GuidanceOption(
                            GuidanceOptionKind.OpenQueue,
                            string.Empty,
                            "Reserved by queued training — cancel entries to free them."));
                    model.Blockers.Add(resourceBlocker);
                }
            }

            if (!string.IsNullOrWhiteSpace(reason)
                && (shortages == null || shortages.Count == 0))
            {
                model.Blockers.Add(new GuidanceBlocker
                {
                    Kind = GuidanceBlockerKind.Generic,
                    Title = string.IsNullOrWhiteSpace(unitTypeId)
                        ? "Recruitment" : unitTypeId,
                    Detail = reason,
                    Required = 1f,
                });
            }

            Refresh(model, ownerId);
            return model;
        }

        /// <summary>Recomputes each blocker's <see cref="GuidanceBlocker.Resolved"/>
        /// from live state so an open popup marks completed conditions.</summary>
        public void Refresh(GuidanceModel model, string ownerId)
        {
            if (model?.Blockers == null)
                return;
            for (int i = 0; i < model.Blockers.Count; i++)
            {
                var blocker = model.Blockers[i];
                switch (blocker.Kind)
                {
                    case GuidanceBlockerKind.Resource:
                        blocker.Resolved = ResourceCovered(
                            blocker.ResourceId, blocker.Required,
                            ownerId, model.Goal?.SettlementId);
                        break;
                    case GuidanceBlockerKind.Population:
                        blocker.Resolved = PopulationCovered(
                            blocker.Required, ownerId, model.Goal?.Position ?? default);
                        break;
                    case GuidanceBlockerKind.Placement:
                        blocker.Resolved = PlacementCleared(model.Goal);
                        break;
                    default:
                        blocker.Resolved = false;
                        break;
                }
            }
        }

        private bool ResourceCovered(string resourceId, float required,
            string ownerId, string settlementId)
        {
            if (_economyInfo == null || string.IsNullOrWhiteSpace(resourceId))
                return false;
            IReadOnlyDictionary<string, float> totals =
                !string.IsNullOrWhiteSpace(settlementId)
                    ? _economyInfo.GetSettlementAvailableResourceTotals(settlementId)
                    : _economyInfo.GetOwnerResourceTotals(ownerId);
            return totals != null
                && totals.TryGetValue(resourceId, out float available)
                && available + Epsilon >= required;
        }

        private bool PopulationCovered(float required, string ownerId, Vector2Int position)
        {
            if (_economyInfo == null)
                return false;
            return _economyInfo.GetRecruitmentPopulation(ownerId, position).Available
                + Epsilon >= required;
        }

        private bool PlacementCleared(GuidanceGoal goal)
        {
            if (goal == null || goal.Kind != GuidanceGoalKind.Placement
                || _construction == null)
                return false;
            if (!_construction.TryGetPendingPlacementStatus(goal.Position, out var status))
                return false;
            return string.IsNullOrWhiteSpace(status.ErrorMessage);
        }

        /// <summary>Concrete next-step options for one resource deficit:
        /// existing producer (producing/idle/under construction), buildable
        /// producer or prerequisite via the bounded feasibility search, a
        /// supply-wagon alternative when the placement is settlement-funded,
        /// or an honest "unobtainable" entry.</summary>
        private List<GuidanceOption> ResolveResourceOptions(
            string ownerId, string settlementId, string resourceId,
            Vector2Int? supplyPosition, bool supplyUseful)
        {
            var options = new List<GuidanceOption>();
            if (string.IsNullOrWhiteSpace(resourceId))
                return options;

            ProducerLocation producing = null, constructing = null, idle = null;
            CollectProducerLocations(ownerId, resourceId,
                ref producing, ref constructing, ref idle);

            if (producing != null)
            {
                options.Add(new GuidanceOption(
                    GuidanceOptionKind.FocusProducer,
                    producing.BuildingId,
                    "Already producing — open it.",
                    producing.BuildingId, resourceId, producing.Position));
            }
            else if (constructing != null)
            {
                options.Add(new GuidanceOption(
                    GuidanceOptionKind.ProducerConstructing,
                    constructing.BuildingId,
                    constructing.Detail,
                    constructing.BuildingId, resourceId, constructing.Position));
            }
            else if (idle != null)
            {
                options.Add(new GuidanceOption(
                    GuidanceOptionKind.FocusProducer,
                    idle.BuildingId,
                    idle.Detail,
                    idle.BuildingId, resourceId, idle.Position));
            }

            var feasibility = new ProducerFeasibilityResolver(
                _buildings, _availability, _construction, _economyInfo);
            ProducerSuggestion suggestion =
                feasibility.Suggest(ownerId, settlementId, resourceId);
            switch (suggestion.Kind)
            {
                case ProducerSuggestionKind.Direct:
                    options.Add(new GuidanceOption(
                        GuidanceOptionKind.BuildProducer,
                        suggestion.BuildingId, null,
                        suggestion.BuildingId, suggestion.ProducedResourceId));
                    break;
                case ProducerSuggestionKind.ViaPrerequisite:
                    options.Add(new GuidanceOption(
                        GuidanceOptionKind.BuildProducer,
                        suggestion.BuildingId,
                        $"Produces {suggestion.ProducedResourceId} needed for a producer of {resourceId}.",
                        suggestion.BuildingId, suggestion.ProducedResourceId));
                    break;
                case ProducerSuggestionKind.Impossible:
                    options.Add(new GuidanceOption(
                        GuidanceOptionKind.Unobtainable,
                        resourceId,
                        "Producers exist but their own prerequisites cannot be met yet.",
                        null, resourceId));
                    break;
                default:
                    options.Add(new GuidanceOption(
                        GuidanceOptionKind.Unobtainable,
                        resourceId,
                        "No building produces this resource.",
                        null, resourceId));
                    break;
            }

            if (supplyUseful && _supply != null && supplyPosition.HasValue)
            {
                options.Add(new GuidanceOption(
                    GuidanceOptionKind.OpenSupply,
                    resourceId,
                    "Send it to this settlement by supply wagon.",
                    null, resourceId, supplyPosition));
            }

            return options;
        }

        private sealed class ProducerLocation
        {
            public string BuildingId;
            public Vector2Int Position;
            public string Detail;
        }

        /// <summary>Finds placed producers of the resource and buckets them:
        /// producing (active this tick), constructing (placed but not
        /// operational), idle (operational but not producing — staffing and
        /// inputs are checked automatically each tick, so the honest hint is
        /// "needs workers/inputs").</summary>
        private void CollectProducerLocations(string ownerId, string resourceId,
            ref ProducerLocation producing, ref ProducerLocation constructing,
            ref ProducerLocation idle)
        {
            if (_portfolio == null || _buildings == null)
                return;

            var placements = _portfolio.GetOwnerPlacements(ownerId);
            if (placements == null || placements.Count == 0)
                return;

            var active = _economy?.GetOwnerProductionSnapshot(ownerId)
                ?.ActiveProducerBuildingsByType;

            for (int i = 0; i < placements.Count; i++)
            {
                var placement = placements[i];
                var definition = _buildings.GetById(placement.BuildingId);
                if (definition == null)
                    continue;
                var produced = GameplayHudReadModel.ResolveProducedResourceIds(definition);
                bool produces = false;
                for (int p = 0; p < produced.Length; p++)
                    if (string.Equals(produced[p], resourceId, StringComparison.Ordinal))
                    { produces = true; break; }
                if (!produces)
                    continue;

                if (!(_lifecycle?.IsOperational(placement.Position) ?? true))
                {
                    if (constructing == null)
                    {
                        string detail = "Under construction.";
                        if (_lifecycle != null
                            && _lifecycle.TryGetProgress(
                                placement.Position, out int done, out int total))
                        {
                            detail = $"Under construction ({done}/{total}).";
                        }
                        constructing = new ProducerLocation
                        {
                            BuildingId = placement.BuildingId,
                            Position = placement.Position,
                            Detail = detail,
                        };
                    }
                    continue;
                }

                bool activeNow = active != null
                    && active.TryGetValue(placement.BuildingId, out int count)
                    && count > 0;
                if (activeNow && producing == null)
                {
                    producing = new ProducerLocation
                    {
                        BuildingId = placement.BuildingId,
                        Position = placement.Position,
                    };
                }
                else if (!activeNow && idle == null)
                {
                    idle = new ProducerLocation
                    {
                        BuildingId = placement.BuildingId,
                        Position = placement.Position,
                        Detail = "Built but not producing — it needs free workers or input resources (staffing is automatic).",
                    };
                }
            }
        }

        private void AddFoodProducerOption(GuidanceBlocker blocker,
            string ownerId, string settlementId, ProducerFeasibilityResolver feasibility)
        {
            if (_database?.Resources == null || feasibility == null)
                return;
            for (int i = 0; i < _database.Resources.Count; i++)
            {
                var resource = _database.Resources[i];
                if (resource == null
                    || resource.Category != EconomyResourceCategory.Food
                    || string.IsNullOrWhiteSpace(resource.Id))
                    continue;
                ProducerSuggestion suggestion =
                    feasibility.Suggest(ownerId, settlementId, resource.Id);
                if (suggestion.Kind != ProducerSuggestionKind.Direct
                    && suggestion.Kind != ProducerSuggestionKind.ViaPrerequisite)
                    continue;
                blocker.Options.Add(new GuidanceOption(
                    GuidanceOptionKind.ProduceFood,
                    suggestion.BuildingId,
                    $"Produces food ({resource.Id}).",
                    suggestion.BuildingId, suggestion.ProducedResourceId));
                return;
            }
        }

        /// <summary>First selectable building with a housing module — mirrors
        /// the recruitment card hint so both surfaces agree.</summary>
        private string HousingBuildingId(string ownerId)
        {
            if (_buildings == null)
                return null;
            foreach (var definition in _buildings.GetAll())
            {
                if (!BuildingDefinitionCapabilities.TryGetEnabledModule(
                        definition, out HousingBuildingModule _))
                    continue;
                var availability = _availability?.EvaluateSelectionAvailability(
                    definition.Id, ownerId);
                if (availability.HasValue && !availability.Value.CanSelect)
                    continue;
                return definition.Id;
            }
            return null;
        }

        private static string BuildBlockerTitle(string buildingId, Vector2Int position)
            => string.IsNullOrWhiteSpace(buildingId)
                ? $"({position.x},{position.y})"
                : $"{buildingId} ({position.x},{position.y})";
    }
}
