using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [CreateAssetMenu(menuName = "Moyva/Economy/Production Profile", fileName = "EconomyProductionProfile")]
    public sealed class EconomyProductionProfile : ScriptableObject
    {
        [SerializeField] private string _buildingId;
        [SerializeField] private bool _isActiveByDefault = true;
        [SerializeField] private string _recipeId;
        [SerializeField] private float _cycleDurationSeconds = 60f;
        [SerializeField] private int _outputAmountPerCycle = 1;

        public string BuildingId => _buildingId;
        public bool IsActiveByDefault => _isActiveByDefault;
        public string RecipeId => _recipeId;
        public float CycleDurationSeconds => _cycleDurationSeconds;
        public int OutputAmountPerCycle => _outputAmountPerCycle;
    }
}
