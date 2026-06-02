using System.Collections.Generic;

namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Кеш останніх згенерованих bool-масок по шарах під час виконання графа.
    /// </summary>
    public sealed class LayerMaskRegistry
    {
        private readonly Dictionary<string, bool[,]> _masksByLayerId = new();

        public void SetLatestMask(string layerId, bool[,] mask)
        {
            if (string.IsNullOrEmpty(layerId) || mask == null)
                return;

            _masksByLayerId[layerId] = mask;
        }

        public bool TryGetLatestMask(string layerId, out bool[,] mask)
        {
            if (string.IsNullOrEmpty(layerId))
            {
                mask = null;
                return false;
            }

            return _masksByLayerId.TryGetValue(layerId, out mask) && mask != null;
        }
    }
}
