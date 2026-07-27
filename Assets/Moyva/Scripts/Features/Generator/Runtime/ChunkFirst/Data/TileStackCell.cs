using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class TileStackCell
    {
        private readonly List<GraphTileLayerSample> _samples = new List<GraphTileLayerSample>(4);

        public IReadOnlyList<GraphTileLayerSample> Samples => _samples;
        public int Count => _samples.Count;
        public bool IsEmpty => _samples.Count == 0;

        public void Add(GraphTileLayerSample sample)
        {
            _samples.Add(sample);
        }

        public void Clear()
        {
            _samples.Clear();
        }

        public bool TryGetTopCompatibilitySample(out GraphTileLayerSample sample)
        {
            sample = default;
            if (_samples.Count == 0)
                return false;

            int bestIndex = -1;
            for (int i = 0; i < _samples.Count; i++)
            {
                GraphTileLayerSample candidate = _samples[i];
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
