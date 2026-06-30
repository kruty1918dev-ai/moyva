using UnityEngine;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Вимога до тайла для роботи індустріальної будівлі.
    /// Будівля може потребувати одного або кількох типів тайлів.
    /// </summary>
    [System.Serializable]
    public class TileRequirementDefinition
    {
        [Tooltip("ID потрібного тайла або біому.\nСаме це поле каже системі, який тип місцевості шукати навколо будівлі.\nПриклади: forest-dense для лісу, water для води, grass для відкритої рівнини.")]
        [TileId]
        public string TileId;

        [Tooltip("Радіус пошуку тайла в клітинках.\nВизначає, наскільки далеко від будівлі дозволено шукати потрібну місцевість.\nЗбільшуйте значення для споруд, що використовують широку околицю, і зменшуйте для локальних залежностей.")]
        [Min(1)]
        public int Radius = 3;

        [Tooltip("Мінімальна кількість клітинок цього типу, яку треба знайти в заданому радіусі.\nПоле задає поріг достатності місцевості для роботи будівлі.\nНаприклад, мисливська хатина може вимагати кілька лісових клітин, а місту вистачає однієї водної.")]
        [Min(1)]
        public int MinimumTileCount = 1;
    }

    [System.Serializable]
    public class BuildingDefinition
    {
        [System.Serializable]
        public sealed class BuildingConstructionCostEntry
        {
            [Tooltip("ID ресурсу з EconomyDatabaseSO, який потрібен для будівництва.\nВибирається зі списку ресурсів через dropdown.")]
            [ResourceId]
            public string ResourceId;

            [Tooltip("Кількість ресурсу, яка витрачається на будівництво 1 одиниці цієї споруди.")]
            [Min(1)]
            public int Amount = 1;
        }

        public string Id;               // Унікальний ідентифікатор, наприклад "barracks"
        public string DisplayName;      // Назва для UI, наприклад "Казарма"
        public BuildingCategory Category;
        public Sprite Icon;             // Іконка будівлі для меню будівництва
        [Tooltip("Runtime-safe preview префаба будівлі. Генерується редактором і використовується toolbar UI у білді.")]
        public Sprite RuntimePreview;
        public GameObject Prefab;       // Prefab будівлі (stub: null поки арт не готовий)

        [Header("Вартість будівництва")]
        [Tooltip("Список ресурсів і кількостей, необхідних для побудови 1 екземпляра цієї будівлі.\nПорожній список означає безкоштовне будівництво.")]
        public List<BuildingConstructionCostEntry> ConstructionCost = new List<BuildingConstructionCostEntry>();

        [Header("Модулі")]
        [Tooltip("Компонентна модель будівлі. Runtime використовує модулі для визначення можливостей.")]
        [SerializeReference]
        public List<BuildingModuleDefinition> Modules = new List<BuildingModuleDefinition>();

        [Header("Здоров'я")]
        [Tooltip("Максимальне HP будівлі. Використовується для ініціалізації компонента IHealth під час spawn.")]
        [Min(1)]
        public int MaxHp = 100;

        [Header("Правила розміщення")]
        [Tooltip("Дозволяє розміщення, навіть якщо клітинка ще не Visible у Fog of War.")]
        public bool CanPlaceInFog;

        [Tooltip("Увімкнути кастомні правила розміщення відносно ратуші для цієї будівлі.\nЯкщо вимкнено — застосовуються стандартні правила за класом будівлі.")]
        public bool UseCustomTownHallRules;

        [Tooltip("Чи потребує будівля наявності ратуші в радіусі дії.\nЗа замовчуванням для не-ратуш це true, для ратуші — false.")]
        public bool RequireTownHallInRange = true;

        [Tooltip("Чи забороняти розміщення, якщо в радіусі вже є інша ратуша.\nЗа замовчуванням true для ратуші та false для інших.")]
        public bool BlockIfTownHallAlreadyInRange;

        [Tooltip("Локальний override радіусу правил ратуші для цієї будівлі.\n<= 0 означає використати глобальний радіус із ConstructionInstaller.")]
        public int TownHallProximityRadiusOverride;
    }
}
