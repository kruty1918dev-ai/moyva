using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Noise Object Cluster", "Features", "Розставляє об'єкти за шумом щільності, висотою та необов'язковою маскою. Один нод відповідає за один тип/групу об'єктів.")]
    public sealed class ForestClusterNode : NodeBase, IPreviewableNode
    {
        [Serializable]
        public sealed class SpawnObjectChanceEntry
        {
            [MapObjectId, Tooltip("ID об'єкта для спавну.")]
            public string ObjectId;

            [Tooltip("Відносний шанс вибору цього об'єкта.")]
            [Min(0f)]
            public float Chance = 1f;

            [Tooltip("Дозволяє тимчасово виключити об'єкт без видалення.")]
            public bool Enabled = true;
        }

        private enum PreviewMode
        {
            Density = 0,
            ClusterMask = 1,
            ObjectProbability = 2,
            ObjectPlacement = 3,
            RejectionReasons = 4,
            InputMask = 5
        }

        [Header("Noise")]
        [Tooltip("Масштаб шуму щільності. Менше значення дає більші масиви, більше — дрібніші плями.")]
        [SerializeField, Min(0.0001f)] private float _densityScale = 0.08f;
        [Tooltip("Кількість октав шуму. Більше октав = детальніша та " +
                 "рвана структура лісових масивів.")]
        [SerializeField, Range(1, 8)] private int _densityOctaves = 3;
        [Tooltip("Коефіцієнт збільшення частоти між октавами шуму.")]
        [SerializeField, Min(1f)] private float _densityLacunarity = 2f;
        [Tooltip("Коефіцієнт спадання амплітуди між октавами шуму.")]
        [SerializeField, Range(0.01f, 1f)] private float _densityPersistence = 0.5f;
        [Tooltip("Зсув шумового патерну по X. Дозволяє змінювати композицію без зміни seed.")]
        [SerializeField] private float _densityOffsetX;
        [Tooltip("Зсув шумового патерну по Y. Дозволяє змінювати композицію без зміни seed.")]
        [SerializeField] private float _densityOffsetY;

        [Header("Cluster Settings")]
        [Tooltip("Мінімальна щільність шуму, після якої клітинка вважається придатною для спавну. Підвищення параметра дає рідші, але виразніші кластери.")]
        [SerializeField, Range(0f, 1f)] private float _densityThreshold = 0.45f;
        [Tooltip("Мінімальна висота для спавну. Нижчі клітинки будуть виключені, навіть якщо шум щільності високий.")]
        [SerializeField, Range(0f, 1f)] private float _minHeight = 0.25f;
        [Tooltip("Максимальна висота для спавну.")]
        [SerializeField, Range(0f, 1f)] private float _maxHeight = 0.65f;
        [Tooltip("Об'єкти, які може поставити нода, з індивідуальними шансами вибору.")]
        [SerializeField] private SpawnObjectChanceEntry[] _spawnObjects = Array.Empty<SpawnObjectChanceEntry>();

        [Tooltip("Старий список ID об'єктів, залишений тільки для безпечної десеріалізації старих графів.")]
        [SerializeField, HideInInspector, MapObjectId] private string[] _treeObjects = { "tree-oak", "tree-pine", "tree-birch" };

        [Tooltip("Додатковий множник покриття кластерами. <1 рідшає спавн, >1 ущільнює його на всій мапі.")]
        [FormerlySerializedAs("_forestCoverageMultiplier")]
        [SerializeField, Range(0.2f, 2f)] private float _clusterCoverageMultiplier = 1f;

        [Header("Object Clustering")]
        [Tooltip("Мінімальна кількість сусідніх придатних клітин (8-напрямків), щоб у клітині взагалі можна було ставити об'єкт. Чим вище значення, тим компактніші кластери.")]
        [FormerlySerializedAs("_minForestNeighborsForTree")]
        [SerializeField, Range(0, 8)] private int _minClusterNeighborsForObject = 3;

        [Tooltip("Степінь підсилення ймовірності об'єкта у щільних ділянках. >1 концентрує спавн у ядрах кластерів, <1 робить покриття рівномірнішим.")]
        [FormerlySerializedAs("_treeDensityExponent")]
        [SerializeField, Range(0.5f, 4f)] private float _objectDensityExponent = 1.8f;

        [Tooltip("Глобальний множник ймовірності постановки об'єкта у придатних клітинках.")]
        [FormerlySerializedAs("_treeChanceMultiplier")]
        [SerializeField, Range(0f, 2f)] private float _objectChanceMultiplier = 1f;

        [Header("Preview")]
        [Tooltip("Режим візуалізації впливу ноди в preview." )]
        [SerializeField] private PreviewMode _previewMode = PreviewMode.ObjectPlacement;

        // Legacy поле, залишене для безпечної десеріалізації старих графів.
        [SerializeField, HideInInspector] private UnityEngine.Object _densityNoise;

        [NonSerialized] private float[,] _lastDensityMap;
        [NonSerialized] private bool[,] _lastClusterCandidates;
        [NonSerialized] private bool[,] _lastRejectedByHeight;
        [NonSerialized] private bool[,] _lastRejectedByMask;
        [NonSerialized] private bool[,] _lastRejectedByDensity;
        [NonSerialized] private float[,] _lastObjectProbability;
        [NonSerialized] private bool[,] _lastObjectPlacement;
        [NonSerialized] private bool[,] _lastInputMask;
        [NonSerialized] private bool[,] _lastObjectMask;

        public override string Title => "Noise Object Cluster";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("TileMap"),
            PortDefinition.Input<bool[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TileMap"),
            PortDefinition.Output<string[,]>("ObjectMap"),
            PortDefinition.Output<bool[,]>("ObjectMask")
        };

        private void OnValidate()
        {
            if (_spawnObjects == null || _spawnObjects.Length == 0)
                _spawnObjects = BuildLegacyObjectEntries();
        }

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            var tileMap = inputs[1] as string[,];
            if (heightMap == null || tileMap == null)
                return NodeOutput.Error("HeightMap and TileMap inputs are required.");

            // Маска означає заборону спавну: true = НЕ спавнити дерева в цій клітинці.
            var inputMask = inputs[2] as bool[,];

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            if (inputMask != null && (inputMask.GetLength(0) != w || inputMask.GetLength(1) != h))
                return NodeOutput.Error("Mask must have the same dimensions as HeightMap/TileMap.");

            if (_maxHeight < _minHeight)
                (_minHeight, _maxHeight) = (_maxHeight, _minHeight);

            // Шум керується тільки inline-параметрами ноди, але seed завжди з налаштувань графа.
            float[,] densityMap = GenerateSimpleNoise(
                w,
                h,
                context.Seed,
                _densityScale,
                _densityOctaves,
                _densityLacunarity,
                _densityPersistence,
                _densityOffsetX,
                _densityOffsetY);

            var resultTileMap = (string[,])tileMap.Clone();
            var objectMap = new string[w, h];
            var objectMask = new bool[w, h];
            var effectiveDensityMap = new float[w, h];
            var clusterCandidates = new bool[w, h];
            var rejectedByHeight = new bool[w, h];
            var rejectedByMask = new bool[w, h];
            var rejectedByDensity = new bool[w, h];
            var densityAtCell = new float[w, h];
            var objectProbabilityMap = new float[w, h];
            var objectPlacementMap = new bool[w, h];

            var availableObjects = BuildConfiguredObjectEntries();

            // Pass 1: визначаємо придатні клітини кластера (тільки геометрія; тайли не змінюємо).
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float height = heightMap[x, y];
                    float density = Mathf.Clamp01(densityMap[x, y] * _clusterCoverageMultiplier);
                    effectiveDensityMap[x, y] = density;

                    // Skip if outside valid height range
                    if (height < _minHeight || height > _maxHeight)
                    {
                        rejectedByHeight[x, y] = true;
                        continue;
                    }

                    if (density < _densityThreshold)
                    {
                        rejectedByDensity[x, y] = true;
                        continue;
                    }

                    clusterCandidates[x, y] = true;
                    densityAtCell[x, y] = density;
                }
            }

            if (availableObjects.Length == 0)
            {
                _lastDensityMap = effectiveDensityMap;
                _lastClusterCandidates = clusterCandidates;
                _lastRejectedByHeight = rejectedByHeight;
                _lastRejectedByMask = rejectedByMask;
                _lastRejectedByDensity = rejectedByDensity;
                _lastObjectProbability = objectProbabilityMap;
                _lastObjectPlacement = objectPlacementMap;
                _lastInputMask = inputMask;
                _lastObjectMask = objectMask;
                return NodeOutput.Success(resultTileMap, objectMap, objectMask);
            }

            // Pass 2: ставимо об'єкти тільки в достатньо щільних кластерах.
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!clusterCandidates[x, y])
                        continue;

                    // Exclusion mask: true означає "тут не можна спавнити об'єкти".
                    if (inputMask != null && inputMask[x, y])
                    {
                        rejectedByMask[x, y] = true;
                        continue;
                    }

                    int neighborCount = CountClusterNeighbors(clusterCandidates, x, y, w, h);
                    if (neighborCount < _minClusterNeighborsForObject)
                        continue;

                    float normalized = Mathf.InverseLerp(_densityThreshold, 1f, densityAtCell[x, y]);
                    float objectProbability = Mathf.Clamp01(Mathf.Pow(normalized, _objectDensityExponent) * _objectChanceMultiplier);
                    objectProbabilityMap[x, y] = objectProbability;

                    if (SampleObjectPlacementChance(context.Seed, x, y) < objectProbability)
                    {
                        int objectIndex = PickWeightedObjectIndex(context.Seed, x, y, availableObjects);
                        if (objectIndex < 0)
                            continue;

                        objectMap[x, y] = availableObjects[objectIndex].ObjectId;
                        objectPlacementMap[x, y] = true;
                        objectMask[x, y] = true;
                    }
                }
            }

            _lastDensityMap = effectiveDensityMap;
            _lastClusterCandidates = clusterCandidates;
            _lastRejectedByHeight = rejectedByHeight;
            _lastRejectedByMask = rejectedByMask;
            _lastRejectedByDensity = rejectedByDensity;
            _lastObjectProbability = objectProbabilityMap;
            _lastObjectPlacement = objectPlacementMap;
            _lastInputMask = inputMask;
            _lastObjectMask = objectMask;

            return NodeOutput.Success(resultTileMap, objectMap, objectMask);
        }

        private SpawnObjectChanceEntry[] BuildConfiguredObjectEntries()
        {
            var entries = BuildSanitizedObjectEntries(_spawnObjects);
            return entries.Length > 0 ? entries : BuildLegacyObjectEntries();
        }

        private SpawnObjectChanceEntry[] BuildLegacyObjectEntries()
        {
            if (_treeObjects == null || _treeObjects.Length == 0)
                return Array.Empty<SpawnObjectChanceEntry>();

            var list = new SpawnObjectChanceEntry[_treeObjects.Length];
            int count = 0;
            for (int i = 0; i < _treeObjects.Length; i++)
            {
                string id = _treeObjects[i];
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                list[count++] = new SpawnObjectChanceEntry
                {
                    ObjectId = id.Trim(),
                    Chance = 1f,
                    Enabled = true
                };
            }

            if (count == 0)
                return Array.Empty<SpawnObjectChanceEntry>();

            if (count == list.Length)
                return list;

            var trimmed = new SpawnObjectChanceEntry[count];
            Array.Copy(list, trimmed, count);
            return trimmed;
        }

        private static SpawnObjectChanceEntry[] BuildSanitizedObjectEntries(SpawnObjectChanceEntry[] source)
        {
            if (source == null || source.Length == 0)
                return Array.Empty<SpawnObjectChanceEntry>();

            var temp = new SpawnObjectChanceEntry[source.Length];
            int count = 0;
            for (int i = 0; i < source.Length; i++)
            {
                var entry = source[i];
                if (entry == null || !entry.Enabled)
                    continue;
                if (string.IsNullOrWhiteSpace(entry.ObjectId) || entry.Chance <= 0f)
                    continue;

                temp[count++] = new SpawnObjectChanceEntry
                {
                    ObjectId = entry.ObjectId.Trim(),
                    Chance = Mathf.Max(0.0001f, entry.Chance),
                    Enabled = true
                };
            }

            if (count == 0)
                return Array.Empty<SpawnObjectChanceEntry>();
            if (count == temp.Length)
                return temp;

            var trimmed = new SpawnObjectChanceEntry[count];
            Array.Copy(temp, trimmed, count);
            return trimmed;
        }

        private static int PickWeightedObjectIndex(int seed, int x, int y, SpawnObjectChanceEntry[] objects)
        {
            if (objects == null || objects.Length == 0)
                return -1;

            float total = 0f;
            for (int i = 0; i < objects.Length; i++)
                total += Mathf.Max(0f, objects[i].Chance);

            if (total <= 0f)
                return -1;

            uint hash = HashCell(seed, x, y, 0xD1B54A35u);
            float sample = ((hash & 0x00FFFFFFu) / 16777215f) * total;
            float cursor = 0f;
            for (int i = 0; i < objects.Length; i++)
            {
                cursor += Mathf.Max(0f, objects[i].Chance);
                if (sample <= cursor)
                    return i;
            }

            return objects.Length - 1;
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            int tw = Mathf.Max(16, width);
            int th = Mathf.Max(16, height);
            var tex = new Texture2D(tw, th, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            if (_lastDensityMap == null)
            {
                // Якщо нода ще не виконувалась, показуємо попередній noise-патерн із seed=0.
                var previewNoise = GenerateSimpleNoise(
                    tw,
                    th,
                    0,
                    _densityScale,
                    _densityOctaves,
                    _densityLacunarity,
                    _densityPersistence,
                    _densityOffsetX,
                    _densityOffsetY);

                for (int x = 0; x < tw; x++)
                for (int y = 0; y < th; y++)
                {
                    float v = previewNoise[x, y];
                    tex.SetPixel(x, y, DensityHeatColor(v));
                }

                tex.Apply();
                return tex;
            }

            int sw = _lastDensityMap.GetLength(0);
            int sh = _lastDensityMap.GetLength(1);
            PreviewMode effectiveMode = ResolveEffectivePreviewMode();
            for (int x = 0; x < tw; x++)
            {
                int sx = x * sw / tw;
                for (int y = 0; y < th; y++)
                {
                    int sy = y * sh / th;
                    tex.SetPixel(x, y, PreviewColorAt(sx, sy, effectiveMode));
                }
            }

            tex.Apply();
            return tex;
        }

        private PreviewMode ResolveEffectivePreviewMode()
        {
            switch (_previewMode)
            {
                case PreviewMode.ObjectPlacement:
                    return HasAny(_lastObjectPlacement) ? PreviewMode.ObjectPlacement : PreviewMode.ClusterMask;
                case PreviewMode.ObjectProbability:
                    return HasAnyPositive(_lastObjectProbability) ? PreviewMode.ObjectProbability : PreviewMode.ClusterMask;
                case PreviewMode.RejectionReasons:
                    return HasAny(_lastClusterCandidates) || HasAny(_lastRejectedByHeight) || HasAny(_lastRejectedByMask) || HasAny(_lastRejectedByDensity)
                        ? PreviewMode.RejectionReasons
                        : PreviewMode.Density;
                case PreviewMode.InputMask:
                    return _lastInputMask != null ? PreviewMode.InputMask : PreviewMode.ObjectPlacement;
                default:
                    return _previewMode;
            }
        }

        private Color PreviewColorAt(int x, int y, PreviewMode mode)
        {
            switch (mode)
            {
                case PreviewMode.Density:
                {
                    float v = _lastDensityMap != null ? Mathf.Clamp01(_lastDensityMap[x, y]) : 0f;
                    return DensityHeatColor(v);
                }
                case PreviewMode.ClusterMask:
                {
                    float d = _lastDensityMap != null ? Mathf.Clamp01(_lastDensityMap[x, y]) : 0f;
                    if (_lastClusterCandidates == null || !_lastClusterCandidates[x, y])
                    {
                        Color baseColor = Color.Lerp(new Color(0.08f, 0.10f, 0.16f, 1f), new Color(0.10f, 0.30f, 0.22f, 1f), d);
                        return baseColor;
                    }

                    Color sparse = new Color(0.29f, 0.56f, 0.24f, 1f);
                    Color dense = new Color(0.09f, 0.30f, 0.11f, 1f);
                    return Color.Lerp(sparse, dense, Mathf.InverseLerp(_densityThreshold, 1f, d));
                }
                case PreviewMode.RejectionReasons:
                {
                    if (_lastClusterCandidates != null && _lastClusterCandidates[x, y])
                        return new Color(0.12f, 0.62f, 0.22f, 1f);
                    if (_lastRejectedByHeight != null && _lastRejectedByHeight[x, y])
                        return new Color(0.55f, 0.20f, 0.78f, 1f);
                    if (_lastRejectedByMask != null && _lastRejectedByMask[x, y])
                        return new Color(0.16f, 0.42f, 0.90f, 1f);
                    if (_lastRejectedByDensity != null && _lastRejectedByDensity[x, y])
                        return new Color(0.55f, 0.45f, 0.12f, 1f);
                    return new Color(0.08f, 0.08f, 0.08f, 1f);
                }
                case PreviewMode.ObjectProbability:
                {
                    float p = _lastObjectProbability != null ? Mathf.Clamp01(_lastObjectProbability[x, y]) : 0f;
                    return Color.Lerp(new Color(0.10f, 0.17f, 0.35f, 1f), new Color(0.88f, 0.72f, 0.16f, 1f), p);
                }
                case PreviewMode.ObjectPlacement:
                {
                    bool spawned = _lastObjectPlacement != null && _lastObjectPlacement[x, y];
                    bool candidate = _lastClusterCandidates != null && _lastClusterCandidates[x, y];
                    if (spawned) return new Color(0.92f, 0.90f, 0.33f, 1f);
                    if (candidate) return new Color(0.18f, 0.43f, 0.18f, 1f);
                    return new Color(0.10f, 0.10f, 0.10f, 1f);
                }
                case PreviewMode.InputMask:
                {
                    bool blocked = _lastInputMask != null && _lastInputMask[x, y];
                    return blocked ? new Color(0.92f, 0.30f, 0.24f, 1f) : new Color(0.12f, 0.12f, 0.12f, 1f);
                }
                default:
                    return Color.black;
            }
        }

        private static Color DensityHeatColor(float v)
        {
            v = Mathf.Clamp01(v);
            if (v < 0.33f)
                return Color.Lerp(new Color(0.06f, 0.08f, 0.18f, 1f), new Color(0.10f, 0.42f, 0.38f, 1f), v / 0.33f);
            if (v < 0.66f)
                return Color.Lerp(new Color(0.10f, 0.42f, 0.38f, 1f), new Color(0.48f, 0.72f, 0.20f, 1f), (v - 0.33f) / 0.33f);
            return Color.Lerp(new Color(0.48f, 0.72f, 0.20f, 1f), new Color(0.96f, 0.82f, 0.22f, 1f), (v - 0.66f) / 0.34f);
        }

        private static bool HasAny(bool[,] map)
        {
            if (map == null)
                return false;

            int w = map.GetLength(0);
            int h = map.GetLength(1);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (map[x, y])
                    return true;

            return false;
        }

        private static bool HasAnyPositive(float[,] map)
        {
            if (map == null)
                return false;

            int w = map.GetLength(0);
            int h = map.GetLength(1);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (map[x, y] > 0.001f)
                    return true;

            return false;
        }

        private static int CountClusterNeighbors(bool[,] clusterCandidates, int x, int y, int w, int h)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        continue;

                    if (clusterCandidates[nx, ny])
                        count++;
                }
            }

            return count;
        }

        private static float SampleObjectPlacementChance(int seed, int x, int y)
        {
            uint hash = HashCell(seed, x, y, 0x51ED270Bu);
            return (hash & 0x00FFFFFFu) / 16777215f;
        }

        public void RandomizeClusterSettings()
        {
            int seed = GlobalSeed.Combine(GlobalSeed.Current, GlobalSeed.StableHash("NoiseObjectCluster.RandomizeClusterSettings"));
            var rng = new System.Random(seed);
            _densityThreshold = Mathf.Lerp(0.25f, 0.65f, (float)rng.NextDouble());
            _minClusterNeighborsForObject = rng.Next(1, 6);
            _objectDensityExponent = Mathf.Lerp(1f, 2.8f, (float)rng.NextDouble());
            _objectChanceMultiplier = Mathf.Lerp(0.65f, 1.45f, (float)rng.NextDouble());

            RandomizeAllObjectChances();
        }

        public void RandomizeAllObjectChances()
        {
            int seed = GlobalSeed.Combine(GlobalSeed.Current, GlobalSeed.StableHash("NoiseObjectCluster.RandomizeAllObjectChances"));
            var rng = new System.Random(seed);
            if (_spawnObjects == null)
                return;

            for (int i = 0; i < _spawnObjects.Length; i++)
            {
                var entry = _spawnObjects[i];
                if (entry == null)
                    continue;

                entry.Chance = Mathf.Lerp(0.1f, 1.6f, (float)rng.NextDouble());
            }

            NormalizeAllObjectChances();
        }

        public bool RandomizeSingleObjectChance(int objectIndex)
        {
            if (_spawnObjects == null || objectIndex < 0 || objectIndex >= _spawnObjects.Length)
                return false;

            var entry = _spawnObjects[objectIndex];
            if (entry == null)
                return false;

            int seed = GlobalSeed.Combine(
                GlobalSeed.Current,
                GlobalSeed.StableHash($"NoiseObjectCluster.RandomizeSingleObjectChance.{objectIndex}"));
            var rng = new System.Random(seed);
            entry.Chance = Mathf.Lerp(0.1f, 1.6f, (float)rng.NextDouble());
            NormalizeAllObjectChances();
            return true;
        }

        public void NormalizeAllObjectChances()
        {
            NormalizeObjectChances(_spawnObjects);
        }

        private static void NormalizeObjectChances(SpawnObjectChanceEntry[] entries)
        {
            if (entries == null || entries.Length == 0)
                return;

            float sum = 0f;
            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry == null || !entry.Enabled || string.IsNullOrWhiteSpace(entry.ObjectId))
                    continue;
                sum += Mathf.Max(0f, entry.Chance);
            }

            if (sum <= 0f)
            {
                int validCount = 0;
                for (int i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];
                    if (entry == null || !entry.Enabled || string.IsNullOrWhiteSpace(entry.ObjectId))
                        continue;
                    validCount++;
                }

                if (validCount == 0)
                    return;

                float flat = 1f / validCount;
                for (int i = 0; i < entries.Length; i++)
                {
                    var entry = entries[i];
                    if (entry == null || !entry.Enabled || string.IsNullOrWhiteSpace(entry.ObjectId))
                        continue;
                    entry.Chance = flat;
                }
                return;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                if (entry == null || !entry.Enabled || string.IsNullOrWhiteSpace(entry.ObjectId))
                    continue;
                entry.Chance = Mathf.Max(0f, entry.Chance) / sum;
            }
        }

        private static uint HashCell(int seed, int x, int y, uint salt)
        {
            unchecked
            {
                uint hash = (uint)seed ^ salt;
                hash ^= (uint)x * 0x9E3779B9u;
                hash = (hash << 6) | (hash >> 26);
                hash ^= (uint)y * 0x85EBCA6Bu;
                hash ^= hash >> 15;
                hash *= 0x2C1B3C6Du;
                hash ^= hash >> 12;
                hash *= 0x297A2D39u;
                hash ^= hash >> 15;
                return hash;
            }
        }

        private static float[,] GenerateSimpleNoise(
            int w,
            int h,
            int seed,
            float scale,
            int octaves,
            float lacunarity,
            float persistence,
            float offsetXExtra,
            float offsetYExtra)
        {
            var result = new float[w, h];
            float offsetX = seed * 0.7f + offsetXExtra;
            float offsetY = seed * 1.3f + offsetYExtra;
            scale = Mathf.Max(0.0001f, scale);
            octaves = Mathf.Max(1, octaves);
            lacunarity = Mathf.Max(1f, lacunarity);
            persistence = Mathf.Clamp01(persistence);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float value = 0f;
                    float amplitudeSum = 0f;

                    for (int octave = 0; octave < octaves; octave++)
                    {
                        float nx = x * scale * frequency + offsetX;
                        float ny = y * scale * frequency + offsetY;
                        value += Mathf.PerlinNoise(nx, ny) * amplitude;
                        amplitudeSum += amplitude;
                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    result[x, y] = amplitudeSum > 0f ? Mathf.Clamp01(value / amplitudeSum) : 0f;
                }
            }
            return result;
        }
    }
}
