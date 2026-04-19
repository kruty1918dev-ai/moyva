using System;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    /// <summary>
    /// Метаінформація про один параметр економічних правил.
    /// Визначає, як параметр відображається в UI та чи може гравець його змінити.
    /// </summary>
    [Serializable]
    public class EconomyRuleParameter
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;
        [SerializeField] private EconomyRuleCategory _category;
        [SerializeField] private EconomyRuleParameterType _parameterType;
        
        // Значення за замовчуванням
        [SerializeField] private string _defaultValue;
        
        // Діапазон валідації
        [SerializeField] private string _minValue;
        [SerializeField] private string _maxValue;
        [SerializeField] private string _step;
        
        // Мета про перевизначення
        [SerializeField] private bool _isOverridable = true;
        
        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public EconomyRuleCategory Category => _category;
        public EconomyRuleParameterType ParameterType => _parameterType;
        public string DefaultValue => _defaultValue;
        public string MinValue => _minValue;
        public string MaxValue => _maxValue;
        public string Step => _step;
        public bool IsOverridable => _isOverridable;

        public EconomyRuleParameter(
            string id,
            string displayName,
            string description,
            EconomyRuleCategory category,
            EconomyRuleParameterType parameterType,
            string defaultValue,
            string minValue = null,
            string maxValue = null,
            string step = null,
            bool isOverridable = true)
        {
            _id = id;
            _displayName = displayName;
            _description = description;
            _category = category;
            _parameterType = parameterType;
            _defaultValue = defaultValue;
            _minValue = minValue;
            _maxValue = maxValue;
            _step = step;
            _isOverridable = isOverridable;
        }
    }

    /// <summary>
    /// Категорія групи параметрів.
    /// </summary>
    public enum EconomyRuleCategory
    {
        SettlementRules = 0,
        PopulationRules = 1,
        WorkforceRules = 2,
        ProductionRules = 3,
        StorageRules = 4,
        CaravanRules = 5,
        MarketRules = 6,
        ConsumptionNeeds = 7,
        DeathMortality = 8,
        BuildingRules = 9,
        AIExtensibility = 10
    }

    /// <summary>
    /// Тип параметра для UI відображення та парсингу.
    /// </summary>
    public enum EconomyRuleParameterType
    {
        Integer = 0,
        Float = 1,
        Boolean = 2,
        String = 3
    }
}
