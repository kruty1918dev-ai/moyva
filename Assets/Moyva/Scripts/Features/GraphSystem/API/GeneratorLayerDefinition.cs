using System;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Опис одного шару генератора всередині графа.
    /// Кожен шар має власний міні-підграф вузлів і компілюється в окремий
    /// blueprint-шар TileWorldCreator. Шари можуть посилатися один на одного
    /// (математичні дії над матрицями) через вузол-посилання на шар.
    /// </summary>
    [Serializable]
    public sealed class GeneratorLayerDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private string _name = "Layer";
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private int _sortingOrder;
        [SerializeField] private bool _enabled = true;
        [SerializeField] private float _defaultHeight;
        [SerializeField] private bool _useZeroLayerPadding;
        [SerializeField] private int _extraWidthCells;
        [SerializeField] private int _extraLengthCells;
        [SerializeField] private bool _generateFlatSurface;
        [SerializeField] private Material _flatSurfaceMaterial;

        /// <summary>
        /// Опціональний TilePreset/біулд-маркер для зв'язку шару з візуалом TWC.
        /// Зберігаємо як ім'я мапінгу, щоб не тягнути типи TWC у GraphSystem.
        /// </summary>
        [SerializeField] private string _buildLayerKey;

        /// <summary>
        /// Стабільний зв'язок із BlueprintLayer у companion TileWorldCreator Configuration.
        /// GraphAsset.Layers залишаються джерелом правди, а BlueprintLayer є їхньою проєкцією.
        /// </summary>
        [SerializeField] private string _blueprintLayerGuid;

        public GeneratorLayerDefinition()
        {
            _id = Guid.NewGuid().ToString();
        }

        public GeneratorLayerDefinition(string name) : this()
        {
            if (!string.IsNullOrEmpty(name))
                _name = name;
        }

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                    _id = Guid.NewGuid().ToString();
                return _id;
            }
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        public int SortingOrder
        {
            get => _sortingOrder;
            set => _sortingOrder = value;
        }

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public float DefaultHeight
        {
            get => _defaultHeight;
            set => _defaultHeight = value;
        }

        public bool UseZeroLayerPadding
        {
            get => _useZeroLayerPadding;
            set => _useZeroLayerPadding = value;
        }

        public int ExtraWidthCells
        {
            get => Mathf.Max(0, _extraWidthCells);
            set => _extraWidthCells = Mathf.Max(0, value);
        }

        public int ExtraLengthCells
        {
            get => Mathf.Max(0, _extraLengthCells);
            set => _extraLengthCells = Mathf.Max(0, value);
        }

        public bool GenerateFlatSurface
        {
            get => _generateFlatSurface;
            set => _generateFlatSurface = value;
        }

        public Material FlatSurfaceMaterial
        {
            get => _flatSurfaceMaterial;
            set => _flatSurfaceMaterial = value;
        }

        public string BuildLayerKey
        {
            get => _buildLayerKey;
            set => _buildLayerKey = value;
        }

        public string BlueprintLayerGuid
        {
            get => _blueprintLayerGuid;
            set => _blueprintLayerGuid = value;
        }
    }
}
