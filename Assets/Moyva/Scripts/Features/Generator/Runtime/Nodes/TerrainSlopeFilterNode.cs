using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Terrain Slope Filter", "Terrain",
        "Фільтрує карту висот: підіймає сусідів навколо піків, щоб різниця між сусідніми клітинками " +
        "не перевищувала заданий поріг. Значення лише збільшуються — пікові значення зберігаються, " +
        "а навколо них формуються плавні схили. Вирішує проблему ізольованих піків, " +
        "які заважають нодам пагорбів коректно генерувати контурні тайли.")]
    public sealed class TerrainSlopeFilterNode : NodeBase, IPreviewableNode
    {
        [Header("Slope Constraint")]
        [Tooltip("Максимально допустима різниця висот між двома сусідніми клітинками [0.01–1]. " +
                 "Зменшення значення створює плавніші схили, але потребує більше ітерацій. " +
                 "Рекомендація: ≈ 1 / кількість рівнів у HillGenerator.")]
        [SerializeField, Range(0.01f, 1f)] private float _maxSlopeDelta = 0.15f;

        [Tooltip("Максимальна кількість проходів алгоритму. " +
                 "Якщо карта стабілізується раніше — зупиняється достроково. " +
                 "Збільшуйте якщо є попередження про досягнення ліміту.")]
        [SerializeField, Range(1, 500)] private int _maxIterations = 64;

        [Tooltip("Якщо увімкнено — враховуються лише 4 кардинальних сусіди (N/E/S/W). " +
                 "Вимкнено — всі 8 напрямків включно з діагоналями. " +
                 "Cardinal Only дає чіткіші кути, All-8 — більш округлі схили.")]
        [SerializeField] private bool _cardinalOnly;

        // ── Preview cache (NonSerialized — не зберігається) ──
        [NonSerialized] private float[,] _lastOutput;
        [NonSerialized] private int      _lastIterationsUsed;
        [NonSerialized] private int      _lastCellsRaised;

        // ── Read-only для редактора ──
        public int LastIterationsUsed => _lastIterationsUsed;
        public int LastCellsRaised    => _lastCellsRaised;
        public int MaxIterations      => _maxIterations;

        public override string Title    => "Terrain Slope Filter";
        public override string Category => "Terrain";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("HeightMap")
        };

        // ── Execute ──

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs.Length > 0 ? inputs[0] as float[,] : null;
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            var map = (float[,])heightMap.Clone();

            int totalRaised = 0;
            int iter        = 0;

            for (; iter < _maxIterations; iter++)
            {
                int raised = _cardinalOnly
                    ? RunPassCardinal(map, w, h)
                    : RunPassAll8(map, w, h);

                totalRaised += raised;
                if (raised == 0) break;
            }

            _lastOutput         = map;
            _lastIterationsUsed = iter;
            _lastCellsRaised    = totalRaised;

            return NodeOutput.Success(map);
        }

        // ── Алгоритм: обмеження максимального схилу ──

        /// <summary>
        /// Один прохід з кардинальними сусідами (4 напрямки).
        /// Повертає кількість підняних клітинок.
        /// </summary>
        private int RunPassCardinal(float[,] map, int w, int h)
        {
            int raised = 0;

            // Прямий прохід (↗)
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float c = map[x, y];
                raised += TryRaise(map, x,     y + 1, c, w, h);
                raised += TryRaise(map, x + 1, y,     c, w, h);
            }

            // Зворотній прохід (↙) — усуває напрямкову упередженість
            for (int x = w - 1; x >= 0; x--)
            for (int y = h - 1; y >= 0; y--)
            {
                float c = map[x, y];
                raised += TryRaise(map, x,     y - 1, c, w, h);
                raised += TryRaise(map, x - 1, y,     c, w, h);
            }

            return raised;
        }

        /// <summary>
        /// Один прохід з усіма 8 сусідами.
        /// Повертає кількість підняних клітинок.
        /// </summary>
        private int RunPassAll8(float[,] map, int w, int h)
        {
            int raised = 0;

            // Прямий прохід (↗)
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float c = map[x, y];
                raised += TryRaise(map, x,     y + 1, c, w, h);
                raised += TryRaise(map, x + 1, y,     c, w, h);
                raised += TryRaise(map, x + 1, y + 1, c, w, h);
                raised += TryRaise(map, x - 1, y + 1, c, w, h);
            }

            // Зворотній прохід (↙)
            for (int x = w - 1; x >= 0; x--)
            for (int y = h - 1; y >= 0; y--)
            {
                float c = map[x, y];
                raised += TryRaise(map, x,     y - 1, c, w, h);
                raised += TryRaise(map, x - 1, y,     c, w, h);
                raised += TryRaise(map, x + 1, y - 1, c, w, h);
                raised += TryRaise(map, x - 1, y - 1, c, w, h);
            }

            return raised;
        }

        /// <summary>
        /// Якщо сусід (nx, ny) нижчий ніж (center - maxSlopeDelta) — піднімає його.
        /// Повертає 1 якщо значення було змінено, 0 інакше.
        /// </summary>
        private int TryRaise(float[,] map, int nx, int ny, float center, int w, int h)
        {
            if (nx < 0 || nx >= w || ny < 0 || ny >= h) return 0;
            float needed = center - _maxSlopeDelta;
            if (map[nx, ny] >= needed) return 0;
            map[nx, ny] = needed;
            return 1;
        }

        // ── IPreviewableNode ──

        public Texture2D GeneratePreview(int width, int height)
        {
            if (_lastOutput == null) return null;

            int sw = _lastOutput.GetLength(0);
            int sh = _lastOutput.GetLength(1);
            int tw = Mathf.Clamp(width,  32, 256);
            int th = Mathf.Clamp(height, 32, 256);

            var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            for (int x = 0; x < tw; x++)
            {
                int sx = x * sw / tw;
                for (int y = 0; y < th; y++)
                {
                    int   sy = y * sh / th;
                    float v  = Mathf.Clamp01(_lastOutput[sx, sy]);
                    tex.SetPixel(x, y, HeightToColor(v));
                }
            }

            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Градієнт кольорів для відображення висоти: вода → трава → гори → сніг.
        /// </summary>
        internal static Color HeightToColor(float v)
        {
            if (v < 0.25f) return Color.Lerp(new Color(0.05f, 0.10f, 0.35f), new Color(0.10f, 0.35f, 0.55f), v / 0.25f);
            if (v < 0.50f) return Color.Lerp(new Color(0.20f, 0.50f, 0.20f), new Color(0.50f, 0.68f, 0.28f), (v - 0.25f) / 0.25f);
            if (v < 0.75f) return Color.Lerp(new Color(0.55f, 0.50f, 0.30f), new Color(0.68f, 0.58f, 0.42f), (v - 0.50f) / 0.25f);
            return          Color.Lerp(new Color(0.78f, 0.78f, 0.78f), Color.white, (v - 0.75f) / 0.25f);
        }
    }
}
