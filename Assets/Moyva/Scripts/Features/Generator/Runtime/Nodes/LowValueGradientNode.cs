using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Low Value Gradient", "Noise",
        "Знаходить западини з низькими значеннями шуму та будує плавний градієнт: " +
        "від мінімального значення (мінімальна глибина) в центрі западини до висоти " +
        "сусідньої нормальної ділянки на краю. Усуває різкі чорні провали.")]
    public sealed class LowValueGradientNode : NodeBase
    {
        [Header("Пошук западин")]
        [Tooltip("Поріг: значення нижче цього вважаються низькою западиною.")]
        [SerializeField, Range(0f, 1f)] private float _lowThreshold = 0.15f;
        [Tooltip("Плавність межі виявлення. 0 = жорстка межа, більше = поступово розширює низьку зону.")]
        [SerializeField, Range(0f, 0.2f)] private float _thresholdFeather = 0.05f;

        [Header("Градієнт")]
        [Tooltip("Радіус зони схилу від краю западини до її центру. " +
                 "Більше значення = ширший плавний схил.")]
        [SerializeField, Range(1f, 32f)] private float _gradientRadius = 8f;
        [Tooltip("Мінімальне значення в найглибших точках западини. " +
                 "Пікселі, що виходять за межі градієнта, піднімаються до цього рівня. " +
                 "Збільшуйте, якщо центр западини залишається надто темним.")]
        [SerializeField, Range(0f, 0.5f)] private float _minDepth = 0.05f;
        [Tooltip("Загальна сила ефекту. 0 = без змін, 1 = повне застосування градієнту.")]
        [SerializeField, Range(0f, 1f)] private float _strength = 1f;

        public override string Title    => "Low Value Gradient";
        public override string Category => "Noise";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Noise")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Smoothed Noise")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs[0] as float[,];
            if (source == null)
                return NodeOutput.Error("Вхід Noise є обов'язковим.");

            int width  = source.GetLength(0);
            int height = source.GetLength(1);
            if (width <= 0 || height <= 0)
                return NodeOutput.Error("Розмір Noise некоректний.");

            // ── 1. Визначаємо низькі пікселі ─────────────────────────────────
            float feather = Mathf.Max(0f, _thresholdFeather);
            float threshLower = Mathf.Clamp01(_lowThreshold - feather);
            float threshUpper = Mathf.Clamp01(_lowThreshold + feather);

            var isLow = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float v = Mathf.Clamp01(source[x, y]);
                    // SmoothStep(upper, lower, v): 1 для дуже низьких, 0 для нормальних
                    float mask = Mathf.SmoothStep(threshUpper, threshLower, v);
                    isLow[x, y] = mask > 0.5f;
                }
            }

            // ── 2. Dijkstra від НЕ-низьких пікселів всередину ────────────────
            // Для кожного низького пікселя знаходимо:
            //   distMap[x,y]      = мінімальна відстань до найближчого не-низького пікселя
            //   borderValue[x,y]  = значення того найближчого не-низького пікселя
            // Використовуємо MinHeap щоб коректно обробляти різні ваги кроків.

            var distMap     = new float[width, height];
            var borderValue = new float[width, height];
            var heap        = new MinHeap();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!isLow[x, y])
                    {
                        distMap[x, y]     = 0f;
                        borderValue[x, y] = Mathf.Clamp01(source[x, y]);
                        heap.Push(x * height + y, 0f);
                    }
                    else
                    {
                        distMap[x, y] = float.MaxValue;
                    }
                }
            }

            int[]   dx        = {  0, 1,  0, -1,  1,  1, -1, -1 };
            int[]   dy        = {  1, 0, -1,  0,  1, -1, -1,  1 };
            float[] stepCosts = {  1f, 1f, 1f, 1f, 1.41421f, 1.41421f, 1.41421f, 1.41421f };

            while (heap.Count > 0)
            {
                var (idx, curDist) = heap.Pop();
                int cx = idx / height;
                int cy = idx % height;

                if (curDist > distMap[cx, cy]) continue; // застарілий запис

                float curBv = borderValue[cx, cy];

                for (int s = 0; s < 8; s++)
                {
                    int nx = cx + dx[s];
                    int ny = cy + dy[s];
                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                    float nd = curDist + stepCosts[s];
                    if (nd < distMap[nx, ny])
                    {
                        distMap[nx, ny]     = nd;
                        borderValue[nx, ny] = curBv;
                        heap.Push(nx * height + ny, nd);
                    }
                }
            }

            // ── 3. Будуємо результат ──────────────────────────────────────────
            // Для кожного низького пікселя:
            //   edgeFactor = 1 біля краю (d≈0), 0 в глибокому центрі (d≥radius)
            //   target = Lerp(minDepth, borderValue, edgeFactor)
            //     → на краю: піднімаємо до висоти сусідньої ділянки
            //     → в центрі: піднімаємо до minDepth (мінімальний поріг)
            // Ніколи не знижуємо значення нижче оригіналу.

            var result  = new float[width, height];
            float radius   = Mathf.Max(1f, _gradientRadius);
            float strength = Mathf.Clamp01(_strength);
            float minDepth = Mathf.Clamp01(_minDepth);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float src = Mathf.Clamp01(source[x, y]);

                    if (!isLow[x, y])
                    {
                        result[x, y] = src;
                        continue;
                    }

                    float d  = distMap[x, y];
                    float bv = borderValue[x, y];

                    // 1 на краю, 0 в центрі (або за межами radіus)
                    float edgeFactor = 1f - Mathf.SmoothStep(0f, radius, d);

                    // Цільове значення: від minDepth (центр) до bv (край)
                    float target = Mathf.Lerp(minDepth, bv, edgeFactor);

                    // Застосовуємо лише підняття, ніколи не знижуємо
                    float raised = Mathf.Max(target, src);
                    result[x, y] = Mathf.Lerp(src, raised, strength);
                }
            }

            return NodeOutput.Success(result);
        }

        // ── MinHeap ───────────────────────────────────────────────────────────

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
                var top  = _data[0];
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
