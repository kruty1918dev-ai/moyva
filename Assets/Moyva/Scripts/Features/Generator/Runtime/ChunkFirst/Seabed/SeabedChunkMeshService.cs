using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Owns the shared seabed height field for the active world and emits the
    /// per-chunk opaque seabed mesh: one continuous low-poly surface under
    /// water bodies that deepens with distance from the shore. Replaces the
    /// per-cell bed columns from <see cref="TwcTileMeshSourceProvider"/>.
    /// </summary>
    internal sealed class SeabedChunkMeshService
    {
        private static readonly Vector2Int[] BorderDirs =
        {
            new Vector2Int(0, 1), new Vector2Int(1, 0),
            new Vector2Int(0, -1), new Vector2Int(-1, 0),
        };

        private readonly IRecipeHydrologyMap _hydrology;
        private readonly IAtlasTileSetCatalog _atlas;

        private int _fieldVersion = -1;
        private float _fieldCellSize;
        private int _fieldWidth;
        private int _fieldHeight;
        private SeabedFieldPlanner.Field _field;
        private bool[,] _waterMask;
        private float[,] _waterSurfaces;
        private float[,] _landSurfaces;
        private RecipeWaterKind[,] _kinds;
        private RecipeSeabedConfig _config;
        private Material _sandMaterial;
        private bool _sandResolved;

        public SeabedChunkMeshService(
            [InjectOptional] IRecipeHydrologyMap hydrology = null,
            [InjectOptional] IAtlasTileSetCatalog atlas = null)
        {
            _hydrology = hydrology;
            _atlas = atlas;
        }

        /// <summary>
        /// True when the recipe enables the seabed and every input needed to
        /// build it is present. When false the legacy per-cell bed columns
        /// keep rendering instead.
        /// </summary>
        public bool IsActive
            => _hydrology != null
               && _hydrology.HasHydrology
               && _config != null
               && _config.Enabled
               && ResolveSandMaterial() != null;

        /// <summary>
        /// Refreshes the cached field when the hydrology world changed. Call
        /// once per map before chunk iteration; the field is shared globally
        /// so chunk borders produce identical heights. Rendered surface
        /// heights come from the resolved compositions so the field is
        /// available at chunk-build time, before any deferred level-service
        /// publication.
        /// </summary>
        public void Prepare(
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            int mapWidth,
            int mapHeight,
            float cellSize)
        {
            _config = _hydrology?.Seabed;
            int version = _hydrology?.Version ?? -1;
            if (_field != null
                && _fieldVersion == version
                && _fieldWidth == mapWidth
                && _fieldHeight == mapHeight
                && Mathf.Approximately(_fieldCellSize, cellSize))
            {
                return;
            }

            _fieldVersion = version;
            _fieldWidth = mapWidth;
            _fieldHeight = mapHeight;
            _fieldCellSize = cellSize;
            _field = null;
            _waterMask = null;
            _waterSurfaces = null;
            _landSurfaces = null;
            _kinds = null;
            if (!IsActive || resolvedCells == null
                || mapWidth <= 0 || mapHeight <= 0 || cellSize <= 0.0001f)
            {
                return;
            }

            _waterMask = new bool[mapWidth, mapHeight];
            _waterSurfaces = new float[mapWidth, mapHeight];
            _landSurfaces = new float[mapWidth, mapHeight];
            _kinds = new RecipeWaterKind[mapWidth, mapHeight];
            bool anyWater = false;
            foreach (var pair in resolvedCells)
            {
                var cell = pair.Key;
                if (cell.x < 0 || cell.y < 0 || cell.x >= mapWidth || cell.y >= mapHeight)
                    continue;
                _kinds[cell.x, cell.y] = _hydrology.GetWaterKind(cell);
                // Same criterion the legacy bed columns used: a cell is
                // underwater when its rendered surface is a water sheet
                // floating above the terrain (Height < SurfaceHeight).
                // Flush sheets rest directly on the ground — no volume.
                bool water = false;
                if (pair.Value.HasMainTerrain)
                {
                    var main = pair.Value.MainTerrain;
                    water = main.TileGeometryMode == TileGeometryMode.SurfaceOnly
                        && !(float.IsNaN(main.Height) || float.IsNaN(main.SurfaceHeight))
                        && main.Height < main.SurfaceHeight - 0.0001f;
                }
                _waterMask[cell.x, cell.y] = water;
                anyWater |= water;
                float surface = pair.Value.HasMainTerrain
                    ? pair.Value.MainTerrain.SurfaceHeight
                    : float.NaN;
                if (water)
                {
                    // The rendered sheet height comes from the resolved
                    // sample, not the plan's drainage pseudo-surface.
                    _waterSurfaces[cell.x, cell.y] = surface;
                    _landSurfaces[cell.x, cell.y] = float.NaN;
                }
                else
                {
                    _waterSurfaces[cell.x, cell.y] = float.NaN;
                    _landSurfaces[cell.x, cell.y] = surface;
                }
            }

            if (!anyWater)
                return;

            _field = SeabedFieldPlanner.Build(
                mapWidth, mapHeight, cellSize,
                _waterMask, _waterSurfaces, _landSurfaces, _kinds, _config);
        }

        /// <summary>True once <see cref="Prepare"/> produced a shared field.</summary>
        public bool HasField => _field != null;

        /// <summary>Seabed top Y under a water cell; false on land or when inactive.</summary>
        public bool TryGetBedY(Vector2Int cell, out float bedY)
        {
            bedY = float.NaN;
            return _field != null && _field.TryGetBedY(cell, out bedY);
        }

        /// <summary>Meters of shore distance the field measured for a water cell.</summary>
        public bool TryGetShoreDistance(Vector2Int cell, out float distanceMeters)
        {
            distanceMeters = 0f;
            if (_field?.ShoreDistance == null
                || cell.x < 0 || cell.y < 0
                || cell.x >= _field.Width || cell.y >= _field.Height)
            {
                return false;
            }
            distanceMeters = _field.ShoreDistance[cell.x, cell.y];
            return !float.IsNaN(distanceMeters) && !float.IsInfinity(distanceMeters);
        }

        /// <summary>
        /// Emits the chunk's seabed as a single mesh plus its sand material;
        /// returns false when nothing covers this chunk.
        /// </summary>
        public bool TryBuildChunkMesh(
            RectInt coreRect,
            out Mesh mesh,
            out Material material)
        {
            mesh = null;
            material = null;
            if (_field == null || _config == null)
                return false;

            float cs = _field.CellSize;
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            var skirt = _config.BorderSkirtMeters;
            int w = _field.Width, h = _field.Height;

            for (int y = coreRect.yMin; y < coreRect.yMax; y++)
            for (int x = coreRect.xMin; x < coreRect.xMax; x++)
            {
                if (x < 0 || y < 0 || x >= w || y >= h || !_waterMask[x, y])
                    continue;
                if (!_field.TryGetBedY(new Vector2Int(x, y), out float centerY))
                    continue;

                // Fan: shared corner-lattice vertices + this cell's center.
                float x0 = (x - 0.5f) * cs, x1 = (x + 0.5f) * cs;
                float z0 = (y - 0.5f) * cs, z1 = (y + 0.5f) * cs;
                int baseIndex = verts.Count;
                AddCorner(x0, z0, x, y, centerY);
                AddCorner(x1, z0, x + 1, y, centerY);
                AddCorner(x1, z1, x + 1, y + 1, centerY);
                AddCorner(x0, z1, x, y + 1, centerY);
                verts.Add(new Vector3(x * cs, centerY, y * cs));
                normals.Add(_field.GetCenterNormal(new Vector2Int(x, y), _waterMask, _landSurfaces));
                uvs.Add(new Vector2(x, y));

                // Wound so the face normal points up (+Y) in Unity's LH space.
                for (int i = 0; i < 4; i++)
                {
                    tris.Add(baseIndex + i);
                    tris.Add(baseIndex + 4);
                    tris.Add(baseIndex + ((i + 1) & 3));
                }

                if (skirt > 0.0001f)
                    AddBorderSkirts(x, y, w, h, cs, skirt, verts, tris);
            }

            if (tris.Count == 0)
                return false;

            mesh = new Mesh
            {
                name = $"seabed_{coreRect.xMin}_{coreRect.yMin}",
                vertices = verts.ToArray(),
                normals = normals.ToArray(),
                uv = uvs.ToArray(),
                triangles = tris.ToArray(),
            };
            mesh.RecalculateBounds();
            material = ResolveSandMaterial();
            return material != null;

            void AddCorner(float wx, float wz, int vx, int vy, float fallbackY)
            {
                _field.TryGetCornerY(vx, vy, out float cy);
                if (float.IsNaN(cy))
                    cy = fallbackY;
                verts.Add(new Vector3(wx, cy, wz));
                normals.Add(_field.GetCornerNormal(vx, vy));
                uvs.Add(new Vector2(wx / cs, wz / cs));
            }

            void AddBorderSkirts(int x, int y, int mw, int mh, float csize, float drop,
                List<Vector3> v, List<int> t)
            {
                for (int i = 0; i < BorderDirs.Length; i++)
                {
                    int nx = x + BorderDirs[i].x, ny = y + BorderDirs[i].y;
                    if (nx >= 0 && ny >= 0 && nx < mw && ny < mh)
                        continue;
                    // The cell edge lying on the map border. Endpoints are
                    // ordered so the shared winding below faces the outward
                    // normal: north (+Z) and east (+X) take one direction,
                    // south/west take the opposite.
                    int ax, ay, bx, by;
                    switch (i)
                    {
                        case 0: ax = x; ay = y + 1; bx = x + 1; by = y + 1; break; // north: W->E
                        case 1: ax = x + 1; ay = y + 1; bx = x + 1; by = y; break; // east: N->S
                        case 2: ax = x + 1; ay = y; bx = x; by = y; break;         // south: E->W
                        default: ax = x; ay = y; bx = x; by = y + 1; break;      // west: S->N
                    }
                    if (!_field.TryGetCornerY(ax, ay, out float ya)
                        || !_field.TryGetCornerY(bx, by, out float yb))
                        continue;

                    int s = v.Count;
                    float wax = (ax - 0.5f) * csize, waz = (ay - 0.5f) * csize;
                    float wbx = (bx - 0.5f) * csize, wbz = (by - 0.5f) * csize;
                    var edgeNormal = new Vector3(BorderDirs[i].x, 0f, BorderDirs[i].y);
                    v.Add(new Vector3(wax, ya, waz));
                    v.Add(new Vector3(wbx, yb, wbz));
                    v.Add(new Vector3(wbx, yb - drop, wbz));
                    v.Add(new Vector3(wax, ya - drop, waz));
                    for (int k = 0; k < 4; k++)
                    {
                        normals.Add(edgeNormal);
                        uvs.Add(new Vector2(v[s + k].x / csize, v[s + k].z / csize));
                    }
                    t.Add(s); t.Add(s + 3); t.Add(s + 2);
                    t.Add(s); t.Add(s + 2); t.Add(s + 1);
                }
            }
        }

        private Material ResolveSandMaterial()
        {
            if (_sandResolved)
                return _sandMaterial;
            _sandResolved = true;
            if (_atlas != null
                && _atlas.IsLoaded
                && _atlas.TryGetByTileId("sand", out AtlasTileTheme theme)
                && theme?.Preset != null)
            {
                GameObject fill = theme.ResolveForm(AtlasTileForm.Fill, lowVariant: false);
                var renderer = fill != null
                    ? fill.GetComponentInChildren<MeshRenderer>()
                    : null;
                _sandMaterial = renderer != null ? renderer.sharedMaterial : null;
            }
            return _sandMaterial;
        }
    }
}
