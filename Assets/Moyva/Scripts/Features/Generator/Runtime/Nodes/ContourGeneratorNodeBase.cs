using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    /// <summary>
    /// Напрямок краю — вказує, з якого боку підвищення переходить у нижчий рівень.
    /// Наприклад, South означає, що клітинка є верхнім краєм і нижче неї (з півдня) — нижчий рівень.
    /// </summary>
    public enum HillDirection
    {
        [InspectorName("Південь (South)")]    South,
        [InspectorName("Північ (North)")]     North,
        [InspectorName("Схід (East)")]        East,
        [InspectorName("Захід (West)")]       West,
        [InspectorName("Кут ПН-СХ (NE)")]    CornerNE,
        [InspectorName("Кут ПН-ЗХ (NW)")]    CornerNW,
        [InspectorName("Кут ПД-СХ (SE)")]    CornerSE,
        [InspectorName("Кут ПД-ЗХ (SW)")]    CornerSW,
        [InspectorName("Внутр. кут ПН-СХ")] InnerCornerNE,
        [InspectorName("Внутр. кут ПН-ЗХ")] InnerCornerNW,
        [InspectorName("Внутр. кут ПД-СХ")] InnerCornerSE,
        [InspectorName("Внутр. кут ПД-ЗХ")] InnerCornerSW,
    }

    /// <summary>
    /// Прив'язка напрямку краю до конкретного Tile ID.
    /// </summary>
    [Serializable]
    public sealed class HillTileEntry
    {
        [Tooltip("Напрямок краю: вказує, з якого боку клітинки розміщується перехід.")]
        public HillDirection Direction;

        [TileId, Tooltip("Tile ID, що буде розміщено на краї в цьому напрямку.")]
        public string TileId;
    }

    /// <summary>
    /// Абстрактна база для нодів, що генерують контурні тайли на межах рівнів.
    /// Містить спільну логіку: виконання, класифікацію напрямків, зональну фільтрацію та виключення.
    /// Підкласи надають: джерело рівнів, набір тайлів, порти вводу/виводу та превью.
    /// </summary>
    public abstract class ContourGeneratorNodeBase : NodeBase, IPreviewableNode, ICustomEditorNode
    {
        private const string LogTag = "[MoyvaTWCHeight]";

        [Header("Zone Filter")]
        [Tooltip("Мінімальна кількість клітинок у зв'язній зоні контуру. 0 або 1 = без обмежень.")]
        [SerializeField, Min(0)] protected int _minZoneSize = 0;

        [Header("Exclusions")]
        [Tooltip("Якщо у кардинального сусіда Tile ID або базовий ключ збігається з одним із цих значень — " +
                 "правило контуру для цієї клітинки пропускається.")]
        [TileId, SerializeField] protected string[] _excludedNeighborTileTypes = Array.Empty<string>();

        [Header("Flags")]
        [Tooltip("Якщо увімкнено, нода застосовується лише до клітинок, позначених у FlagMap.")]
        [SerializeField] protected bool _applyOnlyOnFlags;

        [Tooltip("Список flag ID, що активують цю ноду. Якщо порожньо — підходить будь-який непорожній flag.")]
        [SerializeField] protected string[] _targetFlagIds = Array.Empty<string>();

        // ── Preview cache ──
        [NonSerialized] protected int[,]  _lastLevelMap;
        [NonSerialized] protected bool[,] _lastEdgeMask;
        [NonSerialized] protected HillDirection?[,] _lastDirectionMap;

        // ── Abstract interface ──

        /// <summary>
        /// Перевіряє коректність первинного входу (inputs[0]).
        /// Повертає null якщо OK, або рядок помилки.
        /// </summary>
        protected abstract string ValidatePrimaryInput(object[] inputs, int w, int h);

        /// <summary>
        /// Будує карту рівнів з вхідних даних. inputs[0] — первинний вхід, inputs[1] — TileMap.
        /// </summary>
        protected abstract int[,] BuildLevelMap(object[] inputs, int w, int h);

        /// <summary>
        /// Повертає true, якщо клітинка цього рівня є кандидатом для контурних тайлів.
        /// </summary>
        protected abstract bool IsCandidateLevel(int level);

        /// <summary>
        /// Словник напрямок → tile ID для заміни.
        /// </summary>
        protected abstract Dictionary<HillDirection, string> BuildTileLookup();

        /// <summary>
        /// Загальна кількість рівнів (для превью та кольорової мапи).
        /// </summary>
        protected abstract int TotalLevels { get; }

        // ── IPreviewableNode ──
        public abstract Texture2D GeneratePreview(int width, int height);

        // ── ICustomEditorNode ──
#if UNITY_EDITOR
        public void OpenEditorWindow()
        {
            UnityEditor.Selection.activeObject = this;
        }
#endif

        // ── Execute ──

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var tileMap = inputs.Length > 1 ? inputs[1] as string[,] : null;
            var flagMap = inputs.Length > 2 ? inputs[2] as string[,] : null;

            if (tileMap == null)
                return NodeOutput.Error("TileMap input is required.");
            if (_applyOnlyOnFlags && flagMap == null)
                return NodeOutput.Error("FlagMap input is required when flag-based mode is enabled.");

            int w = tileMap.GetLength(0);
            int h = tileMap.GetLength(1);

            string primaryError = ValidatePrimaryInput(inputs, w, h);
            if (primaryError != null)
                return NodeOutput.Error(primaryError);

            if (flagMap != null && (flagMap.GetLength(0) != w || flagMap.GetLength(1) != h))
                return NodeOutput.Error("FlagMap must have the same dimensions as TileMap.");

            var tileLookup  = BuildTileLookup();
            var targetFlags = FlagMapSelectionUtility.BuildFilterSet(_targetFlagIds);
            var excludedSet = BuildExcludedSet();

            var levelMap = BuildLevelMap(inputs, w, h);
            _lastLevelMap = levelMap;
            _lastEdgeMask = new bool[w, h];

            if (ShouldKeepBaseTileMapForAssetDriven3D(context))
            {
                _lastDirectionMap = new HillDirection?[w, h];
                Debug.Log($"{LogTag} {GetType().Name} kept base TileMap for asset-driven 3D/TWC mode. nodeId='{NodeId}', size={w}x{h}, projection={context?.ProjectionMode}, render={context?.RenderMode}, levelStats={FormatLevelStats(levelMap)}.");
                return BuildOutput((string[,])tileMap.Clone(), levelMap);
            }

            bool[,] zoneMask = null;
            if (_minZoneSize > 1)
            {
                // Попередній двопрохід без зонального фільтру — для розрахунку розмірів зон
                var preliminary = ComputeValidDirections(
                    tileMap, flagMap, levelMap, tileLookup, targetFlags, excludedSet, null, w, h);
                var candidateMask = new bool[w, h];
                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    candidateMask[x, y] = preliminary[x, y].HasValue;
                zoneMask = BuildReplacementZoneMask(candidateMask, w, h);
            }

            // Фінальний двопрохідний tile bitmasking алгоритм із зональним фільтром
            var validDirs = ComputeValidDirections(
                tileMap, flagMap, levelMap, tileLookup, targetFlags, excludedSet, zoneMask, w, h);
            _lastDirectionMap = validDirs;

            var result = (string[,])tileMap.Clone();

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!validDirs[x, y].HasValue) continue;
                if (TryResolveTileIdForCell(x, y, validDirs[x, y].Value, tileLookup, out string tileId))
                {
                    result[x, y]        = tileId;
                    _lastEdgeMask[x, y] = true;
                }
            }

            return BuildOutput(result, levelMap);
        }

        /// <summary>
        /// Формує NodeOutput. За замовчуванням повертає (TileMap, LevelMap).
        /// Підкласи можуть перевизначити (наприклад, опустити LevelMap).
        /// </summary>
        protected virtual NodeOutput BuildOutput(string[,] tileMap, int[,] levelMap)
            => NodeOutput.Success(tileMap, levelMap);

        private static string FormatLevelStats(int[,] levelMap)
        {
            if (levelMap == null)
                return "null";

            int width = levelMap.GetLength(0);
            int height = levelMap.GetLength(1);
            if (width <= 0 || height <= 0)
                return $"{width}x{height}, empty";

            int min = int.MaxValue;
            int max = int.MinValue;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = levelMap[x, y];
                    min = Mathf.Min(min, value);
                    max = Mathf.Max(max, value);
                }
            }

            return $"{width}x{height}, min={min}, max={max}, samples=(0,0:{levelMap[0, 0]}), (mid:{levelMap[width / 2, height / 2]}), (last:{levelMap[width - 1, height - 1]})";
        }

        private static bool ShouldKeepBaseTileMapForAssetDriven3D(NodeContext context)
        {
            if (context == null)
                return false;

            if (context.TryGetService<MoyvaProjectSettingsSO>(out var projectSettings)
                && projectSettings != null
                && projectSettings.Uses3DProjectMode())
            {
                return true;
            }

            return context.ProjectionMode == GridProjectionMode.Orthographic3D
                || context.ProjectionMode == GridProjectionMode.Isometric3DPreview
                || context.RenderMode == GridRenderMode.Mesh3D
                || context.RenderMode == GridRenderMode.Mesh3DPreview;
        }

        protected virtual bool TryResolveTileIdForCell(
            int x,
            int y,
            HillDirection direction,
            Dictionary<HillDirection, string> defaultLookup,
            out string tileId)
        {
            return defaultLookup.TryGetValue(direction, out tileId) && !string.IsNullOrEmpty(tileId);
        }

        // ── Shared helpers ──

        /// <summary>
        /// Двопрохідний tile bitmasking алгоритм (за принципом з tutsplus.com/how-to-use-tile-bitmasking).
        /// Прохід 1: класифікує кожну клітинку-кандидат через ClassifyEdge (геометрія сусідів).
        /// Прохід 2: анулює кутові тайли без обох сусідів-з'єднувачів — усуває ізольовані кути.
        /// </summary>
        private HillDirection?[,] ComputeValidDirections(
            string[,] tileMap,
            string[,] flagMap,
            int[,] levelMap,
            Dictionary<HillDirection, string> tileLookup,
            HashSet<string> targetFlags,
            HashSet<string> excludedSet,
            bool[,] zoneMask,
            int w, int h)
        {
            var rawDir = new HillDirection?[w, h];

            // ── Прохід 1: геометрична класифікація ──
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (_applyOnlyOnFlags && !FlagMapSelectionUtility.IsSelected(flagMap, x, y, targetFlags))
                    continue;
                if (!IsCandidateLevel(levelMap[x, y]))
                    continue;
                if (zoneMask != null && !zoneMask[x, y])
                    continue;
                if (HasExcludedNeighbor(tileMap, x, y, w, h, excludedSet))
                    continue;
                if (!TryClassifyDirection(levelMap, x, y, w, h, out var dir))
                    continue;
                if (!tileLookup.TryGetValue(dir, out string tileId) || string.IsNullOrEmpty(tileId))
                    continue;

                rawDir[x, y] = dir;
            }

            // ── Прохід 2: валідація з'єднувачів для кутових тайлів ──
            // Кут дійсний лише якщо обидва сусіди-продовжувачі теж отримають тайл.
            // Це відповідає правилу туторіалу: діагональний біт рахується лише якщо
            // обидва суміжних кардинальних сусіди присутні.
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!rawDir[x, y].HasValue) continue;
                if (!IsCornerConnected(rawDir, rawDir[x, y].Value, x, y, w, h))
                    rawDir[x, y] = null;
            }

            return rawDir;
        }

        /// <summary>
        /// Перевіряє, чи кутовий тайл з'єднаний з обома сусідами-продовжувачами.
        /// Кардинальні напрямки (North/South/East/West) завжди валідні.
        /// </summary>
        private static bool IsCornerConnected(
            HillDirection?[,] rawDir, HillDirection dir, int x, int y, int w, int h)
        {
            bool Has(int nx, int ny) =>
                nx >= 0 && nx < w && ny >= 0 && ny < h && rawDir[nx, ny].HasValue;

            return dir switch
            {
                // Зовнішні кути: продовжуються вздовж двох кардинальних ребер
                HillDirection.CornerNE => Has(x - 1, y) && Has(x,     y - 1),
                HillDirection.CornerNW => Has(x + 1, y) && Has(x,     y - 1),
                HillDirection.CornerSE => Has(x - 1, y) && Has(x,     y + 1),
                HillDirection.CornerSW => Has(x + 1, y) && Has(x,     y + 1),
                // Внутрішні кути: обидва суміжних кардинальних сусіди навколо ввігнутого кута
                HillDirection.InnerCornerNE => Has(x,     y + 1) && Has(x + 1, y),
                HillDirection.InnerCornerNW => Has(x,     y + 1) && Has(x - 1, y),
                HillDirection.InnerCornerSE => Has(x,     y - 1) && Has(x + 1, y),
                HillDirection.InnerCornerSW => Has(x,     y - 1) && Has(x - 1, y),
                // Кардинальні напрямки не потребують валідації з'єднання
                _ => true
            };
        }

        private bool[,] BuildReplacementZoneMask(bool[,] candidateMask, int w, int h)
        {
            var mask    = new bool[w, h];
            var visited = new bool[w, h];
            int[] dx    = { 0, 1, 0, -1 };
            int[] dy    = { 1, 0, -1, 0 };

            var zone  = new List<(int x, int y)>(64);
            var queue = new Queue<(int x, int y)>();

            for (int sx = 0; sx < w; sx++)
            for (int sy = 0; sy < h; sy++)
            {
                if (visited[sx, sy] || !candidateMask[sx, sy]) continue;

                zone.Clear();
                queue.Clear();
                queue.Enqueue((sx, sy));
                visited[sx, sy] = true;

                while (queue.Count > 0)
                {
                    var (cx, cy) = queue.Dequeue();
                    zone.Add((cx, cy));
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = cx + dx[d], ny = cy + dy[d];
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                        if (visited[nx, ny] || !candidateMask[nx, ny]) continue;
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }

                bool allowed = zone.Count >= _minZoneSize;
                foreach (var (zx, zy) in zone)
                    mask[zx, zy] = allowed;
            }

            return mask;
        }

        private HashSet<string> BuildExcludedSet()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (_excludedNeighborTileTypes != null)
                foreach (var t in _excludedNeighborTileTypes)
                    if (!string.IsNullOrWhiteSpace(t)) set.Add(t.Trim());
            return set;
        }

        private static bool HasExcludedNeighbor(
            string[,] tileMap, int x, int y, int w, int h,
            HashSet<string> excludedTiles)
        {
            if (excludedTiles.Count == 0) return false;

            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d], ny = y + dy[d];
                if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;

                string neighborTile = (tileMap[nx, ny] ?? "").Trim();
                string neighborBase = GetBaseTileType(neighborTile);

                foreach (var excluded in excludedTiles)
                {
                    if (neighborTile.Equals(excluded, StringComparison.OrdinalIgnoreCase) ||
                        neighborBase.Equals(excluded, StringComparison.OrdinalIgnoreCase) ||
                        neighborTile.StartsWith(excluded + "-", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        protected bool TryClassifyDirection(
            int[,] levelMap, int x, int y, int w, int h,
            out HillDirection direction)
        {
            int level = levelMap[x, y];

            bool edgeN  = IsLowerInBounds(levelMap, x,     y + 1, level, w, h);
            bool edgeE  = IsLowerInBounds(levelMap, x + 1, y,     level, w, h);
            bool edgeS  = IsLowerInBounds(levelMap, x,     y - 1, level, w, h);
            bool edgeW  = IsLowerInBounds(levelMap, x - 1, y,     level, w, h);
            bool diagNE = IsLowerInBounds(levelMap, x + 1, y + 1, level, w, h);
            bool diagSE = IsLowerInBounds(levelMap, x + 1, y - 1, level, w, h);
            bool diagSW = IsLowerInBounds(levelMap, x - 1, y - 1, level, w, h);
            bool diagNW = IsLowerInBounds(levelMap, x - 1, y + 1, level, w, h);

            var classified = ClassifyEdge(edgeN, edgeE, edgeS, edgeW, diagNE, diagSE, diagSW, diagNW);
            if (!classified.HasValue) { direction = default; return false; }
            direction = classified.Value;
            return true;
        }

        protected static bool IsLowerInBounds(int[,] levelMap, int nx, int ny, int currentLevel, int w, int h)
            => IsInBounds(nx, ny, w, h) && levelMap[nx, ny] < currentLevel;

        protected static bool IsInBounds(int x, int y, int w, int h)
            => x >= 0 && x < w && y >= 0 && y < h;

        /// <summary>
        /// Класифікує клітинку в один із 12 типів краю.
        /// Пріоритет: зовнішній кут > кардинальний край > внутрішній кут.
        /// </summary>
        protected static HillDirection? ClassifyEdge(
            bool edgeN, bool edgeE, bool edgeS, bool edgeW,
            bool diagNE, bool diagSE, bool diagSW, bool diagNW)
        {
            int cardinalCount = (edgeN ? 1 : 0) + (edgeE ? 1 : 0) + (edgeS ? 1 : 0) + (edgeW ? 1 : 0);

            // ── Зовнішні кути (два суміжних кардинальних краї) ──
            if (cardinalCount == 2)
            {
                if (edgeN && edgeE) return HillDirection.CornerNE;
                if (edgeN && edgeW) return HillDirection.CornerNW;
                if (edgeS && edgeE) return HillDirection.CornerSE;
                if (edgeS && edgeW) return HillDirection.CornerSW;
                // Протилежні кардинали (N+S або E+W) — невизначений випадок, пропускаємо
                return null;
            }

            // ── Кардинальний край (рівно один бік) ──
            if (cardinalCount == 1)
            {
                if (edgeN) return HillDirection.North;
                if (edgeE) return HillDirection.East;
                if (edgeS) return HillDirection.South;
                if (edgeW) return HillDirection.West;
            }

            // ── Внутрішній кут (жодного кардинального, рівно один діагональний) ──
            if (cardinalCount == 0)
            {
                int diagCount = (diagNE ? 1 : 0) + (diagSE ? 1 : 0) + (diagSW ? 1 : 0) + (diagNW ? 1 : 0);
                if (diagCount == 1)
                {
                    if (diagNE) return HillDirection.InnerCornerNE;
                    if (diagSE) return HillDirection.InnerCornerSE;
                    if (diagSW) return HillDirection.InnerCornerSW;
                    if (diagNW) return HillDirection.InnerCornerNW;
                }
                // Кілька діагональних або жодного — без заміни
            }

            // 3+ кардинальних країв або неоднозначний випадок — без заміни
            return null;
        }

        protected static string GetBaseTileType(string tileId)
        {
            if (string.IsNullOrEmpty(tileId)) return "";
            int idx = tileId.IndexOf('-');
            return idx > 0 ? tileId.Substring(0, idx) : tileId;
        }
    }
}
