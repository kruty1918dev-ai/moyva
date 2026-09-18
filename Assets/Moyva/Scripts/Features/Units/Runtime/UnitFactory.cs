using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

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
		private readonly IUnitWorldPositionResolver _worldPositionResolver;

		private readonly Dictionary<string, int> _typeCounters = new();

		public UnitFactory(
			DiContainer container,
			IUnitClassConfig unitClassConfig,
			IUnitGameplayProfileService unitGameplayProfileService,
			SignalBus signalBus,
			IGridService gridService,
			IObjectsMapService objectsMapService,
			[InjectOptional] IUnitWorldPositionResolver worldPositionResolver = null)
		{
			_container = container;
			_unitClassConfig = unitClassConfig;
			_unitGameplayProfileService = unitGameplayProfileService;
			_signalBus = signalBus;
			_gridService = gridService;
			_objectsMapService = objectsMapService;
			_worldPositionResolver = worldPositionResolver;
		}

		public string CreateUnit(string typeId, Vector2Int gridPosition)
			=> CreateUnit(typeId, gridPosition, null);

		public string CreateUnit(string typeId, Vector2Int gridPosition, string ownerId)
		{
			if (!_typeCounters.ContainsKey(typeId))
				_typeCounters[typeId] = 0;
			_typeCounters[typeId]++;

			var config = _unitClassConfig.GetConfig(typeId);
			GameObject prefab = config?.ResolvePrefab();
			if (config == null || prefab == null)
			{
				Debug.LogError($"[UnitFactory] Cannot find config or prefab for {typeId}");
				_typeCounters[typeId]--;
				return null;
			}

			if (_objectsMapService.IsOccupied(gridPosition))
			{
				_objectsMapService.TryGetOccupant(gridPosition, out var occupantId);
				_typeCounters[typeId]--;
				return null;
			}

			Vector3 worldPos = ResolveWorldPosition(gridPosition);
			GameObject unitObj = _container.InstantiatePrefab(prefab, worldPos, Quaternion.identity, null);
			ApplyPresentationTransform(unitObj, config, prefab, worldPos);
			if (_worldPositionResolver != null)
			{
				_worldPositionResolver.AlignBottomToSurface(unitObj, gridPosition);
				ApplyPresentationPosition(unitObj, config, unitObj.transform.position);
			}

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
			GameObject prefab = config?.ResolvePrefab();
			if (config == null || prefab == null)
			{
				Debug.LogError($"[UnitFactory] Cannot find config or prefab for {typeId}");
				return null;
			}

			if (_objectsMapService.IsOccupied(gridPosition))
			{
				_objectsMapService.TryGetOccupant(gridPosition, out var occupantId);
				return null;
			}

			Vector3 worldPos = ResolveWorldPosition(gridPosition);
			GameObject unitObj = _container.InstantiatePrefab(prefab, worldPos, Quaternion.identity, null);
			ApplyPresentationTransform(unitObj, config, prefab, worldPos);
			if (_worldPositionResolver != null)
			{
				_worldPositionResolver.AlignBottomToSurface(unitObj, gridPosition);
				ApplyPresentationPosition(unitObj, config, unitObj.transform.position);
			}

			return FireUnitCreated(forcedUnitId, typeId, gridPosition, unitObj, ownerId);
		}

		private string FireUnitCreated(string unitId, string typeId, Vector2Int gridPosition, GameObject unitObj, string ownerId)
		{
			var profile = _unitGameplayProfileService.GetOrDefault(typeId);

			_signalBus.Fire(new UnitCreatedSignal
			{
				UnitId = unitId,
				UnitTypeId = typeId,
				Position = gridPosition,
				VisionRange = profile.ResolveVisionRange(0),
				HasCustomVisionModifiers = true,
				CanSeeCrest = profile.CanSeeCrest,
				CrestVisibilityFactor = profile.CrestVisibilityFactor,
				DownSlopeVisionBonus = profile.DownSlopeVisionBonus,
				SilhouettePenalty = profile.SilhouettePenalty,
				UnitObject = unitObj,
				OwnerId = ownerId
			});

			return unitId;
		}

		private Vector3 ResolveWorldPosition(Vector2Int gridPosition)
			=> _worldPositionResolver != null
				? _worldPositionResolver.ResolveWorldPosition(gridPosition)
				: new Vector3(gridPosition.x, gridPosition.y);

		private static void ApplyPresentationTransform(
			GameObject unitObj,
			UnitClassConfig config,
			GameObject prefab,
			Vector3 worldPos)
		{
			EntityPresentationApplier.ApplyTransform(
				unitObj,
				config?.ResolvePresentation(),
				worldPos,
				Quaternion.identity,
				prefab != null ? prefab.transform.localScale : Vector3.one);
			EntityPresentationApplier.ApplyStyleAndShadows(
				unitObj,
				config?.ResolvePresentation());
		}

		private static void ApplyPresentationPosition(
			GameObject unitObj,
			UnitClassConfig config,
			Vector3 alignedPosition)
		{
			EntityPresentationApplier.ApplyPosition(
				unitObj,
				config?.ResolvePresentation(),
				alignedPosition,
				Quaternion.identity);
			EntityPresentationApplier.ApplyStyleAndShadows(
				unitObj,
				config?.ResolvePresentation());
		}
	}
}
