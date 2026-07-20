using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Defines which already-existing placement may be treated as the movable instance
    /// when the player selects the same unique building again.
    /// </summary>
    public enum BuildingPlacementUniquenessScope
    {
        None = 0,
        PerOwner = 1,
        Global = 2,
    }

    /// <summary>
    /// Identifies the caller that requested placement validation.
    /// Expensive/noisy grid probes remain distinguishable from actual player actions.
    /// </summary>
    public enum ConstructionPlacementAttemptSource
    {
        Unknown = 0,
        GridTileFilter = 1,
        PointerHover = 2,
        PointerClick = 3,
        PreviewMove = 4,
        Confirm = 5,
        DirectPlace = 6,
        DragValidation = 7,
        WallPath = 8,
        NetworkRequest = 9,
    }

    /// <summary>
    /// Immutable snapshot describing why a construction placement was accepted or rejected.
    /// It is attached to every authoritative placement query result and is safe to log later.
    /// </summary>
    public sealed class ConstructionPlacementDiagnostic
    {
        private readonly BuildingPlacementBlocker[] _blockers;

        public ConstructionPlacementDiagnostic(
            long attemptId,
            ConstructionPlacementAttemptSource source,
            string buildingId,
            Vector2Int position,
            Vector2Int contextPosition,
            string ownerId,
            bool isSpatiallyValid,
            bool resourcesValid,
            bool isGateReplacement,
            string reasonCode,
            string reason,
            string tileId,
            int? terrainLevel,
            string terrainReason,
            string fogState,
            int perPlayerLimit,
            int existingOwnedCount,
            int pendingOwnedCount,
            Vector2Int? ignoredPendingPosition,
            Vector2Int? ignoredOccupiedPosition,
            IReadOnlyList<BuildingPlacementBlocker> blockers)
        {
            AttemptId = attemptId;
            Source = source;
            BuildingId = buildingId;
            Position = position;
            ContextPosition = contextPosition;
            OwnerId = ownerId;
            IsSpatiallyValid = isSpatiallyValid;
            ResourcesValid = resourcesValid;
            IsGateReplacement = isGateReplacement;
            ReasonCode = reasonCode;
            Reason = reason;
            TileId = tileId;
            TerrainLevel = terrainLevel;
            TerrainReason = terrainReason;
            FogState = fogState;
            PerPlayerLimit = perPlayerLimit;
            ExistingOwnedCount = existingOwnedCount;
            PendingOwnedCount = pendingOwnedCount;
            IgnoredPendingPosition = ignoredPendingPosition;
            IgnoredOccupiedPosition = ignoredOccupiedPosition;

            if (blockers == null || blockers.Count == 0)
            {
                _blockers = Array.Empty<BuildingPlacementBlocker>();
            }
            else
            {
                _blockers = new BuildingPlacementBlocker[blockers.Count];
                for (int index = 0; index < blockers.Count; index++)
                    _blockers[index] = blockers[index];
            }
        }

        public long AttemptId { get; }
        public ConstructionPlacementAttemptSource Source { get; }
        public string BuildingId { get; }
        public Vector2Int Position { get; }
        public Vector2Int ContextPosition { get; }
        public string OwnerId { get; }
        public bool IsSpatiallyValid { get; }
        public bool ResourcesValid { get; }
        public bool IsGateReplacement { get; }
        public bool IsValid => IsSpatiallyValid && ResourcesValid;
        public string ReasonCode { get; }
        public string Reason { get; }
        public string TileId { get; }
        public int? TerrainLevel { get; }
        public string TerrainReason { get; }
        public string FogState { get; }
        public int PerPlayerLimit { get; }
        public int ExistingOwnedCount { get; }
        public int PendingOwnedCount { get; }
        public int TotalOwnedAndPendingCount => ExistingOwnedCount + PendingOwnedCount;
        public Vector2Int? IgnoredPendingPosition { get; }
        public Vector2Int? IgnoredOccupiedPosition { get; }
        public IReadOnlyList<BuildingPlacementBlocker> Blockers => _blockers;

        public override string ToString()
            => ConstructionPlacementDiagnosticFormatter.FormatSingleLine(this);
    }

    /// <summary>
    /// Produces stable single-line diagnostics suitable for Unity Console filtering.
    /// </summary>
    public static class ConstructionPlacementDiagnosticFormatter
    {
        public const string LogTag = "[MoyvaPlacementAttempt]";

        public static string FormatSingleLine(ConstructionPlacementDiagnostic diagnostic)
        {
            if (diagnostic == null)
                return $"{LogTag} diagnostic=null";

            var builder = new StringBuilder(384);
            builder.Append(LogTag)
                .Append(" id=").Append(diagnostic.AttemptId)
                .Append(" source=").Append(diagnostic.Source)
                .Append(" result=").Append(diagnostic.IsValid ? "allowed" : "blocked")
                .Append(" building='").Append(Escape(diagnostic.BuildingId)).Append('\'')
                .Append(" owner='").Append(Escape(diagnostic.OwnerId)).Append('\'')
                .Append(" origin=").Append(diagnostic.Position)
                .Append(" context=").Append(diagnostic.ContextPosition)
                .Append(" code='").Append(Escape(diagnostic.ReasonCode)).Append('\'')
                .Append(" reason='").Append(Escape(diagnostic.Reason)).Append('\'')
                .Append(" tile='").Append(Escape(diagnostic.TileId)).Append('\'')
                .Append(" terrainLevel=").Append(diagnostic.TerrainLevel?.ToString() ?? "unknown")
                .Append(" terrainReason='").Append(Escape(diagnostic.TerrainReason)).Append('\'')
                .Append(" fog=").Append(string.IsNullOrWhiteSpace(diagnostic.FogState) ? "unknown" : diagnostic.FogState)
                .Append(" limit=").Append(diagnostic.PerPlayerLimit)
                .Append(" existing=").Append(diagnostic.ExistingOwnedCount)
                .Append(" pending=").Append(diagnostic.PendingOwnedCount)
                .Append(" ignoredPending=").Append(diagnostic.IgnoredPendingPosition?.ToString() ?? "none")
                .Append(" ignoredOccupied=").Append(diagnostic.IgnoredOccupiedPosition?.ToString() ?? "none");

            if (diagnostic.Blockers.Count > 0)
            {
                builder.Append(" blockers=[");
                for (int index = 0; index < diagnostic.Blockers.Count; index++)
                {
                    if (index > 0)
                        builder.Append("; ");

                    BuildingPlacementBlocker blocker = diagnostic.Blockers[index];
                    if (blocker == null)
                    {
                        builder.Append("null");
                        continue;
                    }

                    builder.Append(blocker.Kind)
                        .Append('@').Append(blocker.Position?.ToString() ?? "none")
                        .Append(':').Append(Escape(blocker.Message));
                }

                builder.Append(']');
            }

            return builder.ToString();
        }

        public static string ResolveReasonCode(
            BuildingPlacementEvaluationResult evaluationResult,
            bool isSpatiallyValid,
            bool resourcesValid,
            string explicitCode = null)
        {
            if (!string.IsNullOrWhiteSpace(explicitCode))
                return explicitCode.Trim();

            if (evaluationResult != null && evaluationResult.Blockers.Count > 0)
                return ToKebabCase(evaluationResult.Blockers[0].Kind.ToString());

            if (!isSpatiallyValid)
                return "spatial-rules";
            if (!resourcesValid)
                return "resources";
            return "allowed";
        }

        private static string ToKebabCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            var builder = new StringBuilder(value.Length + 4);
            for (int index = 0; index < value.Length; index++)
            {
                char current = value[index];
                if (char.IsUpper(current) && index > 0)
                    builder.Append('-');
                builder.Append(char.ToLowerInvariant(current));
            }

            return builder.ToString();
        }

        private static string Escape(string value)
            => string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", " ").Replace("\n", " ");
    }
}
