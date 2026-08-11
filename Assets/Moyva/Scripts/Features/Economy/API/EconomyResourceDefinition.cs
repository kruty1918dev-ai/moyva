using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Economy.API
{
[System.Serializable]
public sealed class EconomyResourceDefinition : MoyvaJsonConfigObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private EconomyResourceCategory _category = EconomyResourceCategory.None;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _stackLimit;
        [SerializeField] [Min(1)] private int _weightGrams = 1000;

        public string Id => _id;
        public string DisplayName => _displayName;
        public EconomyResourceCategory Category => _category;
        public Sprite Icon => _icon;
        public int StackLimit => _stackLimit;
        public int WeightGrams => _weightGrams;
    }
}
