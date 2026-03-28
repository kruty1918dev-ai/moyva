using UnityEngine;
using System;

namespace Kruty1918.Moyva.Units.Runtime
{
    [Serializable]
    public class UnitClassConfig
    {
        public string TypeId; // "warrior"
        public float BaseStamina;
        public float StaminaRegenBase;
        public GameObject Prefab;
        public Vector2 StaminaRandomRange = new Vector2(0.9f, 1.1f); // +/- 10%
    }
}