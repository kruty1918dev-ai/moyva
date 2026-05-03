using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("River Line Mask", "Terrain",
        "Прокладає лінію річки від точки A до точки B за мінімальним опором шуму. " +
        "Повертає модифікований шум із врізаним руслом.")]
    public sealed class RiverLineMaskNode : NodeBase
    {
        private const float MinTransitionCells = 1f;

        // ── Serialized fields ────────────────────────────────────────────────

        [Header("Точка А (початок)")]
        [Tooltip("Нормалізована X-координата початку річки (0 — лівий край, 1 — правий).")]
        [SerializeField, Range(0f, 1f)] private float _startX = 0.2f;
        [Tooltip("Нормалізована Y-координата початку річки (0 — верх, 1 — низ).")]
        [SerializeField, Range(0f, 1f)] private float _startY = 0.2f;

        [Header("Точка Б (кінець)")]
        [Tooltip("Нормалізована X-координата кінця річки (0 — лівий край, 1 — правий).")]
        [SerializeField, Range(0f, 1f)] private float _endX = 0.8f;
        [Tooltip("Нормалізована Y-координата кінця річки (0 — верх, 1 — низ).")]
        [SerializeField, Range(0f, 1f)] private float _endY = 0.8f;

        [Header("Поведінка шляху")]
        [Tooltip("Вага опору шуму. 0 — ігнорує значення карти; 20 — активно обходить 'гори'.")]
        [SerializeField, Range(0f, 20f)] private float _resistanceScale = 8f;
        [Tooltip("Сила притягування до прямої A→Б. " +
                 "0 — лінія повністю хаотична; 10 — майже пряма лінія.")]
        [SerializeField, Range(0f, 10f)] private float _straightness = 1f;
        [Tooltip("Кількість випадкового відхилення (меандрування). " +
                 "0 — детермінований шлях; 5 — сильне блукання. " +
                 "Для відтворюваності вкажіть Зерно.")]
        [SerializeField, Range(0f, 5f)] private float _wandering = 1f;
        [Tooltip("Зерно генератора випадковості. Однакове зерно = однаковий шлях.")]
        [SerializeField] private int _seed = 0;
        // сонечко я тебе люблю <3
        [Header("Форма русла")]
        [Tooltip("Зовнішній радіус русла у клітинах карти.")]
        [SerializeField, Range(0.5f, 12f)] private float _lineWidth = 2f;
        [Tooltip("Частка ширини, що є твердим ядром (de повна сила riverValue). " +
                 "Решта — плавне згасання до берегів.")]
        [SerializeField, Range(0f, 1f)] private float _coreFraction = 0.4f;
        [Tooltip("Цільове значення ядра русла (0 = мінімум шуму, 1 = максимум).")]
        [SerializeField, Range(0f, 1f)] private float _riverValue = 0f;
        [Tooltip("М'якість переходу між ядром і берегами. " +
                 "0 — різке пласке дно; 1 — плавна куполоподібна крива без плато. " +
                 "Збільшуйте, якщо виріз виглядає надто різко.")]
        [SerializeField, Range(0f, 1f)] private float _edgeSoftness = 0f;
        [Tooltip("Загальна сила ефекту (0 — немає ефекту, 1 — повна заміна).")]
        [SerializeField, Range(0f, 1f)] private float _strength = 1f;

        // ── NodeBase overrides ───────────────────────────────────────────────

        public override string Title => "River Line Mask";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var sourceMap = inputs.Length > 0 ? inputs[0] as float[,] : null;
            if (sourceMap == null)
                return NodeOutput.Error("Вхід Mask є обов'язковим.");

            int width = sourceMap.GetLength(0);
            int height = sourceMap.GetLength(1);
            if (width <= 0 || height <= 0)
                return NodeOutput.Error("Розмір Mask некоректний.");

            int startCol = Mathf.Clamp(Mathf.RoundToInt(_startX * (width - 1)), 0, width - 1);
            int startRow = Mathf.Clamp(Mathf.RoundToInt(_startY * (height - 1)), 0, height - 1);
            int endCol = Mathf.Clamp(Mathf.RoundToInt(_endX * (width - 1)), 0, width - 1);
            int endRow = Mathf.Clamp(Mathf.RoundToInt(_endY * (height - 1)), 0, height - 1);

            var wander = BuildWanderMap(width, height);

            var path = FindPath(sourceMap, wander, width, height, startCol, startRow, endCol, endRow);
            if (path == null || path.Count < 2)
                return NodeOutput.Warning(
                    "[RiverLineMaskNode] Не вдалося знайти шлях між A і Б.",
                    (float[,])sourceMap.Clone());

            return NodeOutput.Success(ApplyRiver(sourceMap, path, width, height));
        }

        // ── Wander map ───────────────────────────────────────────────────────

        // Заздалегідь будуємо карту випадкового шуму для відтворюваного меандрування.
        private float[,] BuildWanderMap(int width, int height)
        {
            var map = new float[width, height];
            if (_wandering <= 0f) return map;

            var rng = new System.Random(_seed);
            for (int c = 0; c < width; c++)
                for (int r = 0; r < height; r++)
                    map[c, r] = (float)rng.NextDouble();
            return map;
        }

        // ── Pathfinding ──────────────────────────────────────────────────────

        // Dijkstra. Вартість кроку = stepDist * (
        //   1
        //   + noise[nc,nr] * resistanceScale          -- опір шуму
        //   + dirPenalty   * straightness              -- штраф за відхилення від A→Б
        //   + wanderNoise  * wandering                 -- випадкове блукання
        // )
        // dirPenalty ∈ [0..2]: 0 = рух точно до цілі, 2 = рух у зворотному напрямку.
        private List<Vector2Int> FindPath(
            float[,] map, float[,] wander, int width, int height,
            int startCol, int startRow, int endCol, int endRow)
        {
            int total = width * height;
            var dist = new float[total];
            var prev = new int[total];
            var visited = new bool[total];

            for (int i = 0; i < total; i++) { dist[i] = float.MaxValue; prev[i] = -1; }

            int startIdx = ToIndex(startCol, startRow, width);
            int endIdx = ToIndex(endCol, endRow, width);
            dist[startIdx] = 0f;

            float dirX = endCol - startCol;
            float dirY = endRow - startRow;
            float dirLen = Mathf.Sqrt(dirX * dirX + dirY * dirY);
            if (dirLen > 0f) { dirX /= dirLen; dirY /= dirLen; }

            var heap = new MinHeap();
            heap.Push(startIdx, 0f);

            int[] dcol = { 0, 1, 0, -1, 1, 1, -1, -1 };
            int[] drow = { -1, 0, 1, 0, -1, 1, 1, -1 };
            float[] stepDists = { 1f, 1f, 1f, 1f, 1.41421f, 1.41421f, 1.41421f, 1.41421f };

            while (heap.Count > 0)
            {
                var (idx, cost) = heap.Pop();
                if (visited[idx]) continue;
                visited[idx] = true;
                if (idx == endIdx) break;

                int col = idx % width;
                int row = idx / width;

                for (int s = 0; s < 8; s++)
                {
                    int nc = col + dcol[s];
                    int nr = row + drow[s];
                    if (nc < 0 || nc >= width || nr < 0 || nr >= height) continue;

                    int nIdx = ToIndex(nc, nr, width);
                    if (visited[nIdx]) continue;

                    float sd = stepDists[s];

                    float resistance = Mathf.Clamp01(map[nc, nr]);
                    float moveDot = (dcol[s] / sd) * dirX + (drow[s] / sd) * dirY;
                    float dirPenalty = 1f - moveDot;                    // [0..2]
                    float wanderCost = wander[nc, nr];                  // [0..1]

                    float stepCost = sd * (1f
                        + resistance * _resistanceScale
                        + dirPenalty * _straightness
                        + wanderCost * _wandering);

                    float newCost = cost + stepCost;
                    if (newCost < dist[nIdx])
                    {
                        dist[nIdx] = newCost;
                        prev[nIdx] = idx;
                        heap.Push(nIdx, newCost);
                    }
                }
            }

            if (prev[endIdx] < 0 && endIdx != startIdx)
                return null;

            var path = new List<Vector2Int>();
            int cur = endIdx;
            while (cur >= 0)
            {
                path.Add(new Vector2Int(cur % width, cur / width));
                if (cur == startIdx) break;
                cur = prev[cur];
            }
            path.Reverse();
            return path;
        }

        // ── Painting ─────────────────────────────────────────────────────────

        private float[,] ApplyRiver(float[,] sourceMap, List<Vector2Int> path, int width, int height)
        {
            var result = (float[,])sourceMap.Clone();

            float radius = Mathf.Max(0.5f, _lineWidth);
            float coreRadius = radius * Mathf.Clamp01(_coreFraction);
            float effectiveRadius = Mathf.Max(radius, coreRadius + MinTransitionCells);
            int paintR = Mathf.CeilToInt(effectiveRadius);

            var influence = new float[width, height];
            foreach (var pt in path)
            {
                for (int r = pt.y - paintR; r <= pt.y + paintR; r++)
                {
                    if (r < 0 || r >= height) continue;
                    for (int c = pt.x - paintR; c <= pt.x + paintR; c++)
                    {
                        if (c < 0 || c >= width) continue;
                        float dc = c - pt.x;
                        float dr = r - pt.y;
                        float dst = Mathf.Sqrt(dc * dc + dr * dr);
                        float val = Falloff(dst, coreRadius, effectiveRadius);
                        if (val > influence[c, r])
                            influence[c, r] = val;
                    }
                }
            }

            float riverVal = Mathf.Clamp01(_riverValue);
            float strength = Mathf.Clamp01(_strength);
            for (int c = 0; c < width; c++)
            {
                for (int r = 0; r < height; r++)
                {
                    float inf = influence[c, r] * strength;
                    if (inf <= 0f) continue;

                    // Незмінне правило: річка не може піднімати карту вище початкового значення.
                    float sourceValue = sourceMap[c, r];
                    float clampedRiverValue = Mathf.Min(riverVal, sourceValue);
                    result[c, r] = Mathf.Lerp(sourceValue, clampedRiverValue, inf);
                }
            }

            return result;
        }

        private float Falloff(float dist, float coreRadius, float outerRadius)
        {
            if (dist >= outerRadius) return 0f;

            // Профіль із пласким ядром: плато + SmoothStep до берегів
            float sharpVal;
            if (dist <= coreRadius)
                sharpVal = 1f;
            else
                sharpVal = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(outerRadius, coreRadius, dist));

            if (_edgeSoftness <= 0f) return sharpVal;

            // Куполоподібний профіль: косинусна крива — без плаского дна
            float t = dist / outerRadius;                          // 0..1
            float smoothVal = (Mathf.Cos(Mathf.PI * t) + 1f) * 0.5f; // 1 в центрі, 0 на краю

            return Mathf.Lerp(sharpVal, smoothVal, _edgeSoftness);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static int ToIndex(int col, int row, int width) => row * width + col;

        // ── MinHeap ──────────────────────────────────────────────────────────

        private sealed class MinHeap
        {
            private readonly List<(int idx, float cost)> _data = new List<(int, float)>();

            public int Count => _data.Count;

            public void Push(int idx, float cost)
            {
                _data.Add((idx, cost));
                SiftUp(_data.Count - 1);
            }

            public (int idx, float cost) Pop()
            {
                var top = _data[0];
                int last = _data.Count - 1;
                _data[0] = _data[last];
                _data.RemoveAt(last);
                if (_data.Count > 0) SiftDown(0);
                return top;
            }

            private void SiftUp(int i)
            {
                while (i > 0)
                {
                    int p = (i - 1) / 2;
                    if (_data[p].cost <= _data[i].cost) break;
                    var tmp = _data[p]; _data[p] = _data[i]; _data[i] = tmp;
                    i = p;
                }
            }

            private void SiftDown(int i)
            {
                while (true)
                {
                    int l = 2 * i + 1;
                    int r = 2 * i + 2;
                    int m = i;
                    if (l < _data.Count && _data[l].cost < _data[m].cost) m = l;
                    if (r < _data.Count && _data[r].cost < _data[m].cost) m = r;
                    if (m == i) break;
                    var tmp = _data[m]; _data[m] = _data[i]; _data[i] = tmp;
                    i = m;
                }
            }
        }
    }
}
