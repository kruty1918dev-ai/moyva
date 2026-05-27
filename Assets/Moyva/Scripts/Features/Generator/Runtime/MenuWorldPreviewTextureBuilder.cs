using System.Collections.Generic;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Будує Texture2D для menu-only прев'ю світу з фінальних мап генератора.
    /// Не використовує gameplay runtime і не залежить від внутрішніх типів генерації.
    /// </summary>
    public static class MenuWorldPreviewTextureBuilder
    {
        private const int DefaultPixelsPerTile = 4;
        private const int DefaultMaxTextureEdge = 1024;
        private const float ObjectOverlayScale = 1.35f;
        private const float BuildingOverlayScale = 0.9f;
        private const float OverlayAlphaScale = 0.9f;
        private const int PrefabPreviewTextureSize = 64;
        private const float PrefabPreviewPadding = 1.25f;
        private const int PreviewRenderLayer = 30;
        private static MoyvaProjectSettingsSO _runtimeFallbackSettings;

        public static Texture2D Build(
            MenuWorldPreviewData previewData,
            TileRegistrySO tileRegistry,
            MapObjectRegistrySO objectRegistry = null,
            BuildingRegistrySO buildingRegistry = null,
            int pixelsPerTile = DefaultPixelsPerTile,
            int maxTextureEdge = DefaultMaxTextureEdge,
            MoyvaProjectSettingsSO projectSettings = null)
        {
            if (previewData == null || previewData.Width <= 0 || previewData.Height <= 0)
                return null;

            projectSettings = ResolveProjectSettings(projectSettings);

            if (ShouldRenderWorldPreview3D(projectSettings)
                && TryBuildWorldPreview3D(previewData, tileRegistry, objectRegistry, buildingRegistry, maxTextureEdge, projectSettings, out var worldPreviewTexture))
            {
                return worldPreviewTexture;
            }

            int tilePixelSize = Mathf.Max(1, pixelsPerTile);
            int maxEdge = Mathf.Max(64, maxTextureEdge);
            var layout = PreviewLayout.Create(previewData, tilePixelSize, maxEdge, projectSettings);

            tilePixelSize = layout.PixelsPerTile;
            int textureWidth = layout.TextureWidth;
            int textureHeight = layout.TextureHeight;

            var canvas = new Color[textureWidth * textureHeight];
            var tileSpriteCache = BuildTileSpriteCache(tileRegistry, projectSettings, out SpritePixelData fallbackTileSprite);

            if (layout.IsProjected)
            {
                DrawTerrainProjected(previewData, canvas, layout, tileSpriteCache, fallbackTileSprite);
                DrawObjectsProjected(previewData, objectRegistry, canvas, layout, projectSettings);
                DrawBuildingsProjected(previewData, buildingRegistry, canvas, layout, projectSettings);
            }
            else
            {
                DrawTerrain(previewData, canvas, textureWidth, tilePixelSize, tileSpriteCache, fallbackTileSprite);
                DrawObjects(previewData, objectRegistry, canvas, textureWidth, textureHeight, tilePixelSize, projectSettings);
                DrawBuildings(previewData, buildingRegistry, canvas, textureWidth, textureHeight, tilePixelSize, projectSettings);
            }

            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                filterMode = projectSettings.Uses3DProjectMode() ? projectSettings.PreviewFilterMode : FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.SetPixels(canvas);
            texture.Apply(false, false);
            return texture;
        }

        private static MoyvaProjectSettingsSO ResolveProjectSettings(MoyvaProjectSettingsSO projectSettings)
        {
            projectSettings ??= _runtimeFallbackSettings ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();
            projectSettings.Normalize();
            return projectSettings;
        }

        private static bool ShouldRenderWorldPreview3D(MoyvaProjectSettingsSO projectSettings)
        {
            return projectSettings != null
                && projectSettings.EnableMeshPrefabPreviews
                && projectSettings.Uses3DProjectMode();
        }

        private static bool TryBuildWorldPreview3D(
            MenuWorldPreviewData previewData,
            TileRegistrySO tileRegistry,
            MapObjectRegistrySO objectRegistry,
            BuildingRegistrySO buildingRegistry,
            int maxTextureEdge,
            MoyvaProjectSettingsSO projectSettings,
            out Texture2D texture)
        {
            texture = null;
            if (previewData?.BiomeMap == null || tileRegistry?.Definitions == null)
                return false;

            var tileMeshCache = BuildTileMeshDrawCache(tileRegistry);
            if (tileMeshCache.Count == 0)
                return false;

            var objectMeshCache = BuildObjectMeshDrawCache(objectRegistry);
            var buildingMeshCache = BuildBuildingMeshDrawCache(buildingRegistry);
            IGridProjection projection = GridProjectionFactory.Create(projectSettings);
            int targetWidth;
            int targetHeight;
            Resolve3DPreviewTextureSize(previewData, maxTextureEdge, out targetWidth, out targetHeight);

            RenderTexture temporary = null;
            RenderTexture previous = RenderTexture.active;
            Texture2D readable = null;
            GameObject cameraObject = null;
            GameObject lightObject = null;

            try
            {
                temporary = RenderTexture.GetTemporary(
                    targetWidth,
                    targetHeight,
                    24,
                    projectSettings.PreviewRenderTextureFormat,
                    projectSettings.PreviewRenderTextureReadWrite);

                cameraObject = new GameObject("MenuWorldPreview3DCamera")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                var camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.cullingMask = 1 << PreviewRenderLayer;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = temporary;

                Bounds worldBounds = Resolve3DWorldBounds(previewData, projection, projectSettings);
                Quaternion cameraRotation = Quaternion.Euler(projectSettings.ResolvePreviewCameraEuler());
                camera.transform.rotation = cameraRotation;
                bool usePerspective = projectSettings.ResolveUsePerspectivePreviewCamera();
                camera.orthographic = !usePerspective;
                Vector3 viewExtents = ResolveViewExtents(worldBounds, cameraRotation);
                float aspect = targetWidth / Mathf.Max(1f, targetHeight);
                float distance;
                if (usePerspective)
                {
                    float fieldOfView = projectSettings.ResolvePreviewPerspectiveFieldOfView();
                    camera.fieldOfView = fieldOfView;
                    float halfHeight = Mathf.Max(viewExtents.y, viewExtents.x / Mathf.Max(0.01f, aspect)) * projectSettings.ResolvePreviewPadding();
                    distance = halfHeight / Mathf.Tan(Mathf.Max(0.01f, fieldOfView * Mathf.Deg2Rad * 0.5f)) + viewExtents.z;
                }
                else
                {
                    camera.orthographicSize = Mathf.Max(viewExtents.y, viewExtents.x / Mathf.Max(0.01f, aspect)) * projectSettings.ResolvePreviewPadding();
                    distance = worldBounds.size.magnitude * 1.75f + 2f;
                }

                distance = Mathf.Max(1f, distance);
                camera.transform.position = worldBounds.center - camera.transform.forward * distance;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = Mathf.Max(distance + worldBounds.size.magnitude * 2f + 10f, 50f);

                lightObject = new GameObject("MenuWorldPreview3DLight")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = projectSettings.ResolvePreviewLightIntensity();
                light.cullingMask = 1 << PreviewRenderLayer;
                light.transform.rotation = Quaternion.Euler(projectSettings.PreviewLightEuler);

                RenderTexture.active = temporary;
                GL.Clear(true, true, Color.clear);

                int renderedTiles = DrawWorldMeshLayer(previewData.BiomeMap, tileMeshCache, projection, previewData, projectSettings, camera, 0f);
                DrawWorldMeshLayer(previewData.ObjectMap, objectMeshCache, projection, previewData, projectSettings, camera, 0.04f);
                DrawWorldMeshLayer(previewData.BuildingMap, buildingMeshCache, projection, previewData, projectSettings, camera, 0.08f);
                if (renderedTiles == 0)
                    return false;

                camera.Render();

                readable = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false, true)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = projectSettings.PreviewFilterMode,
                    wrapMode = TextureWrapMode.Clamp
                };
                RenderTexture.active = temporary;
                readable.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0, false);
                readable.Apply(false, false);

                Color[] pixels = readable.GetPixels();
                if (!HasVisiblePixels(pixels))
                    return false;

                texture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false, true)
                {
                    filterMode = projectSettings.PreviewFilterMode,
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixels(pixels);
                texture.Apply(false, false);

                Debug.Log($"[MenuPreview] Rendered 3D world preview: {renderedTiles} terrain mesh tiles, {targetWidth}x{targetHeight}, perspective={usePerspective}.");
                return true;
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[MenuPreview] 3D world preview failed, falling back to texture preview. {exception.Message}");
                return false;
            }
            finally
            {
                RenderTexture.active = previous;
                if (readable != null)
                    DestroyPreviewObject(readable);
                if (cameraObject != null)
                    DestroyPreviewObject(cameraObject);
                if (lightObject != null)
                    DestroyPreviewObject(lightObject);
                if (temporary != null)
                    RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static int DrawWorldMeshLayer(
            string[,] map,
            Dictionary<string, List<PreviewMeshDraw>> meshCache,
            IGridProjection projection,
            MenuWorldPreviewData previewData,
            MoyvaProjectSettingsSO projectSettings,
            Camera camera,
            float layerOffset)
        {
            if (map == null || meshCache == null || meshCache.Count == 0)
                return 0;

            int width = Mathf.Min(previewData.Width, map.GetLength(0));
            int height = Mathf.Min(previewData.Height, map.GetLength(1));
            int rendered = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    string id = NormalizeId(map[x, y]);
                    if (string.IsNullOrEmpty(id) || !meshCache.TryGetValue(id, out var draws))
                        continue;

                    Vector3 worldPosition = projection.GridToWorld(new Vector2Int(x, y), ResolvePreviewHeight(previewData, x, y, projectSettings), layerOffset);
                    DrawMeshPrefabAt(draws, Matrix4x4.Translate(worldPosition), camera);
                    rendered++;
                }
            }

            return rendered;
        }

        private static void DrawMeshPrefabAt(List<PreviewMeshDraw> draws, Matrix4x4 rootMatrix, Camera camera)
        {
            if (draws == null)
                return;

            for (int drawIndex = 0; drawIndex < draws.Count; drawIndex++)
            {
                var draw = draws[drawIndex];
                Matrix4x4 matrix = rootMatrix * draw.LocalMatrix;
                int subMeshCount = draw.Mesh.subMeshCount;
                for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                {
                    Material material = draw.Materials[Mathf.Min(subMeshIndex, draw.Materials.Length - 1)];
                    if (material == null)
                        continue;

                    Graphics.DrawMesh(
                        draw.Mesh,
                        matrix,
                        material,
                        PreviewRenderLayer,
                        camera,
                        subMeshIndex,
                        null,
                        ShadowCastingMode.Off,
                        receiveShadows: false);
                }
            }
        }

        private static Bounds Resolve3DWorldBounds(MenuWorldPreviewData previewData, IGridProjection projection, MoyvaProjectSettingsSO projectSettings)
        {
            Bounds bounds = projection.GetWorldBounds(previewData.Width, previewData.Height);
            float heightPadding = Mathf.Max(1f, projectSettings.HeightScale * 4f + 2f);
            if (previewData.HeightMap != null && projectSettings.UseHeightForPreview)
            {
                ResolveHeightRange(previewData, out float minHeight, out float maxHeight);
                float minWorldHeight = minHeight * projectSettings.HeightScale;
                float maxWorldHeight = maxHeight * projectSettings.HeightScale;
                bounds.center = new Vector3(bounds.center.x, (minWorldHeight + maxWorldHeight) * 0.5f, bounds.center.z);
                bounds.size = new Vector3(bounds.size.x, Mathf.Max(bounds.size.y, maxWorldHeight - minWorldHeight + heightPadding), bounds.size.z);
            }

            bounds.Expand(new Vector3(projectSettings.OrthogonalCellWidth, heightPadding, projectSettings.OrthogonalCellDepth));
            return bounds;
        }

        private static void ResolveHeightRange(MenuWorldPreviewData previewData, out float minHeight, out float maxHeight)
        {
            minHeight = 0f;
            maxHeight = 0f;
            if (previewData?.HeightMap == null)
                return;

            minHeight = float.MaxValue;
            maxHeight = float.MinValue;
            int width = Mathf.Min(previewData.Width, previewData.HeightMap.GetLength(0));
            int height = Mathf.Min(previewData.Height, previewData.HeightMap.GetLength(1));
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float value = previewData.HeightMap[x, y];
                if (value < minHeight) minHeight = value;
                if (value > maxHeight) maxHeight = value;
            }

            if (minHeight == float.MaxValue || maxHeight == float.MinValue)
            {
                minHeight = 0f;
                maxHeight = 0f;
            }
        }

        private static Vector3 ResolveViewExtents(Bounds bounds, Quaternion cameraRotation)
        {
            Quaternion worldToView = Quaternion.Inverse(cameraRotation);
            Vector3 extents = bounds.extents;
            float maxX = 0.01f;
            float maxY = 0.01f;
            float maxZ = 0.01f;

            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = new Vector3(extents.x * x, extents.y * y, extents.z * z);
                Vector3 view = worldToView * corner;
                maxX = Mathf.Max(maxX, Mathf.Abs(view.x));
                maxY = Mathf.Max(maxY, Mathf.Abs(view.y));
                maxZ = Mathf.Max(maxZ, Mathf.Abs(view.z));
            }

            return new Vector3(maxX, maxY, maxZ);
        }

        private static void Resolve3DPreviewTextureSize(MenuWorldPreviewData previewData, int maxTextureEdge, out int width, out int height)
        {
            int maxEdge = Mathf.Clamp(maxTextureEdge > 0 ? maxTextureEdge : DefaultMaxTextureEdge, 128, 2048);
            float aspect = previewData.Width / Mathf.Max(1f, previewData.Height);
            if (aspect >= 1f)
            {
                width = maxEdge;
                height = Mathf.Clamp(Mathf.RoundToInt(maxEdge / Mathf.Max(0.01f, aspect)), 128, maxEdge);
            }
            else
            {
                height = maxEdge;
                width = Mathf.Clamp(Mathf.RoundToInt(maxEdge * aspect), 128, maxEdge);
            }
        }

        private static Dictionary<string, List<PreviewMeshDraw>> BuildTileMeshDrawCache(TileRegistrySO registry)
        {
            var cache = new Dictionary<string, List<PreviewMeshDraw>>(System.StringComparer.OrdinalIgnoreCase);
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string id = NormalizeId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectMeshPreviewDraws(definition.VisualPrefab, out var draws, out _))
                    cache[id] = draws;
            }

            return cache;
        }

        private static Dictionary<string, List<PreviewMeshDraw>> BuildObjectMeshDrawCache(MapObjectRegistrySO registry)
        {
            var cache = new Dictionary<string, List<PreviewMeshDraw>>(System.StringComparer.OrdinalIgnoreCase);
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string id = NormalizeId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectMeshPreviewDraws(definition.VisualPrefab, out var draws, out _))
                    cache[id] = draws;
            }

            return cache;
        }

        private static Dictionary<string, List<PreviewMeshDraw>> BuildBuildingMeshDrawCache(BuildingRegistrySO registry)
        {
            var cache = new Dictionary<string, List<PreviewMeshDraw>>(System.StringComparer.OrdinalIgnoreCase);
            var definitions = registry?.GetAll();
            if (definitions == null)
                return cache;

            foreach (var definition in definitions)
            {
                string id = NormalizeId(definition?.Id);
                if (!string.IsNullOrEmpty(id) && TryCollectMeshPreviewDraws(definition.Prefab, out var draws, out _))
                    cache[id] = draws;
            }

            return cache;
        }

        private static float ResolvePreviewHeight(MenuWorldPreviewData previewData, int x, int y, MoyvaProjectSettingsSO projectSettings)
        {
            if (previewData.HeightMap == null || projectSettings == null || !projectSettings.UseHeightForPreview)
                return 0f;

            if (x < 0 || y < 0 || x >= previewData.HeightMap.GetLength(0) || y >= previewData.HeightMap.GetLength(1))
                return 0f;

            return previewData.HeightMap[x, y];
        }

        private static void DrawTerrain(
            MenuWorldPreviewData previewData,
            Color[] canvas,
            int canvasWidth,
            int pixelsPerTile,
            Dictionary<string, SpritePixelData> tileSpriteCache,
            SpritePixelData fallbackTileSprite)
        {
            bool hasBiomeMap = HasAnyStringValue(previewData.BiomeMap);
            if (hasBiomeMap)
            {
                bool hasFallbackSprite = fallbackTileSprite.IsValid;
                int hits = 0, misses = 0;
                var missedIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

                for (int y = 0; y < previewData.Height; y++)
                {
                    for (int x = 0; x < previewData.Width; x++)
                    {
                        string tileId = NormalizeId(previewData.BiomeMap[x, y]);
                        if (!string.IsNullOrEmpty(tileId) && tileSpriteCache.TryGetValue(tileId, out var spriteData))
                        {
                            hits++;
                            StampSprite(canvas, canvasWidth, x, y, pixelsPerTile, spriteData);
                        }
                        else if (hasFallbackSprite)
                        {
                            misses++;
                            if (!string.IsNullOrEmpty(tileId))
                                missedIds.Add(tileId);
                            StampSprite(canvas, canvasWidth, x, y, pixelsPerTile, fallbackTileSprite);
                        }
                        else
                        {
                            misses++;
                            if (!string.IsNullOrEmpty(tileId))
                                missedIds.Add(tileId);
                            FillTileRect(canvas, canvasWidth, x, y, pixelsPerTile, Color.black);
                        }
                    }
                }

                Debug.Log($"[MenuPreview] Terrain render: {hits} hits, {misses} misses. " +
                    (missedIds.Count > 0 ? $"Unknown tile IDs: [{string.Join(", ", missedIds)}]" : "All IDs resolved."));

                return;
            }

            DrawHeightFallback(previewData, canvas, canvasWidth, pixelsPerTile);
        }

        private static void DrawHeightFallback(
            MenuWorldPreviewData previewData,
            Color[] canvas,
            int canvasWidth,
            int pixelsPerTile)
        {
            if (previewData.HeightMap == null)
            {
                for (int y = 0; y < previewData.Height; y++)
                for (int x = 0; x < previewData.Width; x++)
                    FillTileRect(canvas, canvasWidth, x, y, pixelsPerTile, Color.black);
                return;
            }

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int y = 0; y < previewData.Height; y++)
            for (int x = 0; x < previewData.Width; x++)
            {
                float value = previewData.HeightMap[x, y];
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
            }

            float span = Mathf.Max(0.0001f, maxValue - minValue);
            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    float value = previewData.HeightMap[x, y];
                    float t = Mathf.Clamp01((value - minValue) / span);
                    FillTileRect(canvas, canvasWidth, x, y, pixelsPerTile, new Color(t, t, t, 1f));
                }
            }
        }

        private static void DrawTerrainProjected(
            MenuWorldPreviewData previewData,
            Color[] canvas,
            PreviewLayout layout,
            Dictionary<string, SpritePixelData> tileSpriteCache,
            SpritePixelData fallbackTileSprite)
        {
            bool hasBiomeMap = HasAnyStringValue(previewData.BiomeMap);
            if (!hasBiomeMap)
            {
                DrawHeightFallbackProjected(previewData, canvas, layout);
                return;
            }

            bool hasFallbackSprite = fallbackTileSprite.IsValid;
            int hits = 0, misses = 0;
            var missedIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    string tileId = NormalizeId(previewData.BiomeMap[x, y]);
                    Vector2Int center = layout.GetTileCenter(previewData, x, y);
                    float shade = layout.GetHeightShade(previewData, x, y);

                    if (!string.IsNullOrEmpty(tileId) && tileSpriteCache.TryGetValue(tileId, out var spriteData))
                    {
                        hits++;
                        StampSpriteCentered(canvas, layout.TextureWidth, layout.TextureHeight, center.x, center.y,
                            layout.TileDrawWidth, layout.TileDrawHeight, spriteData, shade);
                    }
                    else if (hasFallbackSprite)
                    {
                        misses++;
                        if (!string.IsNullOrEmpty(tileId))
                            missedIds.Add(tileId);
                        StampSpriteCentered(canvas, layout.TextureWidth, layout.TextureHeight, center.x, center.y,
                            layout.TileDrawWidth, layout.TileDrawHeight, fallbackTileSprite, shade);
                    }
                    else
                    {
                        misses++;
                        if (!string.IsNullOrEmpty(tileId))
                            missedIds.Add(tileId);
                        FillProjectedTile(canvas, layout, center.x, center.y, ShadeColor(Color.black, shade));
                    }
                }
            }

            Debug.Log($"[MenuPreview] Projected terrain ({layout.ProjectionMode}): {hits} hits, {misses} misses. " +
                (missedIds.Count > 0 ? $"Unknown tile IDs: [{string.Join(", ", missedIds)}]" : "All IDs resolved."));
        }

        private static void DrawHeightFallbackProjected(MenuWorldPreviewData previewData, Color[] canvas, PreviewLayout layout)
        {
            if (previewData.HeightMap == null)
            {
                for (int y = 0; y < previewData.Height; y++)
                for (int x = 0; x < previewData.Width; x++)
                {
                    Vector2Int center = layout.GetTileCenter(previewData, x, y);
                    FillProjectedTile(canvas, layout, center.x, center.y, Color.black);
                }

                return;
            }

            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    float t = layout.GetNormalizedHeight(previewData, x, y);
                    Vector2Int center = layout.GetTileCenter(previewData, x, y);
                    FillProjectedTile(canvas, layout, center.x, center.y, new Color(t, t, t, 1f));
                }
            }
        }

        private static void DrawObjectsProjected(
            MenuWorldPreviewData previewData,
            MapObjectRegistrySO objectRegistry,
            Color[] canvas,
            PreviewLayout layout,
            MoyvaProjectSettingsSO projectSettings)
        {
            if (!HasAnyStringValue(previewData.ObjectMap))
                return;

            var objectSpriteCache = BuildObjectSpriteCache(objectRegistry, projectSettings);
            int renderedWithSprites = 0;
            int renderedWithDots = 0;
            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    string objectId = NormalizeId(previewData.ObjectMap[x, y]);
                    if (string.IsNullOrEmpty(objectId))
                        continue;

                    Vector2Int center = layout.GetTileCenter(previewData, x, y);
                    if (TryResolveSpriteData(objectSpriteCache, objectId, out var spriteData))
                    {
                        StampSpriteOverlayCentered(canvas, layout.TextureWidth, layout.TextureHeight, center.x, center.y,
                            layout.TileDrawWidth, layout.TileDrawHeight, spriteData, ObjectOverlayScale, OverlayAlphaScale, anchorToBottom: true);
                        renderedWithSprites++;
                    }
                    else
                    {
                        StampDotCentered(canvas, layout.TextureWidth, layout.TextureHeight, center.x, center.y, layout.DotRadius, HashColor(objectId, OverlayAlphaScale));
                        renderedWithDots++;
                    }
                }
            }

            if (renderedWithDots > 0)
                Debug.Log($"[MenuPreview] Projected object fallback dots: {renderedWithDots}, sprite overlays: {renderedWithSprites}.");
        }

        private static void DrawBuildingsProjected(
            MenuWorldPreviewData previewData,
            BuildingRegistrySO buildingRegistry,
            Color[] canvas,
            PreviewLayout layout,
            MoyvaProjectSettingsSO projectSettings)
        {
            if (!HasAnyStringValue(previewData.BuildingMap))
                return;

            if (buildingRegistry == null)
            {
                Debug.LogWarning("[MenuPreview] BuildingMap has values, but BuildingRegistry is not assigned. Building overlay will be skipped.");
                return;
            }

            var buildingSpriteCache = BuildBuildingSpriteCache(buildingRegistry, projectSettings);
            int renderedWithSprites = 0;
            int renderedWithDots = 0;
            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    string buildingId = NormalizeId(previewData.BuildingMap[x, y]);
                    if (string.IsNullOrEmpty(buildingId))
                        continue;

                    Vector2Int center = layout.GetTileCenter(previewData, x, y);
                    if (TryResolveSpriteData(buildingSpriteCache, buildingId, out var spriteData))
                    {
                        StampSpriteOverlayCentered(canvas, layout.TextureWidth, layout.TextureHeight, center.x, center.y,
                            layout.TileDrawWidth, layout.TileDrawHeight, spriteData, BuildingOverlayScale, OverlayAlphaScale, anchorToBottom: true);
                        renderedWithSprites++;
                    }
                    else
                    {
                        StampDotCentered(canvas, layout.TextureWidth, layout.TextureHeight, center.x, center.y, layout.DotRadius, HashColor(buildingId, OverlayAlphaScale));
                        renderedWithDots++;
                    }
                }
            }

            if (renderedWithDots > 0)
                Debug.Log($"[MenuPreview] Projected building fallback dots: {renderedWithDots}, sprite overlays: {renderedWithSprites}.");
        }

        private static void DrawObjects(
            MenuWorldPreviewData previewData,
            MapObjectRegistrySO objectRegistry,
            Color[] canvas,
            int canvasWidth,
            int canvasHeight,
            int pixelsPerTile,
            MoyvaProjectSettingsSO projectSettings)
        {
            if (!HasAnyStringValue(previewData.ObjectMap))
                return;

            var objectSpriteCache = BuildObjectSpriteCache(objectRegistry, projectSettings);
            int renderedWithSprites = 0;
            int renderedWithDots = 0;
            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    string objectId = NormalizeId(previewData.ObjectMap[x, y]);
                    if (string.IsNullOrEmpty(objectId))
                        continue;

                    if (TryResolveSpriteData(objectSpriteCache, objectId, out var spriteData))
                    {
                        StampSpriteOverlay(canvas, canvasWidth, canvasHeight, x, y, pixelsPerTile, spriteData, ObjectOverlayScale, OverlayAlphaScale, anchorToBottom: true);
                        renderedWithSprites++;
                    }
                    else
                    {
                        StampDot(canvas, canvasWidth, canvasHeight, x, y, pixelsPerTile, HashColor(objectId, OverlayAlphaScale));
                        renderedWithDots++;
                    }
                }
            }

            if (renderedWithDots > 0)
                Debug.Log($"[MenuPreview] Object overlay fallback dots: {renderedWithDots}, sprite overlays: {renderedWithSprites}.");
        }

        private static void DrawBuildings(
            MenuWorldPreviewData previewData,
            BuildingRegistrySO buildingRegistry,
            Color[] canvas,
            int canvasWidth,
            int canvasHeight,
            int pixelsPerTile,
            MoyvaProjectSettingsSO projectSettings)
        {
            if (!HasAnyStringValue(previewData.BuildingMap))
                return;

            if (buildingRegistry == null)
            {
                Debug.LogWarning("[MenuPreview] BuildingMap has values, but BuildingRegistry is not assigned. Building overlay will be skipped.");
                return;
            }

            var buildingSpriteCache = BuildBuildingSpriteCache(buildingRegistry, projectSettings);
            int renderedWithSprites = 0;
            int renderedWithDots = 0;
            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    string buildingId = NormalizeId(previewData.BuildingMap[x, y]);
                    if (string.IsNullOrEmpty(buildingId))
                        continue;

                    if (TryResolveSpriteData(buildingSpriteCache, buildingId, out var spriteData))
                    {
                        StampSpriteOverlay(canvas, canvasWidth, canvasHeight, x, y, pixelsPerTile, spriteData, BuildingOverlayScale, OverlayAlphaScale, anchorToBottom: true);
                        renderedWithSprites++;
                    }
                    else
                    {
                        StampDot(canvas, canvasWidth, canvasHeight, x, y, pixelsPerTile, HashColor(buildingId, OverlayAlphaScale));
                        renderedWithDots++;
                    }
                }
            }

            if (renderedWithDots > 0)
                Debug.Log($"[MenuPreview] Building overlay fallback dots: {renderedWithDots}, sprite overlays: {renderedWithSprites}.");
        }

        private static bool HasAnyStringValue(string[,] map)
        {
            if (map == null)
                return false;

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!string.IsNullOrEmpty(map[x, y]))
                        return true;
                }
            }

            return false;
        }

        private static Dictionary<string, SpritePixelData> BuildTileSpriteCache(TileRegistrySO registry, MoyvaProjectSettingsSO projectSettings, out SpritePixelData fallbackSprite)
        {
            var cache = new Dictionary<string, SpritePixelData>(System.StringComparer.OrdinalIgnoreCase);
            fallbackSprite = default;

            if (registry?.Definitions == null)
            {
                Debug.LogWarning("[MenuPreview] TileRegistry is null or has no definitions!");
                return cache;
            }

            var failed = new System.Text.StringBuilder();
            foreach (var definition in registry.Definitions)
            {
                string tileId = NormalizeId(definition?.Id);
                if (string.IsNullOrEmpty(tileId))
                    continue;

                if (definition.VisualPrefab == null)
                {
                    failed.Append(tileId).Append("(no prefab) ");
                    continue;
                }

                if (ShouldUsePrefabPreview(projectSettings, definition.VisualPrefab)
                    && TryRenderPrefabPreviewPixels(definition.VisualPrefab, projectSettings, out var prefabData))
                {
                    cache[tileId] = prefabData;
                    if (!fallbackSprite.IsValid)
                        fallbackSprite = prefabData;
                    continue;
                }

                var spriteRenderer = TryGetPrimarySpriteRenderer(definition.VisualPrefab);
                if (spriteRenderer == null || spriteRenderer.sprite == null)
                {
                    if (TryGetPrefabRepresentativeColor(definition.VisualPrefab, tileId, out var prefabColor))
                    {
                        var solidData = CreateSolidSpriteData(prefabColor);
                        cache[tileId] = solidData;
                        if (!fallbackSprite.IsValid)
                            fallbackSprite = solidData;
                        continue;
                    }

                    failed.Append(tileId).Append("(no sprite/color) ");
                    continue;
                }

                if (!TryGetSpritePixels(spriteRenderer.sprite, spriteRenderer.color, out var spriteData))
                {
                    failed.Append(tileId).Append("(read fail) ");
                    continue;
                }

                cache[tileId] = spriteData;
                if (!fallbackSprite.IsValid)
                    fallbackSprite = spriteData;
            }

            Debug.Log($"[MenuPreview] Tile sprite cache: {cache.Count}/{registry.Definitions.Length} entries loaded. Keys: [{string.Join(", ", cache.Keys)}]");
            if (failed.Length > 0)
                Debug.LogWarning($"[MenuPreview] Failed to load sprites for: {failed}");

            return cache;
        }

        private static Dictionary<string, SpritePixelData> BuildObjectSpriteCache(MapObjectRegistrySO registry, MoyvaProjectSettingsSO projectSettings)
        {
            var cache = new Dictionary<string, SpritePixelData>(System.StringComparer.OrdinalIgnoreCase);
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string objectId = NormalizeId(definition?.Id);
                if (string.IsNullOrEmpty(objectId) || definition.VisualPrefab == null)
                    continue;

                if (ShouldUsePrefabPreview(projectSettings, definition.VisualPrefab)
                    && TryRenderPrefabPreviewPixels(definition.VisualPrefab, projectSettings, out var prefabData))
                {
                    cache[objectId] = prefabData;
                    continue;
                }

                var spriteRenderer = TryGetPrimarySpriteRenderer(definition.VisualPrefab);
                if (spriteRenderer == null || spriteRenderer.sprite == null)
                {
                    if (TryGetPrefabRepresentativeColor(definition.VisualPrefab, objectId, out var prefabColor))
                        cache[objectId] = CreateSolidSpriteData(prefabColor);
                    continue;
                }

                if (TryGetSpritePixels(spriteRenderer.sprite, spriteRenderer.color, out var spriteData))
                    cache[objectId] = spriteData;
            }

            return cache;
        }

        private static Dictionary<string, SpritePixelData> BuildBuildingSpriteCache(BuildingRegistrySO registry, MoyvaProjectSettingsSO projectSettings)
        {
            var cache = new Dictionary<string, SpritePixelData>(System.StringComparer.OrdinalIgnoreCase);
            var definitions = registry?.GetAll();
            if (definitions == null)
                return cache;

            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                string buildingId = NormalizeId(definition?.Id);
                if (string.IsNullOrEmpty(buildingId))
                    continue;

                if (ShouldUsePrefabPreview(projectSettings, definition.Prefab)
                    && TryRenderPrefabPreviewPixels(definition.Prefab, projectSettings, out var prefabData))
                {
                    cache[buildingId] = prefabData;
                    continue;
                }

                Sprite sprite = definition.Icon;
                Color tint = Color.white;
                if (sprite == null && definition.Prefab != null)
                {
                    var spriteRenderer = TryGetPrimarySpriteRenderer(definition.Prefab);
                    if (spriteRenderer != null)
                    {
                        sprite = spriteRenderer.sprite;
                        tint = spriteRenderer.color;
                    }
                }

                if (sprite == null)
                {
                    if (definition.Prefab != null && TryGetPrefabRepresentativeColor(definition.Prefab, buildingId, out var prefabColor))
                        cache[buildingId] = CreateSolidSpriteData(prefabColor);
                    continue;
                }

                if (TryGetSpritePixels(sprite, tint, out var spriteData))
                    cache[buildingId] = spriteData;
            }

            return cache;
        }

        private static bool ShouldUsePrefabPreview(MoyvaProjectSettingsSO projectSettings, GameObject prefab)
        {
            return prefab != null && MoyvaPrefabPreviewRenderer.HasMeshPreviewRenderer(prefab);
        }

        private static bool HasMeshPreviewRenderer(GameObject prefab)
        {
            if (prefab == null)
                return false;

            var meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var renderer = meshRenderers[i];
                var filter = renderer != null ? renderer.GetComponent<MeshFilter>() : null;
                if (renderer != null && renderer.enabled && filter != null && filter.sharedMesh != null)
                    return true;
            }

            var skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                var renderer = skinnedRenderers[i];
                if (renderer != null && renderer.enabled && renderer.sharedMesh != null)
                    return true;
            }

            return false;
        }

        private static bool TryRenderPrefabPreviewPixels(GameObject prefab, MoyvaProjectSettingsSO projectSettings, out SpritePixelData spriteData)
        {
            spriteData = default;
            if (!MoyvaPrefabPreviewRenderer.TryRenderMeshPrefabPreview(prefab, projectSettings, out var previewData))
                return false;

            spriteData = new SpritePixelData
            {
                Pixels = previewData.Pixels,
                Width = previewData.Width,
                Height = previewData.Height,
                IsValid = previewData.IsValid
            };
            return spriteData.IsValid;
        }

        private static bool TryCollectMeshPreviewDraws(GameObject prefab, out List<PreviewMeshDraw> draws, out Bounds bounds)
        {
            draws = new List<PreviewMeshDraw>();
            bounds = default;
            if (prefab == null)
                return false;

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
                draws.Add(new PreviewMeshDraw(mesh, renderer.sharedMaterials, localMatrix));
                EncapsulateBounds(ref bounds, ref hasBounds, TransformBounds(localMatrix, mesh.bounds));
            }

            var skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            for (int i = 0; i < skinnedRenderers.Length; i++)
            {
                var renderer = skinnedRenderers[i];
                Mesh mesh = renderer != null ? renderer.sharedMesh : null;
                if (renderer == null || !renderer.enabled || mesh == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Matrix4x4 localMatrix = rootWorldToLocal * renderer.transform.localToWorldMatrix;
                draws.Add(new PreviewMeshDraw(mesh, renderer.sharedMaterials, localMatrix));
                EncapsulateBounds(ref bounds, ref hasBounds, TransformBounds(localMatrix, renderer.localBounds));
            }

            return draws.Count > 0 && hasBounds && IsFinite(bounds.center) && IsFinite(bounds.size) && bounds.size.sqrMagnitude > 0.0001f;
        }

        private static Quaternion ResolvePrefabPreviewRotation(MoyvaProjectSettingsSO projectSettings)
        {
            projectSettings = ResolveProjectSettings(projectSettings);
            return projectSettings.DefaultProjectionMode == GridProjectionMode.Orthographic3D
                ? Quaternion.Euler(90f, 0f, 0f)
                : Quaternion.Euler(60f, 45f, 0f);
        }

        private static float ResolvePrefabPreviewOrthographicSize(Bounds bounds, Quaternion cameraRotation)
        {
            Quaternion worldToView = Quaternion.Inverse(cameraRotation);
            Vector3 extents = bounds.extents;
            float maxX = 0.01f;
            float maxY = 0.01f;

            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = new Vector3(extents.x * x, extents.y * y, extents.z * z);
                Vector3 view = worldToView * corner;
                maxX = Mathf.Max(maxX, Mathf.Abs(view.x));
                maxY = Mathf.Max(maxY, Mathf.Abs(view.y));
            }

            return Mathf.Max(maxX, maxY) * PrefabPreviewPadding;
        }

        private static Bounds TransformBounds(Matrix4x4 matrix, Bounds source)
        {
            Vector3 extents = source.extents;
            var result = new Bounds(matrix.MultiplyPoint3x4(source.center), Vector3.zero);
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 corner = source.center + new Vector3(extents.x * x, extents.y * y, extents.z * z);
                result.Encapsulate(matrix.MultiplyPoint3x4(corner));
            }

            return result;
        }

        private static void EncapsulateBounds(ref Bounds aggregate, ref bool hasBounds, Bounds value)
        {
            if (!hasBounds)
            {
                aggregate = value;
                hasBounds = true;
                return;
            }

            aggregate.Encapsulate(value);
        }

        private static bool HasVisiblePixels(Color[] pixels)
        {
            if (pixels == null)
                return false;

            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0.01f)
                    return true;
            }

            return false;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static void DestroyPreviewObject(Object instance)
        {
            if (instance == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(instance);
            else
                Object.DestroyImmediate(instance);
        }

        private static bool TryGetSpritePixels(Sprite sprite, Color tint, out SpritePixelData spriteData)
        {
            spriteData = default;
            if (sprite == null || sprite.texture == null)
                return false;

            Rect rect = sprite.textureRect;
            int x = Mathf.Max(0, Mathf.RoundToInt(rect.x));
            int y = Mathf.Max(0, Mathf.RoundToInt(rect.y));
            int width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(rect.height));

            Color[] pixels = null;
            try
            {
                pixels = sprite.texture.GetPixels(x, y, width, height);
            }
            catch
            {
                pixels = TryReadSpritePixelsViaRenderTexture(sprite, x, y, width, height);
            }

            if (pixels == null || pixels.Length == 0)
                return false;

            ApplyTint(pixels, tint);
            spriteData = new SpritePixelData
            {
                Pixels = pixels,
                Width = width,
                Height = height,
                IsValid = true
            };

            return true;
        }

        private static SpriteRenderer TryGetPrimarySpriteRenderer(GameObject prefab)
        {
            if (prefab == null)
                return null;

            var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            SpriteRenderer best = null;
            int bestSortingLayer = int.MinValue;
            int bestSortingOrder = int.MinValue;
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.sprite == null)
                    continue;

                if (best == null
                    || renderer.sortingLayerID > bestSortingLayer
                    || (renderer.sortingLayerID == bestSortingLayer && renderer.sortingOrder >= bestSortingOrder))
                {
                    best = renderer;
                    bestSortingLayer = renderer.sortingLayerID;
                    bestSortingOrder = renderer.sortingOrder;
                }
            }

            return best;
        }

        private static bool TryGetPrefabRepresentativeColor(GameObject prefab, string stableId, out Color color)
        {
            color = default;
            if (prefab == null)
                return false;

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer is SpriteRenderer)
                    continue;

                var material = renderer.sharedMaterial;
                if (material != null && material.HasProperty("_BaseColor"))
                {
                    color = material.GetColor("_BaseColor");
                    color.a = Mathf.Max(0.6f, color.a);
                    return true;
                }

                if (material != null && material.HasProperty("_Color"))
                {
                    color = material.color;
                    color.a = Mathf.Max(0.6f, color.a);
                    return true;
                }
            }

            color = HashColor(stableId, 0.9f);
            return true;
        }

        private static SpritePixelData CreateSolidSpriteData(Color color)
        {
            color.a = Mathf.Max(0.35f, color.a);
            return new SpritePixelData
            {
                Pixels = new[] { color },
                Width = 1,
                Height = 1,
                IsValid = true
            };
        }

        private static void StampSprite(Color[] canvas, int canvasWidth, int tileX, int tileY, int pixelsPerTile, SpritePixelData spriteData)
        {
            int startX = tileX * pixelsPerTile;
            int startY = tileY * pixelsPerTile;

            for (int py = 0; py < pixelsPerTile; py++)
            {
                int canvasY = startY + py;
                int sourceY = py * spriteData.Height / pixelsPerTile;
                for (int px = 0; px < pixelsPerTile; px++)
                {
                    int canvasX = startX + px;
                    int sourceX = px * spriteData.Width / pixelsPerTile;
                    Color source = spriteData.Pixels[sourceY * spriteData.Width + sourceX];
                    if (source.a < 0.05f)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = source.a >= 0.95f ? source : AlphaBlend(canvas[index], source);
                }
            }
        }

        private static void StampSpriteOverlay(
            Color[] canvas,
            int canvasWidth,
            int canvasHeight,
            int tileX,
            int tileY,
            int pixelsPerTile,
            SpritePixelData spriteData,
            float overlayScale,
            float alphaScale,
            bool anchorToBottom)
        {
            int targetHeight = Mathf.Max(1, Mathf.RoundToInt(pixelsPerTile * Mathf.Max(overlayScale, 0.1f)));
            int targetWidth = Mathf.Max(1, Mathf.RoundToInt(targetHeight * (spriteData.Width / (float)Mathf.Max(1, spriteData.Height))));

            int startX = tileX * pixelsPerTile + (pixelsPerTile - targetWidth) / 2;
            int startY = anchorToBottom
                ? tileY * pixelsPerTile + pixelsPerTile - targetHeight
                : tileY * pixelsPerTile + (pixelsPerTile - targetHeight) / 2;

            for (int py = 0; py < targetHeight; py++)
            {
                int canvasY = startY + py;
                if (canvasY < 0 || canvasY >= canvasHeight)
                    continue;

                int sourceY = py * spriteData.Height / targetHeight;
                for (int px = 0; px < targetWidth; px++)
                {
                    int canvasX = startX + px;
                    if (canvasX < 0 || canvasX >= canvasWidth)
                        continue;

                    int sourceX = px * spriteData.Width / targetWidth;
                    Color source = spriteData.Pixels[sourceY * spriteData.Width + sourceX];
                    source.a *= alphaScale;
                    if (source.a < 0.05f)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = source.a >= 0.95f ? source : AlphaBlend(canvas[index], source);
                }
            }
        }

        private static void StampSpriteCentered(
            Color[] canvas,
            int canvasWidth,
            int canvasHeight,
            int centerX,
            int centerY,
            int targetWidth,
            int targetHeight,
            SpritePixelData spriteData,
            float shade = 1f)
        {
            if (!spriteData.IsValid)
                return;

            targetWidth = Mathf.Max(1, targetWidth);
            targetHeight = Mathf.Max(1, targetHeight);
            int startX = centerX - targetWidth / 2;
            int startY = centerY - targetHeight / 2;

            for (int py = 0; py < targetHeight; py++)
            {
                int canvasY = startY + py;
                if (canvasY < 0 || canvasY >= canvasHeight)
                    continue;

                int sourceY = py * spriteData.Height / targetHeight;
                for (int px = 0; px < targetWidth; px++)
                {
                    int canvasX = startX + px;
                    if (canvasX < 0 || canvasX >= canvasWidth)
                        continue;

                    int sourceX = px * spriteData.Width / targetWidth;
                    Color source = ShadeColor(spriteData.Pixels[sourceY * spriteData.Width + sourceX], shade);
                    if (source.a < 0.05f)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = source.a >= 0.95f ? source : AlphaBlend(canvas[index], source);
                }
            }
        }

        private static void StampSpriteOverlayCentered(
            Color[] canvas,
            int canvasWidth,
            int canvasHeight,
            int centerX,
            int centerY,
            int tileWidth,
            int tileHeight,
            SpritePixelData spriteData,
            float overlayScale,
            float alphaScale,
            bool anchorToBottom)
        {
            if (!spriteData.IsValid)
                return;

            int targetHeight = Mathf.Max(1, Mathf.RoundToInt(tileHeight * Mathf.Max(overlayScale, 0.1f)));
            int targetWidth = Mathf.Max(1, Mathf.RoundToInt(targetHeight * (spriteData.Width / (float)Mathf.Max(1, spriteData.Height))));

            int startX = centerX - targetWidth / 2;
            int startY = anchorToBottom
                ? centerY + tileHeight / 2 - targetHeight
                : centerY - targetHeight / 2;

            for (int py = 0; py < targetHeight; py++)
            {
                int canvasY = startY + py;
                if (canvasY < 0 || canvasY >= canvasHeight)
                    continue;

                int sourceY = py * spriteData.Height / targetHeight;
                for (int px = 0; px < targetWidth; px++)
                {
                    int canvasX = startX + px;
                    if (canvasX < 0 || canvasX >= canvasWidth)
                        continue;

                    int sourceX = px * spriteData.Width / targetWidth;
                    Color source = spriteData.Pixels[sourceY * spriteData.Width + sourceX];
                    source.a *= alphaScale;
                    if (source.a < 0.05f)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = source.a >= 0.95f ? source : AlphaBlend(canvas[index], source);
                }
            }
        }

        private static void StampDotCentered(Color[] canvas, int canvasWidth, int canvasHeight, int centerX, int centerY, int radius, Color color)
        {
            radius = Mathf.Max(1, radius);
            int radiusSquared = radius * radius;
            for (int py = -radius; py <= radius; py++)
            {
                int canvasY = centerY + py;
                if (canvasY < 0 || canvasY >= canvasHeight)
                    continue;

                for (int px = -radius; px <= radius; px++)
                {
                    if (px * px + py * py > radiusSquared)
                        continue;

                    int canvasX = centerX + px;
                    if (canvasX < 0 || canvasX >= canvasWidth)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = AlphaBlend(canvas[index], color);
                }
            }
        }

        private static void FillProjectedTile(Color[] canvas, PreviewLayout layout, int centerX, int centerY, Color color)
        {
            if (layout.IsHex)
            {
                StampDotCentered(canvas, layout.TextureWidth, layout.TextureHeight, centerX, centerY, Mathf.Max(1, layout.DotRadius), color);
                return;
            }

            if (layout.IsIsometricLike)
            {
                int halfWidth = Mathf.Max(1, layout.TileDrawWidth / 2);
                int halfHeight = Mathf.Max(1, layout.TileDrawHeight / 2);
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    int canvasY = centerY + y;
                    if (canvasY < 0 || canvasY >= layout.TextureHeight)
                        continue;

                    float widthAtY = halfWidth * (1f - Mathf.Abs(y) / (float)Mathf.Max(1, halfHeight));
                    int span = Mathf.Max(1, Mathf.RoundToInt(widthAtY));
                    for (int x = -span; x <= span; x++)
                    {
                        int canvasX = centerX + x;
                        if (canvasX < 0 || canvasX >= layout.TextureWidth)
                            continue;

                        canvas[canvasY * layout.TextureWidth + canvasX] = AlphaBlend(canvas[canvasY * layout.TextureWidth + canvasX], color);
                    }
                }

                return;
            }

            int startX = centerX - layout.TileDrawWidth / 2;
            int startY = centerY - layout.TileDrawHeight / 2;
            for (int y = 0; y < layout.TileDrawHeight; y++)
            {
                int canvasY = startY + y;
                if (canvasY < 0 || canvasY >= layout.TextureHeight)
                    continue;

                for (int x = 0; x < layout.TileDrawWidth; x++)
                {
                    int canvasX = startX + x;
                    if (canvasX < 0 || canvasX >= layout.TextureWidth)
                        continue;

                    canvas[canvasY * layout.TextureWidth + canvasX] = color;
                }
            }
        }

        private static void StampDot(Color[] canvas, int canvasWidth, int canvasHeight, int tileX, int tileY, int pixelsPerTile, Color color)
        {
            int startX = tileX * pixelsPerTile;
            int startY = tileY * pixelsPerTile;
            float center = (pixelsPerTile - 1) * 0.5f;
            float radius = pixelsPerTile * 0.35f;
            float radiusSquared = radius * radius;

            for (int py = 0; py < pixelsPerTile; py++)
            {
                int canvasY = startY + py;
                if (canvasY >= canvasHeight)
                    break;

                for (int px = 0; px < pixelsPerTile; px++)
                {
                    int canvasX = startX + px;
                    if (canvasX >= canvasWidth)
                        break;

                    float deltaX = px - center;
                    float deltaY = py - center;
                    if (deltaX * deltaX + deltaY * deltaY > radiusSquared)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = AlphaBlend(canvas[index], color);
                }
            }
        }

        private static void FillTileRect(Color[] canvas, int canvasWidth, int tileX, int tileY, int pixelsPerTile, Color color)
        {
            int startX = tileX * pixelsPerTile;
            int startY = tileY * pixelsPerTile;
            for (int py = 0; py < pixelsPerTile; py++)
            {
                int rowStart = (startY + py) * canvasWidth + startX;
                for (int px = 0; px < pixelsPerTile; px++)
                    canvas[rowStart + px] = color;
            }
        }

        private static Color AlphaBlend(Color destination, Color source)
        {
            float sourceAlpha = source.a;
            float destinationAlpha = destination.a * (1f - sourceAlpha);
            float outputAlpha = sourceAlpha + destinationAlpha;
            if (outputAlpha < 0.001f)
                return Color.clear;

            return new Color(
                (source.r * sourceAlpha + destination.r * destinationAlpha) / outputAlpha,
                (source.g * sourceAlpha + destination.g * destinationAlpha) / outputAlpha,
                (source.b * sourceAlpha + destination.b * destinationAlpha) / outputAlpha,
                outputAlpha);
        }

            private static Color ShadeColor(Color color, float shade)
            {
                shade = Mathf.Max(0f, shade);
                color.r = Mathf.Clamp01(color.r * shade);
                color.g = Mathf.Clamp01(color.g * shade);
                color.b = Mathf.Clamp01(color.b * shade);
                return color;
            }

        private static void ApplyTint(Color[] pixels, Color tint)
        {
            if (pixels == null)
                return;

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] *= tint;
        }

        private static Color[] TryReadSpritePixelsViaRenderTexture(Sprite sprite, int x, int y, int width, int height)
        {
            RenderTexture temporary = null;
            RenderTexture previous = RenderTexture.active;
            Texture2D readable = null;

            try
            {
                int textureWidth = Mathf.Max(1, sprite.texture.width);
                int textureHeight = Mathf.Max(1, sprite.texture.height);
                temporary = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(sprite.texture, temporary);

                readable = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                RenderTexture.active = temporary;
                readable.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0, false);
                readable.Apply(false, false);

                int safeX = Mathf.Clamp(x, 0, textureWidth - 1);
                int safeY = Mathf.Clamp(y, 0, textureHeight - 1);
                int safeW = Mathf.Clamp(width, 1, textureWidth - safeX);
                int safeH = Mathf.Clamp(height, 1, textureHeight - safeY);
                return readable.GetPixels(safeX, safeY, safeW, safeH);
            }
            catch
            {
                return null;
            }
            finally
            {
                RenderTexture.active = previous;
                if (readable != null)
                    Object.Destroy(readable);
                if (temporary != null)
                    RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static string NormalizeId(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();
        }

        private static bool TryResolveSpriteData(Dictionary<string, SpritePixelData> cache, string id, out SpritePixelData spriteData)
        {
            spriteData = default;
            if (cache == null || cache.Count == 0 || string.IsNullOrWhiteSpace(id))
                return false;

            if (cache.TryGetValue(id, out spriteData))
                return true;

            string baseId = GetBaseId(id);
            if (!string.IsNullOrEmpty(baseId) && cache.TryGetValue(baseId, out spriteData))
                return true;

            foreach (var pair in cache)
            {
                if (pair.Key.StartsWith(baseId + "-", System.StringComparison.OrdinalIgnoreCase))
                {
                    spriteData = pair.Value;
                    return true;
                }
            }

            return false;
        }

        private static string GetBaseId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;

            string normalized = id.Trim();
            int separator = normalized.IndexOf('-');
            return separator > 0 ? normalized.Substring(0, separator) : normalized;
        }

        private static Color HashColor(string id, float alpha)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < id.Length; i++)
                    hash = hash * 31 + id[i];

                float r = ((hash >> 16) & 0xFF) / 255f;
                float g = ((hash >> 8) & 0xFF) / 255f;
                float b = (hash & 0xFF) / 255f;
                var color = Color.Lerp(new Color(0.25f, 0.35f, 0.25f, 1f), new Color(0.95f, 0.9f, 0.55f, 1f), 0.65f);
                color.r = Mathf.Lerp(color.r, r, 0.45f);
                color.g = Mathf.Lerp(color.g, g, 0.45f);
                color.b = Mathf.Lerp(color.b, b, 0.45f);
                color.a = Mathf.Clamp01(alpha);
                return color;
            }
        }

        private struct PreviewLayout
        {
            public GridProjectionMode ProjectionMode;
            public int PixelsPerTile;
            public int TextureWidth;
            public int TextureHeight;
            public int TileDrawWidth;
            public int TileDrawHeight;
            public int DotRadius;
            public int OffsetX;
            public int OffsetY;
            public int StepX;
            public int StepY;
            public int HeightPixelRange;
            public float MinHeight;
            public float HeightSpan;
            public bool UseHeight;
            public bool IsProjected;
            public bool IsHex;
            public bool IsIsometricLike;

            public static PreviewLayout Create(MenuWorldPreviewData previewData, int pixelsPerTile, int maxTextureEdge, MoyvaProjectSettingsSO settings)
            {
                pixelsPerTile = Mathf.Max(1, pixelsPerTile);
                var layout = CreateUnscaled(previewData, pixelsPerTile, settings);
                int longestEdge = Mathf.Max(layout.TextureWidth, layout.TextureHeight);
                if (longestEdge <= maxTextureEdge)
                    return layout;

                float scale = maxTextureEdge / (float)longestEdge;
                pixelsPerTile = Mathf.Max(1, Mathf.FloorToInt(pixelsPerTile * scale));
                return CreateUnscaled(previewData, pixelsPerTile, settings);
            }

            public Vector2Int GetTileCenter(MenuWorldPreviewData previewData, int x, int y)
            {
                int heightOffset = GetHeightOffset(previewData, x, y);
                switch (ProjectionMode)
                {
                    case GridProjectionMode.Isometric2D:
                    case GridProjectionMode.Isometric3DPreview:
                        return new Vector2Int(
                            OffsetX + (x - y) * StepX,
                            OffsetY + (x + y) * StepY - heightOffset);

                    case GridProjectionMode.HexPointy2D:
                        return new Vector2Int(
                            OffsetX + x * StepX + ((y & 1) != 0 ? StepX / 2 : 0),
                            OffsetY + y * StepY - heightOffset);

                    case GridProjectionMode.HexFlat2D:
                        return new Vector2Int(
                            OffsetX + x * StepX,
                            OffsetY + y * StepY + ((x & 1) != 0 ? StepY / 2 : 0) - heightOffset);

                    case GridProjectionMode.Orthographic3D:
                        return new Vector2Int(
                            OffsetX + x * PixelsPerTile + PixelsPerTile / 2,
                            OffsetY + y * PixelsPerTile + PixelsPerTile / 2 - heightOffset);

                    default:
                        return new Vector2Int(
                            x * PixelsPerTile + PixelsPerTile / 2,
                            y * PixelsPerTile + PixelsPerTile / 2);
                }
            }

            public float GetNormalizedHeight(MenuWorldPreviewData previewData, int x, int y)
            {
                if (!UseHeight || previewData.HeightMap == null || HeightSpan <= 0.0001f)
                    return 0f;

                return Mathf.Clamp01((previewData.HeightMap[x, y] - MinHeight) / HeightSpan);
            }

            public float GetHeightShade(MenuWorldPreviewData previewData, int x, int y)
            {
                if (!UseHeight)
                    return 1f;

                float t = GetNormalizedHeight(previewData, x, y);
                return Mathf.Lerp(0.78f, 1.18f, t);
            }

            private int GetHeightOffset(MenuWorldPreviewData previewData, int x, int y)
            {
                if (!UseHeight || HeightPixelRange <= 0)
                    return 0;

                return Mathf.RoundToInt(GetNormalizedHeight(previewData, x, y) * HeightPixelRange);
            }

            private static PreviewLayout CreateUnscaled(MenuWorldPreviewData previewData, int pixelsPerTile, MoyvaProjectSettingsSO settings)
            {
                GridProjectionMode mode = settings.DefaultProjectionMode;
                bool useHeight = settings.UseHeightForPreview && previewData.HeightMap != null;
                CalculateHeightRange(previewData, out float minHeight, out float maxHeight);
                int heightPixels = useHeight ? Mathf.RoundToInt(pixelsPerTile * Mathf.Max(0f, settings.HeightScale) * 3f) : 0;
                float heightSpan = Mathf.Max(0.0001f, maxHeight - minHeight);

                bool projected = mode == GridProjectionMode.Isometric2D
                    || mode == GridProjectionMode.Isometric3DPreview
                    || mode == GridProjectionMode.HexPointy2D
                    || mode == GridProjectionMode.HexFlat2D
                    || mode == GridProjectionMode.Orthographic3D;

                var layout = new PreviewLayout
                {
                    ProjectionMode = mode,
                    PixelsPerTile = pixelsPerTile,
                    UseHeight = useHeight,
                    MinHeight = minHeight,
                    HeightSpan = heightSpan,
                    HeightPixelRange = heightPixels,
                    IsProjected = projected
                };

                if (!projected)
                {
                    layout.TextureWidth = previewData.Width * pixelsPerTile;
                    layout.TextureHeight = previewData.Height * pixelsPerTile;
                    layout.TileDrawWidth = pixelsPerTile;
                    layout.TileDrawHeight = pixelsPerTile;
                    layout.DotRadius = Mathf.Max(1, Mathf.RoundToInt(pixelsPerTile * 0.35f));
                    return layout;
                }

                if (mode == GridProjectionMode.Isometric2D || mode == GridProjectionMode.Isometric3DPreview)
                {
                    int halfWidth = Mathf.Max(2, pixelsPerTile);
                    int halfHeight = Mathf.Max(1, Mathf.RoundToInt(pixelsPerTile * 0.5f));
                    layout.IsIsometricLike = true;
                    layout.StepX = halfWidth;
                    layout.StepY = halfHeight;
                    layout.TileDrawWidth = halfWidth * 2;
                    layout.TileDrawHeight = halfHeight * 2;
                    layout.OffsetX = previewData.Height * halfWidth + halfWidth;
                    layout.OffsetY = heightPixels + halfHeight + 1;
                    layout.TextureWidth = (previewData.Width + previewData.Height + 2) * halfWidth + layout.TileDrawWidth;
                    layout.TextureHeight = (previewData.Width + previewData.Height + 2) * halfHeight + layout.TileDrawHeight + heightPixels;
                    layout.DotRadius = Mathf.Max(1, halfHeight / 2);
                    return layout;
                }

                if (mode == GridProjectionMode.HexPointy2D)
                {
                    int radius = Mathf.Max(2, pixelsPerTile);
                    int hexWidth = Mathf.Max(2, Mathf.RoundToInt(Mathf.Sqrt(3f) * radius));
                    int hexHeight = radius * 2;
                    layout.IsHex = true;
                    layout.StepX = hexWidth;
                    layout.StepY = Mathf.Max(1, Mathf.RoundToInt(radius * 1.5f));
                    layout.TileDrawWidth = hexWidth;
                    layout.TileDrawHeight = hexHeight;
                    layout.OffsetX = hexWidth;
                    layout.OffsetY = heightPixels + radius + 1;
                    layout.TextureWidth = previewData.Width * hexWidth + hexWidth * 2;
                    layout.TextureHeight = (previewData.Height - 1) * layout.StepY + hexHeight + heightPixels + hexHeight;
                    layout.DotRadius = Mathf.Max(1, radius / 2);
                    return layout;
                }

                if (mode == GridProjectionMode.HexFlat2D)
                {
                    int radius = Mathf.Max(2, pixelsPerTile);
                    int hexWidth = radius * 2;
                    int hexHeight = Mathf.Max(2, Mathf.RoundToInt(Mathf.Sqrt(3f) * radius));
                    layout.IsHex = true;
                    layout.StepX = Mathf.Max(1, Mathf.RoundToInt(radius * 1.5f));
                    layout.StepY = hexHeight;
                    layout.TileDrawWidth = hexWidth;
                    layout.TileDrawHeight = hexHeight;
                    layout.OffsetX = radius + 1;
                    layout.OffsetY = heightPixels + hexHeight / 2 + 1;
                    layout.TextureWidth = (previewData.Width - 1) * layout.StepX + hexWidth * 2;
                    layout.TextureHeight = previewData.Height * hexHeight + hexHeight + heightPixels;
                    layout.DotRadius = Mathf.Max(1, radius / 2);
                    return layout;
                }

                layout.TileDrawWidth = pixelsPerTile;
                layout.TileDrawHeight = pixelsPerTile;
                layout.OffsetX = 0;
                layout.OffsetY = heightPixels;
                layout.TextureWidth = previewData.Width * pixelsPerTile;
                layout.TextureHeight = previewData.Height * pixelsPerTile + heightPixels;
                layout.DotRadius = Mathf.Max(1, Mathf.RoundToInt(pixelsPerTile * 0.35f));
                return layout;
            }

            private static void CalculateHeightRange(MenuWorldPreviewData previewData, out float minHeight, out float maxHeight)
            {
                minHeight = 0f;
                maxHeight = 1f;
                if (previewData.HeightMap == null)
                    return;

                minHeight = float.MaxValue;
                maxHeight = float.MinValue;
                for (int y = 0; y < previewData.Height; y++)
                for (int x = 0; x < previewData.Width; x++)
                {
                    float value = previewData.HeightMap[x, y];
                    if (value < minHeight) minHeight = value;
                    if (value > maxHeight) maxHeight = value;
                }

                if (minHeight == float.MaxValue || maxHeight == float.MinValue)
                {
                    minHeight = 0f;
                    maxHeight = 1f;
                }
            }
        }

        private readonly struct PreviewMeshDraw
        {
            public PreviewMeshDraw(Mesh mesh, Material[] materials, Matrix4x4 localMatrix)
            {
                Mesh = mesh;
                Materials = materials;
                LocalMatrix = localMatrix;
            }

            public readonly Mesh Mesh;
            public readonly Material[] Materials;
            public readonly Matrix4x4 LocalMatrix;
        }

        private struct SpritePixelData
        {
            public Color[] Pixels;
            public int Width;
            public int Height;
            public bool IsValid;
        }
    }
}