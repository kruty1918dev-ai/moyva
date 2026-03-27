using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    [System.Serializable]
    public class TileTypeDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private float _movementCost = 1f;

        public string Id => _id;
        public float MovementCost => _movementCost;
    }

    [CreateAssetMenu(fileName = "TileRegistry", menuName = "Moyva/Grid/TileRegistry")]
    public class TileRegistrySO : ScriptableObject
    {
        [SerializeField] private TileTypeDefinition[] _definitions;
        public TileTypeDefinition[] Definitions => _definitions;
    }
}