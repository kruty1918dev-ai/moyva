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

        /// <summary>
        /// Опціональний TilePreset/біулд-маркер для зв'язку шару з візуалом TWC.
        /// Зберігаємо як ім'я мапінгу, щоб не тягнути типи TWC у GraphSystem.
        /// </summary>
        [SerializeField] private string _buildLayerKey;

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

        public string BuildLayerKey
        {
            get => _buildLayerKey;
            set => _buildLayerKey = value;
        }
    }
}
