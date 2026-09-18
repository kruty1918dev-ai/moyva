using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// Shared map settings authored inside a <see cref="GeneratorMapRecipe"/>.
    /// Map size values above zero are authoritative for both preview and gameplay
    /// generation; zero leaves the decision to the launch context.
    /// </summary>
    [System.Serializable]
    public sealed class GeneratorMapSharedSettings
    {
        [Min(0)]
        [Tooltip("Authoritative map width in tiles. 0 = runtime/menu decides.")]
        public int MapWidth;

        [Min(0)]
        [Tooltip("Authoritative map height in tiles. 0 = runtime/menu decides.")]
        public int MapHeight;

        public GridTopology GridTopology = GridTopology.Orthogonal;
        public GridProjectionMode ProjectionMode = GridProjectionMode.Orthographic3D;
        public GridRenderMode RenderMode = GridRenderMode.Mesh3D;
        public GridNeighborhoodMode NeighborhoodMode = GridNeighborhoodMode.Auto;

        public Vector2Int MapSize => new(MapWidth, MapHeight);
        public bool HasMapSize => MapWidth > 0 && MapHeight > 0;

        public GridNeighborhoodMode ResolveNeighborhoodMode()
        {
            if (NeighborhoodMode != GridNeighborhoodMode.Auto)
                return NeighborhoodMode;

            if (GridTopology == GridTopology.HexAxial)
                return GridNeighborhoodMode.HexAxial6;

            return ProjectionMode == GridProjectionMode.Isometric3DPreview
                ? GridNeighborhoodMode.VonNeumann4
                : GridNeighborhoodMode.Moore8;
        }
    }
}
