using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Generator.API
{
[System.Serializable]
public class MapObjectRegistrySO : MoyvaJsonConfigObject
    {
        [SerializeField] private MapObjectDefinition[] _definitions;
        public MapObjectDefinition[] Definitions => _definitions;
    }
}
