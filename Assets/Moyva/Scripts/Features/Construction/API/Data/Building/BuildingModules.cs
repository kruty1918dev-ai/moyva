using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class BuildingResourceAmount
    {
        [ResourceId]
        [LabelText("Ресурс")]
        [Tooltip("Що робить: Вказує resource ID для входу або виходу рецепта.\nВплив у грі: За цим ID економіка знаходить потрібний тип ресурсу.")]
        public string ResourceId;

        [Min(1)]
        [LabelText("Кількість")]
        [Tooltip("Що робить: Задає кількість ресурсу в рецепті.\nВплив у грі: Визначає обсяг споживання або виробництва за цикл.")]
        public int Amount = 1;
    }

    public enum BuildingStorageKind
    {
        [InspectorName("Будь-які ресурси")]
        Any = 0,
        [InspectorName("Матеріали")]
        Material = 1,
        [InspectorName("Їжа")]
        Food = 2,
        [InspectorName("Військові запаси")]
        Military = 3,
        [InspectorName("Власний набір")]
        Custom = 4,
    }

    public enum BuildingModuleScope
    {
        [InspectorName("Без обмеження")]
        None = 0,
        [InspectorName("Один на будівлю")]
        PerBuilding = 1,
        [InspectorName("Один на поселення")]
        PerSettlement = 2,
        [InspectorName("Один глобально")]
        Global = 3,
    }

    [Serializable]
    public abstract class BuildingModuleDefinition
    {
        [Tooltip("Чи активний цей модуль.\nЯкщо вимкнути прапорець, runtime і валідація ігноруватимуть модуль без його видалення.\nКорисно для тимчасового відключення поведінки під час балансування або тестування.")]
        [LabelText("Активний")]
        public bool IsEnabled = true;

        [Tooltip("Область унікальності модуля.\nВизначає, чи можна мати кілька таких модулів на одній будівлі, у поселенні або глобально.\nУ більшості випадків залишайте PerBuilding, щоб не створювати конфліктів у логіці.")]
        [LabelText("Область унікальності")]
        public BuildingModuleScope SingletonScope = BuildingModuleScope.PerBuilding;
    }

    [Serializable]
    public sealed class HousingBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Скільки мешканців може розмістити ця будівля.\nПоле формує житловий ліміт поселення, тому без нього будівля не збільшує населення.\nВикористовуйте для хат, будинків, гуртожитків та інших житлових споруд.")]
        [LabelText("Місткість населення")]
        public int Capacity;

        [Tooltip("Чи можна використовувати це житло як місце розміщення гарнізону.\nПотрібно для будівель, де мешканці або військові можуть тимчасово ховатися чи оборонятися.\nДля звичайних цивільних будинків зазвичай лишається вимкненим.")]
        [LabelText("Підтримує гарнізон")]
        public bool IsGarrisonCapable;
    }

    [Serializable]
    public sealed class TownHallBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Радіус дії поселення, яке створює ця ратуша.\nВін використовується для правил розміщення, перевірки зони впливу та прив'язки навколишніх будівель до поселення.\nЗбільшуйте значення, якщо хочете дозволити ширше розростання поселення.")]
        [LabelText("Радіус будівництва")]
        public int BuildRadius;

        [Tooltip("Позначає модуль як центральну споруду поселення.\nЦе потрібно, щоб runtime трактував будівлю як ратушу, а не як звичайну цивільну споруду.\nЗазвичай для TownHallModule це значення має лишатися увімкненим.")]
        [LabelText("Центральна споруда")]
        public bool IsCentral = true;
    }

    [Serializable]
    public sealed class CastleBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Позначає будівлю як столичний або ключовий центр, відмінний від звичайної ратуші.\nПотрібно для сценаріїв, де замок є окремою центральною точкою фракції.\nУ типовій конфігурації це значення має залишатися увімкненим.")]
        [LabelText("Столиця")]
        public bool IsCapital = true;

        [Tooltip("Скільки юнітів або захисників може вмістити замок у гарнізоні.\nПоле потрібне для оборонної логіки та майбутніх бойових інтеграцій.\nЯкщо гарнізон не планується, можна лишити 0.")]
        [LabelText("Місткість гарнізону")]
        public int GarrisonCapacity;

        [Tooltip("Радіус виключення навколо замку.\nВикористовується, щоб не дозволяти іншим центральним спорудам ставитися надто близько та не змішувати зони контролю.\nЧим більше значення, тим сильніше замок 'резервує' територію.")]
        [LabelText("Радіус виключення")]
        public int ExclusionRadius;
    }

    [Serializable]
    public sealed class WarehouseBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Список resourceId, які склад може приймати або пріоритезувати.\nПорожній список зазвичай означає, що обмеження за типами ресурсів немає.\nЗаповнюйте поле, якщо хочете зробити спеціалізований склад під конкретні ресурси.")]
        [ResourceIdArray]
        [LabelText("Дозволені ресурси")]
        public string[] ResourceIds = Array.Empty<string>();

        [Tooltip("Максимальна місткість складу.\nЗначення -1 означає безлімітне зберігання, що відповідає поточним правилам економіки.\nВикористовуйте обмеження лише якщо пізніше вирішите ввести кап по зберіганню.")]
        [LabelText("Максимальна місткість")]
        public int MaxCapacity = -1;
    }

    [Serializable]
    public sealed class BarnBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Список харчових resourceId, які дозволено зберігати в амбарі.\nПотрібно для розділення харчових і нехарчових ресурсів у системі складів.\nПорожній список зазвичай означає, що амбар приймає всю їжу.")]
        [ResourceIdArray]
        [LabelText("Харчові ресурси")]
        public string[] FoodResourceIds = Array.Empty<string>();
    }

    [Serializable]
    public sealed class ProductionBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("ID ресурсу, який ця будівля виробляє або обробляє.\nЦе ключове поле для економічного ланцюга: без нього runtime не знає, який результат дає будівля.\nЗавжди задавайте валідний resourceId з реєстру ресурсів.")]
        [ResourceId]
        [LabelText("Основний ресурс")]
        public string ResourceId;

        [Tooltip("Скільки робітників потрібно, щоб будівля почала працювати.\nПоле визначає навантаження на населення під час worker allocation.\nЯкщо значення вище за доступну робочу силу, будівля простоюватиме частково або повністю.")]
        [LabelText("Потрібно робітників")]
        public int WorkersRequired;

        [Tooltip("Пріоритет розподілу робітників для цієї будівлі.\nЧим вище число, тим раніше система намагатиметься виділити сюди людей під час дефіциту робочої сили.\nВикористовуйте це для побудови правильного порядку: їжа, вода, базові матеріали, потім другорядне виробництво.")]
        [LabelText("Пріоритет")]
        public int Priority;

        [Tooltip("Новий data-driven production pipeline. Якщо список порожній, runtime тимчасово використовує legacy ResourceId.")]
        [LabelText("Рецепти")]
        public List<ProductionRecipeDefinition> Recipes = new List<ProductionRecipeDefinition>();
    }

    [Serializable]
    public sealed class ProductionRecipeDefinition
    {
        [LabelText("ID рецепта")]
        [Tooltip("Що робить: Задає стабільний ідентифікатор рецепта.\nВплив у грі: Використовується для збереження стану та діагностики виробництва.")]
        public string RecipeId = "recipe";

        [LabelText("Вхідні ресурси")]
        [Tooltip("Що робить: Перелічує ресурси, які споживає виробничий цикл.\nВплив у грі: Без потрібних ресурсів рецепт не запускається.")]
        public List<BuildingResourceAmount> Inputs = new List<BuildingResourceAmount>();
        [LabelText("Вихідні ресурси")]
        [Tooltip("Що робить: Перелічує ресурси, які створює виробничий цикл.\nВплив у грі: Результат додається до відповідного сховища або пулу.")]
        public List<BuildingResourceAmount> Outputs = new List<BuildingResourceAmount>();

        [Min(1)]
        [LabelText("Ходів на цикл")]
        [Tooltip("Що робить: Задає тривалість одного виробничого циклу.\nВплив у грі: Більше значення зменшує частоту отримання результату.")]
        public int TurnsPerCycle = 1;

        [LabelText("Потребує робітників")]
        [Tooltip("Що робить: Вимагає призначену робочу силу для рецепта.\nВплив у грі: Без працівників виробництво простоює.")]
        public bool RequiresWorkers = true;
        [LabelText("Потребує місця в сховищі")]
        [Tooltip("Що робить: Перевіряє вільну місткість перед завершенням циклу.\nВплив у грі: Переповнене сховище зупиняє виробництво.")]
        public bool RequiresStorageSpace = true;
    }

    [Serializable]
    public sealed class WorkforceBuildingModule : BuildingModuleDefinition
    {
        [Min(0)]
        [LabelText("Потрібно робітників")]
        [Tooltip("Що робить: Задає кількість працівників для функціонування будівлі.\nВплив у грі: Недостатня робоча сила знижує або зупиняє роботу.")]
        public int WorkersRequired;

        [LabelText("Пріоритет")]
        [Tooltip("Що робить: Задає порядок розподілу доступних робітників.\nВплив у грі: Більше значення отримує працівників раніше.")]
        public int Priority;
        [LabelText("Тип робітника")]
        [Tooltip("Що робить: Обмежує призначення конкретним типом працівника.\nВплив у грі: Порожнє значення дозволяє стандартну робочу силу.")]
        public string WorkerTypeId;
    }

    [Serializable]
    public sealed class StorageBuildingModule : BuildingModuleDefinition
    {
        [LabelText("Тип сховища")]
        [Tooltip("Що робить: Визначає категорію ресурсів, яку приймає сховище.\nВплив у грі: Невідповідні ресурси не будуть складатися в цій будівлі.")]
        public BuildingStorageKind StorageKind = BuildingStorageKind.Any;

        [Min(-1)]
        [LabelText("Місткість")]
        [Tooltip("Що робить: Задає максимальний запас у сховищі; -1 означає безліміт.\nВплив у грі: Після заповнення нові ресурси не приймаються.")]
        public int Capacity = -1;

        [ResourceIdArray]
        [LabelText("Дозволені ресурси")]
        [Tooltip("Що робить: Перелічує конкретні resource ID для цього сховища.\nВплив у грі: Порожній список використовує правило вибраного типу сховища.")]
        public string[] AcceptedResourceIds = Array.Empty<string>();
    }

    [Serializable]
    public sealed class DefenseBuildingModule : BuildingModuleDefinition
    {
        [Min(0)]
        [LabelText("Броня")]
        [Tooltip("Що робить: Додає оборонний показник будівлі.\nВплив у грі: Зменшує отриману шкоду за правилами бойової системи.")]
        public int Armor;

        [Min(0)]
        [LabelText("Місткість гарнізону")]
        [Tooltip("Що робить: Задає кількість захисників усередині споруди.\nВплив у грі: Обмежує число юнітів, що можуть отримувати захист будівлі.")]
        public int GarrisonCapacity;

        [Min(0)]
        [LabelText("Дальність атаки")]
        [Tooltip("Що робить: Задає радіус ураження оборонної споруди.\nВплив у грі: Цілі поза цим радіусом не можуть бути атаковані.")]
        public int AttackRange;

        [Min(0)]
        [LabelText("Шкода атаки")]
        [Tooltip("Що робить: Задає базову шкоду одного удару.\nВплив у грі: Більше значення швидше знищує ворожі цілі.")]
        public int AttackDamage;

        [Min(0)]
        [LabelText("Бонус огляду")]
        [Tooltip("Що робить: Розширює огляд оборонної споруди.\nВплив у грі: Допомагає раніше помічати цілі та відкривати туман.")]
        public int VisionRevealBonus;
    }

    [Serializable]
    public sealed class FogRevealBuildingModule : BuildingModuleDefinition
    {
        [Min(0)]
        [LabelText("Радіус відкриття")]
        [Tooltip("Що робить: Задає радіус відкриття туману війни.\nВплив у грі: Більше значення відкриває ширшу область навколо споруди.")]
        public int RevealRadius = 3;

        [LabelText("Форма області")]
        [Tooltip("Що робить: Визначає геометрію області видимості.\nВплив у грі: Змінює набір клітинок, що стають видимими.")]
        public FogRevealShape Shape = FogRevealShape.PixelCircle;
        [LabelText("Відкрити після будівництва")]
        [Tooltip("Що робить: Одноразово відкриває область, коли споруду поставлено.\nВплив у грі: Гравець одразу бачить територію навколо нової будівлі.")]
        public bool RevealOnBuilt = true;
        [LabelText("Відкривати поки активна")]
        [Tooltip("Що робить: Підтримує постійну видимість навколо активної споруди.\nВплив у грі: Після деактивації область може знову покритися туманом.")]
        public bool RevealWhileActive = true;
        [LabelText("Лише після завершення")]
        [Tooltip("Що робить: Відкладає ефект до повного завершення будівництва.\nВплив у грі: Незавершений об'єкт не дає розвідданих.")]
        public bool OnlyAfterConstructionComplete = true;
    }

    [Serializable]
    public sealed class SettlementCenterBuildingModule : BuildingModuleDefinition
    {
        [Min(0)]
        [LabelText("Радіус впливу")]
        [Tooltip("Що робить: Задає область контролю центру поселення.\nВплив у грі: У цій зоні дозволяється розміщення залежних споруд.")]
        public int InfluenceRadius = 4;

        [Min(0)]
        [LabelText("Відстань до інших центрів")]
        [Tooltip("Що робить: Визначає мінімальний проміжок між центрами поселень.\nВплив у грі: Запобігає надмірному накладанню зон контролю.")]
        public int MinimumDistanceFromOtherCenters;
    }

    [Serializable]
    public sealed class TileRequirementBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Набір умов по тайлах навколо будівлі.\nМодуль потрібен для споруд, ефективність або сама робота яких залежить від біому чи місцевості.\nНаприклад: ліс для лісоруба, вода для криниці або мосту.")]
        [LabelText("Вимоги")]
        public TileRequirementDefinition[] Requirements = Array.Empty<TileRequirementDefinition>();
    }

    [Serializable]
    public sealed class BuildingPerPlayerLimitModule : BuildingModuleDefinition
    {
        [Tooltip("Максимальна кількість будівель цього типу для одного гравця (власника).\n0 означає, що ліміт вимкнений. Значення 1 робить споруду унікальною для кожного гравця.\nЛіміт перевіряється під час прев'ю, підтвердження та авторитативного розміщення в мультиплеєрі.")]
        [Min(0)]
        [LabelText("Максимум на гравця")]
        public int MaxBuildingsPerPlayer;
    }

    [Serializable]
    public sealed class WorkerlessBuildingModule : BuildingModuleDefinition
    {
    }

    [Serializable]
    public sealed class WallBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Запас міцності стіни.\nПоле потрібне для руйнування, облоги та відображення стану оборонної споруди.\nЧим вище значення, тим довше стіна витримує пошкодження.")]
        [LabelText("Міцність")]
        public int HitPoints;

        [Tooltip("Чи можна проходити крізь цю споруду.\nДля звичайної стіни значення має бути вимкненим, інакше вона не блокуватиме рух.\nУвімкнення доречне лише для особливих декоративних або технічних сегментів.")]
        [LabelText("Прохідна")]
        public bool IsPassable;
    }

    [Serializable]
    public sealed class GateBuildingModule : BuildingModuleDefinition
    {
        [Tooltip("Запас міцності воріт.\nПоле використовується для бойової взаємодії та визначає, скільки ушкоджень витримають ворота до руйнування.\nЗазвичай ворота мають нижчу або співмірну міцність відносно стіни тієї ж технології.")]
        [LabelText("Міцність")]
        public int HitPoints;

        [Tooltip("Швидкість відкриття воріт.\nВпливає на те, наскільки швидко прохід через ворота стане доступним після команди або тригера.\nВикористовуйте більше значення для легких воріт і менше для важких укріплених конструкцій.")]
        [LabelText("Швидкість відкриття")]
        public float OpenSpeed;
    }
}
