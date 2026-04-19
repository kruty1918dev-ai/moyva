using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public enum BuildingModuleScope
    {
        None = 0,
        PerBuilding = 1,
        PerSettlement = 2,
        Global = 3,
    }

    [Serializable]
    public abstract class BuildingModuleDefinition
    {
        [Tooltip("Чи активний цей модуль.\nЯкщо вимкнути прапорець, runtime і валідація ігноруватимуть модуль без його видалення.\nКорисно для тимчасового відключення поведінки під час балансування або тестування.")]
        public bool IsEnabled = true;

        [Tooltip("Область унікальності модуля.\nВизначає, чи можна мати кілька таких модулів на одній будівлі, у поселенні або глобально.\nУ більшості випадків залишайте PerBuilding, щоб не створювати конфліктів у логіці.")]
        public BuildingModuleScope SingletonScope = BuildingModuleScope.PerBuilding;
    }

    [Serializable]
    public sealed class HousingBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Скільки мешканців може розмістити ця будівля.\nПоле формує житловий ліміт поселення, тому без нього будівля не збільшує населення.\nВикористовуйте для хат, будинків, гуртожитків та інших житлових споруд.")]
        public int Capacity;

        [Tooltip("Чи можна використовувати це житло як місце розміщення гарнізону.\nПотрібно для будівель, де мешканці або військові можуть тимчасово ховатися чи оборонятися.\nДля звичайних цивільних будинків зазвичай лишається вимкненим.")]
        public bool IsGarrisonCapable;
    }

    [Serializable]
    public sealed class TownHallBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Радіус дії поселення, яке створює ця ратуша.\nВін використовується для правил розміщення, перевірки зони впливу та прив'язки навколишніх будівель до поселення.\nЗбільшуйте значення, якщо хочете дозволити ширше розростання поселення.")]
        public int BuildRadius;

        [Tooltip("Позначає модуль як центральну споруду поселення.\nЦе потрібно, щоб runtime трактував будівлю як ратушу, а не як звичайну цивільну споруду.\nЗазвичай для TownHallModule це значення має лишатися увімкненим.")]
        public bool IsCentral = true;
    }

    [Serializable]
    public sealed class CastleBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Позначає будівлю як столичний або ключовий центр, відмінний від звичайної ратуші.\nПотрібно для сценаріїв, де замок є окремою центральною точкою фракції.\nУ типовій конфігурації це значення має залишатися увімкненим.")]
        public bool IsCapital = true;

        [Tooltip("Скільки юнітів або захисників може вмістити замок у гарнізоні.\nПоле потрібне для оборонної логіки та майбутніх бойових інтеграцій.\nЯкщо гарнізон не планується, можна лишити 0.")]
        public int GarrisonCapacity;

        [Tooltip("Радіус виключення навколо замку.\nВикористовується, щоб не дозволяти іншим центральним спорудам ставитися надто близько та не змішувати зони контролю.\nЧим більше значення, тим сильніше замок 'резервує' територію.")]
        public int ExclusionRadius;
    }

    [Serializable]
    public sealed class WarehouseBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Список resourceId, які склад може приймати або пріоритезувати.\nПорожній список зазвичай означає, що обмеження за типами ресурсів немає.\nЗаповнюйте поле, якщо хочете зробити спеціалізований склад під конкретні ресурси.")]
        [ResourceIdArray]
        public string[] ResourceIds = Array.Empty<string>();

        [Tooltip("Максимальна місткість складу.\nЗначення -1 означає безлімітне зберігання, що відповідає поточним правилам економіки.\nВикористовуйте обмеження лише якщо пізніше вирішите ввести кап по зберіганню.")]
        public int MaxCapacity = -1;
    }

    [Serializable]
    public sealed class BarnBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Список харчових resourceId, які дозволено зберігати в амбарі.\nПотрібно для розділення харчових і нехарчових ресурсів у системі складів.\nПорожній список зазвичай означає, що амбар приймає всю їжу.")]
        [ResourceIdArray]
        public string[] FoodResourceIds = Array.Empty<string>();
    }

    [Serializable]
    public sealed class ProductionBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("ID ресурсу, який ця будівля виробляє або обробляє.\nЦе ключове поле для економічного ланцюга: без нього runtime не знає, який результат дає будівля.\nЗавжди задавайте валідний resourceId з реєстру ресурсів.")]
        [ResourceId]
        public string ResourceId;

        [Tooltip("Скільки робітників потрібно, щоб будівля почала працювати.\nПоле визначає навантаження на населення під час worker allocation.\nЯкщо значення вище за доступну робочу силу, будівля простоюватиме частково або повністю.")]
        public int WorkersRequired;

        [Tooltip("Пріоритет розподілу робітників для цієї будівлі.\nЧим вище число, тим раніше система намагатиметься виділити сюди людей під час дефіциту робочої сили.\nВикористовуйте це для побудови правильного порядку: їжа, вода, базові матеріали, потім другорядне виробництво.")]
        public int Priority;
    }

    [Serializable]
    public sealed class TileRequirementBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Набір умов по тайлах навколо будівлі.\nМодуль потрібен для споруд, ефективність або сама робота яких залежить від біому чи місцевості.\nНаприклад: ліс для лісоруба, вода для криниці або мосту.")]
        public TileRequirementDefinition[] Requirements = Array.Empty<TileRequirementDefinition>();
    }

    [Serializable]
    public sealed class WorkerlessBuildingModule : BuildingModuleDefinition
    {
    }

    [Serializable]
    public sealed class WallBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Запас міцності стіни.\nПоле потрібне для руйнування, облоги та відображення стану оборонної споруди.\nЧим вище значення, тим довше стіна витримує пошкодження.")]
        public int HitPoints;

        [Tooltip("Чи можна проходити крізь цю споруду.\nДля звичайної стіни значення має бути вимкненим, інакше вона не блокуватиме рух.\nУвімкнення доречне лише для особливих декоративних або технічних сегментів.")]
        public bool IsPassable;
    }

    [Serializable]
    public sealed class GateBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Запас міцності воріт.\nПоле використовується для бойової взаємодії та визначає, скільки ушкоджень витримають ворота до руйнування.\nЗазвичай ворота мають нижчу або співмірну міцність відносно стіни тієї ж технології.")]
        public int HitPoints;

        [Tooltip("Швидкість відкриття воріт.\nВпливає на те, наскільки швидко прохід через ворота стане доступним після команди або тригера.\nВикористовуйте більше значення для легких воріт і менше для важких укріплених конструкцій.")]
        public float OpenSpeed;
    }
}