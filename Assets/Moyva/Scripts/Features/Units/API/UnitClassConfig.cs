using UnityEngine;
using System;
using Kruty1918.Moyva.Animations.API;

namespace Kruty1918.Moyva.Units.API
{
    [Serializable]
    public class UnitClassConfig
    {
        /// <summary>
        /// ВАЖЛИВО: У написані айді НЕ повино використовуватися нижнє підкреслення, окільки це є зарезервований символ для внутрішнього використання (наприклад, для позначення інстанцій юнітів). Рекомендується використовувати дефіси або camelCase. Наприклад: "warrior-01" або "Warrior01".
        /// </summary>
        [Tooltip("У написані айді НЕ повино використовуватися нижнє підкреслення, окільки це є зарезервований символ для внутрішнього використання (наприклад, для позначення інстанцій юнітів). Рекомендується використовувати дефіси або camelCase. Наприклад: \"warrior-01\" або \"Warrior01\".")]
        public string TypeId; // наприклад "warrior-01"
        public float BaseStamina;
        public GameObject Prefab;
        public Vector2 StaminaRandomRange = new Vector2(-5, 5); // +/- 5 випадкових одиниць до базової стаміни
        public PathAnimationSettings AnimationSettings = PathAnimationSettings.Default;
    }
}