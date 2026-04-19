using System;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Спільні налаштування графа — пряма частина GraphAsset, не є нодом.
    /// Зберігають дані, які мають бути доступні всім нодам під час виконання
    /// без необхідності розміщувати додатковий вузол у граф.
    /// Реєструються в NodeContext як сервіс: context.TryGetService&lt;GraphSharedSettings&gt;.
    /// </summary>
    [Serializable]
    public sealed class GraphSharedSettings
    {
        [Header("Map Size")]
        [Tooltip("Ширина мапи (у тайлах). 0 = брати розмір із GridService (зовнішнє джерело).")]
        [SerializeField] private int _mapWidth;

        [Tooltip("Висота мапи (у тайлах). 0 = брати розмір із GridService (зовнішнє джерело).")]
        [SerializeField] private int _mapHeight;

        public int MapWidth => _mapWidth;
        public int MapHeight => _mapHeight;
        public Vector2Int MapSize => new Vector2Int(_mapWidth, _mapHeight);

        /// <summary>
        /// true — якщо обидва виміри задані в налаштуваннях графа (>0).
        /// Якщо true, GraphBasedMapDataGenerator ігнорує зовнішні width/height і
        /// використовує ці значення.
        /// </summary>
        public bool HasMapSize => _mapWidth > 0 && _mapHeight > 0;
    }
}
