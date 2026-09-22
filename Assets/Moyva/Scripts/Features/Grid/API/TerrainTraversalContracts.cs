using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Physical classification of a single orthogonal step between adjacent
    /// cells. Derived from authored terrain surfaces and generated passage
    /// metadata — never from tile or mesh names.
    /// </summary>
    public enum TerrainTransitionKind
    {
        /// <summary>Surface height difference is within the profile's direct step limit.</summary>
        DirectWalk = 0,

        /// <summary>The step is part of a generated stair passage.</summary>
        Stair = 1,

        /// <summary>The step is not physically traversable for the profile.</summary>
        Blocked = 2,
    }

    /// <summary>
    /// One authored stair module occupying a full grid cell. The canonical mesh
    /// rises toward +Z; <see cref="DirectionIndex"/> rotates it around Y in
    /// 90-degree steps (0 = +Z, 1 = +X, 2 = -Z, 3 = -X).
    /// </summary>
    public struct TerrainPassageModule
    {
        public Vector2Int Cell;
        public int DirectionIndex;
        /// <summary>World-space height of the module's top (+end) surface.</summary>
        public float TopY;
        /// <summary>Surface height of the low plateau the flight starts from.</summary>
        public float LowSurfaceY;
        /// <summary>Surface height of the high plateau the flight exits onto.</summary>
        public float HighSurfaceY;
        /// <summary>Atlas theme id used to render the module (e.g. "stone").</summary>
        public string ThemeId;
    }

    /// <summary>
    /// Read-only view of generated terrain passages (stairs). Implemented by the
    /// generator's passage store; consumed by mesh providers and traversal.
    /// </summary>
    public interface ITerrainPassageMap
    {
        int Version { get; }
        bool HasPassages { get; }
        bool TryGetModule(Vector2Int cell, out TerrainPassageModule module);
        bool IsStairCell(Vector2Int cell);

        /// <summary>
        /// True when the orthogonal step from->to is part of a generated stair
        /// passage: consecutive modules inside a flight, or the plateau->first /
        /// last->plateau entry and exit steps.
        /// </summary>
        bool IsStairStep(Vector2Int from, Vector2Int to);
    }

    /// <summary>
    /// Physical height limits of a movement profile. Resolved from JSON movement
    /// profiles; consumed by traversal classification.
    /// </summary>
    public sealed class MovementHeightLimits
    {
        public MovementHeightLimits(
            float autoStepMaxMeters,
            float stairModuleRiseMeters,
            float maxStairRiseMeters)
        {
            AutoStepMaxMeters = autoStepMaxMeters;
            StairModuleRiseMeters = stairModuleRiseMeters;
            MaxStairRiseMeters = maxStairRiseMeters;
        }

        /// <summary>Largest surface delta a unit can cross by plain walking.</summary>
        public float AutoStepMaxMeters { get; }

        /// <summary>Height one stair module covers (0.25 for the atlas pack).</summary>
        public float StairModuleRiseMeters { get; }

        /// <summary>Largest total rise a generated stair flight may cover.</summary>
        public float MaxStairRiseMeters { get; }

        public static MovementHeightLimits Default { get; } =
            new MovementHeightLimits(0.25f, 0.25f, 1f);
    }

    /// <summary>
    /// Height-rule lookup for movement profiles, resolved next to traversal
    /// costs so preview and execution share the same limits.
    /// </summary>
    public interface IMovementHeightPolicy
    {
        bool TryGetLimits(string movementProfileId, out MovementHeightLimits limits);
    }

    /// <summary>
    /// Result of evaluating one orthogonal step between adjacent cells.
    /// </summary>
    public readonly struct TerrainTransitionEvaluation
    {
        public TerrainTransitionEvaluation(
            TerrainTransitionKind kind,
            float riseMeters,
            float dropMeters,
            Vector2Int lowCell,
            Vector2Int highCell)
        {
            Kind = kind;
            RiseMeters = riseMeters;
            DropMeters = dropMeters;
            LowCell = lowCell;
            HighCell = highCell;
        }

        public TerrainTransitionKind Kind { get; }
        /// <summary>Signed surface delta in the direction of travel (positive = climb).</summary>
        public float RiseMeters { get; }
        public float DropMeters { get; }
        public Vector2Int LowCell { get; }
        public Vector2Int HighCell { get; }
    }

    /// <summary>
    /// Classifies an adjacent-cell step using surface heights, generated stair
    /// passages and the movement profile's height limits.
    /// </summary>
    public static class TerrainTransitionClassifier
    {
        private const float Epsilon = 0.001f;

        public static TerrainTransitionKind Classify(
            float riseMeters,
            float dropMeters,
            bool isStairStep,
            IMovementHeightPolicy heightPolicy,
            string movementProfileId)
        {
            MovementHeightLimits limits = MovementHeightLimits.Default;
            if (heightPolicy != null
                && heightPolicy.TryGetLimits(movementProfileId, out MovementHeightLimits resolved)
                && resolved != null)
            {
                limits = resolved;
            }

            float delta = Mathf.Max(riseMeters, dropMeters);
            if (isStairStep && delta <= limits.StairModuleRiseMeters + Epsilon)
                return TerrainTransitionKind.Stair;
            if (delta <= limits.AutoStepMaxMeters + Epsilon)
                return TerrainTransitionKind.DirectWalk;
            return TerrainTransitionKind.Blocked;
        }
    }
}
