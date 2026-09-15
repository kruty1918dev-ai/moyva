using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [System.Serializable]
    public class MapObjectDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private GameObject _visualPrefab;

        public MapObjectDefinition() { }

        public MapObjectDefinition(string id, GameObject visualPrefab = null)
        {
            _id = id;
            _visualPrefab = visualPrefab;
        }

        public string Id => _id;
        public GameObject VisualPrefab => _visualPrefab;
    }
}
