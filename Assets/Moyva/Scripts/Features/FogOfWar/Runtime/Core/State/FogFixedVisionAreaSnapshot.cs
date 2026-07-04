using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Save-friendly snapshot однієї fixed vision area.
    /// Використовується <see cref="FogOfWarSaveModule"/> для серіалізації постійних reveal sources.
    /// </summary>
    internal readonly struct FogFixedVisionAreaSnapshot
    {
        /// <summary>
        /// Створює snapshot постійної області видимості.
        /// </summary>
        /// <param name="areaId">Стабільний ідентифікатор області.</param>
        /// <param name="position">Центр області у координатах клітинок.</param>
        /// <param name="visionRange">Радіус області.</param>
        /// <param name="shape">Форма reveal.</param>
        public FogFixedVisionAreaSnapshot(string areaId, Vector2Int position, int visionRange, FogRevealShape shape)
        {
            AreaId = areaId;
            Position = position;
            VisionRange = visionRange;
            Shape = shape;
        }

        /// <summary>
        /// Ідентифікатор області.
        /// </summary>
        public string AreaId { get; }

        /// <summary>
        /// Центр області у координатах клітинок.
        /// </summary>
        public Vector2Int Position { get; }

        /// <summary>
        /// Радіус reveal області.
        /// </summary>
        public int VisionRange { get; }

        /// <summary>
        /// Форма reveal області.
        /// </summary>
        public FogRevealShape Shape { get; }
    }
}
