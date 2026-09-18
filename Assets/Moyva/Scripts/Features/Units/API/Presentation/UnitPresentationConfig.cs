using Kruty1918.Moyva.Presentation.API;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    [System.Serializable]
    public sealed class UnitPresentationConfig : EntityPresentationConfig
    {
        public GameObject Prefab;
        public Sprite CustomSprite;
    }
}
