using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Economy.API
{
[System.Serializable]
public sealed class EconomySettlementDefinition : MoyvaJsonConfigObject
    {
        [SerializeField] private string _settlementId;
        [SerializeField] private EconomySettlementType _settlementType;
        [SerializeField] private string _centerBuildingId;
        [SerializeField] private int _buildRadius;

        public string SettlementId => _settlementId;
        public EconomySettlementType SettlementType => _settlementType;
        public string CenterBuildingId => _centerBuildingId;
        public int BuildRadius => _buildRadius;
    }
}
