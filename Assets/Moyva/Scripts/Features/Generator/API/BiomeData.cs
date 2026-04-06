using System;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [Serializable]
    public struct BiomeData
    {
        [Tooltip("ID тайла, який буде поставлено, якщо клітинка задовольняє умови цього біому. Це фактичний результат, який побачить карта після резолву біомів.")]
        [TileId] public string TileID;
        [Tooltip("Людська назва біому для зручності налаштування. Використовується в інспекторі як підказка для дизайнера і не бере участі у логіці генерації.")]
        public string BiomeName;
        
        [Header("Height Range (0.0 - 1.0)")]
        [Tooltip("Мінімальна висота, з якої цей біом починає діяти. Дозволяє відокремити низини, рівнини, височини та інші вертикальні пояси місцевості.")]
        public float MinHeight;
        [Tooltip("Максимальна висота, до якої цей біом залишається валідним. Разом із MinHeight формує повний вертикальний діапазон застосування.")]
        public float MaxHeight;

        [Header("Moisture Range (0.0 - 1.0)")]
        [Tooltip("Мінімальна вологість для цього біому. 0 означає дуже суху зону, 1 — максимально вологу. Дає змогу розділяти, наприклад, степ, ліс, болото чи озерні низини.")]
        public float MinMoisture;
        [Tooltip("Максимальна вологість для цього біому. Разом із MinMoisture утворює вологісний коридор, у межах якого цей біом може з'явитися.")]
        public float MaxMoisture;
    }
}