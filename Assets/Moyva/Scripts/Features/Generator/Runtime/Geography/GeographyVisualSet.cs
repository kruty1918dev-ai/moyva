using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Visual binding for one geography tile id: which provisioned TWC build
    /// layer renders it and which preset id the chunk-first mesh provider
    /// should select inside that layer.
    /// </summary>
    internal sealed class GeographyVisualLayer
    {
        public string TileId;
        public string GraphLayerId;
        public string BuildLayerGuid;
        public string BlueprintLayerGuid;
        public string PresetId;
        public int SortOrder;
    }

    /// <summary>TileId → visual layer lookup for one generated world.</summary>
    internal sealed class GeographyVisualSet
    {
        private readonly Dictionary<string, GeographyVisualLayer> _byTileId =
            new Dictionary<string, GeographyVisualLayer>(System.StringComparer.Ordinal);

        public IReadOnlyCollection<GeographyVisualLayer> Layers => _byTileId.Values;

        public void Add(GeographyVisualLayer layer)
        {
            if (layer == null || string.IsNullOrWhiteSpace(layer.TileId))
                return;
            _byTileId[layer.TileId] = layer;
        }

        public bool TryGet(string tileId, out GeographyVisualLayer layer)
            => _byTileId.TryGetValue(tileId ?? string.Empty, out layer);
    }
}
