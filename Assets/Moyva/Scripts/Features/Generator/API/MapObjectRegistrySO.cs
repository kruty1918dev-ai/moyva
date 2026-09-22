using UnityEngine;

using Kruty1918.JsonConfig;
namespace Kruty1918.Moyva.Generator.API
{
[System.Serializable]
public class MapObjectRegistrySO : JsonConfigObject
    {
        [SerializeField] private MapObjectDefinition[] _definitions;
        public MapObjectDefinition[] Definitions => _definitions;
    }
}
