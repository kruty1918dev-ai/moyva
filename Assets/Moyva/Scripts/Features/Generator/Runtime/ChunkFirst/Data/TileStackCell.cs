using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class TileStackCell
    {
        private readonly List<TileLayerSample> _samples = new List<TileLayerSample>(4);

        public IReadOnlyList<TileLayerSample> Samples => _samples;
        public int Count => _samples.Count;
        public bool IsEmpty => _samples.Count == 0;

        public void Add(TileLayerSample sample)
        {
            _samples.Add(sample);
        }

        public void Clear()
        {
            _samples.Clear();
        }

        public bool TryGetTopCompatibilitySample(out TileLayerSample sample)
        {
            sample = default;
            if (_samples.Count == 0)
                return false;

            int bestIndex = -1;
            for (int i = 0; i < _samples.Count; i++)
            {
                TileLayerSample candidate = _samples[i];
                if (!candidate.IsTerrainLike
                    || candidate.LayerKind == LayerKind.OverlayTerrain)
                    continue;

                if (bestIndex < 0 || _samples[bestIndex].CompareTo(candidate) <= 0)
                    bestIndex = i;
            }

            // Compatibility maps can also represent object-only graphs. Preserve
            // the historical fallback when there is no main-terrain candidate.
            if (bestIndex < 0)
            {
                bestIndex = 0;
                for (int i = 1; i < _samples.Count; i++)
                {
                    if (_samples[bestIndex].CompareTo(_samples[i]) <= 0)
                        bestIndex = i;
                }
            }

            sample = _samples[bestIndex];
            return true;
        }
    }
}
