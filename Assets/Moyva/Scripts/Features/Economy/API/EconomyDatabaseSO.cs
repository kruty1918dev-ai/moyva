using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [CreateAssetMenu(menuName = "Moyva/Economy/Database", fileName = "EconomyDatabase")]
    public sealed class EconomyDatabaseSO : ScriptableObject
    {
        [SerializeField] private int _schemaVersion = 1;
        [SerializeField] private List<EconomyResourceDefinition> _resources = new List<EconomyResourceDefinition>();
        [SerializeField] private List<EconomySettlementDefinition> _settlements = new List<EconomySettlementDefinition>();
        [SerializeField] private List<EconomyWarehousePolicy> _warehousePolicies = new List<EconomyWarehousePolicy>();
        [SerializeField] private List<EconomyProductionProfile> _productionProfiles = new List<EconomyProductionProfile>();
        [SerializeField] private List<EconomyCaravanTemplate> _caravanTemplates = new List<EconomyCaravanTemplate>();
        [SerializeField] private List<EconomyAiRuleProfile> _aiRuleProfiles = new List<EconomyAiRuleProfile>();

        public int SchemaVersion
        {
            get => _schemaVersion;
            set => _schemaVersion = value;
        }

        public IReadOnlyList<EconomyResourceDefinition> Resources => _resources;
        public IReadOnlyList<EconomySettlementDefinition> Settlements => _settlements;
        public IReadOnlyList<EconomyWarehousePolicy> WarehousePolicies => _warehousePolicies;
        public IReadOnlyList<EconomyProductionProfile> ProductionProfiles => _productionProfiles;
        public IReadOnlyList<EconomyCaravanTemplate> CaravanTemplates => _caravanTemplates;
        public IReadOnlyList<EconomyAiRuleProfile> AiRuleProfiles => _aiRuleProfiles;
    }
}
