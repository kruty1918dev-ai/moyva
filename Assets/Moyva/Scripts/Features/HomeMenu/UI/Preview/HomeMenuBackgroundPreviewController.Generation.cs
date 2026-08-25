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
        [ContextMenu("Regenerate Menu Preview")]
        public void RegeneratePreview()
        {
            var projectSettings = ResolveProjectSettings();
            bool useLiveMeshPreview = ShouldUseLiveMeshPreview(projectSettings);

            if (_targetImage == null && !useLiveMeshPreview)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] RawImage не призначено. Прев'ю меню не буде згенеровано.");
                return;
            }

            if (_graphAsset == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] GraphAsset не призначено. Прев'ю меню не буде згенеровано.");
                return;
            }

            var tileRegistry = ResolveTileRegistry();
            if (tileRegistry == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] TileRegistry не знайдено (ні в override, ні в GraphAsset). Прев'ю меню не буде згенеровано.");
                return;
            }

            Vector2Int mapSize = ResolveMapSize();
            int seed = Guid.NewGuid().GetHashCode();

            if (!MenuWorldPreviewGenerator.TryGenerate(_graphAsset, mapSize.x, mapSize.y, seed, out var previewData, out var errorMessage))
            {
                Debug.LogWarning($"[HomeMenuBackgroundPreview] Не вдалося згенерувати прев'ю меню: {errorMessage}");
                return;
            }

            if (_kingdomPlacement != null && _kingdomPlacement.Enabled)
            {
                var placementReport = _kingdomPlacementService != null
                    ? _kingdomPlacementService.Apply(previewData, _kingdomPlacement)
                    : MenuWorldPreviewKingdomPlacer.Apply(previewData, _kingdomPlacement);
                if (!string.IsNullOrWhiteSpace(placementReport.Warning))
                    Debug.LogWarning($"[HomeMenuBackgroundPreview] Kingdom placement warning: {placementReport.Warning}");

                Debug.Log($"[HomeMenuBackgroundPreview] Kingdom placement: {placementReport}");
            }

            if (useLiveMeshPreview && TryBuildLiveMeshPreview(previewData, tileRegistry, projectSettings))
            {
                DisposeGeneratedTexture();
                SetTexturePreviewVisible(false);
                _currentSeed = seed;
                ResetClouds();
                return;
            }

            DestroyLiveMeshPreview();
            SetTexturePreviewVisible(true);

            if (_targetImage == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] RawImage не призначено, а live mesh preview не вдалося побудувати.");
                return;
            }

            var textureRequest = new MenuWorldPreviewTextureBuildRequest(
                previewData,
                tileRegistry,
                _mapObjectRegistry,
                _buildingRegistry,
                _pixelsPerTile,
                _maxTextureEdge,
                projectSettings);
            var texture = _textureBuilderService != null
                ? _textureBuilderService.Build(textureRequest)
                : MenuWorldPreviewTextureBuilder.Build(
                    previewData,
                    tileRegistry,
                    _mapObjectRegistry,
                    _buildingRegistry,
                    _pixelsPerTile,
                    _maxTextureEdge,
                    projectSettings);

            if (texture == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] Texture builder повернув null. Прев'ю меню не оновлено.");
                return;
            }

            DisposeGeneratedTexture();
            _generatedTexture = texture;
            _currentSeed = seed;

            _targetImage.texture = _generatedTexture;
            _targetImage.color = Color.white;
            SetTexturePreviewVisible(true);
            ApplyCoverUv();

            ResetClouds();
        }

        private MoyvaProjectSettingsSO ResolveProjectSettings()
        {
            var settings = _projectSettings != null
                ? _projectSettings
                : _runtimeFallbackSettings ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();

            settings.Normalize();
            return settings;
        }

        private static bool ShouldUseLiveMeshPreview(MoyvaProjectSettingsSO projectSettings)
        {
            return projectSettings != null
                && projectSettings.UseLiveHomeMenuMeshPreview
                && projectSettings.EnableMeshPrefabPreviews
                && projectSettings.Uses3DProjectMode();
        }

        private Vector2Int ResolveMapSize()
        {
            if (_mapTileCount.x > 0 && _mapTileCount.y > 0)
                return new Vector2Int(_mapTileCount.x, _mapTileCount.y);

            if (_graphAsset != null && _graphAsset.SharedSettings != null && _graphAsset.SharedSettings.HasMapSize)
                return _graphAsset.SharedSettings.MapSize;

            return new Vector2Int(128, 72);
        }

        private TileRegistrySO ResolveTileRegistry()
        {
            if (_tileRegistryOverride != null)
                return _tileRegistryOverride;

            return _graphAsset != null ? _graphAsset.TileRegistry : null;
        }

        [ContextMenu("Validate Kingdom Placement Rules")]
        private void ValidateKingdomPlacementRules()
        {
            ValidateKingdomPlacementSettings();

            if (_kingdomPlacement == null)
            {
                Debug.LogWarning("[HomeMenuBackgroundPreview] Kingdom placement settings is null.");
                return;
            }

            string issues = BuildKingdomPlacementIssues();
            if (string.IsNullOrEmpty(issues))
            {
                Debug.Log("[HomeMenuBackgroundPreview] Kingdom placement rules look valid.");
                return;
            }

            Debug.LogWarning($"[HomeMenuBackgroundPreview] Kingdom placement rules warnings:\n{issues}");
        }

        private void ValidateKingdomPlacementSettings()
        {
            if (_kingdomPlacement == null)
                _kingdomPlacement = new MenuPreviewKingdomPlacementSettings();

            _kingdomPlacement.ClampAndNormalize();
        }

        private string BuildKingdomPlacementIssues()
        {
            if (_kingdomPlacement == null)
                return "- Settings are missing.";

            var issues = new List<string>();

            if (_kingdomPlacement.KingdomAZone.Overlaps(_kingdomPlacement.KingdomBZone))
                issues.Add("- KingdomAZone overlaps KingdomBZone.");

            if (string.IsNullOrWhiteSpace(_kingdomPlacement.CastleBuildingId))
                issues.Add("- CastleBuildingId is empty.");

            if (string.IsNullOrWhiteSpace(_kingdomPlacement.TownHallBuildingId))
                issues.Add("- TownHallBuildingId is empty.");

            if (_kingdomPlacement.MinHeight > _kingdomPlacement.MaxHeight)
                issues.Add("- MinHeight is greater than MaxHeight.");

            return issues.Count == 0 ? string.Empty : string.Join("\n", issues);
        }

    }
}
