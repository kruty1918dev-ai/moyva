using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal readonly struct BotPlacementProbe
    {
        public BotPlacementProbe(
            ConstructionPlacementQueryResult legality,
            ConstructionPlacementQueryResult commit)
        {
            Legality = legality;
            Commit = commit;
        }

        public ConstructionPlacementQueryResult Legality { get; }
        public ConstructionPlacementQueryResult Commit { get; }

        // "Legal" deliberately excludes resources. A strategically good site must
        // remain discoverable even when the bot cannot afford it this turn.
        public bool IsLegal =>
            Legality.AvailabilityValid &&
            Legality.SpatialValid;

        public bool IsAffordable => Commit.ResourcesValid;
        public bool HasAuthority => Commit.AuthorityValid;
        public bool CanCommit => Commit.CanCommit;

        public string ReasonCode =>
            Commit.Diagnostic?.ReasonCode ??
            Legality.Diagnostic?.ReasonCode ??
            string.Empty;

        public string Reason =>
            !string.IsNullOrWhiteSpace(Commit.Reason)
                ? Commit.Reason
                : Legality.Reason ?? string.Empty;
    }

    internal sealed class BotConstructionPlacementProbe
    {
        private readonly IConstructionPlacementQuery _query;

        public BotConstructionPlacementProbe(
            IConstructionPlacementQuery query)
        {
            _query = query;
        }

        public bool IsAvailable => _query != null;

        public BotPlacementProbe Evaluate(
            string ownerId,
            string buildingId,
            Vector2Int cell)
        {
            if (_query == null)
                return default;

            // Pass 1: legality / spatial feasibility only.
            var legalityRequest =
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    cell,
                    includeResources: false,
                    includeDetails: true,
                    ownerId: ownerId,
                    attemptSource:
                        ConstructionPlacementAttemptSource.DirectPlace);

            ConstructionPlacementQueryResult legality =
                _query.EvaluatePlacement(legalityRequest);

            // Pass 2: commit readiness, including affordability + authority.
            var commitRequest =
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    cell,
                    includeResources: true,
                    includeDetails: true,
                    ownerId: ownerId,
                    attemptSource:
                        ConstructionPlacementAttemptSource.DirectPlace);

            ConstructionPlacementQueryResult commit =
                _query.EvaluatePlacement(commitRequest);

            return new BotPlacementProbe(
                legality,
                commit);
        }
    }

    internal sealed class BotPlacementRejectionAccumulator
    {
        private readonly Dictionary<string, int> _reasonCounts =
            new(StringComparer.OrdinalIgnoreCase);

        public int Evaluated { get; private set; }
        public int Legal { get; private set; }
        public int Affordable { get; private set; }
        public int CommitReady { get; private set; }
        public int AvailabilityRejected { get; private set; }
        public int SpatialRejected { get; private set; }
        public int ResourceRejected { get; private set; }
        public int AuthorityRejected { get; private set; }

        public void Observe(BotPlacementProbe probe)
        {
            Evaluated++;

            if (probe.IsLegal)
                Legal++;

            if (probe.IsAffordable)
                Affordable++;

            if (probe.CanCommit)
                CommitReady++;

            if (!probe.Legality.AvailabilityValid)
                AvailabilityRejected++;

            if (probe.Legality.AvailabilityValid &&
                !probe.Legality.SpatialValid)
            {
                SpatialRejected++;
            }

            if (probe.IsLegal &&
                !probe.Commit.ResourcesValid)
            {
                ResourceRejected++;
            }

            if (probe.IsLegal &&
                probe.Commit.ResourcesValid &&
                !probe.Commit.AuthorityValid)
            {
                AuthorityRejected++;
            }

            string code = probe.ReasonCode;
            if (string.IsNullOrWhiteSpace(code))
                code = ResolveFallbackReasonCode(probe);

            if (!_reasonCounts.TryGetValue(code, out int count))
                count = 0;

            _reasonCounts[code] = count + 1;
        }

        public string BuildSummary(int maxReasons = 5)
        {
            var reasons =
                new List<KeyValuePair<string, int>>(_reasonCounts);

            reasons.Sort((left, right) =>
            {
                int count =
                    right.Value.CompareTo(left.Value);

                return count != 0
                    ? count
                    : string.CompareOrdinal(
                        left.Key,
                        right.Key);
            });

            int take = Math.Min(
                Math.Max(0, maxReasons),
                reasons.Count);

            var parts = new List<string>(take);

            for (int i = 0; i < take; i++)
                parts.Add($"{reasons[i].Key}={reasons[i].Value}");

            return
                $"evaluated={Evaluated}; legal={Legal}; " +
                $"affordable={Affordable}; commitReady={CommitReady}; " +
                $"availabilityRejected={AvailabilityRejected}; " +
                $"spatialRejected={SpatialRejected}; " +
                $"resourceRejected={ResourceRejected}; " +
                $"authorityRejected={AuthorityRejected}; " +
                $"topReasons=[{string.Join(", ", parts)}]";
        }

        private static string ResolveFallbackReasonCode(
            BotPlacementProbe probe)
        {
            if (!probe.Legality.AvailabilityValid)
                return "availability";

            if (!probe.Legality.SpatialValid)
                return "spatial";

            if (!probe.Commit.ResourcesValid)
                return "resources";

            if (!probe.Commit.AuthorityValid)
                return "authority";

            return probe.CanCommit
                ? "allowed"
                : "unknown";
        }
    }
}
