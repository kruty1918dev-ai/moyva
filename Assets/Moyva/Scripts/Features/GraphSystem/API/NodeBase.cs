using System;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public abstract class NodeBase : ScriptableObject
    {
        [HideInInspector, SerializeField] private string _nodeId;
        [HideInInspector, SerializeField] private Vector2 _editorPosition;
        [HideInInspector, SerializeField] private string _layerId;

        public string NodeId
        {
            get
            {
                if (string.IsNullOrEmpty(_nodeId))
                    _nodeId = Guid.NewGuid().ToString();
                return _nodeId;
            }
            set => _nodeId = value;
        }

        /// <summary>
        /// Ідентифікатор шару (<see cref="GeneratorLayerDefinition.Id"/>), до підграфа
        /// якого належить цей вузол. Порожній рядок означає глобальний/нерозподілений вузол.
        /// </summary>
        public string LayerId
        {
            get => _layerId;
            set => _layerId = value;
        }

        public Vector2 EditorPosition
        {
            get => _editorPosition;
            set => _editorPosition = value;
        }

        public abstract string Title { get; }
        public virtual string Category => "General";

        public abstract PortDefinition[] Inputs { get; }
        public abstract PortDefinition[] Outputs { get; }

        public abstract NodeOutput Execute(object[] inputs, NodeContext context);
    }
}
