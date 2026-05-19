using UnityEngine;

namespace Kruty1918.Moyva.Camera.API
{
    /// <summary>
    /// Axis-aligned rectangular bounds of the playable world, in world units.
    /// </summary>
    public readonly struct CameraWorldBounds
    {
        public readonly float MinX;
        public readonly float MaxX;
        public readonly float MinY;
        public readonly float MaxY;
        public readonly bool HasValue;

        public float Width => Mathf.Max(0.01f, MaxX - MinX);
        public float Height => Mathf.Max(0.01f, MaxY - MinY);
        public Vector2 Center => new Vector2((MinX + MaxX) * 0.5f, (MinY + MaxY) * 0.5f);

        public CameraWorldBounds(float minX, float maxX, float minY, float maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
            HasValue = true;
        }
    }

    /// <summary>
    /// Provides axis-aligned world bounds the camera should clamp its viewport to.
    /// Implementations may derive bounds from tilemap renderers, world-gen config, etc.
    /// </summary>
    public interface ICameraBoundsProvider
    {
        /// <summary>
        /// Returns current bounds. <see cref="CameraWorldBounds.HasValue"/> is <c>false</c>
        /// when bounds cannot be resolved (e.g. tilemaps not yet loaded).
        /// </summary>
        CameraWorldBounds GetWorldBounds();
    }
}
