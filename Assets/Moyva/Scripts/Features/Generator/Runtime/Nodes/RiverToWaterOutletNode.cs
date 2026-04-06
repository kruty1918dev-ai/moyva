using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum OutletDirection
    {
        North,
        East,
        South,
        West
    }

    [Serializable]
    public struct DirectionalOutletRule
    {
        [Tooltip("Напрямок, у який річка впадає у воду.")]
        public OutletDirection Direction;

        [Tooltip("Object ID outlet-варіанту для цього напрямку.")]
        [MapObjectId] public string OutletObjectId;
    }

    [NodeInfo("River To Water Outlet", "Features", "Знаходить кінці річок у ObjectMap, дотягує їх найкоротшим шляхом прямо до води й ставить outlet-об'єкт у фінальній клітинці шляху.")]
    public sealed class RiverToWaterOutletNode : NodeBase
    {
        [Header("River")]
        [Tooltip("Базовий Object ID річки, для якого виконується пошук країв.")]
        [SerializeField, MapObjectId] private string _riverBaseObjectId = "river";

        [Tooltip("Якщо увімкнено, порівнює базовий тип об'єкта (до роздільника), напр. river для river-vertical.")]
        [SerializeField] private bool _matchBaseRiverType = true;

        [Tooltip("Роздільник для базового типу object ID.")]
        [SerializeField] private char _objectSeparator = '-';

        [Header("Water")]
        [Tooltip("Tile ID, які вважаються водою.")]
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

        [Tooltip("Якщо увімкнено, порівнює базовий тип тайла (до роздільника).")]
        [SerializeField] private bool _matchBaseWaterType = true;

        [Header("Path")]
        [Tooltip("Максимальна довжина пошуку шляху до води для кожного краю річки.")]
        [SerializeField, Range(1, 512)] private int _maxSearchDistance = 96;

        [Tooltip("Якщо увімкнено, шлях може прокладатися по діагоналі.")]
        [SerializeField] private bool _allowDiagonalPath = false;

        [Tooltip("Якщо увімкнено, нода може заміщати будь-які об'єкти на шляху річкою.")]
        [SerializeField] private bool _overwriteObjectsOnPath = true;

        [Header("Outlet")]
        [Tooltip("Базовий Object ID для клітин шляху, які добудовуються до води.")]
        [SerializeField, MapObjectId] private string _pathRiverObjectId = "river";

        [Tooltip("Outlet Object ID за замовчуванням (якщо немає directional-правила).")]
        [SerializeField, MapObjectId] private string _defaultOutletObjectId = "river-outlet";

        [Tooltip("Directional-правила для вибору outlet-об'єкта залежно від напрямку у воду.")]
        [SerializeField] private DirectionalOutletRule[] _directionalOutletRules;

        public override string Title => "River To Water Outlet";
        public override string Category => "Features";

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

            int w = biomeMap.GetLength(0);
            int h = biomeMap.GetLength(1);
            var result = (string[,])objectMap.Clone();

            var waterSet = BuildWaterLikeSet(GetEffectiveWaterTileIds(context));
            var outletRules = BuildOutletRuleMap();

            var endpoints = FindRiverEndpoints(result, w, h);
            foreach (var endpoint in endpoints)
            {
                if (HasWaterNeighbor(biomeMap, endpoint.x, endpoint.y, w, h, waterSet))
                {
                    ApplyOutletAtCell(endpoint, endpoint, biomeMap, result, w, h, waterSet, outletRules);
                    continue;
                }

                if (!TryFindPathToWater(endpoint, biomeMap, result, w, h, waterSet, out var path))
                    continue;

                if (path == null || path.Count == 0)
                    continue;

                for (int i = 1; i < path.Count; i++)
                {
                    var p = path[i];
                    if (!_overwriteObjectsOnPath && !string.IsNullOrEmpty(result[p.x, p.y]) && !IsRiverObject(result[p.x, p.y]))
                        continue;

                    result[p.x, p.y] = _pathRiverObjectId;
                }

                var goal = path[path.Count - 1];
                var prev = path.Count > 1 ? path[path.Count - 2] : goal;
                ApplyOutletAtCell(goal, prev, biomeMap, result, w, h, waterSet, outletRules);
            }

            return NodeOutput.Success(result);
        }

        private List<Vector2Int> FindRiverEndpoints(string[,] objectMap, int w, int h)
        {
            var endpoints = new List<Vector2Int>();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!IsRiverObject(objectMap[x, y]))
                        continue;

                    int count = 0;
                    if (x > 0 && IsRiverObject(objectMap[x - 1, y])) count++;
                    if (x < w - 1 && IsRiverObject(objectMap[x + 1, y])) count++;
                    if (y > 0 && IsRiverObject(objectMap[x, y - 1])) count++;
                    if (y < h - 1 && IsRiverObject(objectMap[x, y + 1])) count++;

                    if (count <= 1)
                        endpoints.Add(new Vector2Int(x, y));
                }
            }

            return endpoints;
        }

        private bool TryFindPathToWater(
            Vector2Int start,
            string[,] biomeMap,
            string[,] objectMap,
            int w,
            int h,
            HashSet<string> waterSet,
            out List<Vector2Int> path)
        {
            path = null;

            var queue = new Queue<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var distance = new Dictionary<Vector2Int, int>();

            queue.Enqueue(start);
            distance[start] = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int dist = distance[current];

                if (dist > _maxSearchDistance)
                    continue;

                if (current != start && IsWaterTile(biomeMap[current.x, current.y], waterSet))
                {
                    path = ReconstructPath(cameFrom, current);
                    return true;
                }

                foreach (var next in EnumerateNeighbors(current, w, h, _allowDiagonalPath))
                {
                    if (distance.ContainsKey(next))
                        continue;

                    bool isWater = IsWaterTile(biomeMap[next.x, next.y], waterSet);
                    string obj = objectMap[next.x, next.y];
                    bool passable = isWater
                        || string.IsNullOrEmpty(obj)
                        || IsRiverObject(obj)
                        || _overwriteObjectsOnPath;
                    if (!passable)
                        continue;

                    distance[next] = dist + 1;
                    cameFrom[next] = current;
                    queue.Enqueue(next);
                }
            }

            return false;
        }

        private void ApplyOutletAtCell(
            Vector2Int outletCell,
            Vector2Int previousCell,
            string[,] biomeMap,
            string[,] objectMap,
            int w,
            int h,
            HashSet<string> waterSet,
            Dictionary<OutletDirection, string> outletRules)
        {
            if (!TryResolveOutletDirection(outletCell, previousCell, biomeMap, w, h, waterSet, out var dir))
                return;

            if (!outletRules.TryGetValue(dir, out var outletId) || string.IsNullOrWhiteSpace(outletId))
                outletId = _defaultOutletObjectId;

            if (string.IsNullOrWhiteSpace(outletId))
                outletId = _pathRiverObjectId;

            objectMap[outletCell.x, outletCell.y] = outletId;
        }

        private bool TryResolveOutletDirection(
            Vector2Int outletCell,
            Vector2Int previousCell,
            string[,] biomeMap,
            int w,
            int h,
            HashSet<string> waterSet,
            out OutletDirection direction)
        {
            var delta = outletCell - previousCell;
            if (delta == Vector2Int.up)
            {
                direction = OutletDirection.North;
                return true;
            }

            if (delta == Vector2Int.right)
            {
                direction = OutletDirection.East;
                return true;
            }

            if (delta == Vector2Int.down)
            {
                direction = OutletDirection.South;
                return true;
            }

            if (delta == Vector2Int.left)
            {
                direction = OutletDirection.West;
                return true;
            }

            return TryGetWaterDirection(outletCell, biomeMap, w, h, waterSet, out direction);
        }

        private bool TryGetWaterDirection(
            Vector2Int p,
            string[,] biomeMap,
            int w,
            int h,
            HashSet<string> waterSet,
            out OutletDirection direction)
        {
            if (p.y < h - 1 && IsWaterTile(biomeMap[p.x, p.y + 1], waterSet))
            {
                direction = OutletDirection.North;
                return true;
            }

            if (p.x < w - 1 && IsWaterTile(biomeMap[p.x + 1, p.y], waterSet))
            {
                direction = OutletDirection.East;
                return true;
            }

            if (p.y > 0 && IsWaterTile(biomeMap[p.x, p.y - 1], waterSet))
            {
                direction = OutletDirection.South;
                return true;
            }

            if (p.x > 0 && IsWaterTile(biomeMap[p.x - 1, p.y], waterSet))
            {
                direction = OutletDirection.West;
                return true;
            }

            direction = OutletDirection.North;
            return false;
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int end)
        {
            var path = new List<Vector2Int> { end };
            var current = end;

            while (cameFrom.TryGetValue(current, out var prev))
            {
                path.Add(prev);
                current = prev;
            }

            path.Reverse();
            return path;
        }

        private IEnumerable<Vector2Int> EnumerateNeighbors(Vector2Int p, int w, int h, bool allowDiagonal)
        {
            if (p.x > 0) yield return new Vector2Int(p.x - 1, p.y);
            if (p.x < w - 1) yield return new Vector2Int(p.x + 1, p.y);
            if (p.y > 0) yield return new Vector2Int(p.x, p.y - 1);
            if (p.y < h - 1) yield return new Vector2Int(p.x, p.y + 1);

            if (!allowDiagonal)
                yield break;

            if (p.x > 0 && p.y > 0) yield return new Vector2Int(p.x - 1, p.y - 1);
            if (p.x > 0 && p.y < h - 1) yield return new Vector2Int(p.x - 1, p.y + 1);
            if (p.x < w - 1 && p.y > 0) yield return new Vector2Int(p.x + 1, p.y - 1);
            if (p.x < w - 1 && p.y < h - 1) yield return new Vector2Int(p.x + 1, p.y + 1);
        }

        private bool HasWaterNeighbor(string[,] biomeMap, int x, int y, int w, int h, HashSet<string> waterSet)
        {
            if (y < h - 1 && IsWaterTile(biomeMap[x, y + 1], waterSet)) return true;
            if (x < w - 1 && IsWaterTile(biomeMap[x + 1, y], waterSet)) return true;
            if (y > 0 && IsWaterTile(biomeMap[x, y - 1], waterSet)) return true;
            if (x > 0 && IsWaterTile(biomeMap[x - 1, y], waterSet)) return true;
            return false;
        }

        private Dictionary<OutletDirection, string> BuildOutletRuleMap()
        {
            var map = new Dictionary<OutletDirection, string>();
            if (_directionalOutletRules == null)
                return map;

            foreach (var r in _directionalOutletRules)
            {
                if (string.IsNullOrWhiteSpace(r.OutletObjectId))
                    continue;

                map[r.Direction] = r.OutletObjectId.Trim();
            }

            return map;
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

                    if (_matchBaseWaterType)
                    {
                        int sep = trimmed.IndexOf(_objectSeparator);
                        if (sep > 0)
                            set.Add(trimmed.Substring(0, sep));
                    }
                }
            }

            if (set.Count == 0)
                set.Add("water");

            return set;
        }

        private bool IsWaterTile(string tileId, HashSet<string> waterSet)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            string trimmed = tileId.Trim();
            if (waterSet.Contains(trimmed))
                return true;

            if (_matchBaseWaterType)
            {
                int sep = trimmed.IndexOf(_objectSeparator);
                if (sep > 0 && waterSet.Contains(trimmed.Substring(0, sep)))
                    return true;
            }

            string lowered = trimmed.ToLowerInvariant();
            return lowered.Contains("water")
                || lowered.Contains("sea")
                || lowered.Contains("lake")
                || lowered.Contains("river");
        }

        private bool IsRiverObject(string objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId) || string.IsNullOrWhiteSpace(_riverBaseObjectId))
                return false;

            string objectTrim = objectId.Trim();
            string riverTrim = _riverBaseObjectId.Trim();

            if (!_matchBaseRiverType)
                return string.Equals(objectTrim, riverTrim, StringComparison.OrdinalIgnoreCase);

            return string.Equals(GetBaseId(objectTrim), GetBaseId(riverTrim), StringComparison.OrdinalIgnoreCase);
        }

        private string GetBaseId(string id)
        {
            int sep = id.IndexOf(_objectSeparator);
            return sep >= 0 ? id.Substring(0, sep) : id;
        }
    }
}
