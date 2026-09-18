using Kruty1918.Moyva.Construction.API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private readonly struct PlacementAvailabilityCacheKey
            : IEquatable<PlacementAvailabilityCacheKey>
        {
            public PlacementAvailabilityCacheKey(
                string buildingId,
                string ownerId,
                bool includePendingPlacements,
                int pendingVersion)
            {
                BuildingId = buildingId;
                OwnerId = ownerId;
                IncludePendingPlacements =
                    includePendingPlacements;
                PendingVersion = pendingVersion;
            }

            private string BuildingId { get; }
            private string OwnerId { get; }
            private bool IncludePendingPlacements { get; }
            private int PendingVersion { get; }

            public bool Equals(
                PlacementAvailabilityCacheKey other)
            {
                return IncludePendingPlacements
                       == other.IncludePendingPlacements
                    && PendingVersion
                       == other.PendingVersion
                    && string.Equals(
                        BuildingId,
                        other.BuildingId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        OwnerId,
                        other.OwnerId,
                        StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj
                    is PlacementAvailabilityCacheKey other
                    && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash =
                        PendingVersion;

                    hash =
                        hash * 397
                        ^ (
                            IncludePendingPlacements
                                ? 1
                                : 0
                        );

                    hash =
                        hash * 397
                        ^ (
                            BuildingId != null
                                ? StringComparer.Ordinal.GetHashCode(
                                    BuildingId)
                                : 0
                        );

                    hash =
                        hash * 397
                        ^ (
                            OwnerId != null
                                ? StringComparer.Ordinal.GetHashCode(
                                    OwnerId)
                                : 0
                        );

                    return hash;
                }
            }
        }

        private readonly struct PlacementAvailabilityCacheValue
        {
            private PlacementAvailabilityCacheValue(
                bool isValid,
                string reason,
                string reasonCode,
                BuildingPlacementBlockerKind blockerKind,
                BuildingPerPlayerLimitEvaluation limitEvaluation)
            {
                IsValid = isValid;
                Reason = reason;
                ReasonCode = reasonCode;
                BlockerKind = blockerKind;
                LimitEvaluation = limitEvaluation;
            }

            public bool IsValid { get; }
            public string Reason { get; }
            public string ReasonCode { get; }
            public BuildingPlacementBlockerKind BlockerKind { get; }
            public BuildingPerPlayerLimitEvaluation LimitEvaluation { get; }

            public static PlacementAvailabilityCacheValue Valid(
                BuildingPerPlayerLimitEvaluation limitEvaluation)
            {
                return new PlacementAvailabilityCacheValue(
                    true,
                    null,
                    null,
                    BuildingPlacementBlockerKind.Configuration,
                    limitEvaluation);
            }

            public static PlacementAvailabilityCacheValue Invalid(
                string reason,
                string reasonCode,
                BuildingPlacementBlockerKind blockerKind,
                BuildingPerPlayerLimitEvaluation limitEvaluation)
            {
                return new PlacementAvailabilityCacheValue(
                    false,
                    reason,
                    reasonCode,
                    blockerKind,
                    limitEvaluation);
            }
        }


        private readonly struct ResourceValidationCacheKey : IEquatable<ResourceValidationCacheKey>
        {
            public ResourceValidationCacheKey(string buildingId, string ownerId, string fundingContext, int pendingVersion)
            {
                BuildingId = buildingId;
                OwnerId = ownerId;
                FundingContext = fundingContext;
                PendingVersion = pendingVersion;
            }

            private string BuildingId { get; }
            private string OwnerId { get; }
            private string FundingContext { get; }
            private int PendingVersion { get; }

            public bool Equals(ResourceValidationCacheKey other)
                => PendingVersion == other.PendingVersion
                    && string.Equals(BuildingId, other.BuildingId, StringComparison.Ordinal)
                    && string.Equals(OwnerId, other.OwnerId, StringComparison.Ordinal)
                    && string.Equals(FundingContext, other.FundingContext, StringComparison.Ordinal);

            public override bool Equals(object obj)
                => obj is ResourceValidationCacheKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = PendingVersion;
                    hash = hash * 397 ^ (BuildingId != null ? StringComparer.Ordinal.GetHashCode(BuildingId) : 0);
                    hash = hash * 397 ^ (OwnerId != null ? StringComparer.Ordinal.GetHashCode(OwnerId) : 0);
                    hash = hash * 397 ^ (FundingContext != null ? StringComparer.Ordinal.GetHashCode(FundingContext) : 0);
                    return hash;
                }
            }
        }

        private readonly struct ResourceValidationCacheValue
        {
            public ResourceValidationCacheValue(bool isValid, string reason)
            {
                IsValid = isValid;
                Reason = reason;
            }

            public bool IsValid { get; }
            public string Reason { get; }
        }
    }
}
