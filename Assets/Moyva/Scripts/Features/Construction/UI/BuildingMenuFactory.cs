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
        public List<BuildingListItemData> BuildMenuItems(BuildingDefinition[] allBuildings, UnityEngine.Object context)
        {
            var result = new List<BuildingListItemData>();
            var categories = (BuildingCategory[])Enum.GetValues(typeof(BuildingCategory));
            var source = allBuildings ?? Array.Empty<BuildingDefinition>();

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

                foreach (var building in categoryBuildings)
                {
                    var sprite = ExtractSpriteForMenu(building, context);
                    result.Add(new BuildingListItemData(building.Id, building.DisplayName, building.Category, sprite));
                }
            }

            return result;
        }

        public Sprite ExtractSpriteForMenu(BuildingDefinition building, UnityEngine.Object context)
        {
            if (building == null)
                return null;

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
    }
}
