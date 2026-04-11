using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    /// <summary>
    /// Централізована конфіг для всіх економічних правил.
    /// Зберігає дефолтні значення та метаінформацію про кожен параметр.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/Economy/Rules Configuration", fileName = "EconomyRulesConfiguration")]
    public sealed class EconomyRulesConfiguration : ScriptableObject
    {
        [SerializeField] private List<EconomyRuleParameter> _parameters = new List<EconomyRuleParameter>();

        public IReadOnlyList<EconomyRuleParameter> Parameters => _parameters.AsReadOnly();

        /// <summary>
        /// Отримати параметр за ID.
        /// </summary>
        public EconomyRuleParameter GetParameterById(string id)
        {
            return _parameters.Find(p => p.Id == id);
        }

        /// <summary>
        /// Отримати всі параметри категорії.
        /// </summary>
        public List<EconomyRuleParameter> GetParametersByCategory(EconomyRuleCategory category)
        {
            return _parameters.FindAll(p => p.Category == category);
        }

        /// <summary>
        /// Отримати дозволені для гравця параметри.
        /// </summary>
        public List<EconomyRuleParameter> GetOverridableParameters()
        {
            return _parameters.FindAll(p => p.IsOverridable);
        }

        /// <summary>
        /// Отримати дозволені параметри категорії.
        /// </summary>
        public List<EconomyRuleParameter> GetOverridableParametersByCategory(EconomyRuleCategory category)
        {
            return _parameters.FindAll(p => p.Category == category && p.IsOverridable);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Автоматично викликається Unity при створенні нового SO-ассета.
        /// </summary>
        private void Reset()
        {
            InitializeDefaults();
        }

        /// <summary>
        /// [Editor only] Ініціалізувати дефолтні правила.
        /// Викликається один раз при створенні SO.
        /// </summary>
        public void InitializeDefaults()
        {
            if (_parameters.Count > 0) return;

            _parameters.Clear();

            // Settlement Rules
            _parameters.Add(new EconomyRuleParameter(
                "min-townhall-distance", "Мінімальна відстань між ратушами",
                "Мінімальна відстань в тайлах між центрами двох ратуш.",
                EconomyRuleCategory.SettlementRules, EconomyRuleParameterType.Integer,
                "25", "10", "50", "1", true));

            _parameters.Add(new EconomyRuleParameter(
                "max-settlements", "Максимум поселень",
                "Максимальна кількість активних поселень одночасно.",
                EconomyRuleCategory.SettlementRules, EconomyRuleParameterType.Integer,
                "3", "1", "10", "1", false));

            // Population Rules
            _parameters.Add(new EconomyRuleParameter(
                "new-residents-arrival-turns", "Період прибуття жителів",
                "Кожні N ходів прибувають нові жителі та формуються сім'ї.",
                EconomyRuleCategory.PopulationRules, EconomyRuleParameterType.Integer,
                "10", "5", "30", "1", true));

            // Workforce Rules
            _parameters.Add(new EconomyRuleParameter(
                "worker-reallocation-frequency", "Частота розподілу робітників",
                "Кожні N ходів перераховується розподіл робітників по будівлях.",
                EconomyRuleCategory.WorkforceRules, EconomyRuleParameterType.Integer,
                "1", "1", "5", "1", false));

            // Production Rules
            _parameters.Add(new EconomyRuleParameter(
                "base-production-speed", "Базова швидкість виробництва",
                "Множник для базової швидкості виробництва всіх будівель.",
                EconomyRuleCategory.ProductionRules, EconomyRuleParameterType.Float,
                "1.0", "0.1", "5.0", "0.1", true));

            // Storage Rules
            _parameters.Add(new EconomyRuleParameter(
                "granary-capacity-unlimited", "Амбар без ліміту",
                "Чи мають амбари безлімітну ємність.",
                EconomyRuleCategory.StorageRules, EconomyRuleParameterType.Boolean,
                "true", null, null, null, false));

            // Caravan Rules
            _parameters.Add(new EconomyRuleParameter(
                "max-caravans-per-settlement", "Максимум караванів на поселення",
                "Ліміт кількості караванів, які можна мати одночасно.",
                EconomyRuleCategory.CaravanRules, EconomyRuleParameterType.Integer,
                "1", "1", "3", "1", false));

            _parameters.Add(new EconomyRuleParameter(
                "caravan-max-weight-grams", "Максимальна вага повозки (г)",
                "Максимальна сумарна вага вантажу, який повозка може перевезти.",
                EconomyRuleCategory.CaravanRules, EconomyRuleParameterType.Integer,
                "50000", "1000", "500000", "500", true));

            _parameters.Add(new EconomyRuleParameter(
                "caravan-max-total-size", "Максимальний сумарний розмір повозки",
                "Сумарний розмір вантажу. 1.0 = одна велика одиниця або кілька малих сумарно до 1.0.",
                EconomyRuleCategory.CaravanRules, EconomyRuleParameterType.Float,
                "1.0", "0.1", "10.0", "0.1", true));

            _parameters.Add(new EconomyRuleParameter(
                "caravan-single-full-size-only", "Розмір 1.0 займає повозку повністю",
                "Якщо увімкнено, предмет із розміром 1.0 дозволено перевозити лише в одиничній кількості.",
                EconomyRuleCategory.CaravanRules, EconomyRuleParameterType.Boolean,
                "true", null, null, null, true));

            // Market Rules — ціноутворення
            _parameters.Add(new EconomyRuleParameter(
                "stock-exponent", "Показник запасу",
                "Показник степеня для формули дефіциту в ціноутворенні.",
                EconomyRuleCategory.MarketRules, EconomyRuleParameterType.Float,
                "0.35", "0.1", "1.0", "0.05", true));

            _parameters.Add(new EconomyRuleParameter(
                "volume-exponent", "Показник об'єму",
                "Показник степеня для формули об'єму в ціноутворенні.",
                EconomyRuleCategory.MarketRules, EconomyRuleParameterType.Float,
                "0.20", "0.1", "1.0", "0.05", true));

            _parameters.Add(new EconomyRuleParameter(
                "ui-summary-materials-format", "Формат UI: підсумок матеріалів",
                "Рядок форматування для загального підсумку матеріалів у UI гравця. Використовуйте плейсхолдер {0} для числа.",
                EconomyRuleCategory.MarketRules, EconomyRuleParameterType.String,
                "Матеріали: {0:0.#}", null, null, null, false));

            _parameters.Add(new EconomyRuleParameter(
                "ui-summary-food-format", "Формат UI: підсумок їжі",
                "Рядок форматування для загального підсумку їжі у UI гравця. Використовуйте плейсхолдер {0} для числа.",
                EconomyRuleCategory.MarketRules, EconomyRuleParameterType.String,
                "Їжа: {0:0.#}", null, null, null, false));

            // Consumption/Needs Rules
            _parameters.Add(new EconomyRuleParameter(
                "food-consumption-adult", "Споживання їжі (дорослі)",
                "Одиниці їжі за хід для дорослого жителя.",
                EconomyRuleCategory.ConsumptionNeeds, EconomyRuleParameterType.Float,
                "1.0", "0.1", "5.0", "0.1", true));

            _parameters.Add(new EconomyRuleParameter(
                "food-consumption-child", "Споживання їжі (діти)",
                "Одиниці їжі за хід для дитини (0-15 років).",
                EconomyRuleCategory.ConsumptionNeeds, EconomyRuleParameterType.Float,
                "0.6", "0.1", "3.0", "0.1", true));

            _parameters.Add(new EconomyRuleParameter(
                "food-consumption-elder", "Споживання їжі (пенсіонери)",
                "Одиниці їжі за хід для пенсіонера (60+).",
                EconomyRuleCategory.ConsumptionNeeds, EconomyRuleParameterType.Float,
                "0.8", "0.1", "3.0", "0.1", true));

            // Death/Mortality Rules
            _parameters.Add(new EconomyRuleParameter(
                "hunger-death-modifier", "Модифікатор смертності від голоду",
                "Множник впливу голоду на ймовірність смерті.",
                EconomyRuleCategory.DeathMortality, EconomyRuleParameterType.Float,
                "0.015", "0.001", "0.1", "0.001", true));

            _parameters.Add(new EconomyRuleParameter(
                "cold-death-modifier", "Модифікатор смертності від холоду",
                "Множник впливу холоду на ймовірність смерті.",
                EconomyRuleCategory.DeathMortality, EconomyRuleParameterType.Float,
                "0.010", "0.001", "0.1", "0.001", true));

            // Building Rules
            _parameters.Add(new EconomyRuleParameter(
                "one-recipe-per-building", "Один рецепт на будівлю",
                "Чи обмежена кожна будівля одним активним рецептом.",
                EconomyRuleCategory.BuildingRules, EconomyRuleParameterType.Boolean,
                "true", null, null, null, false));

            // AI Extensibility Rules
            _parameters.Add(new EconomyRuleParameter(
                "ai-trading-enabled", "AI торгівля включена",
                "Чи включена автоматична AI торгівля.",
                EconomyRuleCategory.AIExtensibility, EconomyRuleParameterType.Boolean,
                "false", null, null, null, false));

            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// [Editor only] Додає обов'язкові UI-параметри, якщо їх бракує в існуючому ассеті.
        /// </summary>
        public void EnsureDefaultUiFormattingParameters()
        {
            EnsureParameterExists(new EconomyRuleParameter(
                "ui-summary-materials-format", "Формат UI: підсумок матеріалів",
                "Рядок форматування для загального підсумку матеріалів у UI гравця. Використовуйте плейсхолдер {0} для числа.",
                EconomyRuleCategory.MarketRules, EconomyRuleParameterType.String,
                "Матеріали: {0:0.#}", null, null, null, false));

            EnsureParameterExists(new EconomyRuleParameter(
                "ui-summary-food-format", "Формат UI: підсумок їжі",
                "Рядок форматування для загального підсумку їжі у UI гравця. Використовуйте плейсхолдер {0} для числа.",
                EconomyRuleCategory.MarketRules, EconomyRuleParameterType.String,
                "Їжа: {0:0.#}", null, null, null, false));
        }

        private void EnsureParameterExists(EconomyRuleParameter parameter)
        {
            if (_parameters.Exists(p => p != null && p.Id == parameter.Id))
                return;

            _parameters.Add(parameter);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
