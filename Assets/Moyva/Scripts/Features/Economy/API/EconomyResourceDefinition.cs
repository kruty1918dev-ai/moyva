using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    [CreateAssetMenu(menuName = "Moyva/Economy/Resource Definition", fileName = "EconomyResourceDefinition")]
    public sealed class EconomyResourceDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private EconomyResourceCategory _category = EconomyResourceCategory.None;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _stackLimit;
        [SerializeField] [Min(1)] private int _weightGrams = 1000;
        [SerializeField] [Range(0f, 1f)] private float _sizeNormalized = 0.25f;

        public string Id => _id;
        public string DisplayName => _displayName;
        public EconomyResourceCategory Category => _category;
        public Sprite Icon => _icon;
        public int StackLimit => _stackLimit;
        public int WeightGrams => _weightGrams;
        public float SizeNormalized => _sizeNormalized;
    }
}
