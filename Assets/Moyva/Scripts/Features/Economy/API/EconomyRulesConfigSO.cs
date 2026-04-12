using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [Serializable]
    public sealed class EconomySettlementRules
    {
        [Tooltip("Максимальна кількість поселень на карті одночасно.\nПриклад: 3 — гравець може мати не більше 3 ратуш.")]
        [SerializeField] private int _maxSettlements = 3;
        [Tooltip("Мінімальна відстань (тайлів) між ратушами різних поселень.\nЗапобігає будівництву нового поселення впритул до існуючого.\nПриклад: 25.")]
        [SerializeField] private int _minTownHallDistance = 25;
        [Tooltip("Чи може поселення існувати без жителів і не деактивуватись.\nfalse = поселення без жителів автоматично знімається з гри.")]
        [SerializeField] private bool _allowSettlementWithoutPopulation;
        [Tooltip("Автоматично деактивувати поселення, якщо населення = 0.\nПрацює лише коли AllowSettlementWithoutPopulation = false.")]
        [SerializeField] private bool _deactivateSettlementWhenPopulationIsZero = true;

        public int MaxSettlements => _maxSettlements;
        public int MinTownHallDistance => _minTownHallDistance;
        public bool AllowSettlementWithoutPopulation => _allowSettlementWithoutPopulation;
        public bool DeactivateSettlementWhenPopulationIsZero => _deactivateSettlementWhenPopulationIsZero;
    }

    [Serializable]
    public sealed class EconomyPopulationRules
    {
        [Tooltip("Кожні N ходів до поселення прибувають нові жителі (якщо є вільне житло).\nПриклад: 10 — приріст кожні 10 ходів.")]
        [SerializeField] private int _newResidentsArrivalIntervalTurns = 10;
        [Tooltip("true = нові родини утворюються лише за наявності вільного місця в житловій будівлі.\nfalse = населення зростає незалежно від наявності будинків.")]
        [SerializeField] private bool _requireHousingForFamilyCreation = true;
        [Tooltip("Враховувати смертність від бойових дій.\nВимкніть для мирних режимів або прототипування.")]
        [SerializeField] private bool _enableWarMortality = true;
        [Tooltip("Враховувати смертність від хвороб.\nВимкніть щоб прибрати цей чинник зі складності.")]
        [SerializeField] private bool _enableDiseaseMortality = true;

        public int NewResidentsArrivalIntervalTurns => _newResidentsArrivalIntervalTurns;
        public bool RequireHousingForFamilyCreation => _requireHousingForFamilyCreation;
        public bool EnableWarMortality => _enableWarMortality;
        public bool EnableDiseaseMortality => _enableDiseaseMortality;
    }

    [Serializable]
    public sealed class EconomyWorkforceRules
    {
        [Tooltip("Пріоритет розподілу робітників у харчових будівлях (ферми, мисливські табори, пекарні).\nЧим вище — тим раніше ці будівлі отримують персонал.\nПриклад: 400.")]
        [SerializeField] private int _foodChainPriority = 400;
        [Tooltip("Пріоритет для будівель тепла та одягу (дровоколи, ткацькі, кравецькі тощо).\nПриклад: 300.")]
        [SerializeField] private int _heatAndClothingPriority = 300;
        [Tooltip("Пріоритет для виробничих будівель матеріалів (лісопилки, каменярні, шахти).\nПриклад: 200.")]
        [SerializeField] private int _materialsPriority = 200;
        [Tooltip("Пріоритет для решти будівель, що не підпадають під інші категорії.\nПриклад: 100.")]
        [SerializeField] private int _otherPriority = 100;

        public int FoodChainPriority => _foodChainPriority;
        public int HeatAndClothingPriority => _heatAndClothingPriority;
        public int MaterialsPriority => _materialsPriority;
        public int OtherPriority => _otherPriority;
    }

    [Serializable]
    public sealed class EconomyProductionRules
    {
        [Tooltip("true = будівля зупиняє виробництво, якщо відсутні вхідні ресурси.\nПриклад: лісопилка без дерева — не виробляє дошки.")]
        [SerializeField] private bool _stopProductionWhenInputMissing = true;
        [Tooltip("true = тайли місцевості (ліс, річка, родючий ґрунт) впливають на ефективність сусідніх будівель.")]
        [SerializeField] private bool _enableTileDependencyModifiers = true;
        [Tooltip("true = їжа повільно псується кожен хід.\nРозмір псування задається FoodDecayPerTurn.")]
        [SerializeField] private bool _enableFoodDecay = true;
        [Tooltip("Частка запасів їжі що псується за хід.\n0.02 = 2% на хід. Використовується лише якщо EnableFoodDecay = true.")]
        [SerializeField] private float _foodDecayPerTurn = 0.02f;

        public bool StopProductionWhenInputMissing => _stopProductionWhenInputMissing;
        public bool EnableTileDependencyModifiers => _enableTileDependencyModifiers;
        public bool EnableFoodDecay => _enableFoodDecay;
        public float FoodDecayPerTurn => _foodDecayPerTurn;
    }

    [Serializable]
    public sealed class EconomyStorageRules
    {
        [Tooltip("true = склади не мають обмеження по кількості ресурсів.\nВикористовується під час розробки; вимкніть для реалістичної гри.")]
        [SerializeField] private bool _unlimitedCapacity = true;
        [Tooltip("true = у поселенні може бути лише один амбар (для зберігання їжі).")]
        [SerializeField] private bool _singleBarnPerSettlement = true;
        [Tooltip("true = у поселенні може бути лише один склад (для матеріалів).")]
        [SerializeField] private bool _singleWarehousePerSettlement = true;
        [Tooltip("true = всі будівлі поселення беруть та здають ресурси в один спільний пул.\nfalse = кожна будівля має власне сховище (складніша логістика).")]
        [SerializeField] private bool _sharedSettlementResourcePool = true;

        public bool UnlimitedCapacity => _unlimitedCapacity;
        public bool SingleBarnPerSettlement => _singleBarnPerSettlement;
        public bool SingleWarehousePerSettlement => _singleWarehousePerSettlement;
        public bool SharedSettlementResourcePool => _sharedSettlementResourcePool;
    }

    [Serializable]
    public sealed class EconomyCaravanRules
    {
        [Tooltip("true = каравани керуються виключно гравцем, без автоматичного AI управління.")]
        [SerializeField] private bool _manualControlOnly = true;
        [Tooltip("Максимальна кількість активних каравань за раз для одного поселення.\nПриклад: 1 — лише один караван виходить одночасно.")]
        [SerializeField] private int _maxCaravansPerSettlement = 1;
        [Tooltip("Максимальна сумарна вага вантажу в грамах.\nПриклад: 50000 = 50 кг.")]
        [SerializeField] private int _maxWeightGrams = 50000;
        [Tooltip("Множник дальності руху каравану поза дорогами.\n0.7 = -30% до базового радіусу. Менше 1.0 — бездоріжжя сповільнює.")]
        [SerializeField] private float _offRoadRangeMultiplier = 0.70f;
        [Tooltip("Множник дальності руху каравану по дорогах.\n1.2 = +20% до базового радіусу. Більше 1.0 — дороги прискорюють.")]
        [SerializeField] private float _roadRangeMultiplier = 1.20f;
        [Tooltip("true = якщо бойове перехоплення каравану не розраховується (AI вимкнений), використовувати миттєву втрату каравану.")]
        [SerializeField] private bool _enableInstantCombatInterceptionFallback = true;

        public bool ManualControlOnly => _manualControlOnly;
        public int MaxCaravansPerSettlement => _maxCaravansPerSettlement;
        public int MaxWeightGrams => _maxWeightGrams;
        public float OffRoadRangeMultiplier => _offRoadRangeMultiplier;
        public float RoadRangeMultiplier => _roadRangeMultiplier;
        public bool EnableInstantCombatInterceptionFallback => _enableInstantCombatInterceptionFallback;
    }

    [Serializable]
    public sealed class EconomyResourceBasePrice
    {
        [Tooltip("ID ресурсу з реєстру (має збігатися з ResourceDefinition.Id).\nПриклад: \"Wood\", \"Grain\", \"Tools\".")]
        [SerializeField] private string _resourceId;
        [Tooltip("Базова ціна одиниці цього ресурсу на ринку (перед динамічними модифікаторами).\nПриклад: Wood=5, Tools=14. Менша ціна — більш доступний ресурс.")]
        [SerializeField] private int _basePrice = 1;

        public EconomyResourceBasePrice()
        {
        }

        public EconomyResourceBasePrice(string resourceId, int basePrice)
        {
            _resourceId = resourceId;
            _basePrice = basePrice;
        }

        public string ResourceId => _resourceId;
        public int BasePrice => _basePrice;
    }

    [Serializable]
    public sealed class EconomyMarketRules
    {
        [Tooltip("Ступінь впливу наявного запасу на ціну (log-крива).\n0.35 — помірна чутливість: дефіцит підіймає ціну, надлишок знижує.\nЗбільшіть для різкіших коливань ціни.")]
        [SerializeField] private float _stockExponent = 0.35f;
        [Tooltip("Ступінь впливу обсягу торгівлі на ціну.\n0.20 — великий обсяг торгів злегка знижує ціну (оптовий ефект).")]
        [SerializeField] private float _volumeExponent = 0.20f;
        [Tooltip("Мінімальний множник ціни відносно базової.\n0.50 = ціна не може бути нижчою ніж 50% від базової, навіть при повних складах.")]
        [SerializeField] private float _minPriceMultiplier = 0.50f;
        [Tooltip("Максимальний множник ціни відносно базової.\n2.50 = при критичному дефіциті ціна може зрости до 250% від базової.")]
        [SerializeField] private float _maxPriceMultiplier = 2.50f;
        [Tooltip("Цільовий нормальний запас ресурсу (при якому ціна = базова).\n200 = коли на складі 200 одиниць — ціна без коригувань.")]
        [SerializeField] private float _targetStock = 200f;
        [Tooltip("Референсний обсяг торгівлі за хід (при якому об'ємний знижувач = нейтральний).\n50 = 50 одиниць за хід вважається нормою.")]
        [SerializeField] private float _referenceTradeVolume = 50f;
        [Tooltip("Базові ціни для кожного ресурсу. ID має збігатися з ResourceDefinition.\nЦі значення є стартовою точкою перед динамічним розрахунком ринкової ціни.")]
        [SerializeField] private List<EconomyResourceBasePrice> _resourceBasePrices = new List<EconomyResourceBasePrice>
        {
            new EconomyResourceBasePrice("Wood", 5),
            new EconomyResourceBasePrice("Stone", 6),
            new EconomyResourceBasePrice("Grain", 5),
            new EconomyResourceBasePrice("Meat", 8),
            new EconomyResourceBasePrice("Berries", 6),
            new EconomyResourceBasePrice("Water", 3),
            new EconomyResourceBasePrice("Coal", 9),
            new EconomyResourceBasePrice("Clothing", 12),
            new EconomyResourceBasePrice("Planks", 8),
            new EconomyResourceBasePrice("Tools", 14),
        };

        public float StockExponent => _stockExponent;
        public float VolumeExponent => _volumeExponent;
        public float MinPriceMultiplier => _minPriceMultiplier;
        public float MaxPriceMultiplier => _maxPriceMultiplier;
        public float TargetStock => _targetStock;
        public float ReferenceTradeVolume => _referenceTradeVolume;
        public IReadOnlyList<EconomyResourceBasePrice> ResourceBasePrices => _resourceBasePrices;
    }

    [Serializable]
    public sealed class EconomyComfortWeights
    {
        [Tooltip("Вага наявності їжі у формулі задоволеності жителів.\n0.35 = їжа найсильніше впливає на комфорт (35%). Сума всіх ваг = 1.0.")]
        [SerializeField] private float _foodWeight = 0.35f;
        [Tooltip("Вага наявності опалення (дрова/вугілля) у формулі комфорту.\n0.25 = холод — другий за важливістю чинник незадоволеності.")]
        [SerializeField] private float _heatWeight = 0.25f;
        [Tooltip("Вага освітлення (свічки, смолоскипи) у формулі комфорту.\n0.10 = відсутність світла помірно знижує настрій.")]
        [SerializeField] private float _lightWeight = 0.10f;
        [Tooltip("Вага наявності одягу у формулі комфорту.\n0.10 = відсутність одягу помірно знижує задоволеність.")]
        [SerializeField] private float _clothingWeight = 0.10f;
        [Tooltip("Вага рівня оподаткування у формулі комфорту.\n0.10 =高 податки знижують настрій. 0 = податки ігноруються.")]
        [SerializeField] private float _taxWeight = 0.10f;
        [Tooltip("Вага правопорядку (присутність варти, карні покарання) у формулі комфорту.\n0.10 = низький правопорядок знижує задоволеність.")]
        [SerializeField] private float _lawWeight = 0.10f;

        public float FoodWeight => _foodWeight;
        public float HeatWeight => _heatWeight;
        public float LightWeight => _lightWeight;
        public float ClothingWeight => _clothingWeight;
        public float TaxWeight => _taxWeight;
        public float LawWeight => _lawWeight;
    }

    [Serializable]
    public sealed class EconomyAgeConsumptionRule
    {
        [Tooltip("Назва вікової групи для зручності в редакторі.\nПриклад: \"Child\", \"Adult\", \"Elder\".")]
        [SerializeField] private string _label;
        [Tooltip("Мінімальний вік жителя (включно) для цієї групи споживання.\nПриклад: 0 — починаючи з новонароджених.")]
        [SerializeField] private int _minAge;
        [Tooltip("Максимальний вік жителя (включно) для цієї групи споживання.\nПриклад: 200 — практично без обмеження зверху.")]
        [SerializeField] private int _maxAge = 200;
        [Tooltip("Кількість їжі що споживає один житель цієї групи за хід.\nПриклад: дорослий = 1.0, дитина = 0.6.")]
        [SerializeField] private float _foodPerTurn = 1f;
        [Tooltip("Кількість води що споживає один житель цієї групи за хід.\nПриклад: дорослий = 1.0, дитина = 0.7.")]
        [SerializeField] private float _waterPerTurn = 1f;
        [Tooltip("Кількість дров що споживає один житель цієї групи за хід.\nПриклад: дорослий = 0.7, дитина = 0.4.")]
        [SerializeField] private float _firewoodPerTurn = 0.5f;
        [Tooltip("Кількість одягу що споживає один житель цієї групи за хід.\nПриклад: дорослий = 0.3, дитина = 0.2.")]
        [SerializeField] private float _clothingPerTurn = 0.25f;

        public EconomyAgeConsumptionRule()
        {
        }

        public EconomyAgeConsumptionRule(string label, int minAge, int maxAge, float foodPerTurn, float waterPerTurn, float firewoodPerTurn, float clothingPerTurn)
        {
            _label = label;
            _minAge = minAge;
            _maxAge = maxAge;
            _foodPerTurn = foodPerTurn;
            _waterPerTurn = waterPerTurn;
            _firewoodPerTurn = firewoodPerTurn;
            _clothingPerTurn = clothingPerTurn;
        }

        public string Label => _label;
        public int MinAge => _minAge;
        public int MaxAge => _maxAge;
        public float FoodPerTurn => _foodPerTurn;
        public float WaterPerTurn => _waterPerTurn;
        public float FirewoodPerTurn => _firewoodPerTurn;
        public float ClothingPerTurn => _clothingPerTurn;
    }

    [Serializable]
    public sealed class EconomyDeficitPenalties
    {
        [Tooltip("Зміна HP жителя за хід при дефіциті їжі або води.\n-2 = без їжі здоров'я падає на 2 одиниці за хід.")]
        [SerializeField] private int _foodOrWaterHpDelta = -2;
        [Tooltip("Зміна рівня комфорту жителя за хід при дефіциті їжі або води.\n-4 = голод та спрага сильно знижують настрій.")]
        [SerializeField] private int _foodOrWaterComfortDelta = -4;
        [Tooltip("Зміна HP жителя за хід при відсутності дров (замерзання).\n-1 = холод повільніше вбиває ніж голод.")]
        [SerializeField] private int _firewoodHpDelta = -1;
        [Tooltip("Зміна рівня комфорту жителя за хід при відсутності опалення.\n-3 = мерзнути некомфортно, але менше ніж голодувати.")]
        [SerializeField] private int _firewoodComfortDelta = -3;
        [Tooltip("Зміна рівня комфорту жителя за хід при відсутності одягу.\n-2 = без одягу помірний дискомфорт.")]
        [SerializeField] private int _clothingComfortDelta = -2;

        public int FoodOrWaterHpDelta => _foodOrWaterHpDelta;
        public int FoodOrWaterComfortDelta => _foodOrWaterComfortDelta;
        public int FirewoodHpDelta => _firewoodHpDelta;
        public int FirewoodComfortDelta => _firewoodComfortDelta;
        public int ClothingComfortDelta => _clothingComfortDelta;
    }

    [Serializable]
    public sealed class EconomyConsumptionRules
    {
        [Tooltip("Ваги різних факторів у формулі задоволеності жителів.\nСума всіх ваг має дорівнювати 1.0 для коректного розрахунку.")]
        [SerializeField] private EconomyComfortWeights _comfortWeights = new EconomyComfortWeights();
        [Tooltip("Правила споживання ресурсів для різних вікових груп.\nКожен запис визначає добові норми їжі, води, дров та одягу для групи.")]
        [SerializeField] private List<EconomyAgeConsumptionRule> _ageConsumption = new List<EconomyAgeConsumptionRule>
        {
            new EconomyAgeConsumptionRule("Child", 0, 15, 0.6f, 0.7f, 0.4f, 0.2f),
            new EconomyAgeConsumptionRule("Adult", 16, 59, 1f, 1f, 0.7f, 0.3f),
            new EconomyAgeConsumptionRule("Elder", 60, 200, 0.8f, 0.9f, 0.8f, 0.35f),
        };
        [Tooltip("Штрафи HP та комфорту жителів при дефіциті основних ресурсів (їжа, вода, дрова, одяг).")]
        [SerializeField] private EconomyDeficitPenalties _deficitPenalties = new EconomyDeficitPenalties();

        public EconomyComfortWeights ComfortWeights => _comfortWeights;
        public IReadOnlyList<EconomyAgeConsumptionRule> AgeConsumption => _ageConsumption;
        public EconomyDeficitPenalties DeficitPenalties => _deficitPenalties;
    }

    [Serializable]
    public sealed class EconomyMortalityAgeTier
    {
        [Tooltip("Мінімальний вік жителя (включно) для цього рівня смертності від старості.\nПриклад: 65.")]
        [SerializeField] private int _minAge;
        [Tooltip("Максимальний вік жителя (включно) для цього рівня.\nПриклад: 74. Останній рівень зазвичай має MaxAge=200.")]
        [SerializeField] private int _maxAge = 200;
        [Tooltip("Базовий шанс смерті від старості за хід (0.0–1.0).\n0.004 = 0.4% шанс за хід для 65–74 р. Модифікується голодом/холодом/хворобою.")]
        [SerializeField] private float _baseDeathChance;

        public EconomyMortalityAgeTier()
        {
        }

        public EconomyMortalityAgeTier(int minAge, int maxAge, float baseDeathChance)
        {
            _minAge = minAge;
            _maxAge = maxAge;
            _baseDeathChance = baseDeathChance;
        }

        public int MinAge => _minAge;
        public int MaxAge => _maxAge;
        public float BaseDeathChance => _baseDeathChance;
    }

    [Serializable]
    public sealed class EconomyMortalityRules
    {
        [Tooltip("Вікові рівні природної смертності. Кожен запис — діапазон віку та базовий шанс смерті від старості за хід.")]
        [SerializeField] private List<EconomyMortalityAgeTier> _ageTiers = new List<EconomyMortalityAgeTier>
        {
            new EconomyMortalityAgeTier(0, 44, 0.0000f),
            new EconomyMortalityAgeTier(45, 54, 0.0005f),
            new EconomyMortalityAgeTier(55, 64, 0.0015f),
            new EconomyMortalityAgeTier(65, 74, 0.0040f),
            new EconomyMortalityAgeTier(75, 200, 0.0100f),
        };
        [Tooltip("Вага голоду у додатковому шансі смерті за хід.\n0.015 = при максимальному голоді +1.5% смерті на хід.")]
        [SerializeField] private float _hungerWeight = 0.015f;
        [Tooltip("Вага замерзання у додатковому шансі смерті за хід.\n0.010 = при відсутності дров +1.0% смерті на хід.")]
        [SerializeField] private float _coldWeight = 0.010f;
        [Tooltip("Вага хвороби у додатковому шансі смерті за хід.\n0.012 = епідемія додає до +1.2% смерті на хід.")]
        [SerializeField] private float _diseaseWeight = 0.012f;
        [Tooltip("Вага бойових дій у додатковому шансі смерті за хід.\n0.020 = активна війна дає найбільший приріст смертності.")]
        [SerializeField] private float _warWeight = 0.020f;
        [Tooltip("Максимальний додатковий шанс смерті від голоду (обмежувач).\n0.15 = голод не може додати більше 15% шансу смерті за хід.")]
        [SerializeField] private float _hungerCap = 0.15f;
        [Tooltip("Максимальний додатковий шанс смерті від холоду (обмежувач).\n0.10 = замерзання обмежено 10%.")]
        [SerializeField] private float _coldCap = 0.10f;
        [Tooltip("Максимальний додатковий шанс смерті від хвороби (обмежувач).\n0.12 = хвороба обмежена 12%.")]
        [SerializeField] private float _diseaseCap = 0.12f;
        [Tooltip("Максимальний додатковий шанс смерті від війни (обмежувач).\n0.20 = бойові дії обмежено 20%.")]
        [SerializeField] private float _warCap = 0.20f;
        [Tooltip("Шанс смерті при повному колапсі поселення (0 населення, 0 ресурсів).\n1.0 = 100% — при колапсі всі залишки вмирають.")]
        [SerializeField] private float _collapseDeathChance = 1f;

        public IReadOnlyList<EconomyMortalityAgeTier> AgeTiers => _ageTiers;
        public float HungerWeight => _hungerWeight;
        public float ColdWeight => _coldWeight;
        public float DiseaseWeight => _diseaseWeight;
        public float WarWeight => _warWeight;
        public float HungerCap => _hungerCap;
        public float ColdCap => _coldCap;
        public float DiseaseCap => _diseaseCap;
        public float WarCap => _warCap;
        public float CollapseDeathChance => _collapseDeathChance;
    }

    [Serializable]
    public sealed class EconomyBuildingRules
    {
        [Tooltip("true = будівлі потребують регулярного ремонту (витрачають матеріали).\nНаразі не реалізовано — запланована функція.")]
        [SerializeField] private bool _enableMaintenance;
        [Tooltip("true = гравець може вручну поставити будівлю на паузу (зупинити виробництво без знесення).")]
        [SerializeField] private bool _allowBuildingPause = true;
        [Tooltip("true = пошкоджена будівля автоматично зупиняє виробництво.\nНаразі не реалізовано — запланована функція.")]
        [SerializeField] private bool _buildingDamageStopsProduction;

        public bool EnableMaintenance => _enableMaintenance;
        public bool AllowBuildingPause => _allowBuildingPause;
        public bool BuildingDamageStopsProduction => _buildingDamageStopsProduction;
    }

    [Serializable]
    public sealed class EconomyAiExtensibilityRules
    {
        [Tooltip("true = AI фракції мають власну автономну економіку (виробляють, споживають, торгують без гравця).\nЗараз вимкнено — увімкнення в наступних версіях.")]
        [SerializeField] private bool _enableAutonomousAiEconomy;
        [Tooltip("true = AI-поселення можуть торгувати з гравцем через ринок або каравани.\nПотребує EnableAutonomousAiEconomy = true для повноцінної роботи.")]
        [SerializeField] private bool _enableAiTradeWithPlayer;
        [Tooltip("true = у коді залишаються хуки (порожні зворотні виклики) для підключення системи ескорту каравану в майбутньому.")]
        [SerializeField] private bool _reserveHooksForCaravanEscort = true;

        public bool EnableAutonomousAiEconomy => _enableAutonomousAiEconomy;
        public bool EnableAiTradeWithPlayer => _enableAiTradeWithPlayer;
        public bool ReserveHooksForCaravanEscort => _reserveHooksForCaravanEscort;
    }

    [CreateAssetMenu(menuName = "Moyva/Economy/Rules Config", fileName = "EconomyRulesConfig")]
    public sealed class EconomyRulesConfigSO : ScriptableObject
    {
        [Tooltip("Правила поселень: макс. кількість, мінімальна відстань між ратушами, поведінка без населення.")]
        [SerializeField] private EconomySettlementRules _settlement = new EconomySettlementRules();
        [Tooltip("Правила населення: швидкість приросту, вимоги до житла, смертність від війни/хвороби.")]
        [SerializeField] private EconomyPopulationRules _population = new EconomyPopulationRules();
        [Tooltip("Правила розподілу робочої сили: пріоритети між харчовими, тепловими, матеріальними та іншими будівлями.")]
        [SerializeField] private EconomyWorkforceRules _workforce = new EconomyWorkforceRules();
        [Tooltip("Правила виробництва: зупинка без сировини, вплив тайлів, псування їжі.")]
        [SerializeField] private EconomyProductionRules _production = new EconomyProductionRules();
        [Tooltip("Правила складів: ліміт місткості, кількість амбарів/складів, спільний пул ресурсів.")]
        [SerializeField] private EconomyStorageRules _storage = new EconomyStorageRules();
        [Tooltip("Правила каравань: ручне/автоматичне керування, кількість, дальність по дорогах і без.")]
        [SerializeField] private EconomyCaravanRules _caravan = new EconomyCaravanRules();
        [Tooltip("Правила ринку: динамічне ціноутворення, базові ціни ресурсів, мінімум/максимум ціни.")]
        [SerializeField] private EconomyMarketRules _market = new EconomyMarketRules();
        [Tooltip("Правила споживання: ваги комфорту, норми їжі/води/дров/одягу за віком, штрафи при дефіциті.")]
        [SerializeField] private EconomyConsumptionRules _consumption = new EconomyConsumptionRules();
        [Tooltip("Правила смертності: природна (вік), від голоду, холоду, хвороби, війни та колапсу поселення.")]
        [SerializeField] private EconomyMortalityRules _mortality = new EconomyMortalityRules();
        [Tooltip("Правила будівель: обслуговування, пауза, зупинка виробництва при пошкодженні.")]
        [SerializeField] private EconomyBuildingRules _building = new EconomyBuildingRules();
        [Tooltip("Правила AI-розширюваності: автономна економіка ворогів, торгівля з гравцем, ескорт каравань.")]
        [SerializeField] private EconomyAiExtensibilityRules _aiExtensibility = new EconomyAiExtensibilityRules();

        public EconomySettlementRules Settlement => _settlement;
        public EconomyPopulationRules Population => _population;
        public EconomyWorkforceRules Workforce => _workforce;
        public EconomyProductionRules Production => _production;
        public EconomyStorageRules Storage => _storage;
        public EconomyCaravanRules Caravan => _caravan;
        public EconomyMarketRules Market => _market;
        public EconomyConsumptionRules Consumption => _consumption;
        public EconomyMortalityRules Mortality => _mortality;
        public EconomyBuildingRules Building => _building;
        public EconomyAiExtensibilityRules AiExtensibility => _aiExtensibility;
    }
}
