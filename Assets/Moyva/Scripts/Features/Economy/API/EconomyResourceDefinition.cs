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

        public string Id => _id;
        public string DisplayName => _displayName;
        public EconomyResourceCategory Category => _category;
        public Sprite Icon => _icon;
        public int StackLimit => _stackLimit;
    }
}
