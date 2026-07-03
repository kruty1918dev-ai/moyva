using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    internal sealed class GraphPresetApplyResult
    {
        public string PresetName;
        public string LayerId;
        public string GraphId;
        public GeneratorLayerDefinition Layer;
        public readonly List<NodeBase> CreatedNodes = new();
        public readonly List<Connection> CreatedConnections = new();
        public readonly List<string> Warnings = new();
        public GraphValidationReport ValidationReport;
        public bool Success;
        public string Message;

        public bool Changed => CreatedNodes.Count > 0 || CreatedConnections.Count > 0 || Layer != null;
    }

    internal static class GraphBuiltInPresetUtility
    {
        private const string DefaultAssetFolder = "Assets/Moyva/Generated/GraphPresetDefaults";
        private const string DecorShaderName = "Moyva/3D/Decor Shared Stylized";

        internal static GraphPresetApplyResult AddLayerPreset(
            GraphAsset graph,
            string layerName,
            Color color)
        {
            var result = BeginResult(layerName);
            if (graph == null)
                return Fail(result, "Graph asset не задано.");

            var layer = graph.AddLayer(layerName);
            result.Layer = layer;
            result.LayerId = layer.Id;
            layer.Color = color;
            layer.SortingOrder = graph.Layers
                .Where(existing => existing != null && existing != layer)
                .Select(existing => existing.SortingOrder)
                .DefaultIfEmpty(-1)
                .Max() + 1;
            layer.DefaultHeight = graph.Layers
                .Where(existing => existing != null && existing != layer)
                .Select(existing => existing.DefaultHeight)
                .DefaultIfEmpty(0f)
                .Max() + 0.1f;

            switch (NormalizePresetName(layerName))
            {
                case "shoreline":
                    AddShorelineObjectPipeline(graph, layer.Id, result);
                    break;
                case "grasssmall":
                    AddObjectPipeline(graph, layer.Id, result, "Grass Small", typeof(PlacementMaskNode), typeof(ClusterScatterNode), false, PresetObjectKind.Grass);
                    break;
                case "bush":
                    AddObjectPipeline(graph, layer.Id, result, "Bush", typeof(PlacementMaskNode), typeof(ClusterScatterNode), false, PresetObjectKind.Bush);
                    break;
                case "drygrass":
                    AddObjectPipeline(graph, layer.Id, result, "Dry Grass", typeof(PlacementMaskNode), typeof(ObjectScatterNode), false, PresetObjectKind.DryGrass);
                    break;
                case "rock":
                    AddObjectPipeline(graph, layer.Id, result, "Rock", typeof(PlacementMaskNode), typeof(ObjectScatterNode), false, PresetObjectKind.Rock);
                    break;
                case "resourceitem":
                    AddObjectPipeline(graph, layer.Id, result, "Resource Item", typeof(PlacementMaskNode), typeof(ObjectScatterNode), false, PresetObjectKind.Resource);
                    break;
                case "treeleaves":
                    AddObjectPipeline(graph, layer.Id, result, "Tree Leaves", typeof(PlacementMaskNode), typeof(ClusterScatterNode), false, PresetObjectKind.TreeLeaves);
                    break;
                default:
                    AddBaseTilePipeline(graph, layer.Id, result);
                    break;
            }

            return Finish(graph, result);
        }

        internal static GraphPresetApplyResult AddGrassObjectsBranch(
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes) =>
            AddBranchPreset(graph, layerId, selectedNodes, "Grass Small", typeof(PlacementMaskNode), typeof(ClusterScatterNode), false, PresetObjectKind.Grass);

        internal static GraphPresetApplyResult AddEdgeObjectsBranch(
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes) =>
            AddBranchPreset(graph, layerId, selectedNodes, "Edge Objects", typeof(EdgeMaskNode), typeof(ClusterScatterNode), true, PresetObjectKind.Bush);

        internal static GraphPresetApplyResult AddClusterObjectsBranch(
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes) =>
            AddBranchPreset(graph, layerId, selectedNodes, "Cluster Objects", typeof(PlacementMaskNode), typeof(ClusterScatterNode), false, PresetObjectKind.Bush);

        internal static GraphPresetApplyResult AddShorelineDecorBranch(
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes) =>
            AddBranchPreset(graph, layerId, selectedNodes, "Shoreline Decor", typeof(EdgeMaskNode), typeof(ClusterScatterNode), true, PresetObjectKind.Rock);

        internal static GraphPresetApplyResult AddResourceScatterBranch(
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes) =>
            AddBranchPreset(graph, layerId, selectedNodes, "Resource Scatter", typeof(PlacementMaskNode), typeof(ObjectScatterNode), false, PresetObjectKind.Resource);

        private static GraphPresetApplyResult AddBranchPreset(
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes,
            string presetName,
            Type maskNodeType,
            Type scatterNodeType,
            bool requireMaskSource,
            PresetObjectKind objectKind)
        {
            var result = BeginResult(presetName);
            if (graph == null)
                return Fail(result, "Graph asset не задано.");

            if (string.IsNullOrEmpty(layerId) || graph.GetLayerById(layerId) == null)
                layerId = graph.EnsureDefaultLayer();

            result.LayerId = layerId;
            result.Layer = graph.GetLayerById(layerId);

            var source = FindMaskSource(graph, layerId, selectedNodes);
            if (requireMaskSource && !source.IsValid)
                return Finish(graph, Fail(result, source.ErrorMessage));

            AddObjectPipeline(graph, layerId, result, presetName, maskNodeType, scatterNodeType, requireMaskSource, objectKind, source);
            return Finish(graph, result);
        }

        private static void AddBaseTilePipeline(GraphAsset graph, string layerId, GraphPresetApplyResult result)
        {
            var shape = AddTwcNode(graph, layerId, "GiantGrey.TileWorldCreator.Shapes, GiantGrey.TileWorldCreator", new Vector2(-620f, 0f), result);
            var smooth = AddTwcNode(graph, layerId, "GiantGrey.TileWorldCreator.Smooth, GiantGrey.TileWorldCreator", new Vector2(-300f, 0f), result);
            var expand = AddTwcNode(graph, layerId, "GiantGrey.TileWorldCreator.Expand, GiantGrey.TileWorldCreator", new Vector2(20f, 0f), result);
            var tileSettings = AddNode<TileSettingsNode>(graph, layerId, new Vector2(340f, 0f), result);

            ConfigureFullMapShape(shape?.Modifier);
            SetIntField(smooth?.Modifier, "smoothCount", 1);
            SetIntField(expand?.Modifier, "iterations", 1);
            SetField(tileSettings, "_generateFlatSurface", true);

            Connect(graph, shape, 0, smooth, 0, result);
            Connect(graph, smooth, 0, expand, 0, result);
            Connect(graph, expand, 0, tileSettings, 0, result);
            EnsureLayerOutputNode(graph, layerId, result, tileSettings, 0, LayerOutputKind.Tiles);
        }

        private static void AddShorelineObjectPipeline(GraphAsset graph, string layerId, GraphPresetApplyResult result)
        {
            var sourceLayer = graph.Layers
                .Where(layer => layer != null && layer.Id != layerId)
                .OrderByDescending(layer => layer.SortingOrder)
                .FirstOrDefault();

            MaskSource source = default;
            if (sourceLayer != null)
            {
                var layerRef = AddNode<LayerMaskReferenceNode>(graph, layerId, new Vector2(-880f, 0f), result);
                layerRef?.SetSourceLayerId(sourceLayer.Id);
                source = new MaskSource(layerRef, 0, null);
                AddObjectPipeline(graph, layerId, result, "Shoreline", typeof(EdgeMaskNode), typeof(ClusterScatterNode), true, PresetObjectKind.Rock, source);
            }
            else
            {
                result.Warnings.Add("Shoreline preset не знайшов попередній шар-джерело, тому створив fallback placement branch.");
                AddObjectPipeline(graph, layerId, result, "Shoreline", typeof(PlacementMaskNode), typeof(ClusterScatterNode), false, PresetObjectKind.Rock);
            }
        }

        private static void AddObjectPipeline(
            GraphAsset graph,
            string layerId,
            GraphPresetApplyResult result,
            string branchName,
            Type maskNodeType,
            Type scatterNodeType,
            bool requireMaskSource,
            PresetObjectKind objectKind,
            MaskSource source = default)
        {
            if (graph == null || string.IsNullOrEmpty(layerId))
                return;

            if (HasNamedObjectOutput(graph, layerId, branchName))
            {
                result.Warnings.Add($"Preset '{branchName}' уже є в цьому шарі.");
                return;
            }

            float y = GetNextBranchY(graph, layerId);
            var mask = AddNode(graph, maskNodeType, layerId, new Vector2(-620f, y), result);
            var scatter = AddNode(graph, scatterNodeType, layerId, new Vector2(-330f, y), result);
            var objectLayer = AddNode<ObjectLayerNode>(graph, layerId, new Vector2(-40f, y), result);
            var objectOutput = AddNode<ObjectOutputToTWCNode>(graph, layerId, new Vector2(260f, y), result);

            if (mask == null || scatter == null || objectLayer == null || objectOutput == null)
            {
                result.Warnings.Add($"Не вдалося створити всі ноди preset-а '{branchName}'.");
                return;
            }

            ConfigureScatter(scatter, objectKind);
            ConfigureObjectLayer(objectLayer, branchName, layerId, objectKind);

            if (source.IsValid)
                Connect(graph, source.Node, source.OutputIndex, mask, 0, result);
            else if (requireMaskSource)
                result.Warnings.Add($"Preset '{branchName}' потребує bool mask source, але source не знайдено.");

            int maskOutputIndex = mask is EdgeMaskNode ? 1 : 0;
            Connect(graph, mask, maskOutputIndex, scatter, 0, result);
            Connect(graph, scatter, 0, objectLayer, 0, result);
            Connect(graph, objectLayer, 0, objectOutput, 0, result);
            EnsureLayerOutputNode(graph, layerId, result, objectOutput, 0, LayerOutputKind.Objects);
        }

        private static MaskSource FindMaskSource(
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes)
        {
            var selected = (selectedNodes ?? Array.Empty<NodeBase>())
                .Where(node => node != null && node.LayerId == layerId)
                .Select(node => new MaskSource(node, FindBoolOutputIndex(node), null))
                .Where(source => source.IsValid)
                .ToList();

            if (selected.Count == 1)
                return selected[0];
            if (selected.Count > 1)
                return MaskSource.Invalid("Виділено кілька bool-mask нодів. Виділи тільки одну source mask ноду для edge/shoreline preset.");

            var layerConnections = graph.GetConnectionsForLayer(layerId, false);
            var terminalCandidates = graph.GetNodesForLayer(layerId)
                .Select(node => new MaskSource(node, FindBoolOutputIndex(node), null))
                .Where(source => source.IsValid)
                .Where(source => !layerConnections.Any(connection =>
                    connection != null
                    && connection.SourceNodeId == source.Node.NodeId
                    && connection.SourcePortIndex == source.OutputIndex))
                .ToList();

            if (terminalCandidates.Count == 1)
                return terminalCandidates[0];
            if (terminalCandidates.Count > 1)
                return MaskSource.Invalid("У шарі є кілька terminal bool-mask нодів. Виділи конкретну source mask ноду.");

            return MaskSource.Invalid("Не знайдено bool-mask source у активному шарі.");
        }

        private static int FindBoolOutputIndex(NodeBase node)
        {
            if (node?.Outputs == null)
                return -1;

            for (int i = 0; i < node.Outputs.Length; i++)
            {
                if (node.Outputs[i].ValueType == typeof(bool[,]))
                    return i;
            }

            return -1;
        }

        private static T AddNode<T>(GraphAsset graph, string layerId, Vector2 position, GraphPresetApplyResult result)
            where T : NodeBase =>
            AddNode(graph, typeof(T), layerId, position, result) as T;

        private static NodeBase AddNode(
            GraphAsset graph,
            Type nodeType,
            string layerId,
            Vector2 position,
            GraphPresetApplyResult result)
        {
            var node = graph.AddNode(nodeType, false, layerId);
            if (node == null)
                return null;

            node.LayerId = layerId;
            node.EditorPosition = position;
            result.CreatedNodes.Add(node);
            return node;
        }

        private static TwcModifierNode AddTwcNode(
            GraphAsset graph,
            string layerId,
            string modifierTypeName,
            Vector2 position,
            GraphPresetApplyResult result)
        {
            var modifierType = Type.GetType(modifierTypeName);
            if (modifierType == null)
            {
                result.Warnings.Add($"TWC modifier type not found: {modifierTypeName}");
                return null;
            }

            var node = AddNode<TwcModifierNode>(graph, layerId, position, result);
            node?.ConfigureModifier(modifierType);
            return node;
        }

        private static void Connect(
            GraphAsset graph,
            NodeBase source,
            int sourcePort,
            NodeBase target,
            int targetPort,
            GraphPresetApplyResult result)
        {
            if (graph == null || source == null || target == null)
                return;

            var connection = graph.AddConnection(source.NodeId, sourcePort, target.NodeId, targetPort);
            if (connection != null)
                result.CreatedConnections.Add(connection);
        }

        private static OutputNode EnsureLayerOutputNode(
            GraphAsset graph,
            string layerId,
            GraphPresetApplyResult result,
            NodeBase source,
            int sourcePort,
            LayerOutputKind outputKind)
        {
            if (graph == null || string.IsNullOrEmpty(layerId))
                return null;

            var layerOutput = graph.GetNodesForLayer(layerId)
                .OfType<OutputNode>()
                .FirstOrDefault();

            if (layerOutput == null)
            {
                var position = source != null
                    ? source.EditorPosition + new Vector2(320f, 0f)
                    : new Vector2(260f, 120f);
                layerOutput = AddNode<OutputNode>(graph, layerId, position, result);
            }

            if (layerOutput == null)
                return null;

            bool hasIncoming = graph.GetConnectionsForLayer(layerId, includeGlobal: false)
                .Any(connection => connection != null && connection.TargetNodeId == layerOutput.NodeId);
            if (hasIncoming)
                return layerOutput;

            layerOutput.OutputKind = outputKind;
            if (source != null
                && TryResolveLayerOutputTargetPort(layerOutput, source, sourcePort, outputKind, out int targetPort))
                Connect(graph, source, sourcePort, layerOutput, targetPort, result);

            return layerOutput;
        }

        private static bool TryResolveLayerOutputTargetPort(
            OutputNode output,
            NodeBase source,
            int sourcePortIndex,
            LayerOutputKind outputKind,
            out int targetPort)
        {
            targetPort = -1;
            var sourceOutputs = source?.Outputs;
            var outputInputs = output?.Inputs;
            if (sourceOutputs == null
                || outputInputs == null
                || sourcePortIndex < 0
                || sourcePortIndex >= sourceOutputs.Length)
                return false;

            var sourcePort = sourceOutputs[sourcePortIndex];
            targetPort = outputKind switch
            {
                LayerOutputKind.Objects => OutputNode.DataInputIndex,
                LayerOutputKind.Masks => OutputNode.MaskInputIndex,
                LayerOutputKind.InternalData => OutputNode.DataInputIndex,
                _ when sourcePort.ValueType == typeof(bool[,]) => OutputNode.MaskInputIndex,
                _ when sourcePort.ValueType == typeof(float[,]) => OutputNode.HeightMapInputIndex,
                _ when sourcePort.ValueType == typeof(string[,]) => OutputNode.BiomeMapInputIndex,
                _ => OutputNode.DataInputIndex
            };

            return targetPort >= 0
                && targetPort < outputInputs.Length
                && PortDefinition.AreValueTypesCompatible(sourcePort.ValueType, outputInputs[targetPort].ValueType);
        }

        private static void ConfigureFullMapShape(ScriptableObject modifier)
        {
            if (modifier == null || modifier.GetType().Name != "Shapes")
                return;

            var shapesField = modifier.GetType().GetField("shapes");
            if (shapesField == null || shapesField.GetValue(modifier) is not IList shapes)
                return;

            var shapeType = modifier.GetType().GetNestedType("AvailableShapes");
            if (shapeType == null)
                return;

            var shape = Activator.CreateInstance(shapeType);
            SetField(shape, "shapeType", 1);
            SetField(shape, "rndPosition", false);
            SetField(shape, "position", new Vector2Int(64, 64));
            SetField(shape, "size", new Vector2Int(256, 256));
            SetField(shape, "radius", 64);
            shapes.Clear();
            shapes.Add(shape);
            EditorUtility.SetDirty(modifier);
        }

        private static void ConfigureScatter(NodeBase scatter, PresetObjectKind kind)
        {
            if (scatter == null)
                return;

            var serialized = new SerializedObject(scatter);
            var rule = serialized.FindProperty("_rule");
            if (rule != null)
            {
                SetRelative(rule, "Density", kind is PresetObjectKind.Grass or PresetObjectKind.DryGrass ? 0.28f : 0.12f);
                SetRelative(rule, "MinDistance", kind is PresetObjectKind.Grass or PresetObjectKind.DryGrass ? 1f : 2f);
                SetRelative(rule, "Jitter", 0.25f);
                SetRelative(rule, "RandomSeed", 17 + (int)kind * 13);
                SetRelative(rule, "RotationRandomization", 180f);
                SetRelative(rule, "UseTWCObjectLayer", true);
                SetVector2Relative(rule, "ScaleRandomization", ResolveScaleRange(kind));
            }

            var cluster = serialized.FindProperty("_cluster");
            if (cluster != null)
            {
                SetRelative(cluster, "Enabled", true);
                SetRelative(cluster, "ClusterCount", kind == PresetObjectKind.TreeLeaves ? 5 : 8);
                SetRelative(cluster, "ClusterRadius", kind is PresetObjectKind.Rock or PresetObjectKind.Resource ? 2 : 3);
                SetRelative(cluster, "ClusterDensity", kind is PresetObjectKind.Grass or PresetObjectKind.DryGrass ? 0.65f : 0.45f);
                SetRelative(cluster, "NoiseScale", 0.2f);
                SetRelative(cluster, "NoiseThreshold", 0.35f);
                SetRelative(cluster, "EdgePreference", kind == PresetObjectKind.Rock ? 0.45f : 0.2f);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureObjectLayer(
            ObjectLayerNode node,
            string branchName,
            string layerId,
            PresetObjectKind kind)
        {
            if (node == null)
                return;

            var serialized = new SerializedObject(node);
            serialized.FindProperty("_layerName").stringValue = branchName;
            serialized.FindProperty("_targetGraphLayerId").stringValue = layerId;

            var prefabs = serialized.FindProperty("_prefabs");
            if (prefabs != null)
            {
                prefabs.arraySize = 1;
                var entry = prefabs.GetArrayElementAtIndex(0);
                entry.FindPropertyRelative("Prefab").objectReferenceValue = EnsureDefaultPrefab(kind);
                entry.FindPropertyRelative("Weight").floatValue = 1f;
                entry.FindPropertyRelative("MinScale").floatValue = ResolveScaleRange(kind).x;
                entry.FindPropertyRelative("MaxScale").floatValue = ResolveScaleRange(kind).y;
                entry.FindPropertyRelative("RandomYaw").boolValue = true;
                entry.FindPropertyRelative("AlignToSurface").boolValue = true;
                entry.FindPropertyRelative("ClusterAffinity").floatValue = 1f;
                entry.FindPropertyRelative("ColorVariation").colorValue = ResolveColor(kind);
            }

            var rule = serialized.FindProperty("_rule");
            if (rule != null)
            {
                SetRelative(rule, "Density", kind is PresetObjectKind.Grass or PresetObjectKind.DryGrass ? 0.28f : 0.12f);
                SetRelative(rule, "MinDistance", kind is PresetObjectKind.Grass or PresetObjectKind.DryGrass ? 1f : 2f);
                SetRelative(rule, "Jitter", 0.25f);
                SetRelative(rule, "RotationRandomization", 180f);
                SetRelative(rule, "UseTWCObjectLayer", true);
                SetRelative(rule, "MergeInTWC", false);
                SetVector2Relative(rule, "ScaleRandomization", ResolveScaleRange(kind));
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject EnsureDefaultPrefab(PresetObjectKind kind)
        {
            EnsureFolder(DefaultAssetFolder);

            string key = kind.ToString();
            string prefabPath = $"{DefaultAssetFolder}/Default_{key}.prefab";
            string materialPath = $"{DefaultAssetFolder}/Default_{key}_Mat.mat";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                RefreshExistingDefaultPrefab(prefabPath, materialPath, kind);
                return existing;
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(
                    Shader.Find(DecorShaderName)
                    ?? Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard"))
                {
                    name = $"Default_{key}_Mat"
                };
                ApplyDefaultDecorMaterialSettings(material, kind);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                ApplyDefaultDecorMaterialSettings(material, kind);
                EditorUtility.SetDirty(material);
            }

            GameObject root = kind is PresetObjectKind.Grass or PresetObjectKind.DryGrass or PresetObjectKind.TreeLeaves
                ? CreateCrossCardObject($"Default_{key}", ResolveColor(kind), material)
                : GameObject.CreatePrimitive(kind == PresetObjectKind.Rock ? PrimitiveType.Sphere : PrimitiveType.Cube);
            root.name = $"Default_{key}";
            root.transform.localScale = ResolvePrefabScale(kind);

            var renderer = root.GetComponentInChildren<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;

            var meshFilter = root.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null && AssetDatabase.GetAssetPath(meshFilter.sharedMesh).Length == 0)
            {
                string meshPath = $"{DefaultAssetFolder}/Default_{key}_Mesh.asset";
                AssetDatabase.CreateAsset(meshFilter.sharedMesh, meshPath);
            }

            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static void RefreshExistingDefaultPrefab(
            string prefabPath,
            string materialPath,
            PresetObjectKind kind)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                string key = kind.ToString();
                material = new Material(
                    Shader.Find(DecorShaderName)
                    ?? Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard"))
                {
                    name = $"Default_{key}_Mat"
                };
                ApplyDefaultDecorMaterialSettings(material, kind);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            else
            {
                ApplyDefaultDecorMaterialSettings(material, kind);
                EditorUtility.SetDirty(material);
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var renderer = root.GetComponentInChildren<Renderer>();
                if (renderer != null)
                    renderer.sharedMaterial = material;

                var meshFilter = root.GetComponent<MeshFilter>();
                if (meshFilter != null && IsCardLike(kind))
                    meshFilter.sharedMesh = EnsureCrossCardMeshAsset(kind, ResolveColor(kind));

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void ApplyDefaultDecorMaterialSettings(Material material, PresetObjectKind kind)
        {
            if (material == null)
                return;

            bool cardLike = IsCardLike(kind);
            Color color = ResolveColor(kind);
            material.color = color;
            SetColorIfExists(material, "_BaseColor", color);
            SetColorIfExists(material, "_Color", color);
            SetFloatIfExists(material, "_Alpha", color.a);
            SetFloatIfExists(material, "_AlphaClipEnabled", cardLike ? 1f : 0f);
            SetFloatIfExists(material, "_AlphaClipThreshold", 0.35f);
            SetFloatIfExists(material, "_CullMode", cardLike ? (float)CullMode.Off : (float)CullMode.Back);
            SetFloatIfExists(material, "_Cull", cardLike ? (float)CullMode.Off : (float)CullMode.Back);
            SetFloatIfExists(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            SetFloatIfExists(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            SetFloatIfExists(material, "_ZWrite", 1f);
            SetFloatIfExists(material, "_ContactBlobMode", cardLike ? 1f : 0f);
            SetFloatIfExists(material, "_ContactShadowEnabled", 1f);
            SetFloatIfExists(material, "_ContactDarkness", cardLike ? 0.09f : 0.14f);
            SetVectorIfExists(material, "_ContactBlobAspect", cardLike ? new Vector4(1.25f, 0.55f, 0f, 0f) : new Vector4(1f, 1f, 0f, 0f));
            SetFloatIfExists(material, "_OutlineEnabled", 1f);
            SetFloatIfExists(material, "_OutlineScreenWidthPx", 1.5f);
            SetFloatIfExists(material, "_LeafPlaneShading", cardLike ? 1f : 0f);
            SetVectorIfExists(material, "_TextureFill", new Vector4(1f, 1f, 0f, 0f));
            SetVectorIfExists(material, "_TextureFillOffset", Vector4.zero);
            SetFloatIfExists(material, "_TextureFitClamp", 1f);
            SetFloatIfExists(material, "_TextureVolumeStrength", cardLike ? 0.35f : 0.18f);
            SetFloatIfExists(material, "_TextureVolumeRoundness", cardLike ? 0.62f : 0.45f);
            SetVectorIfExists(material, "_TextureVolumeDirection", new Vector4(-0.45f, 0.75f, 0f, 0f));

            material.EnableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)RenderQueue.AlphaTest + 40;
        }

        private static bool IsCardLike(PresetObjectKind kind) =>
            kind is PresetObjectKind.Grass or PresetObjectKind.DryGrass or PresetObjectKind.TreeLeaves;

        private static void SetColorIfExists(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
                material.SetColor(propertyName, value);
        }

        private static void SetFloatIfExists(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
                material.SetFloat(propertyName, value);
        }

        private static void SetVectorIfExists(Material material, string propertyName, Vector4 value)
        {
            if (material.HasProperty(propertyName))
                material.SetVector(propertyName, value);
        }

        private static GameObject CreateCrossCardObject(string name, Color color, Material material)
        {
            var root = new GameObject(name);
            var meshFilter = root.AddComponent<MeshFilter>();
            var renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            meshFilter.sharedMesh = BuildCrossCardMesh(name + "_Mesh", color);
            return root;
        }

        private static Mesh EnsureCrossCardMeshAsset(PresetObjectKind kind, Color color)
        {
            string key = kind.ToString();
            string meshPath = $"{DefaultAssetFolder}/Default_{key}_Mesh.asset";
            Mesh mesh = BuildCrossCardMesh($"Default_{key}_Mesh", color);
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, meshPath);
                return mesh;
            }

            EditorUtility.CopySerialized(mesh, existing);
            EditorUtility.SetDirty(existing);
            UnityEngine.Object.DestroyImmediate(mesh);
            return existing;
        }

        private static Mesh BuildCrossCardMesh(string name, Color color)
        {
            float width = 0.55f;
            float height = 0.85f;
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            for (int plane = 0; plane < 3; plane++)
            {
                float angle = Mathf.PI * plane / 3f;
                var right = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * width * 0.5f;
                var normal = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)).normalized;
                AddCardPlaneFace(vertices, normals, uvs, triangles, right, normal, height, false);
                AddCardPlaneFace(vertices, normals, uvs, triangles, right, -normal, height, true);
            }

            var mesh = new Mesh
            {
                name = name,
                vertices = vertices.ToArray(),
                normals = normals.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray(),
                colors = Enumerable.Repeat(color, vertices.Count).ToArray()
            };
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddCardPlaneFace(
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 right,
            Vector3 normal,
            float height,
            bool backFace)
        {
            int start = vertices.Count;
            vertices.Add(-right);
            vertices.Add(right);
            vertices.Add(right + Vector3.up * height);
            vertices.Add(-right + Vector3.up * height);

            for (int i = 0; i < 4; i++)
                normals.Add(normal);

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(0f, 1f));

            if (backFace)
            {
                triangles.AddRange(new[] { start, start + 2, start + 1, start, start + 3, start + 2 });
                return;
            }

            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        private static GraphPresetApplyResult Finish(GraphAsset graph, GraphPresetApplyResult result)
        {
            if (graph == null || result == null)
                return result;

            graph.EnsureLayerGraphStates();
            result.GraphId = graph.GetLayerGraphState(result.LayerId)?.GraphId;
            result.ValidationReport = new GraphValidator().ValidateDetailed(graph);
            result.Success = IsPresetValid(result);
            if (string.IsNullOrEmpty(result.Message))
                result.Message = BuildMessage(result);
            EditorUtility.SetDirty(graph);
            return result;
        }

        private static bool IsPresetValid(GraphPresetApplyResult result)
        {
            if (result == null)
                return false;

            bool hasUsefulNode = result.CreatedNodes.Any(node =>
                node != null
                && node is not OutputNode
                && node is not ObjectOutputToTWCNode);
            if (!hasUsefulNode)
                return false;

            var layerIssues = result.ValidationReport?.Issues
                .Where(issue => string.IsNullOrEmpty(issue.LayerId) || issue.LayerId == result.LayerId)
                .ToList() ?? new List<GraphValidationIssue>();

            if (layerIssues.Any(issue => issue.Severity == ValidationSeverity.Error))
                return false;

            if (layerIssues.Any(issue => issue.Code == "INPUT_REQUIRED_UNCONNECTED"))
                return false;

            return true;
        }

        private static string BuildMessage(GraphPresetApplyResult result)
        {
            if (result == null)
                return "Preset failed.";

            string status = result.Success ? "valid" : "invalid";
            return $"Preset '{result.PresetName}' {status}: created {result.CreatedNodes.Count} node(s), {result.CreatedConnections.Count} connection(s).";
        }

        private static GraphPresetApplyResult BeginResult(string presetName) =>
            new() { PresetName = presetName };

        private static GraphPresetApplyResult Fail(GraphPresetApplyResult result, string message)
        {
            result.Success = false;
            result.Message = message;
            result.Warnings.Add(message);
            return result;
        }

        private static bool HasNamedObjectOutput(GraphAsset graph, string layerId, string branchName)
        {
            return graph.GetNodesForLayer(layerId)
                .OfType<ObjectLayerNode>()
                .Any(node => string.Equals(ReadObjectLayerName(node), branchName, StringComparison.Ordinal));
        }

        private static string ReadObjectLayerName(ObjectLayerNode node)
        {
            if (node == null)
                return null;

            var serialized = new SerializedObject(node);
            return serialized.FindProperty("_layerName")?.stringValue;
        }

        private static float GetNextBranchY(GraphAsset graph, string layerId)
        {
            var nodes = graph.GetNodesForLayer(layerId);
            if (nodes.Count == 0)
                return 0f;

            return nodes.Max(node => node?.EditorPosition.y ?? 0f) + 190f;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }

        private static string NormalizePresetName(string value) =>
            new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        private static Vector2 ResolveScaleRange(PresetObjectKind kind) =>
            kind switch
            {
                PresetObjectKind.Grass => new Vector2(0.75f, 1.15f),
                PresetObjectKind.DryGrass => new Vector2(0.7f, 1.1f),
                PresetObjectKind.Rock => new Vector2(0.55f, 1.05f),
                PresetObjectKind.Resource => new Vector2(0.75f, 1.25f),
                PresetObjectKind.TreeLeaves => new Vector2(1.1f, 1.8f),
                _ => new Vector2(0.85f, 1.2f)
            };

        private static Vector3 ResolvePrefabScale(PresetObjectKind kind) =>
            kind switch
            {
                PresetObjectKind.Grass => new Vector3(0.7f, 0.9f, 0.7f),
                PresetObjectKind.DryGrass => new Vector3(0.65f, 0.8f, 0.65f),
                PresetObjectKind.Rock => new Vector3(0.45f, 0.28f, 0.45f),
                PresetObjectKind.Resource => new Vector3(0.35f, 0.35f, 0.35f),
                PresetObjectKind.TreeLeaves => new Vector3(1.1f, 0.9f, 1.1f),
                _ => new Vector3(0.7f, 0.7f, 0.7f)
            };

        private static Color ResolveColor(PresetObjectKind kind) =>
            kind switch
            {
                PresetObjectKind.Grass => new Color(0.36f, 0.52f, 0.25f, 1f),
                PresetObjectKind.DryGrass => new Color(0.62f, 0.56f, 0.32f, 1f),
                PresetObjectKind.Bush => new Color(0.28f, 0.44f, 0.23f, 1f),
                PresetObjectKind.Rock => new Color(0.44f, 0.44f, 0.40f, 1f),
                PresetObjectKind.Resource => new Color(0.67f, 0.48f, 0.28f, 1f),
                PresetObjectKind.TreeLeaves => new Color(0.30f, 0.46f, 0.25f, 1f),
                _ => Color.white
            };

        private static void SetIntField(ScriptableObject target, string fieldName, int value) =>
            SetField(target, fieldName, value);

        private static void SetField(object target, string fieldName, object value)
        {
            if (target == null || string.IsNullOrEmpty(fieldName))
                return;

            var field = target.GetType().GetField(fieldName);
            if (field == null)
                return;

            if (field.FieldType.IsEnum && value is int enumIndex)
                value = Enum.ToObject(field.FieldType, enumIndex);
            field.SetValue(target, value);
        }

        private static void SetRelative(SerializedProperty root, string name, float value)
        {
            var property = root?.FindPropertyRelative(name);
            if (property != null)
                property.floatValue = value;
        }

        private static void SetRelative(SerializedProperty root, string name, int value)
        {
            var property = root?.FindPropertyRelative(name);
            if (property != null)
                property.intValue = value;
        }

        private static void SetRelative(SerializedProperty root, string name, bool value)
        {
            var property = root?.FindPropertyRelative(name);
            if (property != null)
                property.boolValue = value;
        }

        private static void SetVector2Relative(SerializedProperty root, string name, Vector2 value)
        {
            var property = root?.FindPropertyRelative(name);
            if (property != null)
                property.vector2Value = value;
        }

        private readonly struct MaskSource
        {
            public readonly NodeBase Node;
            public readonly int OutputIndex;
            public readonly string ErrorMessage;

            public MaskSource(NodeBase node, int outputIndex, string errorMessage)
            {
                Node = node;
                OutputIndex = outputIndex;
                ErrorMessage = errorMessage;
            }

            public bool IsValid => Node != null && OutputIndex >= 0;
            public static MaskSource Invalid(string message) => new(null, -1, message);
        }

        private enum PresetObjectKind
        {
            Grass = 0,
            Bush = 1,
            DryGrass = 2,
            Rock = 3,
            Resource = 4,
            TreeLeaves = 5
        }
    }
}
