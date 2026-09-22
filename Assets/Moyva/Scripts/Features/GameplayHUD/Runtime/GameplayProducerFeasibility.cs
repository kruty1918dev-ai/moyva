using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal enum ProducerSuggestionKind
    {
        /// <summary>No building in the registry produces the resource at all.</summary>
        None = 0,
        /// <summary>A producer for the requested resource can be built now.</summary>
        Direct = 1,
        /// <summary>Every producer for the requested resource is itself blocked
        /// by a missing resource; the suggestion names a producer of that
        /// prerequisite that can be built now.</summary>
        ViaPrerequisite = 2,
        /// <summary>Producers exist but none is achievable — all are locked or
        /// depend on a resource cycle with no initial stock.</summary>
        Impossible = 3,
    }

    /// <summary>Result of the bounded producer-prerequisite search.</summary>
    internal readonly struct ProducerSuggestion
    {
        public ProducerSuggestion(ProducerSuggestionKind kind, string buildingId,
            string producedResourceId)
        {
            Kind = kind;
            BuildingId = buildingId;
            ProducedResourceId = producedResourceId;
        }

        public ProducerSuggestionKind Kind { get; }
        /// <summary>Building the player can actually construct next.</summary>
        public string BuildingId { get; }
        /// <summary>Resource the suggested building produces — the requested
        /// resource for Direct, the blocking prerequisite for ViaPrerequisite.</summary>
        public string ProducedResourceId { get; }
    }

    /// <summary>
    /// Resolves which producer a shortage hint can honestly recommend.
    /// Walks resource -> producer building -> construction-cost resources with a
    /// visited set and a depth cap, so cyclic prerequisites (A needs B, B needs
    /// A) terminate instead of recursing forever or reporting false availability.
    /// </summary>
    internal sealed class ProducerFeasibilityResolver
    {
        private const int MaxDepth = 4;
        private const float Epsilon = 0.0001f;

        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionSelectionAvailabilityQuery _availability;
        private readonly IConstructionSessionCommands _construction;
        private readonly IEconomyInfoMediator _economy;

        public ProducerFeasibilityResolver(
            IBuildingRegistry buildings,
            IConstructionSelectionAvailabilityQuery availability,
            IConstructionSessionCommands construction,
            IEconomyInfoMediator economy)
        {
            _buildings = buildings;
            _availability = availability;
            _construction = construction;
            _economy = economy;
        }

        public ProducerSuggestion Suggest(string ownerId, string settlementId,
            string resourceId)
        {
            if (_buildings == null || string.IsNullOrWhiteSpace(resourceId))
                return default(ProducerSuggestion);

            string trimmed = resourceId.Trim();
            var visited = new HashSet<string>(StringComparer.Ordinal);
            if (TryFindStep(trimmed, ownerId, settlementId, visited, 0,
                    out string buildingId, out string producedResourceId))
            {
                return new ProducerSuggestion(
                    string.Equals(producedResourceId, trimmed, StringComparison.Ordinal)
                        ? ProducerSuggestionKind.Direct
                        : ProducerSuggestionKind.ViaPrerequisite,
                    buildingId, producedResourceId);
            }

            return new ProducerSuggestion(
                AnyRegisteredProducer(trimmed)
                    ? ProducerSuggestionKind.Impossible
                    : ProducerSuggestionKind.None,
                null, trimmed);
        }

        private bool TryFindStep(string resourceId, string ownerId, string settlementId,
            HashSet<string> visited, int depth,
            out string buildingId, out string producedResourceId)
        {
            buildingId = null;
            producedResourceId = null;
            if (!visited.Add(resourceId) || depth > MaxDepth)
                return false;

            // A producer whose own construction costs are already covered by
            // the funding settlement's free stock is achievable right now.
            foreach (var definition in _buildings.GetAll())
            {
                if (!Produces(definition, resourceId) || !Selectable(definition, ownerId))
                    continue;
                if (FirstUncoveredResource(
                        _construction?.GetBuildingResourceCosts(definition.Id),
                        ownerId, settlementId) == null)
                {
                    buildingId = definition.Id;
                    producedResourceId = resourceId;
                    return true;
                }
            }

            if (depth >= MaxDepth)
                return false;

            // Producers blocked by a missing prerequisite: recurse into each
            // uncovered cost resource. The visited set guarantees a cyclic
            // graph terminates instead of looping A -> B -> A.
            foreach (var definition in _buildings.GetAll())
            {
                if (!Produces(definition, resourceId) || !Selectable(definition, ownerId))
                    continue;
                var costs = _construction?.GetBuildingResourceCosts(definition.Id);
                if (costs == null)
                    continue;
                foreach (var pair in costs)
                {
                    if (pair.Value <= Epsilon
                        || IsCoveredResource(pair.Key, pair.Value, ownerId, settlementId)
                        || visited.Contains(pair.Key))
                        continue;
                    if (TryFindStep(pair.Key, ownerId, settlementId, visited,
                            depth + 1, out buildingId, out producedResourceId))
                        return true;
                }
            }
            return false;
        }

        private bool AnyRegisteredProducer(string resourceId)
        {
            foreach (var definition in _buildings.GetAll())
                if (Produces(definition, resourceId))
                    return true;
            return false;
        }

        private static bool Produces(BuildingDefinition definition, string resourceId)
        {
            var produced = GameplayHudReadModel.ResolveProducedResourceIds(definition);
            for (int i = 0; i < produced.Length; i++)
                if (string.Equals(produced[i], resourceId, StringComparison.Ordinal))
                    return true;
            return false;
        }

        private bool Selectable(BuildingDefinition definition, string ownerId)
        {
            if (_availability == null)
                return true;
            return _availability.EvaluateSelectionAvailability(definition.Id, ownerId).CanSelect;
        }

        private string FirstUncoveredResource(
            IReadOnlyDictionary<string, float> costs, string ownerId, string settlementId)
        {
            if (costs == null)
                return null;
            foreach (var pair in costs)
                if (pair.Value > Epsilon
                    && !IsCoveredResource(pair.Key, pair.Value, ownerId, settlementId))
                    return pair.Key;
            return null;
        }

        private bool IsCoveredResource(string resourceId, float required,
            string ownerId, string settlementId)
        {
            if (_economy == null)
                return true;
            IReadOnlyDictionary<string, float> totals =
                !string.IsNullOrWhiteSpace(settlementId)
                    ? _economy.GetSettlementAvailableResourceTotals(settlementId)
                    : _economy.GetOwnerResourceTotals(ownerId);
            return totals != null
                && totals.TryGetValue(resourceId, out float available)
                && available >= required - Epsilon;
        }
    }
}
