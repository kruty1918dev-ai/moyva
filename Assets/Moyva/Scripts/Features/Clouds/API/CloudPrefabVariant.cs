using UnityEngine;

namespace Kruty1918.Moyva.Clouds.API
{
    [System.Serializable]
    public sealed class CloudPrefabVariant
    {
        [Tooltip("Prefab із MeshFilter/MeshRenderer, який представляє одну форму 3D-хмари.")]
        public GameObject Prefab;

        [Tooltip("Вага вибору цього варіанта відносно інших.")]
        [Min(0f)] public float Chance = 1f;

        [Tooltip("Додатковий множник масштабу саме для цього варіанта.")]
        [Min(0.01f)] public float ScaleMultiplier = 1f;
    }
}
