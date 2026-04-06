using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/GenerationRules", fileName = "GenerationRules")]
    public class GenerationRules : ScriptableObject
    {
        [Tooltip("Вмикає або вимикає генерацію річок та інших водних фіч, які використовують feature generators. Коли вимкнено, карта залишиться без річкових об'єктів і пов'язаного постпроцесингу.")]
        public bool GenerateRivers = true;
        [Tooltip("Вмикає призначення біомів поверх базової карти висот. Якщо вимкнути, результат HeightMapSettings залишиться без заміни на ліс, траву, болото та інші біоми.")]
        public bool GenerateBiomes = true;
        [Tooltip("Запускає етап Wave Function Collapse після основної генерації. Корисно для полірування тайлів, але може суттєво змінити фінальний вигляд карти.")]
        public bool ApplyWFC = true;
    }
}