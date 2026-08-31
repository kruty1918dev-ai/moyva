using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualFallbackPresenter : IDisposable
    {
        private const string RootName = "Moyva Generated Map Fallback Visual";
        private const string MeshName = "MoyvaGeneratedMapFallbackTerrain";
        private const float HeightStep = 0.12f;

        private static readonly Vector2[] TileUvs =
        {
            new(0f, 0f),
            new(1f, 0f),
            new(1f, 1f),
            new(0f, 1f),
        };

        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly List<Material> _runtimeMaterials = new();

        private GameObject _root;
        private Mesh _mesh;

        public MapVisualFallbackPresenter(
            [InjectOptional] ITileWorldCreatorBuildEnvironment environment = null)
        {
            _environment = environment;
        }

        public TileWorldCreatorWorldBuildResult Present(GeneratedWorldData worldData)
        {
            Clear();

            if (worldData == null || worldData.Width <= 0 || worldData.Height <= 0)
                return TileWorldCreatorWorldBuildResult.Disabled;

            string[,] tileMap = worldData.VisualTileMap
                ?? worldData.GameplayTileMap
                ?? worldData.BiomeMap;
            if (tileMap == null)
                return TileWorldCreatorWorldBuildResult.Disabled;

            float cellSize = worldData.CellSize > 0.0001f ? worldData.CellSize : 1f;
            Transform parent = _environment?.Manager != null
                ? _environment.Manager.transform
                : null;

            _root = new GameObject(RootName)
            {
                hideFlags = HideFlags.DontSave
            };
            if (parent != null)
                _root.transform.SetParent(parent, worldPositionStays: false);

            _mesh = BuildMesh(worldData, tileMap, cellSize, out Material[] materials);
            if (_mesh == null || _mesh.vertexCount == 0 || materials == null || materials.Length == 0)
            {
                Clear();
                return TileWorldCreatorWorldBuildResult.Disabled;
            }

            var filter = _root.AddComponent<MeshFilter>();
            filter.sharedMesh = _mesh;

            var renderer = _root.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            var collider = _root.AddComponent<MeshCollider>();
            collider.sharedMesh = _mesh;

            bool hasBounds = GeneratedWorldBoundsUtility.TryCreateTileWorldBounds(
                _root.transform,
                worldData.Width,
                worldData.Height,
                cellSize,
                out Bounds bounds);

            return new TileWorldCreatorWorldBuildResult(
                terrainIds: null,
                objectIds: null,
                buildingIds: null,
                replaceTerrainVisuals: false,
                replaceObjectVisuals: false,
                replaceBuildingVisuals: false,
                suppressMoyvaLayerData: false,
                cellSize: cellSize,
                hasBaseMapWorldBounds: hasBounds,
                baseMapWorldBounds: bounds);
        }

        public void Clear()
        {
            if (_root != null)
                DestroyUnityObject(_root);
            _root = null;

            if (_mesh != null)
                DestroyUnityObject(_mesh);
            _mesh = null;

            for (int i = 0; i < _runtimeMaterials.Count; i++)
            {
                if (_runtimeMaterials[i] != null)
                    DestroyUnityObject(_runtimeMaterials[i]);
            }
            _runtimeMaterials.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        private Mesh BuildMesh(
            GeneratedWorldData worldData,
            string[,] tileMap,
            float cellSize,
            out Material[] materials)
        {
            int width = Mathf.Min(worldData.Width, tileMap.GetLength(0));
            int height = Mathf.Min(worldData.Height, tileMap.GetLength(1));
            int tileCount = width * height;

            var vertices = new Vector3[tileCount * 4];
            var normals = new Vector3[vertices.Length];
            var uvs = new Vector2[vertices.Length];
            var indicesByKind = new Dictionary<FallbackTileKind, List<int>>();

            int vertexIndex = 0;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                string tileId = tileMap[x, y];
                if (string.IsNullOrWhiteSpace(tileId))
                    continue;

                float elevation = ResolveElevation(worldData, x, y);
                float x0 = x * cellSize;
                float z0 = y * cellSize;
                float x1 = x0 + cellSize;
                float z1 = z0 + cellSize;

                vertices[vertexIndex] = new Vector3(x0, elevation, z0);
                vertices[vertexIndex + 1] = new Vector3(x1, elevation, z0);
                vertices[vertexIndex + 2] = new Vector3(x1, elevation, z1);
                vertices[vertexIndex + 3] = new Vector3(x0, elevation, z1);

                for (int i = 0; i < 4; i++)
                {
                    normals[vertexIndex + i] = Vector3.up;
                    uvs[vertexIndex + i] = TileUvs[i];
                }

                var kind = ResolveKind(tileId);
                if (!indicesByKind.TryGetValue(kind, out List<int> indices))
                {
                    indices = new List<int>(tileCount * 6 / 4);
                    indicesByKind.Add(kind, indices);
                }

                indices.Add(vertexIndex);
                indices.Add(vertexIndex + 2);
                indices.Add(vertexIndex + 1);
                indices.Add(vertexIndex);
                indices.Add(vertexIndex + 3);
                indices.Add(vertexIndex + 2);

                vertexIndex += 4;
            }

            if (vertexIndex == 0 || indicesByKind.Count == 0)
            {
                materials = Array.Empty<Material>();
                return null;
            }

            if (vertexIndex != vertices.Length)
            {
                Array.Resize(ref vertices, vertexIndex);
                Array.Resize(ref normals, vertexIndex);
                Array.Resize(ref uvs, vertexIndex);
            }

            var mesh = new Mesh
            {
                name = MeshName,
                hideFlags = HideFlags.DontSave,
                indexFormat = vertexIndex > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = indicesByKind.Count;

            materials = new Material[indicesByKind.Count];
            int subMesh = 0;
            foreach (KeyValuePair<FallbackTileKind, List<int>> pair in indicesByKind)
            {
                mesh.SetTriangles(pair.Value, subMesh, calculateBounds: false);
                materials[subMesh] = CreateMaterial(pair.Key);
                subMesh++;
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static float ResolveElevation(GeneratedWorldData worldData, int x, int y)
        {
            if (worldData.TerrainLevelMap != null
                && x < worldData.TerrainLevelMap.GetLength(0)
                && y < worldData.TerrainLevelMap.GetLength(1))
            {
                return worldData.TerrainLevelMap[x, y] * HeightStep;
            }

            if (worldData.HeightMap != null
                && x < worldData.HeightMap.GetLength(0)
                && y < worldData.HeightMap.GetLength(1))
            {
                return worldData.HeightMap[x, y] * HeightStep;
            }

            return 0f;
        }

        private Material CreateMaterial(FallbackTileKind kind)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Unlit/Color")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Hidden/InternalErrorShader");
            if (shader == null)
                return null;

            var material = new Material(shader)
            {
                name = $"Moyva Fallback {kind}",
                hideFlags = HideFlags.DontSave
            };
            ApplyColor(material, ResolveColor(kind));
            _runtimeMaterials.Add(material);
            return material;
        }

        private static void ApplyColor(Material material, Color color)
        {
            if (material == null)
                return;

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
        }

        private static FallbackTileKind ResolveKind(string tileId)
        {
            string id = tileId?.ToLowerInvariant() ?? string.Empty;
            if (id.Contains("water") || id.Contains("ocean"))
                return FallbackTileKind.Water;
            if (id.Contains("sand") || id.Contains("beach") || id.Contains("coast"))
                return FallbackTileKind.Sand;
            if (id.Contains("snow"))
                return FallbackTileKind.Snow;
            if (id.Contains("mountain"))
                return FallbackTileKind.Mountain;
            if (id.Contains("hill") || id.Contains("stone"))
                return FallbackTileKind.Hill;
            if (id.Contains("forest"))
                return FallbackTileKind.Forest;
            return FallbackTileKind.Grass;
        }

        private static Color ResolveColor(FallbackTileKind kind) => kind switch
        {
            FallbackTileKind.Water => new Color(0.12f, 0.30f, 0.48f, 1f),
            FallbackTileKind.Sand => new Color(0.68f, 0.58f, 0.34f, 1f),
            FallbackTileKind.Forest => new Color(0.16f, 0.38f, 0.20f, 1f),
            FallbackTileKind.Hill => new Color(0.37f, 0.36f, 0.31f, 1f),
            FallbackTileKind.Mountain => new Color(0.48f, 0.48f, 0.45f, 1f),
            FallbackTileKind.Snow => new Color(0.78f, 0.82f, 0.84f, 1f),
            _ => new Color(0.30f, 0.52f, 0.24f, 1f),
        };

        private static void DestroyUnityObject(UnityEngine.Object instance)
        {
            if (instance == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(instance);
            else
                UnityEngine.Object.DestroyImmediate(instance);
        }

        private enum FallbackTileKind
        {
            Grass,
            Water,
            Sand,
            Forest,
            Hill,
            Mountain,
            Snow
        }
    }
}
