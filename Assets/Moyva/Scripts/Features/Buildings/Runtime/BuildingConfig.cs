using UnityEngine;
using Kruty1918.Moyva.Buildings.API;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// ScriptableObject із конфігурацією одного типу будівлі.
    /// Створюється через: Assets → Create → Moyva → Buildings → BuildingConfig
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingConfig", menuName = "Moyva/Buildings/BuildingConfig")]
    public class BuildingConfig : ScriptableObject, IBuildingConfig
    {
        [Tooltip("Унікальний ID типу будівлі. Не використовуйте підкреслення у BuildingId — воно є зарезервованим символом для розділення типу та ID екземпляру (наприклад, barracks-01_<guid>). Використовуйте дефіси або camelCase.")]
        [SerializeField] private string _buildingId;

        [SerializeField] private string _displayName;

        [SerializeField] private BuildingCategory _category;

        [SerializeField] private Sprite _sprite;

        [SerializeField] private Vector2Int _size = Vector2Int.one;

        [Tooltip("Префаб будівлі, який буде spawned при підтвердженні будівництва.")]
        [SerializeField] private GameObject _prefab;

        public string BuildingId => _buildingId;
        public string DisplayName => _displayName;
        public BuildingCategory Category => _category;
        public Sprite Sprite => _sprite;
        public Vector2Int Size => _size;
        public GameObject Prefab => _prefab;
    }
}
