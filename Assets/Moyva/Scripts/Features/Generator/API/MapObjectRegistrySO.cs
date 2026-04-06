using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(fileName = "MapObjectRegistry", menuName = "Moyva/Generator/MapObjectRegistry")]
    public class MapObjectRegistrySO : ScriptableObject
    {
        [SerializeField] private MapObjectDefinition[] _definitions;
        public MapObjectDefinition[] Definitions => _definitions;
    }
}
