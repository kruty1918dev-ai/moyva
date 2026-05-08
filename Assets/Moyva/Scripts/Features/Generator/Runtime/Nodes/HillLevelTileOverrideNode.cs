using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Hill Level Tile Override", "Terrain",
        "Приймає HillLevelData, вибирає конкретний рівень і замінює hill-тайли, які поставив Hill Generator, " +
        "на інший набір тайлів за напрямками схилу. Вихід — оновлений HillLevelData, а preview підсвічує змінені клітинки.")]
    public sealed class HillLevelTileOverrideNode : NodeBase, IPreviewableNode
    {
        [Header("Selection")]
        [Tooltip("Рівень з HillLevelData, для якого потрібно перевизначити hill-тайли.")]
        [SerializeField, Min(0)] private int _targetLevel;

        [Tooltip("Якщо увімкнено, нода змінює лише ті клітинки, які Hill Generator справді модифікував.")]
        [SerializeField] private bool _onlyModifiedTiles = true;

        [Header("Replacement Tiles")]
        [Tooltip("Новий набір тайлів для вибраного рівня. Ключ — напрямок, значення — Tile ID заміни.")]
        [SerializeField] private HillTileEntry[] _replacementTiles = Array.Empty<HillTileEntry>();

        [Header("Preview")]
        [Tooltip("Колір, яким preview підсвічує клітинки, змінені цією нодою.")]
        [SerializeField] private Color _changedHighlightColor = new(1f, 0.15f, 0.75f, 1f);

        [NonSerialized] private HillLevelDataMap _lastInput;
        [NonSerialized] private HillLevelDataMap _lastOutput;
        [NonSerialized] private bool[,] _lastChangedMask;
        [NonSerialized] private int _lastMaxLevel;

        public override string Title => "Hill Level Tile Override";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<HillLevelDataMap>("HillLevelData")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<HillLevelDataMap>("HillLevelData")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var input = inputs[0] as HillLevelDataMap;
            if (input == null)
                return NodeOutput.Error("HillLevelData input is required.");

            var lookup = BuildLookup();
            int width = input.Width;
            int height = input.Height;
            var sourceTiles = input.CopyTiles();

            if (lookup.Count == 0)
            {
                var unchangedMask = new bool[width, height];
                int unchangedMaxLevel = 0;
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    unchangedMaxLevel = Mathf.Max(unchangedMaxLevel, sourceTiles[x, y].Level);

                _lastInput = input.Clone();
                _lastOutput = input.Clone();
                _lastChangedMask = unchangedMask;
                _lastMaxLevel = unchangedMaxLevel;

                return NodeOutput.Success(_lastOutput);
            }

            var resultTiles = new HillLevelTileData[width, height];
            var changedMask = new bool[width, height];
            int maxLevel = 0;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                HillLevelTileData tile = sourceTiles[x, y];
                maxLevel = Mathf.Max(maxLevel, tile.Level);

                if (!ShouldReplace(tile))
                {
                    resultTiles[x, y] = tile;
                    continue;
                }

                if (!TryGetDirection(tile.DirectionId, out var direction))
                {
                    resultTiles[x, y] = tile;
                    continue;
                }

                if (!lookup.TryGetValue(direction, out string replacementTileId) || string.IsNullOrEmpty(replacementTileId))
                {
                    resultTiles[x, y] = tile;
                    continue;
                }

                bool changed = !string.Equals(tile.TileId, replacementTileId, StringComparison.Ordinal);
                changedMask[x, y] = changed;
                resultTiles[x, y] = changed
                    ? new HillLevelTileData(
                        tile.X,
                        tile.Y,
                        replacementTileId,
                        tile.SourceTileId,
                        tile.DirectionId,
                        tile.Height,
                        tile.Level,
                        !string.Equals(tile.SourceTileId, replacementTileId, StringComparison.Ordinal))
                    : tile;
            }

            _lastInput = input.Clone();
            _lastOutput = new HillLevelDataMap(resultTiles);
            _lastChangedMask = changedMask;
            _lastMaxLevel = maxLevel;

            return NodeOutput.Success(_lastOutput);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            if (_lastInput == null)
                return null;

            int tw = Mathf.Clamp(width, 32, 256);
            int th = Mathf.Clamp(height, 32, 256);
            var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            int sourceWidth = _lastInput.Width;
            int sourceHeight = _lastInput.Height;
            int totalLevels = Mathf.Max(2, _lastMaxLevel + 1);

            for (int x = 0; x < tw; x++)
            {
                int sx = x * sourceWidth / tw;
                for (int y = 0; y < th; y++)
                {
                    int sy = y * sourceHeight / th;
                    HillLevelTileData tile = _lastInput.GetTile(sx, sy);
                    Color baseColor = HillGeneratorNode.LevelIndexToColor(tile.Level, totalLevels);
                    Color finalColor = _lastChangedMask != null && _lastChangedMask[sx, sy]
                        ? Color.Lerp(baseColor, _changedHighlightColor, 0.75f)
                        : baseColor;
                    tex.SetPixel(x, y, finalColor);
                }
            }

            tex.Apply();
            return tex;
        }

        private bool ShouldReplace(HillLevelTileData tile)
        {
            if (tile.Level != _targetLevel)
                return false;
            if (_onlyModifiedTiles && !tile.WasModified)
                return false;

            return !string.IsNullOrWhiteSpace(tile.DirectionId);
        }

        private static bool TryGetDirection(string directionId, out HillDirection direction)
            => Enum.TryParse(directionId, true, out direction);

        private Dictionary<HillDirection, string> BuildLookup()
        {
            var lookup = new Dictionary<HillDirection, string>();
            if (_replacementTiles == null)
                return lookup;

            for (int i = 0; i < _replacementTiles.Length; i++)
            {
                var entry = _replacementTiles[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.TileId))
                    continue;

                lookup[entry.Direction] = entry.TileId;
            }

            return lookup;
        }
    }
}