using System.Collections.Generic;
using System.Reflection;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Attributes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.Twc
{
    /// <summary>
    /// Вузол-обгортка над TileWorldCreator-модифікатором (генератором або модифікатором).
    /// Зберігає інстанс <see cref="BlueprintModifier"/> з його серіалізованими параметрами
    /// та делегує генерацію матриці безпосередньо логіці TileWorldCreator.
    ///
    /// Генератори (Category.Generators) не мають вхідного порту й створюють матрицю з нуля.
    /// Модифікатори (Category.Modifiers) приймають вхідну матрицю та трансформують її.
    /// </summary>
    public sealed class TwcModifierNode : NodeBase
    {
        [SerializeField] private string _modifierTypeName;
        [SerializeField] private BlueprintModifier _modifier;
    #if UNITY_EDITOR
        [System.NonSerialized] private Configuration _editorInspectorConfiguration;
    #endif

        /// <summary>Інстанс TWC-модифікатора. Його серіалізовані поля редагуються в інспекторі.</summary>
        public BlueprintModifier Modifier => _modifier;
        public ScriptableObject ModifierAsset => _modifier;
        public string ModifierTypeName => _modifierTypeName;

        public bool IsGenerator =>
            TwcModifierCatalog.TryGet(_modifierTypeName, out var entry) && entry.IsGenerator;

        public override string Title
        {
            get
            {
                if (TwcModifierCatalog.TryGet(_modifierTypeName, out var entry))
                    return entry.DisplayName;
                return _modifier != null ? _modifier.GetType().Name : "TWC Modifier";
            }
        }

        public override string Category =>
            IsGenerator ? "TileWorldCreator/Generators" : "TileWorldCreator/Modifiers";

        public override PortDefinition[] Inputs =>
            IsGenerator
                ? System.Array.Empty<PortDefinition>()
                : new[] { PortDefinition.Input<bool[,]>("Source") };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            EnsureModifierInstance();
            if (_modifier == null)
                return NodeOutput.Error($"TWC-модифікатор '{_modifierTypeName}' не ініціалізовано.");

            int width = Mathf.Max(1, context?.MapSize.x ?? 0);
            int height = Mathf.Max(1, context?.MapSize.y ?? 0);
            if (width <= 1 && height <= 1)
            {
                width = 50;
                height = 50;
            }

            uint seed = NonZeroSeed(context);

            var config = ScriptableObject.CreateInstance<Configuration>();
            var layer = ScriptableObject.CreateInstance<BlueprintLayer>();
            try
            {
                config.width = width;
                config.height = height;
                config.useGlobalRandomSeed = true;
                config.globalRandomSeed = (int)seed;
                config.currentRandomSeed = seed;

                InitializeLayerRandom(layer, seed);

                var positions = ToPositions(inputs, width, height);

                _modifier.asset = config;
                _modifier.isEnabled = true;
                var result = _modifier.Execute(positions, layer);
                if (result == null)
                    result = positions;

                var mask = ToMask(result, width, height);
                return NodeOutput.Success(mask);
            }
            catch (System.Exception ex)
            {
                return NodeOutput.Error($"Помилка виконання TWC-модифікатора '{Title}': {ex.Message}");
            }
            finally
            {
                Object.DestroyImmediate(layer);
                Object.DestroyImmediate(config);
            }
        }

        private static uint NonZeroSeed(NodeContext context)
        {
            // Unity.Mathematics.Random не приймає 0 як seed.
            long raw = context?.Seed ?? 1;
            uint seed = unchecked((uint)raw);
            return seed == 0u ? 1u : seed;
        }

        private static void InitializeLayerRandom(BlueprintLayer layer, uint seed)
        {
            if (layer == null)
                return;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var layerType = layer.GetType();

            // TWC layer has a Unity.Mathematics.Random field/property named "random".
            // We initialize it via reflection to avoid a hard asmdef dependency on Unity.Mathematics.
            var randomField = layerType.GetField("random", flags);
            if (randomField != null)
            {
                object randomValue = CreateRandomValue(randomField.FieldType, seed);
                if (randomValue != null)
                    randomField.SetValue(layer, randomValue);
                return;
            }

            var randomProperty = layerType.GetProperty("random", flags);
            if (randomProperty != null && randomProperty.CanWrite)
            {
                object randomValue = CreateRandomValue(randomProperty.PropertyType, seed);
                if (randomValue != null)
                    randomProperty.SetValue(layer, randomValue, null);
            }
        }

        private static object CreateRandomValue(System.Type randomType, uint seed)
        {
            if (randomType == null)
                return null;

            try
            {
                return System.Activator.CreateInstance(randomType, new object[] { seed });
            }
            catch
            {
                return null;
            }
        }

        private void EnsureModifierInstance()
        {
            if (_modifier != null || string.IsNullOrWhiteSpace(_modifierTypeName))
                return;

            var modifierType = TwcModifierCatalog.ResolveType(_modifierTypeName);
            if (modifierType == null || !typeof(BlueprintModifier).IsAssignableFrom(modifierType))
                return;

            _modifier = CreateInstance(modifierType) as BlueprintModifier;
            if (_modifier == null)
                return;

            _modifier.name = modifierType.Name;
            _modifier.hideFlags = HideFlags.HideInHierarchy;

#if UNITY_EDITOR
            var owningAsset = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(owningAsset))
                UnityEditor.AssetDatabase.AddObjectToAsset(_modifier, this);
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private static HashSet<Vector2> ToPositions(object[] inputs, int width, int height)
        {
            var positions = new HashSet<Vector2>();
            if (inputs == null || inputs.Length == 0 || inputs[0] is not bool[,] source)
                return positions;

            int w = Mathf.Min(width, source.GetLength(0));
            int h = Mathf.Min(height, source.GetLength(1));
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (source[x, y])
                        positions.Add(new Vector2(x, y));
                }
            }
            return positions;
        }

        private static bool[,] ToMask(HashSet<Vector2> positions, int width, int height)
        {
            var mask = new bool[width, height];
            foreach (var pos in positions)
            {
                int x = Mathf.RoundToInt(pos.x);
                int y = Mathf.RoundToInt(pos.y);
                if (x >= 0 && x < width && y >= 0 && y < height)
                    mask[x, y] = true;
            }
            return mask;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Налаштовує вузол на конкретний тип TWC-модифікатора, створюючи його інстанс
        /// як під-ассет цього вузла. Викликається з меню створення вузлів.
        /// </summary>
        public void ConfigureModifier(System.Type modifierType)
        {
            if (modifierType == null || !typeof(BlueprintModifier).IsAssignableFrom(modifierType))
                return;

            _modifierTypeName = modifierType.FullName;
            _modifier = CreateInstance(modifierType) as BlueprintModifier;
            if (_modifier == null)
                return;

            _modifier.name = modifierType.Name;
            _modifier.hideFlags = HideFlags.HideInHierarchy;

            var owningAsset = UnityEditor.AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(owningAsset))
            {
                UnityEditor.AssetDatabase.AddObjectToAsset(_modifier, this);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        public bool TryRestoreModifierInEditor()
        {
            if (_modifier != null)
                return true;

            EnsureModifierInstance();
            return _modifier != null;
        }

        public VisualElement CreateModifierInspectorElement(Vector2Int mapSize)
        {
            EnsureModifierInstance();
            if (_modifier == null)
                return null;

            var configuration = GetOrCreateEditorInspectorConfiguration(mapSize);
            _modifier.asset = configuration;
            return _modifier.BuildInspector(configuration);
        }

        private Configuration GetOrCreateEditorInspectorConfiguration(Vector2Int mapSize)
        {
            if (_editorInspectorConfiguration == null)
            {
                _editorInspectorConfiguration = CreateInstance<Configuration>();
                _editorInspectorConfiguration.name = "TWC Node Inspector Configuration";
                _editorInspectorConfiguration.hideFlags = HideFlags.HideAndDontSave;
            }

            _editorInspectorConfiguration.width = Mathf.Max(1, mapSize.x);
            _editorInspectorConfiguration.height = Mathf.Max(1, mapSize.y);
            _editorInspectorConfiguration.useGlobalRandomSeed = true;
            _editorInspectorConfiguration.globalRandomSeed = 1;
            _editorInspectorConfiguration.currentRandomSeed = 1;
            return _editorInspectorConfiguration;
        }
#endif
    }
}
