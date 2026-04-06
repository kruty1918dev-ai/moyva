using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/HeightMapSettings", fileName = "HeightMapSettings")]
    public class HeightMapSettings : ScriptableObject
    {
        [Tooltip("Набір шарів висоти, які перетворюють числову карту висот у базову карту тайлів. Кожен шар задає інтервал висот і Tile ID, що має використовуватись у цьому інтервалі.")]
        public HeightLayer[] HeightLayers;
    }

    [System.Serializable]
    public class HeightLayer
    {
        [Tooltip("ID тайла, який буде використано для всіх клітинок, що потрапили у вказаний інтервал висоти. Це базовий матеріал поверхні до додаткової обробки біомами та фічами.")]
        [TileId] public string TileID;
        [Tooltip("Нижня межа висоти для цього шару. Якщо висота клітинки нижча за це значення, шар не спрацює.")]
        public float MinHeight;
        [Tooltip("Верхня межа висоти для цього шару. Використовується для визначення, до якого базового тайла належить клітинка.")]
        public float MaxHeight;
    }
}