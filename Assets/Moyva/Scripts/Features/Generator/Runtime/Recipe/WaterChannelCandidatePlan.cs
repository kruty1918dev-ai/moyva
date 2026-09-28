using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// How much terrain intervention a detected corridor would need to carry
    /// water between two regions. Detection only classifies; nothing is
    /// applied to the generated world.
    /// </summary>
    internal enum WaterChannelCandidateStatus
    {
        /// <summary>Every corridor cell already sits at/below the source water surface.</summary>
        Natural = 0,
        /// <summary>Deepest single-cell cut stays within the minor threshold.</summary>
        MinorCorrection = 1,
        /// <summary>Allowed but beyond the minor threshold.</summary>
        MajorCorrection = 2,
        /// <summary>Failed a hard constraint; weights never compensate for it.</summary>
        Rejected = 3,
    }

    /// <summary>Why a region pair failed to produce a channel candidate.</summary>
    internal enum WaterChannelRejectReason
    {
        None = 0,
        /// <summary>No traversable land corridor between the regions within bounds.</summary>
        NoCorridor = 1,
        /// <summary>Best corridor exceeds the configured cell length.</summary>
        CorridorTooLong = 2,
        /// <summary>Corridor saddle rises too far above the source water surface.</summary>
        RidgeTooHigh = 3,
        /// <summary>Total estimated excavation exceeds the configured volume.</summary>
        ExcavationTooLarge = 4,
        /// <summary>A connected water region has no finite surface height to reason about.</summary>
        MissingWaterSurface = 5,
        /// <summary>Search budget exhausted before a corridor was proven.</summary>
        SearchBudgetExceeded = 6,
    }

    /// <summary>
    /// One connected water body discovered on the exported logical map
    /// (4-connected cells whose winning terrain sample is water).
    /// </summary>
    internal sealed class WaterChannelRegionInfo
    {
        public int Id;
        public int CellCount;
        public float MinSurfaceMeters;
        public float MaxSurfaceMeters;
    }

    /// <summary>
    /// A single evaluated corridor between two water regions. Kept for both
    /// accepted and rejected outcomes so consumers can explain why a corridor
    /// won or was dropped; <see cref="RejectReason"/> is set for rejected ones.
    /// </summary>
    internal sealed class WaterChannelCandidate
    {
        /// <summary>Region with the higher (or equal) endpoint water surface.</summary>
        public int SourceRegionId;
        public int TargetRegionId;
        /// <summary>Water cell feeding the corridor at the source end.</summary>
        public Vector2Int SourceWaterCell;
        public Vector2Int TargetWaterCell;
        /// <summary>Ordered land cells of the corridor, source end first.</summary>
        public Vector2Int[] LandCells;
        public float SourceSurfaceMeters;
        public float TargetSurfaceMeters;
        /// <summary>Highest land surface along the corridor.</summary>
        public float SaddleMeters;
        public int LengthCells;
        /// <summary>Corridor cells above the source surface (would need carving).</summary>
        public int CellsAboveSourceLevel;
        /// <summary>Sum of per-cell cut depth needed to flood the corridor at source level.</summary>
        public float ExcavationVolumeMeters;
        /// <summary>Deepest single-cell cut (SaddleMeters - SourceSurfaceMeters).</summary>
        public float MaxCellCutMeters;
        /// <summary>
        /// Land components remaining if corridor cells were flooded at source
        /// level. 1 = corridor does not split a landmass, 0 = it consumes the
        /// whole component, &gt;1 = it severs it (a strait through an isthmus).
        /// </summary>
        public int LandFragmentCount;
        /// <summary>Smallest fragment size when LandFragmentCount &gt; 1; 0 otherwise.</summary>
        public int SmallestLandFragmentCells;
        public WaterChannelCandidateStatus Status;
        public WaterChannelRejectReason RejectReason;
    }

    /// <summary>
    /// A region pair that reached detailed search but produced no accepted
    /// corridor. Kept separate from <see cref="WaterChannelCandidate"/> rows so
    /// rejection reasons stay inspectable without fake path data.
    /// </summary>
    internal struct WaterChannelRejection
    {
        public int RegionA;
        public int RegionB;
        public WaterChannelRejectReason Reason;

        public WaterChannelRejection(int regionA, int regionB, WaterChannelRejectReason reason)
        {
            RegionA = regionA;
            RegionB = regionB;
            Reason = reason;
        }
    }

    /// <summary>
    /// Read-only result of <see cref="WaterChannelCandidatePlanner"/>: the water
    /// regions found on the map plus evaluated corridor candidates between
    /// them. Empty candidate lists are a valid outcome — detection only reports
    /// where existing terrain already supports a natural connection.
    /// </summary>
    internal sealed class WaterChannelCandidatePlan
    {
        public WaterChannelRegionInfo[] Regions = System.Array.Empty<WaterChannelRegionInfo>();
        /// <summary>Evaluated corridors, accepted and rejected alike — check Status.</summary>
        public List<WaterChannelCandidate> Candidates = new();
        /// <summary>Region pairs where no corridor was even found for evaluation.</summary>
        public List<WaterChannelRejection> Rejections = new();

        public int AcceptedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Candidates.Count; i++)
                {
                    if (Candidates[i].Status != WaterChannelCandidateStatus.Rejected)
                        count++;
                }
                return count;
            }
        }
    }
}
