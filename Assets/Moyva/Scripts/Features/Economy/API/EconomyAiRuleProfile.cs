using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [Serializable]
    public sealed class EconomyResourceThreshold
    {
        [SerializeField] private string _resourceId;
        [SerializeField] private int _shortageThreshold;
        [SerializeField] private int _excessThreshold;

        public string ResourceId => _resourceId;
        public int ShortageThreshold => _shortageThreshold;
        public int ExcessThreshold => _excessThreshold;
    }

    [CreateAssetMenu(menuName = "Moyva/Economy/AI Rule Profile", fileName = "EconomyAiRuleProfile")]
    public sealed class EconomyAiRuleProfile : ScriptableObject
    {
        [SerializeField] private string _profileId;
        [SerializeField] private List<EconomyResourceThreshold> _resourceThresholds = new List<EconomyResourceThreshold>();
        [SerializeField] private bool _useConservativeSpending;
        [SerializeField] private bool _prioritizeFoodSecurity = true;

        public string ProfileId => _profileId;
        public IReadOnlyList<EconomyResourceThreshold> ResourceThresholds => _resourceThresholds;
        public bool UseConservativeSpending => _useConservativeSpending;
        public bool PrioritizeFoodSecurity => _prioritizeFoodSecurity;
    }
}
