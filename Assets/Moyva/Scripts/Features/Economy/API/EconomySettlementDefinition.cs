using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [CreateAssetMenu(menuName = "Moyva/Economy/Settlement Definition", fileName = "EconomySettlementDefinition")]
    public sealed class EconomySettlementDefinition : ScriptableObject
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
