using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{

internal sealed partial class FogVolumePreviewBuilder
    {
        /// <summary>
        /// Мінімальна preview-реалізація fog service.
        /// Вона завжди повертає unexplored/невидимий стан і існує лише для побудови editor preview geometry.
        /// </summary>
        private sealed class PreviewFogService : IFogOfWarService
        {
            private readonly int _width;
            private readonly int _height;

            /// <summary>
            /// Створює preview fog service для фіксованого розміру карти.
            /// </summary>
            /// <param name="width">Ширина preview map.</param>
            /// <param name="height">Висота preview map.</param>
            public PreviewFogService(int width, int height)
            {
                _width = Mathf.Max(1, width);
                _height = Mathf.Max(1, height);
            }

            /// <summary>
            /// Preview-реалізація не потребує окремої ініціалізації fog state.
            /// </summary>
            /// <param name="width">Ігнорується у preview stub.</param>
            /// <param name="height">Ігнорується у preview stub.</param>
            public void Initialize(int width, int height) { }

            /// <summary>
            /// Preview stub не відстежує runtime юнітів.
            /// </summary>
            public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }

            /// <summary>
            /// Preview stub не оновлює runtime vision ranges.
            /// </summary>
            public void UpdateUnitVisionRange(string unitId, int visionRange) { }

            /// <summary>
            /// Preview stub не відстежує fixed vision areas.
            /// </summary>
            public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }

            /// <summary>
            /// Preview stub не змінює власний fog state через runtime reveal operations.
            /// </summary>
            public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }

            /// <summary>
            /// Preview stub не відстежує переміщення юнітів.
            /// </summary>
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }

            /// <summary>
            /// Preview stub не тримає runtime unit registrations.
            /// </summary>
            public void UnregisterUnit(string unitId) { }

            /// <summary>
            /// Завжди повертає unexplored стан для preview geometry build.
            /// </summary>
            /// <param name="position">Клітинка, яку запитують.</param>
            /// <returns>Завжди <see cref="FogStateType.Unexplored"/>.</returns>
            public FogStateType GetFogState(Vector2Int position) => FogStateType.Unexplored;

            /// <summary>
            /// Preview stub не має runtime visible-клітинок.
            /// </summary>
            public bool IsVisible(Vector2Int position) => false;

            /// <summary>
            /// Preview stub не зберігає explored state.
            /// </summary>
            public bool IsExplored(Vector2Int position) => false;

            /// <summary>
            /// Повертає порожній explored snapshot для preview path.
            /// </summary>
            public bool[,] GetExploredSnapshot() => new bool[_width, _height];

            /// <summary>
            /// Preview stub ігнорує завантаження snapshot-ів.
            /// </summary>
            public void LoadFromSnapshot(bool[,] explored) { }

            /// <summary>
            /// Preview stub не накопичує dirty tiles.
            /// </summary>
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }
    }
}
