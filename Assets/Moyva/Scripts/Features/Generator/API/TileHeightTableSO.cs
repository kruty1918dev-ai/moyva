using System;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    [CreateAssetMenu(menuName = "Moyva/Generator/TileHeightTable", fileName = "TileHeightTable")]
    public class TileHeightTableSO : ScriptableObject
    {
        [Tooltip("Таблиця тайлів з висотними діапазонами та спрайтами. Описує, який тайл відповідає якому діапазону висот на карті.")]
        public TileHeightEntry[] Entries;
    }

    [Serializable]
    public class TileHeightEntry
    {
        [Tooltip("ID тайла із TileRegistry.")]
        [TileId] public string TileId;

        [Tooltip("Нижня межа висоти (0–1).")]
        public float MinHeight;

        [Tooltip("Верхня межа висоти (0–1).")]
        public float MaxHeight;

        [Tooltip("Спрайт цього тайла для візуальної ідентифікації.")]
        public Sprite Sprite;

        [Tooltip("Опис тайла.")]
        [TextArea(1, 3)] public string Description;
    }
}
