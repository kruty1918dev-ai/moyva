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
        /// <summary>Where the goal unit comes from: an owned operational
        /// recruiter, one under construction, a buildable recruiter or an
        /// honest "no source" entry.</summary>
        UnitSource = 5,
        /// <summary>A settlement is registered at the position but the
        /// canonical resolver cannot fund from it — it is inactive or owned
        /// by another faction.</summary>
        InactiveSettlement = 6,
        /// <summary>The goal owner is not the active turn owner — actions
        /// stay queued until the turn comes back.</summary>
        TurnWait = 7,
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
        /// <summary>A supply order already carries this resource to the
        /// site — open the supply panel instead of sending another wagon.</summary>
        AwaitDelivery = 8,
        /// <summary>The pending placement is blocked on this tile — focus it
        /// so the outline can be moved or cancelled.</summary>
        MovePending = 9,
        /// <summary>A trained unit waits in the building's queue — open the
        /// queue to deploy it.</summary>
        DeployReady = 10,
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
        /// <summary>Resource id for Resource/Population-food blockers; unit
        /// type id for UnitSource blockers.</summary>
        public string ResourceId = string.Empty;
        /// <summary>Recruiter building id for UnitSource blockers — resolved
        /// to a display name by the read model.</summary>
        public string BuildingId = string.Empty;
        public float Required;
        public float Available;
        public float Reserved;
        /// <summary>Position the blocker belongs to when it differs from the
        /// goal position — used by Refresh for per-placement checks.</summary>
        public Vector2Int? Position;
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

            // Not the goal owner's turn — every fix waits; say so first.
            AddTurnWaitBlocker(model, ownerId);

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
                    var spatial = new GuidanceBlocker
                    {
                        Kind = GuidanceBlockerKind.Placement,
                        Title = BuildBlockerTitle(buildingId, position),
                        Detail = status.ErrorMessage,
                        Required = 1f,
                        Position = position,
                    };
                    spatial.Options.Add(new GuidanceOption(
                        GuidanceOptionKind.MovePending,
                        buildingId,
                        "Focus this tile — move the pending outline somewhere valid or cancel it.",
                        buildingId, null, position));
                    model.Blockers.Add(spatial);
                }

                if (!projection.HasSettlement
                    && !TryAddInactiveSettlementBlocker(model, ownerId, position)
                    && projection.HasDeficit
                    && (projection.Balances == null || projection.Balances.Count == 0))
                {
                    // Genuine "nobody funds this" — owner-pool funded
                    // placements (start state) legitimately have no
                    // settlement, and deficit-bearing ones are already
                    // covered by their resource blockers.
                    model.Blockers.Add(new GuidanceBlocker
                    {
                        Kind = GuidanceBlockerKind.Eligibility,
                        Title = BuildBlockerTitle(buildingId, position),
                        Detail = string.IsNullOrWhiteSpace(projection.Message)
                            ? "No settlement funds this placement."
                            : projection.Message,
                        Required = 1f,
                        Position = position,
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
                        Position = position,
                    };
                    float inbound = supplyUseful
                        ? InTransitSupply(position, balance.ResourceId)
                        : 0f;
                    blocker.Options.AddRange(ResolveResourceOptions(
                        ownerId, projection.SettlementId, balance.ResourceId,
                        position, supplyUseful, inbound, -balance.Remaining));
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
            AddTurnWaitBlocker(model, ownerId);

            var query = _recruitment as IUnitRecruitmentQuery;
            if (query != null)
            {
                string settlementId = ResolveFundingSettlement(
                    model, ownerId, buildingPosition);

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
                                Position = buildingPosition,
                            };
                            if (shortage.PopulationBlocker == PopulationGrowthBlocker.Housing)
                            {
                                blocker.Detail = "Units are recruited from free residents — housing is full, so residents cannot grow.";
                                string housing = HousingBuildingId(ownerId, buildingPosition);
                                if (!string.IsNullOrWhiteSpace(housing))
                                    blocker.Options.Add(new GuidanceOption(
                                        GuidanceOptionKind.BuildHousing,
                                        string.Empty, null, housing));
                            }
                            else if (shortage.PopulationBlocker == PopulationGrowthBlocker.Food)
                            {
                                blocker.Detail = "Units are recruited from free residents — food stock is empty, so residents starve."
                                    + FoodReserveSuffix(ownerId, buildingPosition);
                                AddFoodProducerOption(blocker, ownerId, settlementId, feasibility);
                            }
                            else
                            {
                                blocker.Detail = "No free residents in this settlement — units are recruited from free residents."
                                    + FoodReserveSuffix(ownerId, buildingPosition);
                                AddFoodProducerOption(blocker, ownerId, settlementId, feasibility);
                            }
                            if (shortage.Reserved > Epsilon)
                                blocker.Options.Add(new GuidanceOption(
                                    GuidanceOptionKind.OpenQueue,
                                    string.Empty,
                                    "Reserved by queued training — cancel entries to free them."));
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
                            Position = buildingPosition,
                        };
                        resourceBlocker.Options.AddRange(ResolveResourceOptions(
                            ownerId, settlementId, shortage.ResourceId,
                            null, supplyUseful: false, inbound: 0f, missing: 0f));
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
                        Position = buildingPosition,
                    });
                }
            }

            // Unit source: which building trains this unit and how to get
            // one — informational when a recruiter already exists, a real
            // blocker when none is placed or buildable. Always added after
            // the shortage blockers so the "?" help entry still explains the
            // source without displacing the actionable shortages.
            AddUnitSourceBlocker(model, ownerId, unitTypeId, buildingPosition);

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
                    case GuidanceBlockerKind.UnitSource:
                        blocker.Resolved = UnitSourceAvailable(
                            ownerId, blocker.ResourceId);
                        break;
                    case GuidanceBlockerKind.InactiveSettlement:
                        blocker.Resolved = blocker.Position.HasValue
                            && _economyInfo != null
                            && _economyInfo.TryResolveConstructionSettlement(
                                blocker.Position.Value, ownerId, out _);
                        break;
                    case GuidanceBlockerKind.TurnWait:
                        string active = _construction?.GetActiveOwner();
                        blocker.Resolved = string.IsNullOrWhiteSpace(active)
                            || string.Equals(active, ownerId,
                                StringComparison.Ordinal);
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
            IReadOnlyDictionary<string, float> totals;
            if (!string.IsNullOrWhiteSpace(settlementId))
            {
                // Stock inside an inactive settlement cannot fund anything —
                // the settlement list is authoritative for liveness.
                if (!IsActiveSettlement(ownerId, settlementId))
                    return false;
                totals = _economyInfo.GetSettlementAvailableResourceTotals(settlementId);
            }
            else
            {
                totals = _economyInfo.GetOwnerResourceTotals(ownerId);
            }
            return totals != null
                && totals.TryGetValue(resourceId, out float available)
                && available + Epsilon >= required;
        }

        /// <summary>Settlement is usable only while it shows up in the
        /// owner's active settlement list — an unlisted id is dead stock,
        /// not spendable coverage.</summary>
        private bool IsActiveSettlement(string ownerId, string settlementId)
        {
            var settlements = _economy?.GetOwnerSettlementSnapshots(ownerId);
            if (settlements == null)
                return true; // no liveness source — keep historical behavior
            for (int i = 0; i < settlements.Count; i++)
                if (string.Equals(settlements[i].SettlementId, settlementId,
                        StringComparison.Ordinal))
                    return true;
            return false;
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
        /// producer or prerequisite via the bounded feasibility search, an
        /// already-inbound supply order when one covers the gap, a
        /// supply-wagon alternative when the placement is settlement-funded,
        /// or an honest "unobtainable" entry.</summary>
        private List<GuidanceOption> ResolveResourceOptions(
            string ownerId, string settlementId, string resourceId,
            Vector2Int? supplyPosition, bool supplyUseful,
            float inbound = 0f, float missing = 0f)
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
                if (inbound > Epsilon)
                {
                    options.Add(new GuidanceOption(
                        GuidanceOptionKind.AwaitDelivery,
                        resourceId,
                        "A supply wagon is already bringing it — check the order.",
                        null, resourceId, supplyPosition));
                }
                // Sending a second wagon for a fully covered gap is wasteful —
                // offer dispatch only for the part not already inbound.
                if (inbound + Epsilon < missing)
                {
                    options.Add(new GuidanceOption(
                        GuidanceOptionKind.OpenSupply,
                        resourceId,
                        "Send it to this settlement by supply wagon.",
                        null, resourceId, supplyPosition));
                }
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

        /// <summary>Not-the-owner's-turn blocker: all fixes wait for the turn
        /// to come back, so this leads the blocker list.</summary>
        private void AddTurnWaitBlocker(GuidanceModel model, string ownerId)
        {
            if (model == null || _construction == null
                || string.IsNullOrWhiteSpace(ownerId))
                return;
            string active = _construction.GetActiveOwner();
            if (string.IsNullOrWhiteSpace(active)
                || string.Equals(active, ownerId, StringComparison.Ordinal))
                return;
            model.Blockers.Add(new GuidanceBlocker
            {
                Kind = GuidanceBlockerKind.TurnWait,
                Title = "Turn",
                Detail = $"It is not your turn — '{active}' is acting. "
                    + "These steps stay queued until your turn begins.",
                Required = 1f,
            });
        }

        /// <summary>A settlement is registered at the position but cannot
        /// fund the owner — it is inactive or belongs to another faction.
        /// Returns true when such a blocker was added.</summary>
        private bool TryAddInactiveSettlementBlocker(GuidanceModel model,
            string ownerId, Vector2Int position)
        {
            if (model == null || _economyInfo == null
                || !_economyInfo.TryGetSettlementContext(position, out var registered)
                || string.IsNullOrWhiteSpace(registered.SettlementId))
                return false;

            // The canonical funding resolver only returns active settlements
            // owned by this player; a registered id that does not resolve to
            // itself is dead stock, not a funder.
            bool fundsItself = _economyInfo.TryResolveConstructionSettlement(
                position, ownerId, out var resolved)
                && string.Equals(resolved.SettlementId, registered.SettlementId,
                    StringComparison.Ordinal);
            if (fundsItself)
                return false;

            model.Blockers.Add(new GuidanceBlocker
            {
                Kind = GuidanceBlockerKind.InactiveSettlement,
                Title = string.IsNullOrWhiteSpace(registered.SettlementName)
                    ? "Settlement" : registered.SettlementName,
                Detail = "This settlement cannot supply construction or workers — "
                    + "it is inactive or belongs to another faction.",
                Required = 1f,
                Position = position,
            });
            return true;
        }

        /// <summary>Funding settlement for recruitment guidance: the
        /// position-registered settlement when it is the active owner-funded
        /// one; otherwise the nearest active owned settlement (and an
        /// InactiveSettlement blocker explaining the dead one).</summary>
        private string ResolveFundingSettlement(GuidanceModel model,
            string ownerId, Vector2Int position)
        {
            if (_economyInfo == null
                || !_economyInfo.TryGetSettlementContext(position, out var ctx))
                return null;

            bool fundsItself = _economyInfo.TryResolveConstructionSettlement(
                position, ownerId, out var resolved)
                && string.Equals(resolved.SettlementId, ctx.SettlementId,
                    StringComparison.Ordinal);
            if (fundsItself)
                return ctx.SettlementId;

            model.Blockers.Add(new GuidanceBlocker
            {
                Kind = GuidanceBlockerKind.InactiveSettlement,
                Title = string.IsNullOrWhiteSpace(ctx.SettlementName)
                    ? "Settlement" : ctx.SettlementName,
                Detail = "This settlement cannot supply workers or resources — "
                    + "it is inactive or belongs to another faction.",
                Required = 1f,
                Position = position,
            });
            return string.IsNullOrWhiteSpace(resolved.SettlementId)
                ? null
                : resolved.SettlementId;
        }

        /// <summary>Undelivered amount of a resource already dispatched to a
        /// pending placement — honest "in transit" figure for the deficit.</summary>
        private float InTransitSupply(Vector2Int position, string resourceId)
        {
            if (_supply == null || string.IsNullOrWhiteSpace(resourceId)
                || !_supply.TryGetOrderAt(position, out var order)
                || order.Status != ConstructionSupplyOrderStatus.Active
                || order.Remaining == null)
                return 0f;
            return order.Remaining.TryGetValue(resourceId, out float remaining)
                ? remaining
                : 0f;
        }

        /// <summary>Food stock for a population blocker — only appended when
        /// the snapshot actually knows the reserve; no starvation countdown
        /// is shown because no reliable forecast exists.</summary>
        private string FoodReserveSuffix(string ownerId, Vector2Int position)
        {
            if (_economyInfo == null)
                return string.Empty;
            float food = _economyInfo
                .GetRecruitmentPopulation(ownerId, position).FoodAvailable;
            return food >= 0f
                ? $" Food reserve: {food:0.#}."
                : string.Empty;
        }

        /// <summary>"Where does this unit come from": scans owner placements
        /// for an operational or under-construction recruiter, then the
        /// building registry for a selectable recruiter, and reports the
        /// recipe's population/training cost so the player knows what each
        /// unit consumes. A trained-and-ready queue entry is surfaced as a
        /// deploy action.</summary>
        private void AddUnitSourceBlocker(GuidanceModel model,
            string ownerId, string unitTypeId, Vector2Int buildingPosition)
        {
            if (model == null || string.IsNullOrWhiteSpace(unitTypeId))
                return;

            var blocker = new GuidanceBlocker
            {
                Kind = GuidanceBlockerKind.UnitSource,
                ResourceId = unitTypeId.Trim(),
                Title = "Unit source",
                Required = 1f,
            };

            ProducerLocation operational = null, constructing = null;
            CollectRecruiterLocations(ownerId, blocker.ResourceId,
                ref operational, ref constructing);

            string buildableId = null;
            string anyId = null;
            UnitRecruitmentRecipeDefinition recipe = null;
            if (_buildings != null)
            {
                foreach (var definition in _buildings.GetAll())
                {
                    if (!TryGetUnitRecipe(definition, blocker.ResourceId,
                            out var candidate))
                        continue;
                    anyId ??= definition.Id;
                    recipe ??= candidate;
                    var availability = _availability?.EvaluateSelectionAvailability(
                        definition.Id, ownerId, buildingPosition);
                    if (buildableId == null
                        && (!availability.HasValue || availability.Value.CanSelect))
                        buildableId = definition.Id;
                }
            }

            blocker.BuildingId =
                operational?.BuildingId
                ?? constructing?.BuildingId
                ?? buildableId
                ?? anyId
                ?? string.Empty;

            if (operational != null)
            {
                blocker.Detail = UnitSourceDetail(recipe);
                blocker.Options.Add(new GuidanceOption(
                    GuidanceOptionKind.FocusProducer,
                    operational.BuildingId,
                    "This building trains this unit.",
                    operational.BuildingId, null, operational.Position));
                if (_recruitment != null
                    && _recruitment.TryPeekReady(ownerId, buildingPosition,
                        out var readyItem)
                    && readyItem.IsReady)
                {
                    blocker.Options.Add(new GuidanceOption(
                        GuidanceOptionKind.DeployReady,
                        readyItem.UnitTypeId,
                        "A trained unit is waiting in the queue — deploy it to free the slot.",
                        readyItem.RecruitingBuildingId, null,
                        buildingPosition));
                }
            }
            else if (constructing != null)
            {
                blocker.Detail = UnitSourceDetail(recipe);
                blocker.Options.Add(new GuidanceOption(
                    GuidanceOptionKind.ProducerConstructing,
                    constructing.BuildingId,
                    "Recruiter is under construction — it trains this unit once finished.",
                    constructing.BuildingId, null, constructing.Position));
            }
            else if (!string.IsNullOrWhiteSpace(buildableId))
            {
                blocker.Detail = UnitSourceDetail(recipe);
                blocker.Options.Add(new GuidanceOption(
                    GuidanceOptionKind.BuildProducer,
                    buildableId,
                    "Build this to train the unit.",
                    buildableId));
            }
            else
            {
                blocker.Detail = anyId != null
                    ? "A building that trains this unit exists but cannot be built yet."
                    : "No building can train this unit.";
                blocker.Options.Add(new GuidanceOption(
                    GuidanceOptionKind.Unobtainable,
                    blocker.ResourceId, blocker.Detail));
            }
            model.Blockers.Add(blocker);
        }

        private static string UnitSourceDetail(UnitRecruitmentRecipeDefinition recipe)
        {
            if (recipe == null)
                return "Trained at a recruiter building.";
            return $"Needs {Math.Max(1, recipe.PopulationCost)} free resident(s) "
                + $"per unit and {Math.Max(1, recipe.TrainingTurns)} turn(s) of training.";
        }

        private bool UnitSourceAvailable(string ownerId, string unitTypeId)
        {
            ProducerLocation operational = null, constructing = null;
            CollectRecruiterLocations(ownerId, unitTypeId,
                ref operational, ref constructing);
            return operational != null;
        }

        /// <summary>Finds owner placements whose building can recruit the
        /// unit type, bucketed by operational vs under construction.</summary>
        private void CollectRecruiterLocations(string ownerId, string unitTypeId,
            ref ProducerLocation operational, ref ProducerLocation constructing)
        {
            if (_portfolio == null || _buildings == null
                || string.IsNullOrWhiteSpace(unitTypeId))
                return;
            var placements = _portfolio.GetOwnerPlacements(ownerId);
            if (placements == null)
                return;
            for (int i = 0; i < placements.Count; i++)
            {
                var placement = placements[i];
                if (!TryGetUnitRecipe(
                        _buildings.GetById(placement.BuildingId), unitTypeId, out _))
                    continue;

                if (!(_lifecycle?.IsOperational(placement.Position) ?? true))
                {
                    if (constructing == null)
                    {
                        string detail = "Under construction.";
                        if (_lifecycle != null
                            && _lifecycle.TryGetProgress(
                                placement.Position, out int done, out int total))
                            detail = $"Under construction ({done}/{total}).";
                        constructing = new ProducerLocation
                        {
                            BuildingId = placement.BuildingId,
                            Position = placement.Position,
                            Detail = detail,
                        };
                    }
                    continue;
                }

                if (operational == null)
                {
                    operational = new ProducerLocation
                    {
                        BuildingId = placement.BuildingId,
                        Position = placement.Position,
                    };
                }
            }
        }

        private static bool TryGetUnitRecipe(BuildingDefinition definition,
            string unitTypeId, out UnitRecruitmentRecipeDefinition recipe)
        {
            recipe = null;
            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition, out UnitRecruitmentBuildingModule module)
                || module.Recipes == null)
                return false;
            for (int i = 0; i < module.Recipes.Count; i++)
            {
                var candidate = module.Recipes[i];
                if (candidate == null
                    || !string.Equals(candidate.UnitTypeId?.Trim(),
                        unitTypeId, StringComparison.Ordinal))
                    continue;
                recipe = candidate;
                return true;
            }
            return false;
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
        /// the recruitment card hint so both surfaces agree. The funding
        /// position keeps the affordability check settlement-scoped.</summary>
        private string HousingBuildingId(string ownerId, Vector2Int fundingPosition)
        {
            if (_buildings == null)
                return null;
            foreach (var definition in _buildings.GetAll())
            {
                if (!BuildingDefinitionCapabilities.TryGetEnabledModule(
                        definition, out HousingBuildingModule _))
                    continue;
                var availability = _availability?.EvaluateSelectionAvailability(
                    definition.Id, ownerId, fundingPosition);
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
