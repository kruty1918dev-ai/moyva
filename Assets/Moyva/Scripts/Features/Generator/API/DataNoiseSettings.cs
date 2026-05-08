using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/NoiseSettings", fileName = "DataNoiseSettings")]
    public class DataNoiseSettings : ScriptableObject
    {

        [Tooltip("Масштаб шуму. Визначає, наскільки 'розтягнутий' або 'стиснутий' буде шум. Великі значення — плавні області, малі — дрібні деталі. Приклад: 50 — великі континенти, 5 — дрібні острови.")]
        [Min(0.0001f)]
        public float Scale = 20f;


        [Tooltip("Кількість октав. Визначає, скільки шарів шуму буде накладено. 1 — гладко, 8 — багато деталей. Приклад: 4 — баланс між деталізацією та продуктивністю.")]
        [Range(1, 12)]
        public int Octaves = 4;


        [Tooltip("Persistance — як швидко зменшується амплітуда шуму для кожної октави. 0.3 — плавно, 0.8 — багато дрібних деталей. Приклад: 0.5 — природний рельєф.")]
        [Range(0.01f, 1f)]
        public float Persistance = 0.5f;


        [Tooltip("Lacunarity — як швидко зростає частота шуму для кожної октави. 2 — типовий для природних карт, 3+ — дуже 'шумно'. Приклад: 2 — класика для перлинного шуму.")]
        [Min(1f)]
        public float Lacunarity = 2f;

        [Tooltip("Offset — зсув карти шуму по X та Y. Дозволяє зміщувати карту без зміни інших параметрів. Приклад: (100, 200) — карта зміщена праворуч і вгору.")]
        public Vector2 Offset = Vector2.zero;
    }
}