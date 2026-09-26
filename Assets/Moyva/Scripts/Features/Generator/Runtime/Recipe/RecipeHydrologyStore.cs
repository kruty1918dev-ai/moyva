using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>Water body classification of a hydrology cell.</summary>
    internal enum RecipeWaterKind
    {
        None = 0,
        River = 1,
        Lake = 2,
        /// <summary>Open-water/sink cell (sea, border drain) — surface set, no channel.</summary>
        Sink = 3,
    }

    /// <summary>
    /// Read-only query surface over the active world's recipe hydrology plan:
    /// river/lake membership, per-cell water surfaces and waterfall edges.
    /// </summary>
    internal interface IRecipeHydrologyMap
    {
        int Version { get; }
        bool HasHydrology { get; }
        /// <summary>Drop threshold (meters) that qualifies an edge as a waterfall.</summary>
        float WaterfallMinDropMeters { get; }
        bool IsRiverCell(Vector2Int cell);
        bool IsLakeCell(Vector2Int cell);
        bool IsWaterCell(Vector2Int cell);
        /// <summary>River, lake, sink (open water/border) or none.</summary>
        RecipeWaterKind GetWaterKind(Vector2Int cell);
        bool TryGetWaterSurface(Vector2Int cell, out float surfaceY);
        /// <summary>Channel/lake bed height in meters for water cells.</summary>
        bool TryGetBedHeight(Vector2Int cell, out float bedY);
        /// <summary>
        /// Downstream neighbor of a water cell (D8 flow parent); false at
        /// sinks and non-water cells. Consumers: flow animation, foam
        /// direction, waterfall orientation.
        /// </summary>
        bool TryGetFlowDirection(Vector2Int cell, out Vector2Int downstream);
        /// <summary>
        /// True when the water cell pours over a ledge: returns the downstream
        /// cell and the upper/lower water surface heights of the fall.
        /// </summary>
        bool TryGetWaterfall(Vector2Int cell, out Vector2Int downstream,
            out float upperY, out float lowerY);
        /// <summary>Seabed profile config from the active plan; null when absent.</summary>
        RecipeSeabedConfig Seabed { get; }
        /// <summary>Waterfall front config from the active plan; null when absent.</summary>
        RecipeWaterfallConfig Waterfalls { get; }
    }

    /// <summary>
    /// Runtime store for the active world's recipe hydrology plan. The
    /// generation pipeline writes it before the chunk build; mesh providers
    /// and traversal read it through <see cref="IRecipeHydrologyMap"/>.
    /// </summary>
    internal sealed class RecipeHydrologyStore : IRecipeHydrologyMap
    {
        private RecipeHydrologyPlan _plan;
        private int _width;
        private int _height;
        private int _version;

        public int Version => _version;
        public bool HasHydrology => _plan != null;
        public float WaterfallMinDropMeters =>
            _plan != null && _plan.WaterfallMinDropMeters > 0f
                ? _plan.WaterfallMinDropMeters
                : 0.5f;
        public RecipeHydrologyPlan Plan => _plan;
        public RecipeSeabedConfig Seabed => _plan?.Seabed;
        public RecipeWaterfallConfig Waterfalls => _plan?.Waterfalls;

        public void Clear()
        {
            if (_plan == null)
                return;
            _plan = null;
            _width = 0;
            _height = 0;
            unchecked { _version++; }
        }

        public void Replace(RecipeHydrologyPlan plan)
        {
            _plan = plan;
            _width = plan?.RiverMask?.GetLength(0) ?? 0;
            _height = plan?.RiverMask?.GetLength(1) ?? 0;
            unchecked { _version++; }
        }

        public bool IsRiverCell(Vector2Int cell)
            => InBounds(cell) && _plan.RiverMask[cell.x, cell.y];

        public bool IsLakeCell(Vector2Int cell)
            => InBounds(cell) && _plan.LakeMask[cell.x, cell.y];

        public bool IsWaterCell(Vector2Int cell)
            => IsRiverCell(cell) || IsLakeCell(cell);

        public RecipeWaterKind GetWaterKind(Vector2Int cell)
        {
            if (!InBounds(cell))
                return RecipeWaterKind.None;
            if (_plan.RiverMask[cell.x, cell.y])
                return RecipeWaterKind.River;
            if (_plan.LakeMask[cell.x, cell.y])
                return RecipeWaterKind.Lake;
            return IsFinite(_plan.WaterSurface[cell.x, cell.y])
                ? RecipeWaterKind.Sink
                : RecipeWaterKind.None;
        }

        public bool TryGetWaterSurface(Vector2Int cell, out float surfaceY)
        {
            surfaceY = 0f;
            if (!InBounds(cell))
                return false;
            surfaceY = _plan.WaterSurface[cell.x, cell.y];
            return IsFinite(surfaceY);
        }

        public bool TryGetBedHeight(Vector2Int cell, out float bedY)
        {
            bedY = 0f;
            if (!InBounds(cell) || _plan.BedHeight == null)
                return false;
            bedY = _plan.BedHeight[cell.x, cell.y];
            return IsFinite(bedY);
        }

        public bool TryGetFlowDirection(Vector2Int cell, out Vector2Int downstream)
        {
            downstream = default;
            if (!InBounds(cell) || !IsWaterCell(cell))
                return false;
            int parent = _plan.FlowParent[cell.x, cell.y];
            if (parent < 0)
                return false;
            downstream = new Vector2Int(parent % _width, parent / _width);
            return downstream != cell;
        }

        public bool TryGetWaterfall(Vector2Int cell, out Vector2Int downstream,
            out float upperY, out float lowerY)
        {
            downstream = default;
            upperY = 0f;
            lowerY = 0f;
            if (!InBounds(cell) || !_plan.WaterfallMask[cell.x, cell.y])
                return false;

            int parent = _plan.FlowParent[cell.x, cell.y];
            if (parent < 0)
                return false;

            downstream = new Vector2Int(parent % _width, parent / _width);
            upperY = _plan.WaterSurface[cell.x, cell.y];
            if (float.IsNaN(upperY) || float.IsInfinity(upperY))
                upperY = _plan.Filled[cell.x, cell.y];
            lowerY = _plan.WaterSurface[downstream.x, downstream.y];
            if (float.IsNaN(lowerY) || float.IsInfinity(lowerY))
                lowerY = _plan.Filled[downstream.x, downstream.y];
            return upperY > lowerY;
        }

        private bool InBounds(Vector2Int cell)
            => _plan != null
               && cell.x >= 0 && cell.y >= 0
               && cell.x < _width && cell.y < _height;

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
