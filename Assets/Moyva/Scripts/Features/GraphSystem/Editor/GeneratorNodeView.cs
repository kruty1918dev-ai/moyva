using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    public sealed class GeneratorNodeView : Node
    {
        public NodeBase NodeData { get; }

        private readonly List<GeneratorPort> _inputPorts = new();
        private readonly List<GeneratorPort> _outputPorts = new();

        public GeneratorNodeView(NodeBase nodeData)
        {
            NodeData = nodeData;
            title = nodeData.Title;
            viewDataKey = nodeData.NodeId;

            // Style
            AddToClassList("generator-node");

            // Position
            var pos = nodeData.EditorPosition;
            SetPosition(new Rect(pos.x, pos.y, 220, 0));

            // Category badge
            var badge = new Label(nodeData.Category)
            {
                style =
                {
                    fontSize = 9,
                    color = new Color(0.7f, 0.7f, 0.7f),
                    unityTextAlign = TextAnchor.MiddleCenter,
                    marginBottom = 4
                }
            };
            titleContainer.Add(badge);

            // Ports
            CreatePorts(nodeData.Inputs, PortDirection.Input, _inputPorts);
            CreatePorts(nodeData.Outputs, PortDirection.Output, _outputPorts);

            // Custom editor button
            if (nodeData is ICustomEditorNode)
            {
                var btn = new Button(() =>
                {
#if UNITY_EDITOR
                    (nodeData as ICustomEditorNode)?.OpenEditorWindow();
#endif
                })
                { text = "Open Editor" };
                btn.style.marginTop = 4;
                extensionContainer.Add(btn);
                RefreshExpandedState();
            }

            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                    Selection.activeObject = nodeData;
            });
        }

        private void CreatePorts(PortDefinition[] defs, PortDirection direction,
            List<GeneratorPort> list)
        {
            if (defs == null) return;

            var graphDir = direction == PortDirection.Input
                ? Direction.Input : Direction.Output;
            var capacity = direction == PortDirection.Input
                ? Port.Capacity.Single : Port.Capacity.Multi;

            for (int i = 0; i < defs.Length; i++)
            {
                var def = defs[i];
                var port = GeneratorPort.Create(def, i, graphDir, capacity);

                if (direction == PortDirection.Input)
                    inputContainer.Add(port);
                else
                    outputContainer.Add(port);

                list.Add(port);
            }
        }

        public GeneratorPort GetInputPort(int index) =>
            index >= 0 && index < _inputPorts.Count ? _inputPorts[index] : null;

        public GeneratorPort GetOutputPort(int index) =>
            index >= 0 && index < _outputPorts.Count ? _outputPorts[index] : null;
    }
}
