using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum ObjectExclusionMode
    {
        [InspectorName("Список об'єктів")]
        ObjectList = 0,
        [InspectorName("Усі об'єкти")]
        AllObjects = 1,
        [InspectorName("Усі, крім списку")]
        AllObjectsExceptList = 2
    }

    [Serializable]
    public struct TileObjectExclusionRule
    {
        [Tooltip("Цільовий Tile ID для цього правила. Для режиму базового порівняння достатньо вказати базу, наприклад grass.")]
        [TileId] public string TargetTileId;

        [Tooltip("Якщо увімкнено, правило спрацює лише для клітинок, які межують із водою.")]
        public bool OnlyIfNearWater;

        [Tooltip("Режим правила: або видаляти лише об'єкти зі списку, або видаляти всі об'єкти на цільовому тайлі.")]
        public ObjectExclusionMode ExclusionMode;

        [Tooltip("Список Object ID. Для 'Список об'єктів' це заборонені ID. Для 'Усі, крім списку' це дозволені винятки.")]
        [MapObjectId] public string[] ForbiddenObjectIds;
    }

    [NodeInfo("Object Tile Exclusion", "Utility", "Прибирає об'єкти з клітинок, де поточний тайл не дозволяє ці об'єкти за списком правил. Підходить для очищення берегової трави від дерев, доріг від каменів тощо.")]
    public sealed class ObjectTileExclusionNode : NodeBase
    {
        [Header("Rules")]
        [Tooltip("Набір правил: цільовий тайл + список заборонених об'єктів для нього.")]
        [SerializeField] private TileObjectExclusionRule[] _rules;

        [Header("Matching")]
        [Tooltip("Якщо увімкнено, порівнює базовий тип тайла (до роздільника), напр. grass для grass-cliff-N.")]
        [SerializeField] private bool _matchBaseTileType = true;

        [Tooltip("Якщо увімкнено, порівнює базовий тип об'єкта (до роздільника) при перевірці allowed-списку, напр. river для river-vertical.")]
        [SerializeField] private bool _matchBaseObjectType = true;

        [Tooltip("Роздільник для базового типу (тайла та об'єкта).")]
        [SerializeField] private char _tileSeparator = '-';

        [Header("Near Water")]
        [Tooltip("Список Tile ID, які вважаються водою для умови 'Only If Near Water'.")]
        [SerializeField, TileId] private string[] _waterLikeTileIds =
        {
            "water",
            "sea",
            "coast",
            "water-shallow",
            "water-deep",
            "lake",
            "river"
        };

        [Tooltip("Радіус пошуку води навколо клітинки для умови 'Only If Near Water'.")]
        [SerializeField, Range(1, 4)] private int _waterNeighborRadius = 1;

        [Tooltip("Якщо увімкнено, у пошуку води враховуються діагональні сусіди.")]
        [SerializeField] private bool _includeDiagonalWaterNeighbors = true;

        public override string Title => "Object Tile Exclusion";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<string[,]>("ObjectMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = inputs[0] as string[,];
            var objectMap = inputs[1] as string[,];

            if (biomeMap == null || objectMap == null)
                return NodeOutput.Error("BiomeMap and ObjectMap inputs are required.");

            if (_rules == null || _rules.Length == 0)
                return NodeOutput.Success(objectMap);

            int w = biomeMap.GetLength(0);
            int h = biomeMap.GetLength(1);
            var result = (string[,])objectMap.Clone();

            BuildRuleMaps(
                out var allObjectsTiles,
                out var allObjectsNearWaterTiles,
                out var allObjectsExceptTiles,
                out var allObjectsExceptNearWaterTiles,
                out var listedObjectsTiles,
                out var listedObjectsNearWaterTiles);

            if (allObjectsTiles.Count == 0
                && allObjectsNearWaterTiles.Count == 0
                && allObjectsExceptTiles.Count == 0
                && allObjectsExceptNearWaterTiles.Count == 0
                && listedObjectsTiles.Count == 0
                && listedObjectsNearWaterTiles.Count == 0)
                return NodeOutput.Success(result);

            var waterLikeSet = BuildWaterLikeSet(GetEffectiveWaterTileIds(context));
            int waterRadius = Mathf.Max(1, _waterNeighborRadius);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    string currentObject = result[x, y];
                    if (string.IsNullOrWhiteSpace(currentObject))
                        continue;

                    string tileId = biomeMap[x, y];
                    if (string.IsNullOrWhiteSpace(tileId))
                        continue;

                    string key = NormalizeTileKey(tileId);
                    bool nearWaterComputed = false;
                    bool nearWater = false;

                    if (allObjectsTiles.Contains(key))
                    {
                        if (allObjectsExceptTiles.TryGetValue(key, out var allowedInAllMode)
                            && IsObjectAllowed(currentObject, allowedInAllMode))
                            continue;

                        result[x, y] = string.Empty;
                        continue;
                    }

                    if (allObjectsNearWaterTiles.Contains(key))
                    {
                        nearWater = HasWaterLikeNeighbor(
                            biomeMap, x, y, w, h, waterRadius, _includeDiagonalWaterNeighbors, waterLikeSet);
                        nearWaterComputed = true;

                        if (nearWater)
                        {
                            if (allObjectsExceptNearWaterTiles.TryGetValue(key, out var allowedNearWaterInAllMode)
                                && IsObjectAllowed(currentObject, allowedNearWaterInAllMode))
                                continue;

                            result[x, y] = string.Empty;
                            continue;
                        }
                    }

                    if (allObjectsExceptTiles.TryGetValue(key, out var allowedGlobal)
                        && !IsObjectAllowed(currentObject, allowedGlobal))
                    {
                        result[x, y] = string.Empty;
                        continue;
                    }

                    if (allObjectsExceptNearWaterTiles.TryGetValue(key, out var allowedNearWater))
                    {
                        if (!nearWaterComputed)
                        {
                            nearWater = HasWaterLikeNeighbor(
                                biomeMap, x, y, w, h, waterRadius, _includeDiagonalWaterNeighbors, waterLikeSet);
                            nearWaterComputed = true;
                        }

                        if (nearWater && !IsObjectAllowed(currentObject, allowedNearWater))
                        {
                            result[x, y] = string.Empty;
                            continue;
                        }
                    }

                    if (listedObjectsTiles.TryGetValue(key, out var forbidden)
                        && forbidden.Contains(currentObject))
                    {
                        result[x, y] = string.Empty;
                        continue;
                    }

                    if (!listedObjectsNearWaterTiles.TryGetValue(key, out var nearWaterForbidden))
                        continue;

                    if (!nearWaterComputed)
                    {
                        nearWater = HasWaterLikeNeighbor(
                            biomeMap, x, y, w, h, waterRadius, _includeDiagonalWaterNeighbors, waterLikeSet);
                    }

                    if (nearWater && nearWaterForbidden.Contains(currentObject))
                        result[x, y] = string.Empty;
                }
            }

            return NodeOutput.Success(result);
        }

        private void BuildRuleMaps(
            out HashSet<string> allObjectsTiles,
            out HashSet<string> allObjectsNearWaterTiles,
            out Dictionary<string, HashSet<string>> allObjectsExceptTiles,
            out Dictionary<string, HashSet<string>> allObjectsExceptNearWaterTiles,
            out Dictionary<string, HashSet<string>> listedObjectsTiles,
            out Dictionary<string, HashSet<string>> listedObjectsNearWaterTiles)
        {
            allObjectsTiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            allObjectsNearWaterTiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            allObjectsExceptTiles = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            allObjectsExceptNearWaterTiles = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            listedObjectsTiles = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            listedObjectsNearWaterTiles = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var rule in _rules)
            {
                if (string.IsNullOrWhiteSpace(rule.TargetTileId))
                    continue;

                string key = NormalizeTileKey(rule.TargetTileId);
                bool onlyNearWater = rule.OnlyIfNearWater;

                if (rule.ExclusionMode == ObjectExclusionMode.AllObjects)
                {
                    if (onlyNearWater)
                        allObjectsNearWaterTiles.Add(key);
                    else
                        allObjectsTiles.Add(key);
                    continue;
                }

                if (rule.ExclusionMode == ObjectExclusionMode.AllObjectsExceptList)
                {
                    var allExceptMap = onlyNearWater ? allObjectsExceptNearWaterTiles : allObjectsExceptTiles;
                    if (!allExceptMap.TryGetValue(key, out var allowedSet))
                    {
                        allowedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        allExceptMap[key] = allowedSet;
                    }

                    if (rule.ForbiddenObjectIds == null)
                        continue;

                    foreach (var objectId in rule.ForbiddenObjectIds)
                    {
                        if (!string.IsNullOrWhiteSpace(objectId))
                            allowedSet.Add(objectId.Trim());
                    }

                    continue;
                }

                var targetMap = onlyNearWater ? listedObjectsNearWaterTiles : listedObjectsTiles;
                if (!targetMap.TryGetValue(key, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    targetMap[key] = set;
                }

                if (rule.ForbiddenObjectIds == null)
                    continue;

                foreach (var objectId in rule.ForbiddenObjectIds)
                {
                    if (!string.IsNullOrWhiteSpace(objectId))
                        set.Add(objectId.Trim());
                }
            }
        }

        private string[] GetEffectiveWaterTileIds(NodeContext context)
        {
            if (_waterLikeTileIds != null && _waterLikeTileIds.Length > 0)
                return _waterLikeTileIds;

            if (context.TryGetService<ISharedGeneratorSettings>(out var shared)
                && shared.WaterLikeTileIds != null && shared.WaterLikeTileIds.Length > 0)
                return shared.WaterLikeTileIds;

            return new[] { "water" };
        }

        private HashSet<string> BuildWaterLikeSet(string[] waterTileIds)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (waterTileIds != null)
            {
                foreach (var id in waterTileIds)
                {
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    string trimmed = id.Trim();
                    set.Add(trimmed);

                    int sep = trimmed.IndexOf(_tileSeparator);
                    if (sep > 0)
                        set.Add(trimmed.Substring(0, sep));
                }
            }

            if (set.Count == 0)
                set.Add("water");

            return set;
        }

        private bool HasWaterLikeNeighbor(
            string[,] biomeMap,
            int x,
            int y,
            int w,
            int h,
            int radius,
            bool includeDiagonals,
            HashSet<string> waterLikeSet)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    if (!includeDiagonals && Mathf.Abs(dx) + Mathf.Abs(dy) > radius)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        continue;

                    if (IsWaterLikeTile(biomeMap[nx, ny], waterLikeSet))
                        return true;
                }
            }

            return false;
        }

        private bool IsWaterLikeTile(string tileId, HashSet<string> waterLikeSet)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            string trimmed = tileId.Trim();
            if (waterLikeSet.Contains(trimmed))
                return true;

            int sep = trimmed.IndexOf(_tileSeparator);
            if (sep > 0 && waterLikeSet.Contains(trimmed.Substring(0, sep)))
                return true;

            string lowered = trimmed.ToLowerInvariant();
            return lowered.Contains("water")
                || lowered.Contains("sea")
                || lowered.Contains("lake")
                || lowered.Contains("river");
        }

        private string NormalizeTileKey(string tileId)
        {
            string trimmed = tileId.Trim();
            if (!_matchBaseTileType)
                return trimmed;

            int sep = trimmed.IndexOf(_tileSeparator);
            return sep <= 0 ? trimmed : trimmed.Substring(0, sep);
        }

        private bool IsObjectAllowed(string objectId, HashSet<string> allowedSet)
        {
            if (allowedSet.Contains(objectId))
                return true;

            if (_matchBaseObjectType)
            {
                int sep = objectId.IndexOf(_tileSeparator);
                if (sep > 0 && allowedSet.Contains(objectId.Substring(0, sep)))
                    return true;
            }

            return false;
        }
    }
}
