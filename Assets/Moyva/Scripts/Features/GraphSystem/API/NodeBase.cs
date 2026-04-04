using System;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.API
{
    public abstract class NodeBase : ScriptableObject
    {
        [HideInInspector, SerializeField] private string _nodeId;
        [HideInInspector, SerializeField] private Vector2 _editorPosition;

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
