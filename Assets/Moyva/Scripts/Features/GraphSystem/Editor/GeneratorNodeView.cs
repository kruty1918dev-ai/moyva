using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
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
        public bool IsRoutePoint { get; }
        private readonly bool _isPreviewHidden;
        public string HoverTooltipText { get; set; }
        public Texture2D PreviewTexture => _previewTexture;
        public string PreviewStatus => _previewLabel?.text;

        private readonly List<GeneratorPort> _inputPorts = new();
        private readonly List<GeneratorPort> _outputPorts = new();
        private readonly List<Label> _outputValueLabels = new();
        private readonly VisualElement _outputValuesContainer;
        private readonly VisualElement _previewContainer;
        private readonly Image _previewImage;
        private readonly Label _previewLabel;

        private Texture2D _previewTexture;
        private bool _ownsPreviewTexture;

        // Raw output data — використовується у Preview Window для відображення висоти при наведенні
        public float[,]  PreviewFloatMap { get; private set; }
        public string[,] PreviewTileMap  { get; private set; }

        public void SetPreviewRawMaps(float[,] floatMap, string[,] tileMap)
        {
            PreviewFloatMap = floatMap;
            PreviewTileMap  = tileMap;
        }

        public GeneratorNodeView(NodeBase nodeData)
        {
            NodeData = nodeData;
            IsRoutePoint = nodeData is RoutePointNode;
            viewDataKey = nodeData.NodeId;

            if (IsRoutePoint)
            {
                InitRoutePointView(nodeData);
                return;
            }

            title = nodeData.Title;

            _isPreviewHidden = Attribute.IsDefined(nodeData.GetType(), typeof(HidePreviewAttribute));

            var nodeInfo = Attribute.GetCustomAttribute(nodeData.GetType(), typeof(NodeInfoAttribute)) as NodeInfoAttribute;
            HoverTooltipText = BuildDetailedTooltip(nodeData, nodeInfo);
            tooltip = string.Empty;

            // Style
            AddToClassList("generator-node");
            style.width = 280;
            style.minWidth = 280;
            style.maxWidth = 280;

            // Position
            var pos = nodeData.EditorPosition;
            SetPosition(new Rect(pos.x, pos.y, 280, 0));

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

            _previewContainer = new VisualElement
            {
                style =
                {
                    marginTop = 6,
                    borderTopWidth = 1,
                    borderTopColor = new Color(0.22f, 0.22f, 0.22f),
                    paddingTop = 4
                }
            };
            _previewContainer.tooltip = "Інлайн-прев'ю: швидкий перегляд результату цієї ноди. Подвійний клік відкриє окреме вікно прев'ю.";

            _previewLabel = new Label("No preview")
            {
                style =
                {
                    fontSize = 9,
                    color = new Color(0.75f, 0.75f, 0.75f),
                    marginBottom = 3
                }
            };

            bool isOutputNode = nodeData is OutputNode;
            int previewHeight = isOutputNode ? 256 : 108;

            _previewImage = new Image
            {
                scaleMode = UnityEngine.ScaleMode.ScaleToFit,
                style =
                {
                    height = previewHeight,
                    backgroundColor = new Color(0.1f, 0.1f, 0.1f),
                    borderBottomWidth = 1,
                    borderTopWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderBottomColor = new Color(0.25f, 0.25f, 0.25f),
                    borderTopColor = new Color(0.25f, 0.25f, 0.25f),
                    borderLeftColor = new Color(0.25f, 0.25f, 0.25f),
                    borderRightColor = new Color(0.25f, 0.25f, 0.25f)
                }
            };
            _previewImage.tooltip = "Інлайн-прев'ю ноди. Double-click: відкрити окреме Preview Window. Wheel/drag доступні у окремому вікні.";

            _previewContainer.Add(_previewLabel);
            _previewContainer.Add(_previewImage);
            extensionContainer.Add(_previewContainer);

            // Container for simple output values shown under the node
            _outputValuesContainer = new VisualElement
            {
                style =
                {
                    marginTop = 6,
                    paddingLeft = 4,
                    paddingRight = 4,
                    flexDirection = FlexDirection.Column
                }
            };
            extensionContainer.Add(_outputValuesContainer);

            // Inline editable fields (marked with [InlineEditable])
            CreateInlineEditors(nodeData);

            _previewImage.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0 && evt.clickCount == 2)
                {
                    var graphView = GetFirstAncestorOfType<GeneratorGraphView>();
                    graphView?.OpenPreviewWindowForNode(this);
                    evt.StopPropagation();
                }
            });

            RegisterCallback<DetachFromPanelEvent>(_ => ReleaseOwnedPreviewTexture());
            RegisterCallback<AttachToPanelEvent>(_ => BringToFront());

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

            if (direction == PortDirection.Output)
                _outputValueLabels.Clear();

            var graphDir = direction == PortDirection.Input
                ? Direction.Input : Direction.Output;
            var capacity = direction == PortDirection.Input
                ? Port.Capacity.Single : Port.Capacity.Multi;

            for (int i = 0; i < defs.Length; i++)
            {
                var def = defs[i];
                var port = GeneratorPort.Create(def, i, graphDir, capacity);

                if (direction == PortDirection.Input)
                {
                    inputContainer.Add(port);
                }
                else
                {
                    outputContainer.Add(port);
                }

                list.Add(port);
            }
        }

        public GeneratorPort GetInputPort(int index) =>
            index >= 0 && index < _inputPorts.Count ? _inputPorts[index] : null;

        public GeneratorPort GetOutputPort(int index) =>
            index >= 0 && index < _outputPorts.Count ? _outputPorts[index] : null;

        public int OutputCount => _outputPorts.Count;

        public void SetOutputValueText(int portIndex, string text)
        {
            if (portIndex < 0 || portIndex >= _outputValueLabels.Count) return;
            _outputValueLabels[portIndex].text = string.IsNullOrEmpty(text) ? "-" : text;
        }

        public void SetOutputValues(object[] outputs)
        {
            if (_outputValuesContainer == null) return;
            _outputValuesContainer.Clear();

            if (outputs == null || outputs.Length == 0)
            {
                _outputValuesContainer.style.display = DisplayStyle.None;
                return;
            }

            _outputValuesContainer.style.display = DisplayStyle.Flex;

            for (int i = 0; i < outputs.Length; i++)
            {
                var v = outputs[i];
                string text = FormatSimpleValue(v);

                Color textColor = Color.white;
                if (i < _outputPorts.Count)
                    textColor = _outputPorts[i].portColor;

                var lbl = new Label(text)
                {
                    style =
                    {
                        color = textColor,
                        fontSize = 11,
                        unityTextAlign = TextAnchor.MiddleLeft,
                        marginTop = 2,
                        marginBottom = 2
                    }
                };

                _outputValuesContainer.Add(lbl);
            }
        }

        private static string FormatSimpleValue(object v)
        {
            if (v == null) return "-";
            if (v is string s) return s;
            if (v is int or long or short or byte) return v.ToString();
            if (v is float f) return f.ToString("0.###");
            if (v is double d) return d.ToString("0.###");
            if (v is Texture2D) return "Texture2D";
            if (v is Array a)
            {
                if (a.Rank == 1) return $"{a.GetType().GetElementType()?.Name}[{a.Length}]";
                if (a.Rank == 2) return $"{a.GetType().GetElementType()?.Name}[{a.GetLength(0)}x{a.GetLength(1)}]";
                return $"{a.GetType().GetElementType()?.Name}[{a.Length}]";
            }
            return $"<{v.GetType().Name}>";
        }

        public void SetPreview(Texture2D texture, string statusText, bool ownsTexture)
        {
            if (_previewTexture != null && _ownsPreviewTexture && _previewTexture != texture)
            {
                UnityEngine.Object.DestroyImmediate(_previewTexture);
            }

            _previewTexture = texture;
            _ownsPreviewTexture = ownsTexture;

            if (_previewImage != null)
                _previewImage.image = texture;
            if (_previewLabel != null)
                _previewLabel.text = string.IsNullOrEmpty(statusText)
                    ? (texture != null ? "Preview" : "No preview")
                    : statusText;
        }

        public void ClearPreview(string statusText = "No preview")
        {
            SetPreview(null, statusText, false);
        }

        public void SetPreviewVisible(bool visible)
        {
            if (_previewContainer == null) return;
            if (_isPreviewHidden)
            {
                _previewContainer.style.display = DisplayStyle.None;
                RefreshExpandedState();
                return;
            }

            _previewContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshExpandedState();
        }

        private void CreateInlineEditors(NodeBase nodeData)
        {
            var type = nodeData.GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            bool added = false;

            foreach (var field in fields)
            {
                var attr = Attribute.GetCustomAttribute(field, typeof(InlineEditableAttribute)) as InlineEditableAttribute;
                if (attr == null) continue;

                var so = new SerializedObject(nodeData);
                var prop = so.FindProperty(field.Name);
                if (prop == null) continue;

                var label = string.IsNullOrEmpty(attr.Label) ? ObjectNames.NicifyVariableName(field.Name) : attr.Label;
                
                var container = new IMGUIContainer(() =>
                {
                    so.Update();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(nodeData);
                    }
                })
                {
                    style = { marginTop = 4 }
                };

                extensionContainer.Add(container);
                added = true;
            }

            if (!added)
            {
                // nothing to show
            }
        }

        #region Route Point compact view

        private void InitRoutePointView(NodeBase nodeData)
        {
            title = "";
            AddToClassList("route-point-node");

            // Hide title bar and divider
            titleContainer.style.display = DisplayStyle.None;
            var divider = this.Q("divider");
            if (divider != null) divider.style.display = DisplayStyle.None;

            // Compact size
            style.width = 16;
            style.minWidth = 16;
            style.maxWidth = 16;
            style.paddingTop = 0;
            style.paddingBottom = 0;
            style.paddingLeft = 0;
            style.paddingRight = 0;
            style.marginTop = 0;
            style.marginBottom = 0;
            style.marginLeft = 0;
            style.marginRight = 0;
            style.backgroundColor = new Color(0.55f, 0.55f, 0.55f);
            style.borderTopLeftRadius = 8;
            style.borderTopRightRadius = 8;
            style.borderBottomLeftRadius = 8;
            style.borderBottomRightRadius = 8;
            style.borderTopWidth = 1;
            style.borderBottomWidth = 1;
            style.borderLeftWidth = 1;
            style.borderRightWidth = 1;
            style.borderTopColor = new Color(0.7f, 0.7f, 0.7f);
            style.borderBottomColor = new Color(0.7f, 0.7f, 0.7f);
            style.borderLeftColor = new Color(0.7f, 0.7f, 0.7f);
            style.borderRightColor = new Color(0.7f, 0.7f, 0.7f);

            // Hide input/output containers visually — ports still functional for edge rendering
            inputContainer.style.width = 0;
            inputContainer.style.maxWidth = 0;
            inputContainer.style.overflow = Overflow.Hidden;
            outputContainer.style.width = 0;
            outputContainer.style.maxWidth = 0;
            outputContainer.style.overflow = Overflow.Hidden;

            // Create minimal ports
            CreatePorts(nodeData.Inputs, PortDirection.Input, _inputPorts);
            CreatePorts(nodeData.Outputs, PortDirection.Output, _outputPorts);

            // Position
            var pos = nodeData.EditorPosition;
            SetPosition(new Rect(pos.x, pos.y, 16, 16));

            HoverTooltipText = "Route point — подвійний клік на з'єднанні додає точку згину.\nDrag — перетягнути. Delete — видалити.";
            tooltip = string.Empty;

            RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button == 0)
                    Selection.activeObject = nodeData;
            });

            RefreshExpandedState();
            RefreshPorts();
        }

        #endregion

        private void ReleaseOwnedPreviewTexture()
        {
            if (_previewTexture != null && _ownsPreviewTexture)
            {
                UnityEngine.Object.DestroyImmediate(_previewTexture);
            }

            _previewTexture = null;
            _ownsPreviewTexture = false;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendSeparator();

            evt.menu.AppendAction("Копіювати ноду як текст", _ =>
            {
                var graphView = GetFirstAncestorOfType<GeneratorGraphView>();
                if (graphView != null && graphView.CopyNodeAsText(this))
                    Debug.Log($"[Graph] Нода '{title}' скопійована як текст у буфер обміну.");
            });

            evt.menu.AppendAction("Вставити ноду з тексту", _ =>
            {
                var graphView = GetFirstAncestorOfType<GeneratorGraphView>();
                if (graphView == null) return;

                var rect = GetPosition();
                var pastePos = new Vector2(rect.x + 40f, rect.y + 40f);
                if (!graphView.PasteNodeFromText(pastePos, out var error) && !string.IsNullOrEmpty(error))
                {
                    EditorUtility.DisplayDialog("Вставка ноди", error, "OK");
                }
            });
        }

        private static string BuildDetailedTooltip(NodeBase nodeData, NodeInfoAttribute nodeInfo)
        {
            var sb = new StringBuilder(256);

            string title = !string.IsNullOrWhiteSpace(nodeInfo?.Title) ? nodeInfo.Title : nodeData.Title;
            string category = !string.IsNullOrWhiteSpace(nodeInfo?.Category) ? nodeInfo.Category : nodeData.Category;
            string description = !string.IsNullOrWhiteSpace(nodeInfo?.Description)
                ? nodeInfo.Description
                : "Опис для цієї ноди відсутній.";

            sb.AppendLine($"{title} [{category}]");
            sb.AppendLine();
            sb.AppendLine(description);
            sb.AppendLine();

            AppendPorts(sb, "Входи", nodeData.Inputs);
            sb.AppendLine();
            AppendPorts(sb, "Виходи", nodeData.Outputs);

            return sb.ToString().TrimEnd();
        }

        private static void AppendPorts(StringBuilder sb, string header, PortDefinition[] ports)
        {
            sb.AppendLine($"{header}:");
            if (ports == null || ports.Length == 0)
            {
                sb.AppendLine("  • немає");
                return;
            }

            for (int i = 0; i < ports.Length; i++)
            {
                var p = ports[i];
                sb.AppendLine($"  • [{i}] {p.Name}: {FormatTypeName(p.ValueType)}");
            }
        }

        private static string FormatTypeName(Type type)
        {
            if (type == typeof(bool[,])) return "bool[,] (булева маска)";
            if (type == typeof(float[,])) return "float[,] (карта значень)";
            if (type == typeof(string[,])) return "string[,] (карта ID/тайлів)";
            if (type == typeof(int[,])) return "int[,]";
            if (type == typeof(Texture2D)) return "Texture2D";

            if (type != null && type.IsArray)
                return type.GetElementType()?.Name + "[]";

            return type?.Name ?? "unknown";
        }
    }
}
