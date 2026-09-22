using UnityEngine;

using Kruty1918.JsonConfig;
namespace Kruty1918.Moyva.Economy.API
{
[System.Serializable]
public sealed class EconomyResourceDefinition : JsonConfigObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private EconomyResourceCategory _category = EconomyResourceCategory.None;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _stackLimit;
        [SerializeField] [Min(1)] private int _weightGrams = 1000;

        // JSON root metadata "id" is intentionally removed before object Populate()
        // and is stored by JsonConfigRuntime in JsonConfigObject.JsonId.
        // Keep _id as a legacy/compatibility source, but JsonId is the canonical
        // runtime identity for migrated JSON resource definitions.
        public string Id => string.IsNullOrWhiteSpace(_id) ? JsonId : _id;
        public string DisplayName => _displayName;
        public EconomyResourceCategory Category => _category;
        public Sprite Icon => _icon;
        public int StackLimit => _stackLimit;
        public int WeightGrams => _weightGrams;
    }
}
