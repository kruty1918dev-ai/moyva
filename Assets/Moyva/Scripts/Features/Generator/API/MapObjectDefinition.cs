using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [System.Serializable]
    public class MapObjectDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private GameObject _visualPrefab;

        public string Id => _id;
        public GameObject VisualPrefab => _visualPrefab;
    }

    [CreateAssetMenu(fileName = "MapObjectRegistry", menuName = "Moyva/Generator/MapObjectRegistry")]
    public class MapObjectRegistrySO : ScriptableObject
    {
        [SerializeField] private MapObjectDefinition[] _definitions;
        public MapObjectDefinition[] Definitions => _definitions;
    }
}
