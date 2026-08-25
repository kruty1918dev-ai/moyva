using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.UI
{
	/// <summary>
	/// Формує список елементів меню будівель із реєстру.
	/// Групує за категоріями enum, сортує та дістає іконки з Icon або SpriteRenderer prefab.
	/// </summary>
	    public sealed class BuildingMenuFactory
    {
        private static readonly BuildingCategory[] Categories =
            (BuildingCategory[])Enum.GetValues(
                typeof(BuildingCategory));

        private readonly Dictionary<string, BuildingDefinition>
            _sourceById =
                new Dictionary<string, BuildingDefinition>(
                    StringComparer.Ordinal);
        private readonly HashSet<string> _allowedWallIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<BuildingDefinition> _sourceBuffer =
            new List<BuildingDefinition>();
        private readonly List<BuildingDefinition> _categoryBuffer =
            new List<BuildingDefinition>();
        private readonly Dictionary<int, Sprite> _prefabSpriteCache =
            new Dictionary<int, Sprite>();
        private readonly List<BuildingListItemData> _resultBuffer = new();

		public List<BuildingListItemData> BuildMenuItems(
			BuildingDefinition[] allBuildings,
			IBuildingRegistry buildingRegistry,
			UnityEngine.Object context,
			Func<BuildingDefinition, bool> isInteractableSelector = null,
			Func<BuildingDefinition, bool> includeSelector = null,
			Func<BuildingDefinition, string> unavailableReasonSelector = null)
		{
			_resultBuffer.Clear();
			BuildCompleteSource(allBuildings, buildingRegistry);

			foreach (var category in Categories)
			{
				_categoryBuffer.Clear();
				for (int sourceIndex = 0; sourceIndex < _sourceBuffer.Count; sourceIndex++)
				{
					BuildingDefinition candidate = _sourceBuffer[sourceIndex];
					if (candidate != null
					    && candidate.Category == category
					    && (includeSelector == null || includeSelector(candidate)))
					{
						_categoryBuffer.Add(candidate);
					}
				}

				_categoryBuffer.Sort(CompareBuildings);
				for (int buildingIndex = 0; buildingIndex < _categoryBuffer.Count; buildingIndex++)
				{
					BuildingDefinition building = _categoryBuffer[buildingIndex];
					var sprite = ExtractSpriteForMenu(building, buildingRegistry, context);
					var previewSprite = building.RuntimePreview;
					bool isInteractable = isInteractableSelector == null || isInteractableSelector(building);
					string unavailableReason = isInteractable
						? null
						: unavailableReasonSelector?.Invoke(building);
					_resultBuffer.Add(new BuildingListItemData(
						building.Id,
						GetDisplayName(building),
						building.Category,
						sprite,
						previewSprite,
						isInteractable,
						unavailableReason));
				}
			}

			return new List<BuildingListItemData>(_resultBuffer);
		}

		private static int CompareBuildings(BuildingDefinition left, BuildingDefinition right)
		{
			int byName = string.Compare(GetDisplayName(left), GetDisplayName(right), StringComparison.OrdinalIgnoreCase);
			return byName != 0 ? byName : string.Compare(left?.Id, right?.Id, StringComparison.Ordinal);
		}

		private static string GetDisplayName(BuildingDefinition building)
			=> string.IsNullOrWhiteSpace(building?.DisplayName)
				? "Будівля"
				: building.DisplayName.Trim();

		private void BuildCompleteSource(BuildingDefinition[] allBuildings, IBuildingRegistry buildingRegistry)
		{
			_sourceById.Clear();
			_sourceBuffer.Clear();
			_allowedWallIds.Clear();
			var collections = buildingRegistry?.GetWallCollections() ?? Array.Empty<WallCollectionDefinition>();

			for (int i = 0; i < collections.Length; i++)
			{
				var col = collections[i];
				if (col == null)
					continue;

				if (!string.IsNullOrWhiteSpace(col.WallBuildingId))
					_allowedWallIds.Add(col.WallBuildingId);
				if (!string.IsNullOrWhiteSpace(col.GateBuildingId))
					_allowedWallIds.Add(col.GateBuildingId);
			}

			var baseSource = allBuildings ?? Array.Empty<BuildingDefinition>();
			for (int i = 0; i < baseSource.Length; i++)
			{
				var def = baseSource[i];
				if (def == null || string.IsNullOrWhiteSpace(def.Id))
					continue;

				if (def.Category == BuildingCategory.Walls && _allowedWallIds.Count > 0 && !_allowedWallIds.Contains(def.Id))
					continue;

				if (!_sourceById.ContainsKey(def.Id))
					_sourceById.Add(def.Id, def);
			}

			for (int i = 0; i < collections.Length; i++)
			{
				var col = collections[i];
				if (col == null)
				{
					continue;
				}

				AddWallIdIfMissing(_sourceById, buildingRegistry, col, col.WallBuildingId);
				AddWallIdIfMissing(_sourceById, buildingRegistry, col, col.GateBuildingId);
			}

			foreach (BuildingDefinition definition in _sourceById.Values)
				_sourceBuffer.Add(definition);
		}

		private static void AddWallIdIfMissing(
			Dictionary<string, BuildingDefinition> byId,
			IBuildingRegistry buildingRegistry,
			WallCollectionDefinition collection,
			string id)
		{
			            if (string.IsNullOrWhiteSpace(id))
                return;

            if (byId.ContainsKey(id))
                return;

			var existing = buildingRegistry?.GetById(id);
			if (existing != null)
			{
				                byId[id] = existing;
                return;

			}

			var isGate = collection.IsGate(id);
			var prefab = isGate ? collection.GatePrefab : collection.HorizontalPrefab;
			byId[id] = new BuildingDefinition
			{
				Id = id,
				// Synthetic fallback definitions still use a player-facing label.
				DisplayName = isGate ? "Ворота" : "Стіна",
				Category = BuildingCategory.Walls,
				Icon = null,
				Prefab = prefab,
			            };
        }

		public Sprite ExtractSpriteForMenu(BuildingDefinition building, IBuildingRegistry buildingRegistry, UnityEngine.Object context)
		{
			if (building == null)
				return null;

			// Presentation.Icon comes from building JSON and is the canonical UI icon.
			// Prefab sprites are compatibility fallback only.
			if (building.Icon != null)
				return building.Icon;

			GameObject fallbackPrefab = building.ResolvePreviewPrefab();
			if (building.Category == BuildingCategory.Walls)
			{
				var collection = buildingRegistry?.GetWallCollectionByBuildingId(building.Id);
				if (collection != null)
				{
					if (collection.IsGate(building.Id))
					{
						if (building.Icon != null)
						{
							return building.Icon;
						}

						var gateSprite = ExtractSpriteFromPrefab(collection.GatePrefab)
							?? ExtractSpriteFromPrefab(fallbackPrefab)
							?? ExtractSpriteFromPrefab(collection.HorizontalPrefab);
						if (gateSprite != null)
							return gateSprite;
					}
					else
					{
						var horizontalSprite = ExtractSpriteFromPrefab(collection.HorizontalPrefab);
						if (horizontalSprite != null)
						{
							return horizontalSprite;
						}
					}
				}

				if (building.Icon != null)
					return building.Icon;
			}

			if (fallbackPrefab == null)
			{
				return building.Icon;
			}

			int prefabId = fallbackPrefab.GetInstanceID();
			if (_prefabSpriteCache.TryGetValue(prefabId, out Sprite cachedSprite))
				return cachedSprite != null ? cachedSprite : building.Icon;

			var renderers = fallbackPrefab.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (var renderer in renderers)
			{
				if (renderer != null && renderer.sprite != null)
				{
					_prefabSpriteCache[prefabId] = renderer.sprite;
					return renderer.sprite;
				}
			}
			_prefabSpriteCache[prefabId] = null;

			if (building.Icon != null)
			{
				return building.Icon;
			}

			return null;
		}

		private static Sprite ExtractSpriteFromPrefab(GameObject prefab)
		{
			if (prefab == null)
				return null;

			var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (var renderer in renderers)
			{
				if (renderer != null && renderer.sprite != null)
					return renderer.sprite;
			}

			return null;
		}
	}
}
