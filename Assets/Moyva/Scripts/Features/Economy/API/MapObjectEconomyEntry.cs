using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [System.Serializable]
    public class MapObjectEconomyEntry
    {
        [SerializeField] private string _mapObjectId;
        [SerializeField] private string _displayName;
        [SerializeField] private bool _isInteractable;
        [SerializeField] private bool _yieldsResource;
        [SerializeField] private string _harvestResourceId;

        public string MapObjectId => _mapObjectId;
        public string DisplayName => _displayName;
        public bool IsInteractable => _isInteractable;
        public bool YieldsResource => _yieldsResource;
        public string HarvestResourceId => _harvestResourceId;
    }
}
