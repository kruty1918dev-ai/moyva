using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Road Generator", "Cities", "Генерує дороги між містами, уникаючи гір, з м'якими кривими Безьє та типами доріг.")]
    public sealed class RoadGeneratorNode : NodeBase
    {
        [SerializeField, Range(0f, 1f)] private float _mountainThreshold = 0.78f;
        [SerializeField, Range(0f, 12f)] private float _slopePenalty = 4f;
        [SerializeField, Range(0f, 50f)] private float _mountainPenalty = 25f;
        [SerializeField, Range(1, 6)] private int _roadHalfWidth = 1;
        [SerializeField, Range(6, 80)] private int _curveSamples = 24;
        [SerializeField, Range(0f, 1f)] private float _bezierSmoothness = 0.4f;

        public override string Title => "Road Generator";
        public override string Category => "Cities";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("CityMask"),
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("RoadMask"),
            PortDefinition.Output<string[,]>("RoadTypeMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var cityMask = inputs[0] as bool[,];
            var height = inputs[1] as float[,];
            if (cityMask == null || height == null)
                return NodeOutput.Error("CityMask and HeightMap are required.");

            int w = cityMask.GetLength(0);
            int h = cityMask.GetLength(1);
            var roadMask = new bool[w, h];
            var roadTypeMap = new string[w, h];
            var centers = ExtractCityCenters(cityMask);
            if (centers.Count < 2)
                return NodeOutput.Warning("Need at least two cities to build roads.", roadMask, roadTypeMap);

            var linked = new HashSet<int> { 0 };
            while (linked.Count < centers.Count)
            {
                float best = float.MaxValue;
                int from = -1;
                int to = -1;

                foreach (var i in linked)
                {
                    for (int j = 0; j < centers.Count; j++)
                    {
                        if (linked.Contains(j)) continue;
                        float d = (centers[i] - centers[j]).sqrMagnitude;
                        if (d < best)
                        {
                            best = d;
                            from = i;
                            to = j;
                        }
                    }
                }

                if (from < 0 || to < 0) break;

                var path = FindPathAStar(centers[from], centers[to], height, context);
                if (path != null && path.Count >= 2)
                    DrawSmoothedRoad(path, height, roadMask, roadTypeMap, context);

                linked.Add(to);
            }

            return NodeOutput.Success(roadMask, roadTypeMap);
        }

        private List<Vector2Int> ExtractCityCenters(bool[,] cityMask)
        {
            int w = cityMask.GetLength(0);
            int h = cityMask.GetLength(1);
            var visited = new bool[w, h];
            var centers = new List<Vector2Int>();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!cityMask[x, y] || visited[x, y]) continue;

                    var queue = new Queue<Vector2Int>();
                    queue.Enqueue(new Vector2Int(x, y));
                    visited[x, y] = true;

                    int sumX = 0;
                    int sumY = 0;
                    int count = 0;

                    while (queue.Count > 0)
                    {
                        var c = queue.Dequeue();
                        sumX += c.x;
                        sumY += c.y;
                        count++;

                        for (int ox = -1; ox <= 1; ox++)
                        {
                            for (int oy = -1; oy <= 1; oy++)
                            {
                                if (ox == 0 && oy == 0) continue;
                                int nx = c.x + ox;
                                int ny = c.y + oy;
                                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                                if (visited[nx, ny] || !cityMask[nx, ny]) continue;
                                visited[nx, ny] = true;
                                queue.Enqueue(new Vector2Int(nx, ny));
                            }
                        }
                    }

                    centers.Add(new Vector2Int(sumX / Mathf.Max(1, count), sumY / Mathf.Max(1, count)));
                }
            }

            return centers;
        }

        private List<Vector2Int> FindPathAStar(Vector2Int start, Vector2Int goal, float[,] height, NodeContext context)
        {
            int w = height.GetLength(0);
            int h = height.GetLength(1);

            var gScore = new Dictionary<Vector2Int, float> { [start] = 0f };
            var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, goal) };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var open = new List<Vector2Int> { start };
            var closed = new HashSet<Vector2Int>();

            while (open.Count > 0)
            {
                int bestIndex = 0;
                float best = float.MaxValue;
                for (int i = 0; i < open.Count; i++)
                {
                    var key = open[i];
                    float fs = fScore.TryGetValue(key, out var v) ? v : float.MaxValue;
                    if (fs < best)
                    {
                        best = fs;
                        bestIndex = i;
                    }
                }

                var current = open[bestIndex];
                open.RemoveAt(bestIndex);
                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                closed.Add(current);

                foreach (var n in EnumerateNeighbors(current, w, h))
                {
                    if (closed.Contains(n))
                        continue;

                    float cost = SegmentCost(current, n, height);
                    if (cost >= 100000f)
                        continue;

                    float tentative = gScore[current] + cost;
                    if (!gScore.TryGetValue(n, out var gOld) || tentative < gOld)
                    {
                        cameFrom[n] = current;
                        gScore[n] = tentative;
                        fScore[n] = tentative + Heuristic(n, goal);
                        if (!open.Contains(n))
                            open.Add(n);
                    }
                }

                context.CountIteration();
            }

            return null;
        }

        private float SegmentCost(Vector2Int from, Vector2Int to, float[,] height)
        {
            float hFrom = height[from.x, from.y];
            float hTo = height[to.x, to.y];
            float slope = Mathf.Abs(hTo - hFrom);

            float baseCost = (from.x != to.x && from.y != to.y) ? 1.4142f : 1f;
            float cost = baseCost + slope * _slopePenalty;

            if (hTo > _mountainThreshold)
                cost += _mountainPenalty * (1f + (hTo - _mountainThreshold) * 6f);

            if (hTo >= 0.98f)
                return 100000f;

            return cost;
        }

        private static float Heuristic(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return dx + dy;
        }

        private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };
            while (cameFrom.TryGetValue(current, out var prev))
            {
                current = prev;
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        private static IEnumerable<Vector2Int> EnumerateNeighbors(Vector2Int c, int w, int h)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    if (ox == 0 && oy == 0) continue;
                    int nx = c.x + ox;
                    int ny = c.y + oy;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    yield return new Vector2Int(nx, ny);
                }
            }
        }

        private void DrawSmoothedRoad(List<Vector2Int> path, float[,] height,
            bool[,] roadMask, string[,] roadTypeMap, NodeContext context)
        {
            int w = roadMask.GetLength(0);
            int h = roadMask.GetLength(1);

            var points = new List<Vector2>(path.Count);
            for (int i = 0; i < path.Count; i++)
                points.Add(path[i]);

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 prev = points[i - 1];
                Vector2 cur = points[i];
                Vector2 next = points[i + 1];
                Vector2 avg = (prev + cur + next) / 3f;
                points[i] = Vector2.Lerp(cur, avg, _bezierSmoothness);
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2 start = points[i];
                Vector2 end = points[i + 1];

                Vector2 dir = (end - start);
                Vector2 perp = dir.sqrMagnitude > 0.0001f
                    ? new Vector2(-dir.normalized.y, dir.normalized.x)
                    : Vector2.zero;
                float bend = Mathf.Clamp(dir.magnitude * 0.4f, 0f, 2f);
                Vector2 control = (start + end) * 0.5f + perp * bend;

                for (int s = 0; s <= _curveSamples; s++)
                {
                    float t = s / (float)_curveSamples;
                    Vector2 p = Bezier(start, control, end, t);
                    int cx = Mathf.Clamp(Mathf.RoundToInt(p.x), 0, w - 1);
                    int cy = Mathf.Clamp(Mathf.RoundToInt(p.y), 0, h - 1);

                    for (int ox = -_roadHalfWidth; ox <= _roadHalfWidth; ox++)
                    {
                        for (int oy = -_roadHalfWidth; oy <= _roadHalfWidth; oy++)
                        {
                            int nx = cx + ox;
                            int ny = cy + oy;
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                            if (height[nx, ny] >= 0.98f) continue;

                            roadMask[nx, ny] = true;
                            roadTypeMap[nx, ny] = ResolveRoadType(height, nx, ny);
                            context.CountIteration();
                        }
                    }
                }
            }
        }

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }

        private static string ResolveRoadType(float[,] height, int x, int y)
        {
            int w = height.GetLength(0);
            int h = height.GetLength(1);
            float center = height[x, y];

            float maxDelta = 0f;
            for (int ox = -1; ox <= 1; ox++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    if (ox == 0 && oy == 0) continue;
                    int nx = Mathf.Clamp(x + ox, 0, w - 1);
                    int ny = Mathf.Clamp(y + oy, 0, h - 1);
                    maxDelta = Mathf.Max(maxDelta, Mathf.Abs(center - height[nx, ny]));
                }
            }

            if (maxDelta > 0.12f || center > 0.68f) return "stone";
            if (center < 0.45f) return "grass";
            return "dirt";
        }
    }
}
