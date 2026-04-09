using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Формує список елементів меню будівель із реєстру.
    /// Групує за категоріями enum, сортує та дістає іконки зі SpriteRenderer prefab.
    /// </summary>
    public sealed class BuildingMenuFactory
    {
        public List<BuildingListItemData> BuildMenuItems(BuildingDefinition[] allBuildings, IBuildingRegistry buildingRegistry, UnityEngine.Object context)
        {
            Debug.Log($"[BuildMenuItems] START: allBuildings.Length={(allBuildings?.Length ?? 0)}, registry={(buildingRegistry != null ? "ok" : "NULL")}");
            
            var result = new List<BuildingListItemData>();
            var categories = (BuildingCategory[])Enum.GetValues(typeof(BuildingCategory));
            var source = BuildCompleteSource(allBuildings, buildingRegistry);

            Debug.Log($"[Construction UI] BuildMenuItems: загальна кількість BuildingDefinition = {source.Length}", context);

            foreach (var category in categories)
            {
                string className = category.ToString();

                var categoryBuildings = source
                    .Where(x => x != null && x.Category == category)
                    .OrderBy(x => x.DisplayName)
                    .ThenBy(x => x.Id)
                    .ToArray();

                if (categoryBuildings.Length == 0)
                {
                    Debug.Log($"[Construction UI] Клас '{className}' не має будівель у реєстрі.", context);
                    continue;
                }

                Debug.Log($"[Construction UI] Клас '{className}': {categoryBuildings.Length} будівель.", context);

                foreach (var building in categoryBuildings)
                {
                    var sprite = ExtractSpriteForMenu(building, buildingRegistry, context);
                    Debug.Log(
                        $"[Construction UI] → id='{building.Id}' display='{building.DisplayName}' " +
                        $"category={building.Category} prefab={(building.Prefab != null ? building.Prefab.name : "NULL")} " +
                        $"icon={(building.Icon != null ? building.Icon.name : "NULL")} " +
                        $"extractedSprite={(sprite != null ? sprite.name : "NULL")}",
                        context);
                    result.Add(new BuildingListItemData(building.Id, building.DisplayName, building.Category, sprite));
                }
            }

            Debug.Log($"[BuildMenuItems] FINISH: result.Count = {result.Count}");
            return result;
        }

        private static BuildingDefinition[] BuildCompleteSource(BuildingDefinition[] allBuildings, IBuildingRegistry buildingRegistry)
        {
            var byId = new Dictionary<string, BuildingDefinition>(StringComparer.Ordinal);
            var collections = buildingRegistry?.GetWallCollections() ?? Array.Empty<WallCollectionDefinition>();
            var allowedWallIds = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < collections.Length; i++)
            {
                var col = collections[i];
                if (col == null)
                    continue;

                if (!string.IsNullOrWhiteSpace(col.WallBuildingId))
                    allowedWallIds.Add(col.WallBuildingId);
                if (!string.IsNullOrWhiteSpace(col.GateBuildingId))
                    allowedWallIds.Add(col.GateBuildingId);
            }

            var baseSource = allBuildings ?? Array.Empty<BuildingDefinition>();
            Debug.Log($"[BuildCompleteSource] baseSource.Length = {baseSource.Length}");
            for (int i = 0; i < baseSource.Length; i++)
            {
                var def = baseSource[i];
                if (def == null || string.IsNullOrWhiteSpace(def.Id))
                    continue;

                // Якщо є wall-колекції, залишаємо в меню тільки базові wall/gate ID з колекцій.
                // Усі variant ID (horizontal/vertical/corner/топологічні) відсікаємо.
                if (def.Category == BuildingCategory.Walls && allowedWallIds.Count > 0 && !allowedWallIds.Contains(def.Id))
                {
                    Debug.Log($"[BuildCompleteSource] Пропущено variant wall id='{def.Id}'");
                    continue;
                }

                if (!byId.ContainsKey(def.Id))
                {
                    byId.Add(def.Id, def);
                    Debug.Log($"[BuildCompleteSource] Додано з baseSource: id='{def.Id}' category={def.Category}");
                }
            }

            // Додаємо лише базові обєкти стін і воріт з кожної колекції.
            // Варіанти (vertical, horizontal, корнери) вибираються при розміщенні, не в меню.
            Debug.Log($"[BuildCompleteSource] Знайдено {collections.Length} стінових колекцій");
            for (int i = 0; i < collections.Length; i++)
            {
                var col = collections[i];
                if (col == null)
                {
                    Debug.LogWarning($"[BuildCompleteSource] Колекція[{i}] = null");
                    continue;
                }

                Debug.Log($"[BuildCompleteSource] Колекція[{i}]: CollectionId='{col.CollectionId}' WallId='{col.WallBuildingId}' GateId='{col.GateBuildingId}'");
                AddWallIdIfMissing(byId, buildingRegistry, col, col.WallBuildingId);
                AddWallIdIfMissing(byId, buildingRegistry, col, col.GateBuildingId);
            }

            Debug.Log($"[BuildCompleteSource] Фінально в меню буде {byId.Count} обєктів: {string.Join(", ", byId.Keys)}");
            return byId.Values.ToArray();
        }

        private static void AddWallIdIfMissing(
            Dictionary<string, BuildingDefinition> byId,
            IBuildingRegistry buildingRegistry,
            WallCollectionDefinition collection,
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.Log($"[AddWallIdIfMissing] ID пусто для колекції '{collection.CollectionId}'");
                return;
            }

            if (byId.ContainsKey(id))
            {
                Debug.Log($"[AddWallIdIfMissing] ID'{id}' уже є в dicitonary, скачую");
                return;
            }

            var existing = buildingRegistry?.GetById(id);
            if (existing != null)
            {
                byId[id] = existing;
                Debug.Log($"[AddWallIdIfMissing] Знайдено у реєстрі: id='{id}' category={existing.Category}");
                return;
            }

            // Синтетичний fallback для базових ID стін/воріт, що не є у Buildings[].
            // Використовуємо спрайти з колекції (HorizontalPrefab для стін, GatePrefab для воріт).
            var isGate = collection.IsGate(id);
            var prefab = isGate ? collection.GatePrefab : collection.HorizontalPrefab;
            byId[id] = new BuildingDefinition
            {
                Id = id,
                DisplayName = id,
                Category = BuildingCategory.Walls,
                Icon = null,
                Prefab = prefab,
            };
            Debug.Log($"[AddWallIdIfMissing] Створено синтетичний: id='{id}' isGate={isGate} prefab={(prefab != null ? prefab.name : "NULL")}");
        }

        public Sprite ExtractSpriteForMenu(BuildingDefinition building, IBuildingRegistry buildingRegistry, UnityEngine.Object context)
        {
            if (building == null)
                return null;

            if (building.Category == BuildingCategory.Walls)
            {
                var collection = buildingRegistry?.GetWallCollectionByBuildingId(building.Id);
                if (collection != null)
                {
                    if (collection.IsGate(building.Id))
                    {
                        if (building.Icon != null)
                        {
                            Debug.Log($"[Construction UI] Для воріт використано передану Icon з реєстру: id='{building.Id}', icon='{building.Icon.name}'", context);
                            return building.Icon;
                        }

                        var gateSprite = ExtractSpriteFromPrefab(collection.GatePrefab)
                            ?? ExtractSpriteFromPrefab(building.Prefab)
                            ?? ExtractSpriteFromPrefab(collection.HorizontalPrefab);
                        if (gateSprite != null)
                            return gateSprite;
                    }
                    else
                    {
                        var horizontalSprite = ExtractSpriteFromPrefab(collection.HorizontalPrefab);
                        if (horizontalSprite != null)
                        {
                            Debug.Log($"[Construction UI] Для стін використано горизонтальний спрайт колекції: id='{building.Id}', sprite='{horizontalSprite.name}'", context);
                            return horizontalSprite;
                        }
                    }
                }

                if (building.Icon != null)
                    return building.Icon;
            }

            if (building.Prefab == null)
            {
                Debug.LogWarning($"[Construction UI] Для будівлі '{building.Id}' не задано prefab. Використовую поле Icon з реєстру.", context);
                return building.Icon;
            }

            var renderers = building.Prefab.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in renderers)
            {
                if (renderer != null && renderer.sprite != null)
                    return renderer.sprite;
            }

            if (building.Icon != null)
            {
                Debug.LogWarning($"[Construction UI] У prefab '{building.Prefab.name}' не знайдено SpriteRenderer зі спрайтом. Використовую Icon для '{building.Id}'.", context);
                return building.Icon;
            }

            Debug.LogError($"[Construction UI] Не вдалося знайти спрайт для будівлі '{building.Id}'. Меню покаже кнопку без іконки.", context);
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
