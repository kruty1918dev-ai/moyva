using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Clouds.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed partial class HomeMenuBackgroundPreviewController
    {
        private static int ResolveLivePreviewTileStride(MenuWorldPreviewData previewData, MoyvaProjectSettingsSO projectSettings)
        {
            int stride = Mathf.Max(1, projectSettings.HomeMenuPreviewTileStride);
            int maxTerrainTiles = Mathf.Max(1, projectSettings.HomeMenuPreviewMaxTerrainTiles);
            while (CountSampledCells(previewData.Width, previewData.Height, stride) > maxTerrainTiles)
                stride++;

            return stride;
        }

        private static int CountSampledCells(int width, int height, int stride)
        {
            int sampledWidth = Mathf.CeilToInt(width / Mathf.Max(1f, stride));
            int sampledHeight = Mathf.CeilToInt(height / Mathf.Max(1f, stride));
            return Mathf.Max(1, sampledWidth) * Mathf.Max(1, sampledHeight);
        }

        private static float ResolvePreviewHeight(MenuWorldPreviewData previewData, int x, int y, MoyvaProjectSettingsSO projectSettings)
        {
            if (previewData.HeightMap == null || projectSettings == null || !projectSettings.UseHeightForPreview)
                return 0f;

            if (x < 0 || y < 0 || x >= previewData.HeightMap.GetLength(0) || y >= previewData.HeightMap.GetLength(1))
                return 0f;

            return previewData.HeightMap[x, y];
        }

        private static Dictionary<string, LivePreviewPrefabMesh> BuildTileLiveMeshCache(TileRegistrySO registry)
        {
            var cache = new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string id = NormalizePreviewId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectLivePreviewPrefab(definition.VisualPrefab, out var prefabMesh))
                    cache[id] = prefabMesh;
            }

            return cache;
        }

        private static Dictionary<string, LivePreviewPrefabMesh> BuildObjectLiveMeshCache(MapObjectRegistrySO registry)
        {
            var cache = new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string id = NormalizePreviewId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectLivePreviewPrefab(definition.VisualPrefab, out var prefabMesh))
                    cache[id] = prefabMesh;
            }

            return cache;
        }

        private static Dictionary<string, LivePreviewPrefabMesh> BuildBuildingLiveMeshCache(BuildingRegistrySO registry)
        {
            var cache = new Dictionary<string, LivePreviewPrefabMesh>(StringComparer.OrdinalIgnoreCase);
            var definitions = registry?.GetAll();
            if (definitions == null)
                return cache;

            foreach (var definition in definitions)
            {
                string id = NormalizePreviewId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectLivePreviewPrefab(definition.Prefab, out var prefabMesh))
                    cache[id] = prefabMesh;
            }

            return cache;
        }

        private static bool TryCollectLivePreviewPrefab(GameObject prefab, out LivePreviewPrefabMesh prefabMesh)
        {
            prefabMesh = null;
            if (prefab == null)
                return false;

            var draws = new List<LivePreviewMeshDraw>();
            Bounds bounds = default;
            bool hasBounds = false;
            Matrix4x4 rootWorldToLocal = prefab.transform.worldToLocalMatrix;

            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var renderer = meshRenderers[i];
                var filter = renderer != null ? renderer.GetComponent<MeshFilter>() : null;
                Mesh mesh = filter != null ? filter.sharedMesh : null;
                if (renderer == null || !renderer.enabled || mesh == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Matrix4x4 localMatrix = rootWorldToLocal * renderer.transform.localToWorldMatrix;
                Bounds localBounds = TransformBounds(localMatrix, mesh.bounds);
                draws.Add(new LivePreviewMeshDraw(mesh, renderer.sharedMaterials, localMatrix, localBounds));
                EncapsulateBounds(ref bounds, ref hasBounds, localBounds);
            }

            var skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                var renderer = skinnedRenderers[i];
                Mesh mesh = renderer != null ? renderer.sharedMesh : null;
                if (renderer == null || !renderer.enabled || mesh == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Matrix4x4 localMatrix = rootWorldToLocal * renderer.transform.localToWorldMatrix;
                Bounds localBounds = TransformBounds(localMatrix, renderer.localBounds);
                draws.Add(new LivePreviewMeshDraw(mesh, renderer.sharedMaterials, localMatrix, localBounds));
                EncapsulateBounds(ref bounds, ref hasBounds, localBounds);
            }

            if (draws.Count == 0 || !hasBounds || !IsFinite(bounds.center) || !IsFinite(bounds.size) || bounds.size.sqrMagnitude <= 0.0001f)
                return false;

            prefabMesh = new LivePreviewPrefabMesh(draws, bounds);
            return true;
        }

        private static string NormalizePreviewId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static int ToPreviewIndex(int x, int y, int width)
        {
            return y * Mathf.Max(1, width) + x;
        }

        private sealed class LivePreviewMeshBuilder
        {
            private readonly Transform _parent;
            private readonly int _layer;
            private readonly bool _combineMeshes;
            private readonly bool _uploadMeshData;
            private readonly bool _castShadows;
            private readonly bool _receiveShadows;
            private readonly int _maxVerticesPerBatch;
            private readonly int _maxMaterialBatches;
            private readonly List<Mesh> _ownedMeshes;
            private readonly Dictionary<Material, List<LivePreviewMeshBatch>> _batches = new Dictionary<Material, List<LivePreviewMeshBatch>>();
            private Bounds _worldBounds;
            private bool _hasBounds;
            private int _meshObjectCount;

            public LivePreviewMeshBuilder(Transform parent, int layer, MoyvaProjectSettingsSO settings, List<Mesh> ownedMeshes)
            {
                _parent = parent;
                _layer = layer;
                _combineMeshes = settings.HomeMenuPreviewCombineMeshesByMaterial;
                _uploadMeshData = settings.HomeMenuPreviewUploadMeshData;
                _castShadows = settings.HomeMenuPreviewCastShadows;
                _receiveShadows = settings.HomeMenuPreviewReceiveShadows;
                _maxVerticesPerBatch = Mathf.Max(1024, settings.HomeMenuPreviewMaxVerticesPerBatch);
                _maxMaterialBatches = Mathf.Max(1, settings.HomeMenuPreviewMaxMaterialBatches);
                _ownedMeshes = ownedMeshes;
                _worldBounds = new Bounds(Vector3.zero, Vector3.one);
            }

            public Bounds WorldBounds => _hasBounds ? _worldBounds : new Bounds(Vector3.zero, Vector3.one);

            public void AddPrefab(LivePreviewPrefabMesh prefabMesh, Matrix4x4 rootMatrix)
            {
                if (prefabMesh == null || prefabMesh.Draws == null)
                    return;

                Bounds prefabWorldBounds = TransformBounds(rootMatrix, prefabMesh.Bounds);
                EncapsulateBounds(ref _worldBounds, ref _hasBounds, prefabWorldBounds);

                for (int i = 0; i < prefabMesh.Draws.Count; i++)
                {
                    var draw = prefabMesh.Draws[i];
                    Matrix4x4 matrix = rootMatrix * draw.LocalMatrix;
                    if (_combineMeshes)
                        AddDrawToBatches(draw, matrix);
                    else
                        CreateDrawObject(draw, matrix);
                }
            }

            public int Flush()
            {
                if (!_combineMeshes)
                    return _meshObjectCount;

                foreach (var pair in _batches)
                {
                    var materialBatches = pair.Value;
                    for (int i = 0; i < materialBatches.Count; i++)
                        CreateCombinedObject(pair.Key, materialBatches[i], i);
                }

                _batches.Clear();
                return _meshObjectCount;
            }

            private void AddDrawToBatches(LivePreviewMeshDraw draw, Matrix4x4 matrix)
            {
                if (draw.Mesh == null || draw.Materials == null || draw.Materials.Length == 0)
                    return;

                int subMeshCount = draw.Mesh.subMeshCount;
                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                {
                    Material material = draw.Materials[Mathf.Min(subMeshIndex, draw.Materials.Length - 1)];
                    if (material == null || !TryGetWritableBatch(material, draw.Mesh.vertexCount, out var batch))
                        continue;

                    batch.Instances.Add(new CombineInstance
                    {
                        mesh = draw.Mesh,
                        subMeshIndex = subMeshIndex,
                        transform = matrix
                    });
                    batch.VertexCount += draw.Mesh.vertexCount;
                }
            }

            private bool TryGetWritableBatch(Material material, int vertexCount, out LivePreviewMeshBatch batch)
            {
                if (!_batches.TryGetValue(material, out var materialBatches))
                {
                    if (_batches.Count >= _maxMaterialBatches)
                    {
                        batch = null;
                        return false;
                    }

                    materialBatches = new List<LivePreviewMeshBatch>();
                    _batches.Add(material, materialBatches);
                }

                if (materialBatches.Count == 0
                    || materialBatches[materialBatches.Count - 1].VertexCount + vertexCount > _maxVerticesPerBatch)
                {
                    materialBatches.Add(new LivePreviewMeshBatch());
                }

                batch = materialBatches[materialBatches.Count - 1];
                return true;
            }

            private void CreateCombinedObject(Material material, LivePreviewMeshBatch batch, int batchIndex)
            {
                if (batch == null || batch.Instances.Count == 0 || material == null)
                    return;

                var mesh = new Mesh
                {
                    name = $"HomeMenuPreview_{SanitizeName(material.name)}_{batchIndex}",
                    hideFlags = HideFlags.DontSave
                };
                mesh.indexFormat = batch.VertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                mesh.CombineMeshes(batch.Instances.ToArray(), mergeSubMeshes: true, useMatrices: true, hasLightmapData: false);
                mesh.RecalculateBounds();
                if (_uploadMeshData)
                    mesh.UploadMeshData(markNoLongerReadable: true);

                _ownedMeshes.Add(mesh);

                var meshObject = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer))
                {
                    hideFlags = HideFlags.DontSave,
                    layer = _layer
                };
                meshObject.transform.SetParent(_parent, worldPositionStays: false);
                meshObject.GetComponent<MeshFilter>().sharedMesh = mesh;

                var renderer = meshObject.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                ApplyRendererSettings(renderer);
                _meshObjectCount++;
            }

            private void CreateDrawObject(LivePreviewMeshDraw draw, Matrix4x4 matrix)
            {
                if (draw.Mesh == null || draw.Materials == null || draw.Materials.Length == 0)
                    return;

                var meshObject = new GameObject($"HomeMenuPreview_{draw.Mesh.name}", typeof(MeshFilter), typeof(MeshRenderer))
                {
                    hideFlags = HideFlags.DontSave,
                    layer = _layer
                };
                meshObject.transform.SetParent(_parent, worldPositionStays: true);
                ApplyMatrix(meshObject.transform, matrix);
                meshObject.GetComponent<MeshFilter>().sharedMesh = draw.Mesh;

                var renderer = meshObject.GetComponent<MeshRenderer>();
                renderer.sharedMaterials = draw.Materials;
                ApplyRendererSettings(renderer);
                _meshObjectCount++;
            }

            private void ApplyRendererSettings(Renderer renderer)
            {
                renderer.shadowCastingMode = _castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
                renderer.receiveShadows = _receiveShadows;
                renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            }

            private static void ApplyMatrix(Transform target, Matrix4x4 matrix)
            {
                Vector3 position = matrix.GetColumn(3);
                Vector3 right = matrix.GetColumn(0);
                Vector3 up = matrix.GetColumn(1);
                Vector3 forward = matrix.GetColumn(2);

                Vector3 scale = new Vector3(right.magnitude, up.magnitude, forward.magnitude);
                if (scale.x > 0.0001f) right /= scale.x;
                if (scale.y > 0.0001f) up /= scale.y;
                if (scale.z > 0.0001f) forward /= scale.z;

                target.position = position;
                target.rotation = Quaternion.LookRotation(forward.sqrMagnitude > 0.0001f ? forward : Vector3.forward,
                    up.sqrMagnitude > 0.0001f ? up : Vector3.up);
                target.localScale = scale;
            }

            private static string SanitizeName(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return "Material";

                return value.Replace('/', '_').Replace('\\', '_').Replace(':', '_');
            }
        }

        private sealed class LivePreviewMeshBatch
        {
            public readonly List<CombineInstance> Instances = new List<CombineInstance>();
            public int VertexCount;
        }

        private sealed class LivePreviewPrefabMesh
        {
            public LivePreviewPrefabMesh(List<LivePreviewMeshDraw> draws, Bounds bounds)
            {
                Draws = draws;
                Bounds = bounds;
            }

            public List<LivePreviewMeshDraw> Draws { get; }
            public Bounds Bounds { get; }
        }

        private readonly struct LivePreviewMeshDraw
        {
            public LivePreviewMeshDraw(Mesh mesh, Material[] materials, Matrix4x4 localMatrix, Bounds localBounds)
            {
                Mesh = mesh;
                Materials = materials;
                LocalMatrix = localMatrix;
                LocalBounds = localBounds;
            }

            public Mesh Mesh { get; }
            public Material[] Materials { get; }
            public Matrix4x4 LocalMatrix { get; }
            public Bounds LocalBounds { get; }
        }

        private sealed class LivePreviewCameraFogOverride : MonoBehaviour
        {
            private bool _previousFog;
            private bool _hasPreviousFog;

            public bool DisableFog { get; set; }

            private void OnPreCull()
            {
                if (!DisableFog)
                    return;

                _previousFog = RenderSettings.fog;
                _hasPreviousFog = true;
                RenderSettings.fog = false;
            }

            private void OnPostRender()
            {
                RestoreFog();
            }

            private void OnDisable()
            {
                RestoreFog();
            }

            private void RestoreFog()
            {
                if (!_hasPreviousFog)
                    return;

                RenderSettings.fog = _previousFog;
                _hasPreviousFog = false;
            }
        }

    }
}
