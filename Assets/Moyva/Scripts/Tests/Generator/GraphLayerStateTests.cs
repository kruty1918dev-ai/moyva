using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Tests.Generator
{
    [TestFixture]
    public sealed class GraphLayerStateTests
    {
        private const string TestAssetPath = "Assets/Moyva/Scripts/Tests/Generator/GraphLayerStateTest.asset";
        private const string TestPrefabPath = "Assets/Moyva/Scripts/Tests/Generator/GraphObjectPrefabTest.prefab";
        private readonly List<Object> _createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestAssetPath);
            AssetDatabase.DeleteAsset(TestPrefabPath);

            for (int i = 0; i < _createdObjects.Count; i++)
            {
                if (_createdObjects[i] != null)
                    Object.DestroyImmediate(_createdObjects[i]);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void EnsureLayerGraphStates_AssignsOrphanNodesToFirstLayer()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var output = graph.AddNode(typeof(OutputNode), false, layerId);
            output.LayerId = null;

            graph.EnsureLayerGraphStates();

            Assert.AreEqual(layerId, output.LayerId);
            Assert.That(graph.GetLayerGraphState(layerId).NodeIds, Does.Contain(output.NodeId));
        }

        [Test]
        public void ExecutionScope_ContainsOnlyRequestedLayerAndGlobalNodes()
        {
            var graph = CreateGraphAsset();
            string layerA = graph.EnsureDefaultLayer();
            var layerB = graph.AddLayer("Layer B");

            var nodeA = graph.AddNode(typeof(OutputNode), false, layerA);
            var nodeB = graph.AddNode(typeof(OutputNode), false, layerB.Id);

            var scopeA = graph.CreateExecutionScope(layerA);
            var scopeB = graph.CreateExecutionScope(layerB.Id);

            Assert.That(scopeA.Nodes.Select(node => node.NodeId), Does.Contain(nodeA.NodeId));
            Assert.That(scopeA.Nodes.Select(node => node.NodeId), Does.Not.Contain(nodeB.NodeId));
            Assert.That(scopeB.Nodes.Select(node => node.NodeId), Does.Contain(nodeB.NodeId));
            Assert.That(scopeB.Nodes.Select(node => node.NodeId), Does.Not.Contain(nodeA.NodeId));
        }

        [Test]
        public void CreateEnabledLayerExecutionScopes_UsesLayerSortingOrder()
        {
            var graph = CreateGraphAsset();
            var upper = graph.GetLayerById(graph.EnsureDefaultLayer());
            upper.Name = "Upper";
            upper.SortingOrder = 20;
            var middle = graph.AddLayer("Middle");
            middle.SortingOrder = 10;
            var lower = graph.AddLayer("Lower");
            lower.SortingOrder = 0;

            var scopes = graph.CreateEnabledLayerExecutionScopes();

            Assert.That(
                scopes.Select(scope => scope.LayerId),
                Is.EqualTo(new[] { lower.Id, middle.Id, upper.Id }));
        }

        [Test]
        public void RemoveNode_RemovesIncomingAndOutgoingConnections()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var mask = graph.AddNode(typeof(PlacementMaskNode), false, layerId);
            var scatter = graph.AddNode(typeof(ObjectScatterNode), false, layerId);
            var layer = graph.AddNode(typeof(ObjectLayerNode), false, layerId);

            graph.AddConnection(mask.NodeId, 0, scatter.NodeId, 0);
            graph.AddConnection(scatter.NodeId, 0, layer.NodeId, 0);

            graph.RemoveNode(scatter);

            Assert.IsEmpty(graph.Connections);
        }

        [Test]
        public void Validator_CatchesCrossLayerLinks()
        {
            var graph = CreateGraphAsset();
            string layerA = graph.EnsureDefaultLayer();
            var layerB = graph.AddLayer("Layer B");
            var mask = graph.AddNode(typeof(PlacementMaskNode), false, layerA);
            var scatter = graph.AddNode(typeof(ObjectScatterNode), false, layerB.Id);

            graph.AddConnection(mask.NodeId, 0, scatter.NodeId, 0);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue => issue.Code == "CONNECTION_CROSS_LAYER"), Is.True);
        }

        [Test]
        public void Validator_CatchesLayerReferenceCycles()
        {
            var graph = CreateGraphAsset();
            var layerA = graph.GetLayerById(graph.EnsureDefaultLayer());
            layerA.Name = "Layer A";
            layerA.SortingOrder = 0;
            var layerB = graph.AddLayer("Layer B");
            layerB.SortingOrder = 1;

            var refA = graph.AddNode(typeof(LayerMaskReferenceNode), false, layerA.Id) as LayerMaskReferenceNode;
            var refB = graph.AddNode(typeof(LayerMaskReferenceNode), false, layerB.Id) as LayerMaskReferenceNode;
            refA.SetSourceLayerId(layerB.Id);
            refB.SetSourceLayerId(layerA.Id);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "LAYER_REF_CYCLE"
                && issue.Severity == ValidationSeverity.Error
                && issue.Message.Contains("Layer A")
                && issue.Message.Contains("Layer B")), Is.True);
        }

        [Test]
        public void Validator_RejectsLayerReferenceToFutureLayer()
        {
            var graph = CreateGraphAsset();
            var lower = graph.GetLayerById(graph.EnsureDefaultLayer());
            lower.Name = "Lower";
            lower.SortingOrder = 0;
            var upper = graph.AddLayer("Upper");
            upper.SortingOrder = 1;

            var reference = graph.AddNode(typeof(LayerMaskReferenceNode), false, lower.Id) as LayerMaskReferenceNode;
            reference.SetSourceLayerId(upper.Id);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "LAYER_REF_FORWARD"
                && issue.Severity == ValidationSeverity.Error
                && issue.LayerId == lower.Id
                && issue.NodeId == reference.NodeId), Is.True);
        }

        [Test]
        public void Validator_RequiresOutputNodePerEnabledLayer()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            graph.AddNode(typeof(ConstantCellMaskNode), false, layerId);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "LAYER_OUTPUT_MISSING"
                && issue.Severity == ValidationSeverity.Error
                && issue.LayerId == layerId), Is.True);
        }

        [Test]
        public void Validator_RequiresOutputNodeToBeConnected()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            graph.AddNode(typeof(OutputNode), false, layerId);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "LAYER_OUTPUT_UNCONNECTED"
                && issue.Severity == ValidationSeverity.Error
                && issue.LayerId == layerId), Is.True);
        }

        [Test]
        public void Validator_ReportsOutputKindMismatch()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var source = graph.AddNode(typeof(ConstantCellMaskNode), false, layerId);
            var output = graph.AddNode(typeof(OutputNode), false, layerId) as OutputNode;
            output.OutputKind = LayerOutputKind.Objects;

            graph.AddConnection(source.NodeId, 0, output.NodeId, OutputNode.MaskInputIndex);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "LAYER_OUTPUT_KIND_UNCONNECTED"
                && issue.Severity == ValidationSeverity.Error
                && issue.LayerId == layerId), Is.True);
        }

        [Test]
        public void Validator_AcceptsConnectedOutputNode()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var source = graph.AddNode(typeof(ConstantCellMaskNode), false, layerId);
            var output = graph.AddNode(typeof(OutputNode), false, layerId) as OutputNode;
            output.OutputKind = LayerOutputKind.Masks;

            graph.AddConnection(source.NodeId, 0, output.NodeId, OutputNode.MaskInputIndex);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code.StartsWith("LAYER_OUTPUT", System.StringComparison.Ordinal)
                && issue.Severity == ValidationSeverity.Error), Is.False);
        }

        [Test]
        public void Validator_RejectsTileOutputWithoutTileWorldCreatorNode()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var source = graph.AddNode(typeof(ConstantCellMaskNode), false, layerId);
            var output = graph.AddNode(typeof(OutputNode), false, layerId) as OutputNode;
            output.OutputKind = LayerOutputKind.Tiles;

            graph.AddConnection(source.NodeId, 0, output.NodeId, OutputNode.MaskInputIndex);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "TILE_OUTPUT_WITHOUT_TILE_SETTINGS"
                && issue.Severity == ValidationSeverity.Error
                && issue.LayerId == layerId
                && issue.NodeId == output.NodeId), Is.True);
        }

        [Test]
        public void Validator_RejectsObjectOutputWithoutObjectOutputToTwc()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var objectLayer = graph.AddNode(typeof(ObjectLayerNode), false, layerId);
            var output = graph.AddNode(typeof(OutputNode), false, layerId) as OutputNode;
            output.OutputKind = LayerOutputKind.Objects;

            graph.AddConnection(objectLayer.NodeId, 0, output.NodeId, OutputNode.DataInputIndex);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "OBJECT_OUTPUT_PIPELINE_INCOMPLETE"
                && issue.Severity == ValidationSeverity.Error
                && issue.LayerId == layerId
                && issue.NodeId == output.NodeId), Is.True);
            Assert.That(report.Issues.Any(issue =>
                issue.Code == "OBJECT_OUTPUT_MISSING"
                && issue.Severity == ValidationSeverity.Error
                && issue.LayerId == layerId), Is.True);
        }

        [Test]
        public void Validator_RejectsBrokenTwcModifierNode()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var twcNode = graph.AddNode(typeof(TwcModifierNode), false, layerId);
            var output = graph.AddNode(typeof(OutputNode), false, layerId) as OutputNode;
            output.OutputKind = LayerOutputKind.Masks;

            graph.AddConnection(twcNode.NodeId, 0, output.NodeId, OutputNode.MaskInputIndex);

            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "TWC_MODIFIER_TYPE_MISSING"
                && issue.Severity == ValidationSeverity.Error
                && issue.LayerId == layerId
                && issue.NodeId == twcNode.NodeId), Is.True);
        }

        [Test]
        public void Runner_ExecutesNodesByDependencyPlan()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var relay = graph.AddNode(typeof(MaskRelayNode), false, layerId);
            var output = graph.AddNode(typeof(OutputNode), false, layerId) as OutputNode;
            var source = graph.AddNode(typeof(ConstantCellMaskNode), false, layerId);
            output.OutputKind = LayerOutputKind.Masks;

            graph.AddConnection(source.NodeId, 0, relay.NodeId, 0);
            graph.AddConnection(relay.NodeId, 0, output.NodeId, OutputNode.MaskInputIndex);

            var result = new GraphRunner().Execute(
                graph.CreateExecutionScope(layerId),
                new NodeContext(1) { MapSize = new Vector2Int(4, 4) });

            Assert.That(result.Success, Is.True, result.ErrorMessage);
            Assert.That(result.LayerId, Is.EqualTo(layerId));
            Assert.That(result.ExecutionOrderNodeIds, Is.EqualTo(new[] { source.NodeId, relay.NodeId, output.NodeId }));
            Assert.That(result.Logs.Select(log => log.NodeId), Is.EqualTo(result.ExecutionOrderNodeIds));
            Assert.That(result.Logs.Select(log => log.OrderIndex), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(result.Logs.Last().InputDependencyCount, Is.EqualTo(1));
        }

        [Test]
        public void Runner_RejectsCyclicExecutionPlanBeforeExecutingNodes()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var first = graph.AddNode(typeof(MaskRelayNode), false, layerId);
            var second = graph.AddNode(typeof(MaskRelayNode), false, layerId);

            graph.AddConnection(first.NodeId, 0, second.NodeId, 0);
            graph.AddConnection(second.NodeId, 0, first.NodeId, 0);

            var report = new GraphValidator().ValidateDetailed(graph);
            var result = new GraphRunner().Execute(
                graph.CreateExecutionScope(layerId),
                new NodeContext(1) { MapSize = new Vector2Int(4, 4) });

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "GRAPH_CYCLE"
                && issue.Severity == ValidationSeverity.Error
                && issue.Message.Contains("Mask Relay")), Is.True);
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorLayerId, Is.EqualTo(layerId));
            Assert.That(result.ErrorMessage, Does.Contain("dependency cycle"));
            Assert.That(result.Logs.Count, Is.LessThanOrEqualTo(1));
        }

        [Test]
        public void BaseLayerPreset_CreatesConnectedPipeline()
        {
            var graph = CreateGraphAsset();

            var result = InvokeLayerPreset(
                graph,
                "Base Tile Layer",
                Color.green);

            var createdNodes = GetResultList<NodeBase>(result, "CreatedNodes");
            var createdConnections = GetResultList<Connection>(result, "CreatedConnections");
            string layerId = GetResultValue<string>(result, "LayerId");

            Assert.That(GetResultValue<bool>(result, "Success"), Is.True, GetResultValue<string>(result, "Message"));
            Assert.That(createdNodes.Count(node => node is not OutputNode), Is.GreaterThan(1));
            Assert.That(createdConnections.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(graph.GetNodesForLayer(layerId).All(node => node.LayerId == layerId), Is.True);
        }

        [Test]
        public void EdgePreset_WithoutMaskSource_DoesNotCreateDanglingBranch()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();

            var result = InvokeBranchPreset(
                "AddEdgeObjectsBranch",
                graph,
                layerId,
                System.Array.Empty<NodeBase>());

            Assert.That(GetResultValue<bool>(result, "Success"), Is.False);
            Assert.That(GetResultList<NodeBase>(result, "CreatedNodes"), Is.Empty);
            Assert.That(GetResultValue<string>(result, "Message"), Does.Contain("source"));
            Assert.That(graph.GetNodesForLayer(layerId), Is.Empty);
        }

        [Test]
        public void Compile_ObjectOutputCreatesGeneratedObjectBuildLayer()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var mask = graph.AddNode(typeof(PlacementMaskNode), false, layerId);
            var scatter = graph.AddNode(typeof(ObjectScatterNode), false, layerId);
            var objectLayer = graph.AddNode(typeof(ObjectLayerNode), false, layerId) as ObjectLayerNode;
            var output = graph.AddNode(typeof(ObjectOutputToTWCNode), false, layerId);

            ConfigureScatter(scatter as ObjectScatterNode);
            ConfigureObjectLayerPrefab(objectLayer, CreateTestPrefab(), layerId, mergeInTWC: true);

            graph.AddConnection(mask.NodeId, 0, scatter.NodeId, 0);
            graph.AddConnection(scatter.NodeId, 0, objectLayer.NodeId, 0);
            graph.AddConnection(objectLayer.NodeId, 0, output.NodeId, 0);

            var managerObject = new GameObject("TWC Manager Test");
            _createdObjects.Add(managerObject);
            var manager = managerObject.AddComponent<TileWorldCreatorManager>();
            manager.configuration = CreateConfiguration();

            GraphToConfigurationCompiler.Compile(graph, manager, 1);

            bool hasGeneratedObjectLayer = manager.configuration.buildLayerFolders
                .SelectMany(folder => folder.buildLayers)
                .OfType<ObjectBuildLayer>()
                .Any(layer => TWCObjectPlacementAdapter.IsGeneratedObjectLayer(layer));
            Assert.That(hasGeneratedObjectLayer, Is.True);
        }

        [Test]
        public void Compile_DirectObjectsPreserveConfiguredScale()
        {
            var graph = CreateGraphAsset();
            string layerId = graph.EnsureDefaultLayer();
            var mask = graph.AddNode(typeof(PlacementMaskNode), false, layerId);
            var scatter = graph.AddNode(typeof(ObjectScatterNode), false, layerId) as ObjectScatterNode;
            var objectLayer = graph.AddNode(typeof(ObjectLayerNode), false, layerId) as ObjectLayerNode;
            var output = graph.AddNode(typeof(ObjectOutputToTWCNode), false, layerId);

            ConfigureScatter(scatter);
            ConfigureObjectLayerPrefab(objectLayer, CreateTestPrefab(), layerId, mergeInTWC: false, scale: 2f);

            graph.AddConnection(mask.NodeId, 0, scatter.NodeId, 0);
            graph.AddConnection(scatter.NodeId, 0, objectLayer.NodeId, 0);
            graph.AddConnection(objectLayer.NodeId, 0, output.NodeId, 0);

            var managerObject = new GameObject("TWC Manager Direct Test");
            _createdObjects.Add(managerObject);
            var manager = managerObject.AddComponent<TileWorldCreatorManager>();
            manager.configuration = CreateConfiguration();

            GraphToConfigurationCompiler.Compile(graph, manager, 1);

            var root = manager.transform.Find("Generated Props")
                       ?? manager.transform.Find(TWCObjectPlacementAdapter.DirectObjectsRootName);
            Assert.NotNull(root);

            var layerRoot = root.Find(TWCObjectPlacementAdapter.BuildGeneratedLayerName("Test Objects"));
            Assert.NotNull(layerRoot);
            Assert.That(layerRoot.childCount, Is.GreaterThan(0));
            Assert.That(layerRoot.GetChild(0).localScale.x, Is.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void Compile_DuplicateObjectLayerNamesCreateSeparateContainers()
        {
            var graph = CreateGraphAsset();
            string layerA = graph.EnsureDefaultLayer();
            string layerB = graph.AddLayer("Layer B").Id;
            var prefab = CreateTestPrefab();

            AddObjectPipeline(graph, layerA, prefab);
            AddObjectPipeline(graph, layerB, prefab);

            var managerObject = new GameObject("TWC Manager Duplicate Object Layers Test");
            _createdObjects.Add(managerObject);
            var manager = managerObject.AddComponent<TileWorldCreatorManager>();
            manager.configuration = CreateConfiguration();

            GraphToConfigurationCompiler.Compile(graph, manager, 1);

            var root = manager.transform.Find("Generated Props")
                       ?? manager.transform.Find(TWCObjectPlacementAdapter.DirectObjectsRootName);
            Assert.NotNull(root);

            int matchingContainers = 0;
            string baseName = TWCObjectPlacementAdapter.BuildGeneratedLayerName("Test Objects");
            for (int i = 0; i < root.childCount; i++)
            {
                if (root.GetChild(i).name.StartsWith(baseName, System.StringComparison.Ordinal))
                    matchingContainers++;
            }

            Assert.That(matchingContainers, Is.EqualTo(2));
        }

        [Test]
        public void LayerMaskPrewarm_AllowsHigherLayerToReferenceEarlierLayerMask()
        {
            var graph = CreateGraphAsset();
            string lowerLayerId = graph.EnsureDefaultLayer();
            var lowerLayer = graph.GetLayerById(lowerLayerId);
            lowerLayer.SortingOrder = 0;
            var upperLayer = graph.AddLayer("Upper");
            upperLayer.SortingOrder = 1;

            var source = graph.AddNode(typeof(ConstantCellMaskNode), false, lowerLayerId) as ConstantCellMaskNode;
            source.Cell = new Vector2Int(1, 2);

            var reference = graph.AddNode(typeof(LayerMaskReferenceNode), false, upperLayer.Id) as LayerMaskReferenceNode;
            reference.SetSourceLayerId(lowerLayerId);

            var registry = new LayerMaskRegistry();
            LayerMaskPrewarmUtility.PrewarmAllLayerMasks(
                graph,
                1,
                new Vector2Int(4, 4),
                registry);

            var context = new NodeContext(1)
            {
                MapSize = new Vector2Int(4, 4)
            };
            context.RegisterService(registry);

            var result = new GraphRunner().Execute(graph.CreateExecutionScope(upperLayer.Id), context);
            var mask = result.GetOutput<bool[,]>(reference.NodeId);

            Assert.That(result.Success, Is.True);
            Assert.NotNull(mask);
            Assert.That(mask[1, 2], Is.True);
        }

        [Test]
        public void LayerReferenceBlueprintModifier_GeneratesSourceLayerBeforeCopyingMask()
        {
            var config = CreateConfiguration();
            var lower = ScriptableObject.CreateInstance<BlueprintLayer>();
            var upper = ScriptableObject.CreateInstance<BlueprintLayer>();
            var sourceModifier = ScriptableObject.CreateInstance<SingleCellBlueprintModifier>();
            var referenceModifier = ScriptableObject.CreateInstance<MoyvaLayerReferenceBlueprintModifier>();
            _createdObjects.Add(lower);
            _createdObjects.Add(upper);
            _createdObjects.Add(sourceModifier);
            _createdObjects.Add(referenceModifier);

            lower.layerName = "Lower";
            upper.layerName = "Upper";
            config.blueprintLayerFolders[0].blueprintLayers.Add(lower);
            config.blueprintLayerFolders[0].blueprintLayers.Add(upper);

            sourceModifier.Cell = new Vector2(2, 3);
            upper.tileMapModifiers.Add(sourceModifier);
            referenceModifier.sourceBlueprintLayerGuid = upper.guid;
            lower.tileMapModifiers.Add(referenceModifier);

            lower.ExecuteLayer(config, null);

            Assert.That(lower.allPositions, Does.Contain(new Vector2(2, 3)));
        }

        [Test]
        public void BuildLayerStoreSync_CreatesBlueprintLayersInGraphOrder()
        {
            var graph = CreateGraphAsset();
            var baseLayer = graph.GetLayerById(graph.EnsureDefaultLayer());
            baseLayer.Name = "Upper";
            baseLayer.SortingOrder = 10;
            var lowerLayer = graph.AddLayer("Lower");
            lowerLayer.SortingOrder = 0;

            var config = SyncGraphBuildLayers(graph);
            var blueprintLayers = GetRootBlueprintLayers(config)
                .Where(layer => !TWCObjectPlacementAdapter.IsGeneratedBlueprintLayer(layer))
                .ToList();

            Assert.That(blueprintLayers.Select(layer => layer.layerName), Is.EqualTo(new[] { "Lower", "Upper" }));
            Assert.That(lowerLayer.BlueprintLayerGuid, Is.EqualTo(blueprintLayers[0].guid));
            Assert.That(baseLayer.BlueprintLayerGuid, Is.EqualTo(blueprintLayers[1].guid));
        }

        [Test]
        public void BuildLayerStoreSync_RenameUpdatesBlueprintWithoutCreatingDuplicate()
        {
            var graph = CreateGraphAsset();
            var layer = graph.GetLayerById(graph.EnsureDefaultLayer());
            layer.Name = "Before Rename";

            var config = SyncGraphBuildLayers(graph);
            string originalGuid = layer.BlueprintLayerGuid;

            layer.Name = "After Rename";
            config = SyncGraphBuildLayers(graph);
            var blueprintLayers = GetRootBlueprintLayers(config)
                .Where(layer => !TWCObjectPlacementAdapter.IsGeneratedBlueprintLayer(layer))
                .ToList();

            Assert.That(blueprintLayers.Count, Is.EqualTo(1));
            Assert.That(blueprintLayers[0].guid, Is.EqualTo(originalGuid));
            Assert.That(blueprintLayers[0].layerName, Is.EqualTo("After Rename"));
        }

        [Test]
        public void BuildLayerStoreSync_RemovesBlueprintWhenGraphLayerIsRemoved()
        {
            var graph = CreateGraphAsset();
            var layerA = graph.GetLayerById(graph.EnsureDefaultLayer());
            layerA.Name = "Layer A";
            var layerB = graph.AddLayer("Layer B");

            SyncGraphBuildLayers(graph);
            Assert.That(layerB.BlueprintLayerGuid, Is.Not.Empty);

            graph.RemoveLayer(layerB.Id);
            var config = SyncGraphBuildLayers(graph);
            var blueprintLayers = GetRootBlueprintLayers(config)
                .Where(layer => !TWCObjectPlacementAdapter.IsGeneratedBlueprintLayer(layer))
                .ToList();

            Assert.That(blueprintLayers.Select(layer => layer.layerName), Is.EqualTo(new[] { "Layer A" }));
            Assert.That(blueprintLayers.Any(layer => layer.guid == layerB.BlueprintLayerGuid), Is.False);
        }

        [Test]
        public void BuildLayerStoreSync_MigratesExistingBlueprintByNameWhenGuidIsMissing()
        {
            var graph = CreateGraphAsset();
            var layer = graph.GetLayerById(graph.EnsureDefaultLayer());
            layer.Name = "Migrated";
            var config = GetCompanionConfiguration(graph, true);
            var existing = ScriptableObject.CreateInstance<BlueprintLayer>();
            existing.layerName = "Migrated";
            AssetDatabase.AddObjectToAsset(existing, graph);
            config.blueprintLayerFolders = new List<BlueprintLayerFolder> { new BlueprintLayerFolder("Root") };
            config.blueprintLayerFolders[0].blueprintLayers.Add(existing);

            config = SyncGraphBuildLayers(graph);
            var blueprintLayers = GetRootBlueprintLayers(config)
                .Where(layer => !TWCObjectPlacementAdapter.IsGeneratedBlueprintLayer(layer))
                .ToList();

            Assert.That(blueprintLayers.Count, Is.EqualTo(1));
            Assert.That(blueprintLayers[0], Is.SameAs(existing));
            Assert.That(layer.BlueprintLayerGuid, Is.EqualTo(existing.guid));
        }

        [Test]
        public void BuildLayerStoreValidation_ReportsMissingBlueprintGuid()
        {
            var graph = CreateGraphAsset();
            var layer = graph.GetLayerById(graph.EnsureDefaultLayer());
            layer.Name = "Broken";
            var config = SyncGraphBuildLayers(graph);

            layer.BlueprintLayerGuid = "missing-blueprint-guid";
            var report = ValidateBlueprintLayerSync(graph, config);

            Assert.That(report.Issues.Any(issue =>
                issue.Code == "BLUEPRINT_LAYER_REFERENCE_MISSING"
                && issue.LayerId == layer.Id), Is.True);
        }

        private GraphAsset CreateGraphAsset()
        {
            AssetDatabase.DeleteAsset(TestAssetPath);
            var graph = ScriptableObject.CreateInstance<GraphAsset>();
            _createdObjects.Add(graph);
            AssetDatabase.CreateAsset(graph, TestAssetPath);
            return graph;
        }

        private static List<BlueprintLayer> GetRootBlueprintLayers(Configuration config)
        {
            Assert.NotNull(config);
            Assert.NotNull(config.blueprintLayerFolders);
            Assert.That(config.blueprintLayerFolders.Count, Is.GreaterThan(0));
            return config.blueprintLayerFolders[0].blueprintLayers;
        }

        private static Configuration SyncGraphBuildLayers(GraphAsset graph)
        {
            var method = ResolveBuildLayerStoreType().GetMethod(
                "Sync",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return method.Invoke(null, new object[] { graph }) as Configuration;
        }

        private static Configuration GetCompanionConfiguration(GraphAsset graph, bool create)
        {
            var method = ResolveBuildLayerStoreType().GetMethod(
                "GetCompanionConfiguration",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return method.Invoke(null, new object[] { graph, create }) as Configuration;
        }

        private static GraphValidationReport ValidateBlueprintLayerSync(GraphAsset graph, Configuration config)
        {
            var method = ResolveBuildLayerStoreType().GetMethod(
                "ValidateBlueprintLayerSync",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return method.Invoke(null, new object[] { graph, config }) as GraphValidationReport;
        }

        private static System.Type ResolveBuildLayerStoreType()
        {
            var type = System.Type.GetType(
                "Kruty1918.Moyva.Generator.Editor.GraphBuildLayerStore, Kruty1918.Moyva.Generator.Editor");
            Assert.NotNull(type, "GraphBuildLayerStore type must be available from Generator.Editor assembly.");
            return type;
        }

        private static object InvokeLayerPreset(GraphAsset graph, string layerName, Color color)
        {
            var type = ResolvePresetUtilityType();
            var method = type.GetMethod(
                "AddLayerPreset",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(method);
            return method.Invoke(null, new object[] { graph, layerName, color });
        }

        private static object InvokeBranchPreset(
            string methodName,
            GraphAsset graph,
            string layerId,
            IReadOnlyList<NodeBase> selectedNodes)
        {
            var type = ResolvePresetUtilityType();
            var method = type.GetMethod(
                methodName,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Assert.NotNull(method);
            return method.Invoke(null, new object[] { graph, layerId, selectedNodes });
        }

        private static System.Type ResolvePresetUtilityType()
        {
            var type = System.Type.GetType(
                "Kruty1918.Moyva.GraphSystem.Editor.GraphBuiltInPresetUtility, Kruty1918.Moyva.GraphSystem.Editor");
            Assert.NotNull(type, "GraphBuiltInPresetUtility type must be available from GraphSystem.Editor assembly.");
            return type;
        }

        private static T GetResultValue<T>(object result, string fieldName)
        {
            Assert.NotNull(result);
            var field = result.GetType().GetField(fieldName);
            Assert.NotNull(field, fieldName);
            return (T)field.GetValue(result);
        }

        private static List<T> GetResultList<T>(object result, string fieldName)
        {
            var value = GetResultValue<object>(result, fieldName);
            Assert.NotNull(value);
            return ((IEnumerable<T>)value).ToList();
        }

        private Configuration CreateConfiguration()
        {
            var configuration = ScriptableObject.CreateInstance<Configuration>();
            _createdObjects.Add(configuration);
            configuration.blueprintLayerFolders = new List<BlueprintLayerFolder> { new BlueprintLayerFolder("Root") };
            configuration.buildLayerFolders = new List<BuildLayerFolder> { new BuildLayerFolder("Root") };
            configuration.width = 12;
            configuration.height = 12;
            configuration.cellSize = 1f;
            return configuration;
        }

        private GameObject CreateTestPrefab()
        {
            AssetDatabase.DeleteAsset(TestPrefabPath);
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, TestPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(TestPrefabPath);
        }

        private static void ConfigureObjectLayerPrefab(
            ObjectLayerNode node,
            GameObject prefab,
            string layerId,
            bool mergeInTWC = false,
            float scale = 1f)
        {
            var serialized = new SerializedObject(node);
            serialized.FindProperty("_layerName").stringValue = "Test Objects";
            serialized.FindProperty("_targetGraphLayerId").stringValue = layerId;

            var prefabs = serialized.FindProperty("_prefabs");
            prefabs.arraySize = 1;
            var entry = prefabs.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
            entry.FindPropertyRelative("Weight").floatValue = 1f;
            entry.FindPropertyRelative("MinScale").floatValue = mergeInTWC ? 0.9f : scale;
            entry.FindPropertyRelative("MaxScale").floatValue = mergeInTWC ? 1.1f : scale;
            entry.FindPropertyRelative("RandomYaw").boolValue = !mergeInTWC;

            var rule = serialized.FindProperty("_rule");
            rule.FindPropertyRelative("UseTWCObjectLayer").boolValue = true;
            rule.FindPropertyRelative("MergeInTWC").boolValue = mergeInTWC;
            rule.FindPropertyRelative("Density").floatValue = 1f;
            rule.FindPropertyRelative("MinDistance").floatValue = 0f;
            rule.FindPropertyRelative("Jitter").floatValue = 0f;
            rule.FindPropertyRelative("RotationRandomization").floatValue = 0f;
            rule.FindPropertyRelative("ScaleRandomization").vector2Value = mergeInTWC
                ? new Vector2(0.9f, 1.1f)
                : new Vector2(scale, scale);

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AddObjectPipeline(GraphAsset graph, string layerId, GameObject prefab)
        {
            var mask = graph.AddNode(typeof(PlacementMaskNode), false, layerId);
            var scatter = graph.AddNode(typeof(ObjectScatterNode), false, layerId) as ObjectScatterNode;
            var objectLayer = graph.AddNode(typeof(ObjectLayerNode), false, layerId) as ObjectLayerNode;
            var output = graph.AddNode(typeof(ObjectOutputToTWCNode), false, layerId);

            ConfigureScatter(scatter);
            ConfigureObjectLayerPrefab(objectLayer, prefab, layerId, mergeInTWC: false);

            graph.AddConnection(mask.NodeId, 0, scatter.NodeId, 0);
            graph.AddConnection(scatter.NodeId, 0, objectLayer.NodeId, 0);
            graph.AddConnection(objectLayer.NodeId, 0, output.NodeId, 0);
        }

        private static void ConfigureScatter(ObjectScatterNode node)
        {
            var serialized = new SerializedObject(node);
            var rule = serialized.FindProperty("_rule");
            rule.FindPropertyRelative("Density").floatValue = 1f;
            rule.FindPropertyRelative("MinDistance").floatValue = 0f;
            rule.FindPropertyRelative("Jitter").floatValue = 0f;
            rule.FindPropertyRelative("RotationRandomization").floatValue = 0f;
            rule.FindPropertyRelative("ScaleRandomization").vector2Value = Vector2.one;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public sealed class ConstantCellMaskNode : NodeBase
        {
            public Vector2Int Cell;

            public override string Title => "Constant Cell Mask";
            public override string Category => "Tests";
            public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();
            public override PortDefinition[] Outputs => new[]
            {
                PortDefinition.Output<bool[,]>("Mask")
            };

            public override NodeOutput Execute(object[] inputs, NodeContext context)
            {
                int width = Mathf.Max(1, context?.MapSize.x ?? 0);
                int height = Mathf.Max(1, context?.MapSize.y ?? 0);
                var mask = new bool[width, height];
                if (Cell.x >= 0 && Cell.y >= 0 && Cell.x < width && Cell.y < height)
                    mask[Cell.x, Cell.y] = true;

                return NodeOutput.Success(mask);
            }
        }

        public sealed class MaskRelayNode : NodeBase
        {
            public override string Title => "Mask Relay";
            public override string Category => "Tests";
            public override PortDefinition[] Inputs => new[]
            {
                PortDefinition.Input<bool[,]>("Source")
            };
            public override PortDefinition[] Outputs => new[]
            {
                PortDefinition.Output<bool[,]>("Mask")
            };

            public override NodeOutput Execute(object[] inputs, NodeContext context)
            {
                var mask = inputs != null && inputs.Length > 0
                    ? inputs[0] as bool[,]
                    : null;
                if (mask != null)
                    return NodeOutput.Success(mask);

                int width = Mathf.Max(1, context?.MapSize.x ?? 0);
                int height = Mathf.Max(1, context?.MapSize.y ?? 0);
                return NodeOutput.Success(new bool[width, height]);
            }
        }

        public sealed class SingleCellBlueprintModifier : BlueprintModifier
        {
            public Vector2 Cell;

            public override HashSet<Vector2> Execute(HashSet<Vector2> positions, BlueprintLayer layer)
            {
                return new HashSet<Vector2> { Cell };
            }
        }
    }
}
