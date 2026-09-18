using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Construction API adapter over the project-wide terrain-aware pointer resolver.
    /// </summary>
    internal sealed class ScreenToGridConverter : IScreenToGridConverter
    {
        private readonly IWorldPointerGridResolver _resolver;
        private readonly IGridProjection _gridProjection;

        public ScreenToGridConverter(Camera camera)
            : this(
                camera,
                null,
                null,
                null,
                null,
                null)
        {
        }

        public ScreenToGridConverter(
            Camera camera,
            IGridProjection gridProjection,
            IConstructionGridGeometryService gridGeometry,
            IGridService gridService,
            IGeneratedTerrainLevelQuery terrainLevelQuery)
            : this(
                camera,
                null,
                gridProjection,
                gridGeometry as IGridWorldGeometryQuery
                ?? new ConstructionGeometryAdapter(gridGeometry),
                gridService,
                terrainLevelQuery as IGridTerrainSurfaceQuery
                ?? new GeneratedTerrainSurfaceAdapter(terrainLevelQuery))
        {
        }

        [Inject]
        public ScreenToGridConverter(
            Camera camera,
            [InjectOptional] IWorldPointerGridResolver resolver = null,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IGridWorldGeometryQuery gridGeometry = null,
            [InjectOptional] IGridService gridService = null,
            [InjectOptional] IGridTerrainSurfaceQuery terrainSurfaceQuery = null)
        {
            _gridProjection = gridProjection;
            _resolver = resolver
                        ?? new WorldPointerGridResolver(
                            camera,
                            gridProjection,
                            gridGeometry,
                            gridService,
                            terrainSurfaceQuery);
        }

        public Vector2Int ScreenToGrid(Vector2 screenPosition)
            => _resolver.ScreenToGrid(screenPosition);

        internal bool TryResolveTerrainSurfaceTile(Ray ray, out Vector2Int tile)
        {
            if (_resolver is WorldPointerGridResolver concreteResolver)
                return concreteResolver.TryResolveTerrainSurfaceTile(ray, out tile);

            tile = default;
            return false;
        }

        public Vector2Int WorldToGrid(Vector2 worldPosition)
        {
            Vector3 projectedWorldPosition = _gridProjection?.WorldPlane == GridWorldPlane.XZ
                ? new Vector3(worldPosition.x, 0f, worldPosition.y)
                : new Vector3(worldPosition.x, worldPosition.y, 0f);

            return _resolver.WorldToGrid(projectedWorldPosition);
        }

        private sealed class ConstructionGeometryAdapter : IGridWorldGeometryQuery
        {
            private readonly IConstructionGridGeometryService _geometry;

            public ConstructionGeometryAdapter(IConstructionGridGeometryService geometry)
            {
                _geometry = geometry;
            }

            public bool TryGetCellAtWorld(Vector3 worldPosition, out Vector2Int tile)
            {
                if (_geometry != null)
                    return _geometry.TryGetCellAtWorld(worldPosition, out tile);

                tile = default;
                return false;
            }

            public bool TryGetGridPlaneY(out float y)
            {
                if (_geometry != null)
                    return _geometry.TryGetGridPlaneY(out y);

                y = 0f;
                return false;
            }
        }

        private sealed class GeneratedTerrainSurfaceAdapter : IGridTerrainSurfaceQuery
        {
            private readonly IGeneratedTerrainLevelQuery _terrain;

            public GeneratedTerrainSurfaceAdapter(IGeneratedTerrainLevelQuery terrain)
            {
                _terrain = terrain;
            }

            public bool HasExplicitTerrainSurfaceMap
                => _terrain?.HasExplicitTerrainSurfaceMap == true;

            public int TerrainSurfaceVersion
                => _terrain is IGeneratedTerrainSurfaceVersionQuery versionQuery
                    ? versionQuery.TerrainSurfaceVersion
                    : 0;

            public bool TryGetTerrainLevel(Vector2Int position, out int level)
            {
                if (_terrain != null)
                    return _terrain.TryGetTerrainLevel(position, out level);

                level = 0;
                return false;
            }

            public bool TryGetTerrainSurfaceY(Vector2Int position, out float surfaceY)
            {
                if (_terrain != null)
                    return _terrain.TryGetTerrainSurfaceY(position, out surfaceY);

                surfaceY = 0f;
                return false;
            }
        }
    }
}
