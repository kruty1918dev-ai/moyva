using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>Complete deterministic output of one geography attempt.</summary>
    internal sealed class WorldGeographyResult
    {
        public int Width;
        public int Height;
        public int Seed;
        public WorldArchetype Archetype;
        public int Attempt;

        /// <summary>Canonical gameplay/biome tile ids (water/sand/grass/...).</summary>
        public string[,] TileMap;
        /// <summary>Float surface heights (world Y of tile tops).</summary>
        public float[,] HeightMap;
        /// <summary>Integer terrain levels (authored, no post-normalisation).</summary>
        public int[,] TerrainLevelMap;
        /// <summary>Gameplay object ids (POIs); null entries mean empty.</summary>
        public string[,] ObjectMap;
        /// <summary>River channel cells (water gameplay).</summary>
        public bool[,] RiverMask;
        /// <summary>Lake water cells.</summary>
        public bool[,] LakeMask;
        /// <summary>Beach/shore cells.</summary>
        public bool[,] BeachMask;
        /// <summary>Downstream flood-parent (flattened cell index, -1 = sink).</summary>
        public int[,] FlowParent;
        /// <summary>D8 flow accumulation per cell.</summary>
        public float[,] FlowAccumulation;
        /// <summary>Suggested balanced spawn cells (one per player).</summary>
        public Vector2Int[] SpawnHints;
        /// <summary>Forest coverage field for decoration density [0..1].</summary>
        public float[,] ForestField;
        /// <summary>Moisture field for decoration density [0..1].</summary>
        public float[,] MoistureField;
        /// <summary>Per-attempt metrics + gate results.</summary>
        public WorldGenerationReport Report;
    }

    /// <summary>Deterministic spawn candidate proposed by the geography engine.</summary>
    internal readonly struct SpawnHint
    {
        public SpawnHint(Vector2Int cell, float opportunityScore)
        {
            Cell = cell;
            OpportunityScore = opportunityScore;
        }

        public Vector2Int Cell { get; }
        public float OpportunityScore { get; }
    }

    /// <summary>Quality/fairness metrics collected per generation attempt.</summary>
    internal sealed class WorldGenerationReport
    {
        public int Attempt;
        public int Seed;
        public string Archetype;
        public int LandCells;
        public float LandFraction;
        public int WaterCells;
        public int RiverCells;
        public int LakeCells;
        public int BeachCells;
        public int MountainCells;
        public int ForestCells;
        public int ObjectCells;
        public int SpawnCount;
        public float MinOpportunityScore;
        public float MaxOpportunityScore;
        public float OpportunityGap;
        public float MinPairwiseSpawnDistance;
        public bool ConnectivityOk;
        public int LargestLandComponent;
        public readonly List<string> GateFailures = new List<string>();
        public bool Accepted => GateFailures.Count == 0;

        public string ToSummary()
        {
            var sb = new StringBuilder(256);
            sb.Append("attempt=").Append(Attempt)
                .Append(" arch=").Append(Archetype)
                .Append(" land=").Append(LandFraction.ToString("F2"))
                .Append(" rivers=").Append(RiverCells)
                .Append(" lakes=").Append(LakeCells)
                .Append(" mtn=").Append(MountainCells)
                .Append(" forest=").Append(ForestCells)
                .Append(" spawns=").Append(SpawnCount)
                .Append(" gap=").Append(OpportunityGap.ToString("F2"))
                .Append(" conn=").Append(ConnectivityOk);
            if (GateFailures.Count > 0)
                sb.Append(" FAIL:").Append(string.Join(",", GateFailures));
            return sb.ToString();
        }
    }
}
