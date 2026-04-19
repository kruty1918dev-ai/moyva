using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [Serializable]
    public sealed class EconomyResourceThreshold
    {
        [Tooltip("ID ресурсу (має збігатися з ResourceDefinition.Id).\nПриклад: \"Grain\", \"Wood\", \"Tools\".")]
        [SerializeField] private string _resourceId;
        [Tooltip("Поріг нестачі: якщо запас нижчий за це значення — AI вважає ресурс дефіцитним і не продає його.\nПриклад: 30 — менше 30 одиниць зерна → AI не торгує зерном.")]
        [SerializeField] private int _shortageThreshold;
        [Tooltip("Поріг надлишку: якщо запас перевищує це значення — AI вважає ресурс надлишковим і пропонує до продажу.\nПриклад: 200 — більше 200 одиниць дерева → AI пропонує торгувати.")]
        [SerializeField] private int _excessThreshold;

        public string ResourceId => _resourceId;
        public int ShortageThreshold => _shortageThreshold;
        public int ExcessThreshold => _excessThreshold;
    }

    [CreateAssetMenu(menuName = "Moyva/Economy/AI Rule Profile", fileName = "EconomyAiRuleProfile")]
    public sealed class EconomyAiRuleProfile : ScriptableObject
    {
        [Tooltip("Унікальний ідентифікатор профілю AI-правил.\nВикористовується для прив'язки профілю до фракції або поселення.\nПриклад: \"ai-aggressive\", \"ai-passive-merchant\".")]
        [SerializeField] private string _profileId;
        [Tooltip("Список порогів нестачі/надлишку для кожного ресурсу.\nAI приймає торговельні рішення виходячи з цих значень.")]
        [SerializeField] private List<EconomyResourceThreshold> _resourceThresholds = new List<EconomyResourceThreshold>();
        [Tooltip("true = AI витрачає ресурси обережно: зберігає більший запас перед торгівлею.\nfalse = AI агресивно торгує навіть при середніх запасах.")]
        [SerializeField] private bool _useConservativeSpending;
        [Tooltip("true = AI в першу чергу забезпечує запас їжі, навіть якщо це невигідно торговельно.\nРекомендовано залишити true для реалістичної поведінки ворога.")]
        [SerializeField] private bool _prioritizeFoodSecurity = true;

        public string ProfileId => _profileId;
        public IReadOnlyList<EconomyResourceThreshold> ResourceThresholds => _resourceThresholds;
        public bool UseConservativeSpending => _useConservativeSpending;
        public bool PrioritizeFoodSecurity => _prioritizeFoodSecurity;
    }
}
