using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ObjectPlacement
{
    public enum ObjectPlacementRuntimeMode
    {
        RebuildWithWorld,
        Manual,
        Incremental
    }

    [Serializable]
    public sealed class ObjectPrefabEntry
    {
        [Tooltip("Prefab variant that can be spawned by this object layer.")]
        public GameObject Prefab;

        [Min(0f)]
        [Tooltip("Weighted random selection weight. Values are normalized by the TWC object layer.")]
        public float Weight = 1f;

        [Min(0.01f)]
        public float MinScale = 0.9f;

        [Min(0.01f)]
        public float MaxScale = 1.1f;

        public bool RandomYaw = true;
        public bool AlignToSurface = true;

        [Tooltip("Vertical offset applied through the generated TWC Object Build Layer.")]
        public float YOffset;

        [Min(0f)]
        public float MinDistance;

        public string[] AllowedMasks = Array.Empty<string>();
        public string[] BlockedMasks = Array.Empty<string>();

        [Range(0f, 1f)]
        public float ClusterAffinity = 1f;

        public Color ColorVariation = Color.white;
        public Material MaterialOverride;
    }

    [Serializable]
    public sealed class ObjectPlacementRule
    {
        [Range(0f, 1f)]
        public float Density = 0.35f;

        public int RandomSeed = 17;

        [Min(0f)]
        public float MinDistance = 1f;

        [Min(0f)]
        public float Jitter = 0.25f;

        [Range(0f, 180f)]
        public float RotationRandomization = 180f;

        public Vector2 ScaleRandomization = new Vector2(0.9f, 1.1f);

        public Vector2 HeightRange = new Vector2(-999f, 999f);
        public Vector2 SlopeRange = new Vector2(0f, 90f);

        public string BiomeFilter;
        public string LayerFilter;

        [Range(0f, 1f)]
        public float EdgeBias;

        public bool AvoidWater = true;
        public bool AvoidRoads = true;
        public bool AvoidBuildings = true;
        public bool AlignToSurface = true;
        public float EmbedIntoGround;

        public string ParentContainer = "Generated Props";
        public ObjectPlacementRuntimeMode RuntimeMode = ObjectPlacementRuntimeMode.RebuildWithWorld;

        [Tooltip("Use TWC object cluster merge. Good for dense grass cards, bad for interactive prefabs.")]
        public bool MergeInTWC;

        [Tooltip("When enabled, graph output is converted to a generated TWC Object Build Layer.")]
        public bool UseTWCObjectLayer = true;
    }

    [Serializable]
    public sealed class ClusterSettings
    {
        public bool Enabled = true;

        [Min(1)]
        public int ClusterCount = 8;

        [Min(1)]
        public int ClusterRadius = 3;

        [Range(0f, 1f)]
        public float ClusterDensity = 0.55f;

        [Min(0.001f)]
        public float NoiseScale = 0.2f;

        [Range(0f, 1f)]
        public float NoiseThreshold = 0.35f;

        [Range(0f, 1f)]
        public float EdgePreference = 0.25f;

        [Min(0f)]
        public float AvoidCliffEdgeDistance = 1f;
    }

    public enum GrassCardGeometryMode
    {
        CrossedPlanes = 0,
        CameraBillboard = 1
    }

    [Serializable]
    public sealed class GrassCardSettings
    {
        public Texture2D Texture;
        public Material Material;
        public GameObject Prefab;
        public Color Tint = Color.white;

        [Range(0f, 1f)]
        public float AlphaClip = 0.35f;

        [Range(1, 8)]
        public int CrossedPlanes = 3;

        public GrassCardGeometryMode GeometryMode = GrassCardGeometryMode.CrossedPlanes;

        [Min(0.01f)]
        public float Width = 0.7f;

        [Min(0.01f)]
        public float Height = 0.9f;

        public bool DoubleSided = true;
        public bool WindWobble;

        [Range(0f, 1f)]
        public float ColorVariation = 0.12f;
    }

    public readonly struct ScatterCandidate
    {
        public readonly Vector2Int Cell;
        public readonly Vector2 LocalOffset;
        public readonly float Score;
        public readonly float RotationY;
        public readonly float Scale;
        public readonly int PrefabIndex;

        public ScatterCandidate(
            Vector2Int cell,
            Vector2 localOffset,
            float score,
            float rotationY,
            float scale,
            int prefabIndex = -1)
        {
            Cell = cell;
            LocalOffset = localOffset;
            Score = score;
            RotationY = rotationY;
            Scale = scale;
            PrefabIndex = prefabIndex;
        }

        public ScatterCandidate WithPrefabIndex(int prefabIndex) =>
            new(Cell, LocalOffset, Score, RotationY, Scale, prefabIndex);
    }

    public sealed class ScatterMask
    {
        public bool[,] Placement { get; }
        public bool[,] Exclude { get; }
        public float[,] Weights { get; }

        public int Width => Placement?.GetLength(0) ?? Exclude?.GetLength(0) ?? 0;
        public int Height => Placement?.GetLength(1) ?? Exclude?.GetLength(1) ?? 0;

        public ScatterMask(bool[,] placement, bool[,] exclude = null, float[,] weights = null)
        {
            Placement = placement;
            Exclude = exclude;
            Weights = weights;
        }

        public bool IsAllowed(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return false;

            bool placementAllowed = Placement == null || Placement[x, y];
            bool excluded = Exclude != null
                && x < Exclude.GetLength(0)
                && y < Exclude.GetLength(1)
                && Exclude[x, y];

            return placementAllowed && !excluded;
        }

        public float GetWeight(int x, int y)
        {
            if (!IsAllowed(x, y))
                return 0f;

            if (Weights == null || x >= Weights.GetLength(0) || y >= Weights.GetLength(1))
                return 1f;

            return Mathf.Clamp01(Weights[x, y]);
        }

        public bool[,] BuildAllowedMask()
        {
            int w = Mathf.Max(1, Width);
            int h = Mathf.Max(1, Height);
            var result = new bool[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                    result[x, y] = IsAllowed(x, y);
            }

            return result;
        }
    }

    public sealed class ObjectPlacementLayer
    {
        public string LayerName { get; set; }
        public string TargetGraphLayerId { get; set; }
        public ObjectPlacementRule Rule { get; set; } = new();
        public ClusterSettings Cluster { get; set; } = new();
        public List<ObjectPrefabEntry> Prefabs { get; } = new();
        public List<ScatterCandidate> Candidates { get; } = new();

        public ObjectPlacementLayer(string layerName)
        {
            LayerName = string.IsNullOrWhiteSpace(layerName) ? "Object Layer" : layerName.Trim();
        }
    }

    public sealed class ObjectPlacementRegistry
    {
        private readonly List<ObjectPlacementLayer> _layers = new();

        public IReadOnlyList<ObjectPlacementLayer> Layers => _layers;

        public void Register(ObjectPlacementLayer layer)
        {
            if (layer == null)
                return;

            _layers.RemoveAll(existing =>
                existing != null &&
                string.Equals(existing.LayerName, layer.LayerName, StringComparison.Ordinal));
            _layers.Add(layer);
        }
    }
}
