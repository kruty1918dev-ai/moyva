using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitFactory : IUnitFactory
    {
        private readonly DiContainer _container;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IUnitGameplayProfileService _unitGameplayProfileService;
        private readonly SignalBus _signalBus;
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IGridProjection _gridProjection;
        private readonly IGeneratedTerrainLevelQuery _terrainLevelQuery;
        private readonly TileRegistrySO _tileRegistry;
        private readonly ITileSettingsService _tileSettings;
        private readonly Dictionary<string, float> _tileSurfaceOffsetYById = new();
        
        private readonly Dictionary<string, int> _typeCounters = new();

        public UnitFactory(
            DiContainer container,
            IUnitClassConfig unitClassConfig,
            IUnitGameplayProfileService unitGameplayProfileService,
            SignalBus signalBus,
            IGridService gridService,
            IObjectsMapService objectsMapService,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IGeneratedTerrainLevelQuery terrainLevelQuery = null,
            [InjectOptional] TileRegistrySO tileRegistry = null,
            [InjectOptional] ITileSettingsService tileSettings = null)
        {
            _container = container;
            _unitClassConfig = unitClassConfig;
            _unitGameplayProfileService = unitGameplayProfileService;
            _signalBus = signalBus;
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _gridProjection = gridProjection;
            _terrainLevelQuery = terrainLevelQuery;
            _tileRegistry = tileRegistry;
            _tileSettings = tileSettings;
        }

        public string CreateUnit(string typeId, Vector2Int gridPosition)
            => CreateUnit(typeId, gridPosition, null);

        public string CreateUnit(string typeId, Vector2Int gridPosition, string ownerId)
        {
            // Generate an ID the normal way, then delegate.
            if (!_typeCounters.ContainsKey(typeId)) _typeCounters[typeId] = 0;
            _typeCounters[typeId]++;

            var config = _unitClassConfig.GetConfig(typeId);
            if (config == null || config.Prefab == null)
            {
                Debug.LogError($"[UnitFactory] Cannot find config or prefab for {typeId}");
                _typeCounters[typeId]--;
                return null;
            }

            if (_objectsMapService.IsOccupied(gridPosition))
            {
                _objectsMapService.TryGetOccupant(gridPosition, out var occupantId);
                Debug.LogWarning($"[UnitFactory] Cannot create unit '{typeId}' at {gridPosition}: tile is already occupied by '{occupantId}'.");
                _typeCounters[typeId]--;
                return null;
            }

            Vector3 worldPos = ResolveWorldPosition(gridPosition);
            GameObject unitObj = _container.InstantiatePrefab(config.Prefab, worldPos, Quaternion.identity, null);
            AlignUnitToTerrainSurface(unitObj, gridPosition);

            string instanceId = unitObj.GetInstanceID().ToString().Replace("-", "");
            string finalUnitId = $"{typeId}_{_typeCounters[typeId]:D2}_{instanceId}";

            return FireUnitCreated(finalUnitId, typeId, gridPosition, unitObj, ownerId);
        }

        public string CreateUnitWithId(string forcedUnitId, string typeId, Vector2Int gridPosition, string ownerId)
        {
            if (string.IsNullOrEmpty(forcedUnitId))
            {
                Debug.LogError("[UnitFactory] CreateUnitWithId called with null/empty forcedUnitId.");
                return null;
            }

            var config = _unitClassConfig.GetConfig(typeId);
            if (config == null || config.Prefab == null)
            {
                Debug.LogError($"[UnitFactory] Cannot find config or prefab for {typeId}");
                return null;
            }

            if (_objectsMapService.IsOccupied(gridPosition))
            {
                _objectsMapService.TryGetOccupant(gridPosition, out var occupantId);
                Debug.LogWarning($"[UnitFactory] CreateUnitWithId: tile {gridPosition} already occupied by '{occupantId}'.");
                return null;
            }

            Vector3 worldPos = ResolveWorldPosition(gridPosition);
            GameObject unitObj = _container.InstantiatePrefab(config.Prefab, worldPos, Quaternion.identity, null);
            AlignUnitToTerrainSurface(unitObj, gridPosition);

            return FireUnitCreated(forcedUnitId, typeId, gridPosition, unitObj, ownerId);
        }

        private string FireUnitCreated(string unitId, string typeId, Vector2Int gridPosition, GameObject unitObj, string ownerId)
        {
            var profile = _unitGameplayProfileService.GetOrDefault(typeId);

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId                   = unitId,
                UnitTypeId               = typeId,
                Position                 = gridPosition,
                VisionRange              = profile.ResolveVisionRange(0),
                HasCustomVisionModifiers = true,
                CanSeeCrest              = profile.CanSeeCrest,
                CrestVisibilityFactor    = profile.CrestVisibilityFactor,
                DownSlopeVisionBonus     = profile.DownSlopeVisionBonus,
                SilhouettePenalty        = profile.SilhouettePenalty,
                UnitObject               = unitObj,
                OwnerId                  = ownerId
            });

            return unitId;
        }

        private Vector3 ResolveWorldPosition(Vector2Int gridPosition)
        {
            if (_gridProjection == null)
                return new Vector3(gridPosition.x, gridPosition.y);

            float elevation = _terrainLevelQuery != null && _terrainLevelQuery.TryGetTerrainLevel(gridPosition, out int level)
                ? level
                : 0f;
            return _gridProjection.GridToWorld(gridPosition, elevation, 0.05f);
        }

        private void AlignUnitToTerrainSurface(GameObject unitObject, Vector2Int gridPosition)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection) || unitObject == null)
                return;

            GridSurfacePlacementUtility.AlignBottomToSurface(unitObject, ResolveTerrainSurfaceY(gridPosition));
        }

        private float ResolveTerrainSurfaceY(Vector2Int gridPosition)
        {
            float elevation = _terrainLevelQuery != null && _terrainLevelQuery.TryGetTerrainLevel(gridPosition, out int level)
                ? level
                : 0f;
            float baseY = _gridProjection.GridToWorld(gridPosition, elevation, 0f).y;

            if (_gridService.TryGetTileData(gridPosition, out string tileId) && TryResolveTileSurfaceOffsetY(tileId, out float offsetY))
                return baseY + offsetY;

            return baseY;
        }

        private bool TryResolveTileSurfaceOffsetY(string tileId, out float offsetY)
        {
            offsetY = 0f;
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            if (_tileSurfaceOffsetYById.TryGetValue(tileId, out offsetY))
                return true;

            // Новий layer-based шлях: зсув поверхні з профілю шару terrain.
            if (_tileSettings != null)
            {
                offsetY = _tileSettings.GetSurfaceOffset(tileId);
                if (offsetY != 0f)
                {
                    _tileSurfaceOffsetYById[tileId] = offsetY;
                    return true;
                }
            }

            if (_tileRegistry?.Definitions == null)
                return false;

            for (int i = 0; i < _tileRegistry.Definitions.Length; i++)
            {
                var definition = _tileRegistry.Definitions[i];
                var surfacePrefab = definition?.SurfaceReferencePrefab;
                if (definition == null || definition.Id != tileId || surfacePrefab == null)
                    continue;

                if (!GridSurfacePlacementUtility.TryResolveTopOffsetY(surfacePrefab, out offsetY))
                    offsetY = 0f;

                _tileSurfaceOffsetYById[tileId] = offsetY;
                return true;
            }

            return false;
        }
    }
}