using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Вимога до тайла для роботи індустріальної будівлі.
    /// Будівля може потребувати одного або кількох типів тайлів.
    /// </summary>
    [System.Serializable]
    public class TileRequirementDefinition
    {
        [Tooltip("ID тайла, який потребується (наприклад 'forest', 'water', 'mountain').")]
        public string TileId;

        [Tooltip("Радіус, в якому має бути цей тайл від будівлі (в клітинках).")]
        [Min(1)]
        public int Radius = 3;

        [Tooltip("Мінімальна кількість клітинок цього тайла в радіусі.")]
        [Min(1)]
        public int MinimumTileCount = 1;
    }

    [System.Serializable]
    public class BuildingDefinition
    {
        public string Id;               // Унікальний ідентифікатор, наприклад "barracks"
        public string DisplayName;      // Назва для UI, наприклад "Казарма"
        public BuildingCategory Category;
        public Sprite Icon;             // Іконка будівлі для меню будівництва
        public GameObject Prefab;       // Prefab будівлі (stub: null поки арт не готовий)

        [Header("Економіка")]
        [Tooltip("Кількість робітників потрібних для роботи будівлі.\n0 = будівля не потребує робітників (наприклад, стіна).\nПриклад: лісопилка = 2, ферма = 3.")]
        public int RequiredWorkers;

        [Tooltip("Пріоритет розподілу робітників (вищий = важливіший).\nВикористовується системою Worker Allocation.\nПриклад: харчова будівля = 400, матеріали = 200.")]
        public int EconomyPriority;

        [Tooltip("Чи є ця будівля житловою (дає ліміт населення).\nТільки житлові будівлі впливають на Housing.\nПриклад: true для хати/казарми, false для лісопилки.")]
        public bool IsHousing;

        [Tooltip("Чи є ця будівля складом.\nЯкщо true — ресурси можуть відображатися як окремий складський пул для UI/аналітики.")]
        public bool IsWarehouse;

        [Tooltip("Ліміт жителів якщо IsHousing=true.\n0 = не обмежує.\nПриклад: хата = 4, казарма = 10.")]
        public int HousingCapacity;

        [Tooltip("Чи є ця будівля центральною (ратушею) поселення.\nЛише одна центральна будівля на поселення.\nПриклад: true лише для town-hall.")]
        public bool IsTownHall;

        [Tooltip("Чи є ця будівля замком (столичним центром фракції).\nЗамок задається окремо від ратуші та використовується для сценаріїв головного поселення.")]
        public bool IsCastle;

        [Tooltip("Для Industrial-класу: який ресурс виробляє/обробляє ця будівля (resourceId).\nПриклад: wood, stone, grain.\nДля інших класів можна залишити порожнім.")]
        public string IndustrialResourceId;

        [Header("Правила розміщення")]
        [Tooltip("Увімкнути кастомні правила розміщення відносно ратуші для цієї будівлі.\nЯкщо вимкнено — застосовуються стандартні правила за класом будівлі.")]
        public bool UseCustomTownHallRules;

        [Tooltip("Чи потребує будівля наявності ратуші в радіусі дії.\nЗа замовчуванням для не-ратуш це true, для ратуші — false.")]
        public bool RequireTownHallInRange = true;

        [Tooltip("Чи забороняти розміщення, якщо в радіусі вже є інша ратуша.\nЗа замовчуванням true для ратуші та false для інших.")]
        public bool BlockIfTownHallAlreadyInRange;

        [Tooltip("Локальний override радіусу правил ратуші для цієї будівлі.\n<= 0 означає використати глобальний радіус із ConstructionInstaller.")]
        public int TownHallProximityRadiusOverride;

        [Header("Вимоги до тайлів")]
        [Tooltip("Чи потребує ця будівля певних типів тайлів для роботи.\nСтосується переважно індустріальних об'єктів.")]
        public bool RequiresTiles;

        [Tooltip("Массив вимог до тайлів. Кожен елемент задає тип тайла та його мінімальну кількість в радіусі.")]
        public TileRequirementDefinition[] TileRequirements = new TileRequirementDefinition[0];
    }
}
