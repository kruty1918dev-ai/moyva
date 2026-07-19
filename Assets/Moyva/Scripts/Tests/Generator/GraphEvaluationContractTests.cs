using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.Generator.Runtime.Nodes.WFC;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Editor;
using Kruty1918.Moyva.GraphSystem.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Tests.Generator
{
    public sealed class GraphEvaluationContractTests
    {
        private const string GraphPath =
            "Assets/Moyva/Scripts/Tests/Generator/GraphEvaluationContractGraph.asset";
        private const string SubgraphPath =
            "Assets/Moyva/Scripts/Tests/Generator/GraphEvaluationContractSubgraph.asset";
        private const string AuditGraphPath =
            "Assets/Moyva/Scripts/Tests/Generator/GraphSubassetAuditTest.asset";
        private const string AuditReferencePath =
            "Assets/Moyva/Scripts/Tests/Generator/GraphSubassetAuditReference.asset";
        private const string LegacyGraphGuid =
            "e728959edc09a5fea829221821058070";
        private const string ActiveGraphGuid =
            "b1deed2da6c33de4882d0097ea3ce6d3";

        private readonly List<Object> _transientObjects = new();
        private int _previousGlobalSeed;
        private UnityEngine.Random.State _previousRandomState;

        [SetUp]
        public void SetUp()
        {
            AssetDatabase.DeleteAsset(GraphPath);
            AssetDatabase.DeleteAsset(SubgraphPath);
            AssetDatabase.DeleteAsset(AuditReferencePath);
            AssetDatabase.DeleteAsset(AuditGraphPath);
            _previousGlobalSeed = GlobalSeed.Current;
            _previousRandomState = UnityEngine.Random.state;
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = _transientObjects.Count - 1; i >= 0; i--)
            {
                if (_transientObjects[i] != null)
                    Object.DestroyImmediate(_transientObjects[i]);
            }

            _transientObjects.Clear();
            AssetDatabase.DeleteAsset(GraphPath);
            AssetDatabase.DeleteAsset(SubgraphPath);
            AssetDatabase.DeleteAsset(AuditReferencePath);
            AssetDatabase.DeleteAsset(AuditGraphPath);
            GlobalSeed.Set(_previousGlobalSeed);
            UnityEngine.Random.state = _previousRandomState;
        }

        [Test]
        public void BoolAnd_UsesTruthTableIntersection()
        {
            var node = CreateTransient<BoolAndNode>();
            var a = new bool[2, 2]
            {
                { false, false },
                { true, true }
            };
            var b = new bool[2, 2]
            {
                { false, true },
                { false, true }
            };

            var output = node.Execute(
                new object[] { a, b },
                new NodeContext(1) { MapSize = new Vector2Int(2, 2) });
            var result = output.Values[0] as bool[,];

            Assert.That(output.Status, Is.EqualTo(NodeStatus.Success));
            Assert.NotNull(result);
            Assert.That(result[0, 0], Is.False);
            Assert.That(result[0, 1], Is.False);
            Assert.That(result[1, 0], Is.False);
            Assert.That(result[1, 1], Is.True);
        }

        [TestCase(typeof(NoiseMapNode))]
        [TestCase(typeof(ShapeMaskNode))]
        [TestCase(typeof(TextureMaskNode))]
        [TestCase(typeof(ThresholdMaskNode))]
        [TestCase(typeof(RangeMaskNode))]
        [TestCase(typeof(DistanceFieldNode))]
        [TestCase(typeof(SlopeMaskNode))]
        [TestCase(typeof(NormalizeMapNode))]
        [TestCase(typeof(RemapMapNode))]
        [TestCase(typeof(SmoothMapNode))]
        [TestCase(typeof(TerraceMapNode))]
        [TestCase(typeof(MaskMorphologyNode))]
        public void NewMapNode_IsDeterministicAndPreservesRectangularSize(
            Type nodeType)
        {
            const int width = 13;
            const int height = 7;
            var node = CreateTransient(nodeType);
            node.NodeId = "contract-node";
            var context = new NodeContext(98765)
            {
                MapSize = new Vector2Int(width, height)
            };
            object[] inputs = BuildInputs(node, width, height);

            NodeOutput first = node.Execute(inputs, context);
            NodeOutput second = node.Execute(inputs, context);

            Assert.That(first.Status, Is.Not.EqualTo(NodeStatus.Error), first.Message);
            Assert.That(second.Status, Is.Not.EqualTo(NodeStatus.Error), second.Message);
            Assert.That(first.Values.Length, Is.EqualTo(node.Outputs.Length));
            Assert.That(second.Values.Length, Is.EqualTo(node.Outputs.Length));
            Assert.That(HashOutputs(first.Values), Is.EqualTo(HashOutputs(second.Values)));
            for (int i = 0; i < first.Values.Length; i++)
            {
                Assert.That(first.Values[i], Is.TypeOf(node.Outputs[i].ValueType));
                if (first.Values[i] is Array map && map.Rank == 2)
                {
                    Assert.That(map.GetLength(0), Is.EqualTo(width));
                    Assert.That(map.GetLength(1), Is.EqualTo(height));
                }
            }
        }

        [TestCase(typeof(NoiseMapNode))]
        [TestCase(typeof(ShapeMaskNode))]
        [TestCase(typeof(TextureMaskNode))]
        [TestCase(typeof(ThresholdMaskNode))]
        [TestCase(typeof(RangeMaskNode))]
        [TestCase(typeof(DistanceFieldNode))]
        [TestCase(typeof(SlopeMaskNode))]
        [TestCase(typeof(NormalizeMapNode))]
        [TestCase(typeof(RemapMapNode))]
        [TestCase(typeof(SmoothMapNode))]
        [TestCase(typeof(TerraceMapNode))]
        [TestCase(typeof(MaskMorphologyNode))]
        public void NewMapNode_HandlesSingleCellBoundary(Type nodeType)
        {
            var node = CreateTransient(nodeType);
            node.NodeId = "single-cell-contract";
            var context = new NodeContext(19)
            {
                MapSize = Vector2Int.one
            };

            NodeOutput output = node.Execute(
                BuildInputs(node, 1, 1),
                context);

            Assert.That(
                output.Status,
                Is.Not.EqualTo(NodeStatus.Error),
                output.Message);
            for (int i = 0; i < output.Values.Length; i++)
            {
                if (output.Values[i] is Array map && map.Rank == 2)
                    AssertExactSize(map, Vector2Int.one);
            }
        }

        [Test]
        public void Snapshot_NodeLayerAndLogicalPreviewShareOneExactMask()
        {
            GraphAsset graph = CreateGraph(GraphPath);
            string layerId = graph.EnsureDefaultLayer();
            var noise = graph.AddNode(
                typeof(NoiseMapNode),
                false,
                layerId) as NoiseMapNode;
            var threshold = graph.AddNode(
                typeof(ThresholdMaskNode),
                false,
                layerId) as ThresholdMaskNode;
            var output = graph.AddNode(
                typeof(OutputNode),
                false,
                layerId) as OutputNode;
            output.OutputKind = LayerOutputKind.Masks;
            graph.AddConnection(noise.NodeId, 0, threshold.NodeId, 0);
            graph.AddConnection(
                threshold.NodeId,
                0,
                output.NodeId,
                OutputNode.MaskInputIndex);

            GlobalSeed.Set(111);
            UnityEngine.Random.InitState(222);
            UnityEngine.Random.State expectedRandomState = UnityEngine.Random.state;
            var size = new Vector2Int(17, 9);

            GraphEvaluationSnapshot first =
                GraphEvaluationPipeline.Evaluate(graph, 54321, size, 7);
            GraphEvaluationSnapshot second =
                GraphEvaluationPipeline.Evaluate(graph, 54321, size, 8);

            Assert.That(first.Success, Is.True, first.Diagnostics);
            Assert.That(second.Success, Is.True, second.Diagnostics);
            Assert.That(first.SourceGraph, Is.SameAs(graph));
            Assert.That(
                first.IsCompatibleWith(graph, 54321, size),
                Is.True);
            Assert.That(first.NodeRecords.ContainsKey(threshold.NodeId), Is.True);
            Assert.That(
                first.NodeRecords[threshold.NodeId].IsConnectedToOutput,
                Is.True);
            var nodeMask = first.ExecutionResult.GetOutput<bool[,]>(
                threshold.NodeId);
            var layerOutput = first.GetLayerOutput<LayerOutputSnapshot>(layerId);
            Assert.NotNull(nodeMask);
            Assert.NotNull(layerOutput);
            Assert.NotNull(layerOutput.LayerMask);
            Assert.That(first.CompiledLayerMatrices.ContainsKey(layerId), Is.True);
            AssertExactSize(nodeMask, size);
            AssertExactSize(layerOutput.LayerMask, size);
            AssertExactSize(first.CompiledLayerMatrices[layerId], size);
            Assert.That(Hash(nodeMask), Is.EqualTo(Hash(layerOutput.LayerMask)));
            Assert.That(
                Hash(nodeMask),
                Is.EqualTo(Hash(first.CompiledLayerMatrices[layerId])));
            Assert.That(
                Hash(nodeMask),
                Is.EqualTo(Hash(second.ExecutionResult.GetOutput<bool[,]>(
                    threshold.NodeId))));
            Assert.That(GlobalSeed.Current, Is.EqualTo(111));
            Assert.That(UnityEngine.Random.state, Is.EqualTo(expectedRandomState));

            Texture2D preview = NodePreviewTextureFactory.TryBuild(
                first.GetNodeOutputs(threshold.NodeId),
                1,
                1,
                out bool ownsTexture,
                out string status);
            try
            {
                Assert.That(ownsTexture, Is.True);
                Assert.NotNull(preview);
                Assert.That(preview.width, Is.EqualTo(size.x));
                Assert.That(preview.height, Is.EqualTo(size.y));
                Assert.That(preview.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(status, Does.Contain("1 px/tile"));
            }
            finally
            {
                if (preview != null)
                    Object.DestroyImmediate(preview);
            }
        }

        [Test]
        public async Task AsyncSnapshot_MatchesSyncAndRestoresRandomState()
        {
            GraphAsset graph = CreateGraph(GraphPath);
            string layerId = graph.EnsureDefaultLayer();
            var noise = graph.AddNode(
                typeof(NoiseMapNode),
                false,
                layerId);
            var threshold = graph.AddNode(
                typeof(ThresholdMaskNode),
                false,
                layerId);
            var output = graph.AddNode(
                typeof(OutputNode),
                false,
                layerId) as OutputNode;
            output.OutputKind = LayerOutputKind.Masks;
            graph.AddConnection(noise.NodeId, 0, threshold.NodeId, 0);
            graph.AddConnection(
                threshold.NodeId,
                0,
                output.NodeId,
                OutputNode.MaskInputIndex);

            var size = new Vector2Int(19, 6);
            GraphEvaluationSnapshot synchronous =
                GraphEvaluationPipeline.Evaluate(graph, 314159, size, 10);

            GlobalSeed.Set(771);
            UnityEngine.Random.InitState(772);
            UnityEngine.Random.State expectedExternalState =
                UnityEngine.Random.state;
            GraphEvaluationSnapshot asynchronous =
                await GraphEvaluationPipeline.EvaluateAsync(
                    graph,
                    314159,
                    size,
                    11);

            Assert.That(synchronous.Success, Is.True, synchronous.Diagnostics);
            Assert.That(asynchronous.Success, Is.True, asynchronous.Diagnostics);
            Assert.That(
                Hash(synchronous.CompiledLayerMatrices[layerId]),
                Is.EqualTo(Hash(asynchronous.CompiledLayerMatrices[layerId])));
            Assert.That(GlobalSeed.Current, Is.EqualTo(771));
            Assert.That(
                UnityEngine.Random.state,
                Is.EqualTo(expectedExternalState));
        }

        [Test]
        public void RandomScope_SuspendResumePreservesBothRandomSessions()
        {
            GlobalSeed.Set(700);
            UnityEngine.Random.InitState(701);
            UnityEngine.Random.State originalExternalState =
                UnityEngine.Random.state;

            UnityEngine.Random.InitState(42);
            float expectedFirstGraphValue = UnityEngine.Random.value;
            float expectedSecondGraphValue = UnityEngine.Random.value;
            UnityEngine.Random.state = originalExternalState;

            UnityEngine.Random.State evolvedExternalState;
            using (var scope = new GraphRandomScope(42))
            {
                Assert.That(
                    UnityEngine.Random.value,
                    Is.EqualTo(expectedFirstGraphValue));

                scope.Suspend();
                Assert.That(GlobalSeed.Current, Is.EqualTo(700));
                Assert.That(
                    UnityEngine.Random.state,
                    Is.EqualTo(originalExternalState));
                _ = UnityEngine.Random.value;
                evolvedExternalState = UnityEngine.Random.state;
                GlobalSeed.Set(702);

                scope.Resume();
                Assert.That(GlobalSeed.Current, Is.EqualTo(42));
                Assert.That(
                    UnityEngine.Random.value,
                    Is.EqualTo(expectedSecondGraphValue));
                scope.Suspend();
            }

            Assert.That(GlobalSeed.Current, Is.EqualTo(702));
            Assert.That(
                UnityEngine.Random.state,
                Is.EqualTo(evolvedExternalState));
        }

        [Test]
        public void LivePreviewScheduler_CoalescesAndRejectsStaleRevisions()
        {
            var scheduler = new GraphPreviewRevisionScheduler();

            Assert.That(scheduler.Request(1d, true), Is.EqualTo(1));
            Assert.That(
                scheduler.NextRunAt,
                Is.EqualTo(1d + GraphPreviewRevisionScheduler.DebounceSeconds));
            Assert.That(scheduler.Request(1.1d, true), Is.EqualTo(2));
            Assert.That(scheduler.TryBegin(1.29d, true, false, out _), Is.False);
            Assert.That(
                scheduler.TryBegin(1.3d, true, false, out long firstRun),
                Is.True);
            Assert.That(firstRun, Is.EqualTo(2));
            Assert.That(scheduler.IsCurrent(firstRun), Is.True);

            Assert.That(scheduler.Request(1.31d, true), Is.EqualTo(3));
            Assert.That(scheduler.IsCurrent(firstRun), Is.False);
            scheduler.Complete(firstRun, true, 1.32d, true);

            Assert.That(scheduler.AppliedRevision, Is.EqualTo(-1));
            Assert.That(scheduler.IsRunning, Is.False);
            Assert.That(
                scheduler.NextRunAt,
                Is.EqualTo(
                    1.32d
                    + GraphPreviewRevisionScheduler.DebounceSeconds));
            Assert.That(
                scheduler.TryBegin(1.52d, true, false, out long latestRun),
                Is.True);
            Assert.That(latestRun, Is.EqualTo(3));
            scheduler.Complete(latestRun, true, 1.53d, true);
            Assert.That(scheduler.AppliedRevision, Is.EqualTo(3));
        }

        [Test]
        public void LivePreviewScheduler_DisabledModeStillTracksRevision()
        {
            var scheduler = new GraphPreviewRevisionScheduler();

            Assert.That(scheduler.Request(5d, false), Is.EqualTo(1));
            Assert.That(scheduler.NextRunAt, Is.EqualTo(0d));
            Assert.That(
                scheduler.TryBegin(10d, false, false, out _),
                Is.False);
            Assert.That(
                scheduler.TryBegin(10d, false, true, out long manualRun),
                Is.True);
            scheduler.Complete(manualRun, true, 10.1d, false);

            Assert.That(scheduler.AppliedRevision, Is.EqualTo(1));
            Assert.That(scheduler.NextRunAt, Is.EqualTo(0d));
        }

        [Test]
        public void Runner_RejectsRequiredPortAndInvalidOutputContracts()
        {
            AssertRunnerFails<RequiredRelayNode>("Required input");
            AssertRunnerFails<WrongOutputCountNode>("declares 1 port");
            AssertRunnerFails<WrongOutputTypeNode>("Runtime type");
            AssertRunnerFails<WrongOutputSizeNode>("Map size");
            AssertRunnerFails<NullOutputNode>("Value is null");
        }

        [Test]
        public async Task Runner_RejectsMultipleAuthoritativeOutputs()
        {
            GraphAsset graph = CreateGraph(GraphPath);
            string layerId = graph.EnsureDefaultLayer();
            graph.AddNode(typeof(OutputNode), false, layerId);
            graph.AddNode(typeof(OutputNode), false, layerId);
            var context = new NodeContext(17)
            {
                MapSize = new Vector2Int(7, 3)
            };
            var scope = graph.CreateExecutionScope(layerId);
            var runner = new GraphRunner();

            GraphExecutionResult synchronous = runner.Execute(scope, context);
            GraphExecutionResult asynchronous =
                await runner.ExecuteAsync(scope, context);

            Assert.That(synchronous.Success, Is.False);
            Assert.That(
                synchronous.ErrorMessage,
                Does.Contain("multiple Output nodes"));
            Assert.That(synchronous.ErrorLayerId, Is.EqualTo(layerId));
            Assert.That(asynchronous.Success, Is.False);
            Assert.That(
                asynchronous.ErrorMessage,
                Is.EqualTo(synchronous.ErrorMessage));
            Assert.That(
                asynchronous.ErrorNodeId,
                Is.EqualTo(synchronous.ErrorNodeId));
        }

        [Test]
        public void Compiler_DoesNotBuildFromFailedEvaluationSnapshot()
        {
            GraphAsset graph = CreateGraph(GraphPath);
            string layerId = graph.EnsureDefaultLayer();
            graph.AddNode(
                typeof(WrongOutputCountNode),
                false,
                layerId);

            var managerObject = new GameObject("Failed Snapshot TWC Manager");
            _transientObjects.Add(managerObject);
            var manager =
                managerObject.AddComponent<TileWorldCreatorManager>();
            var configuration =
                ScriptableObject.CreateInstance<Configuration>();
            _transientObjects.Add(configuration);
            configuration.width = 4;
            configuration.height = 3;
            manager.configuration = configuration;

            LogAssert.Expect(
                LogType.Error,
                new Regex(
                    @"^\[GraphToConfigurationCompiler\] Graph evaluation failed;"));
            IReadOnlyList<CompiledLayerMap> compiled =
                GraphToConfigurationCompiler.Compile(
                    graph,
                    manager,
                    17,
                    mapSizeOverride: new Vector2Int(4, 3),
                    skippedLayerIds: null);

            Assert.That(compiled, Is.Empty);
            Assert.That(
                configuration.blueprintLayerFolders
                    .SelectMany(folder => folder.blueprintLayers)
                    .Any(),
                Is.False);
            Assert.That(
                configuration.buildLayerFolders
                    .SelectMany(folder => folder.buildLayers)
                    .Any(),
                Is.False);
        }

        [Test]
        public void DetachedCycle_DoesNotReplaceAuthoritativeLayerResult()
        {
            GraphAsset graph = CreateGraph(GraphPath);
            string layerId = graph.EnsureDefaultLayer();
            var source = graph.AddNode(
                typeof(ShapeMaskNode),
                false,
                layerId);
            var output = graph.AddNode(
                typeof(OutputNode),
                false,
                layerId) as OutputNode;
            var firstCycleNode = graph.AddNode(
                typeof(RequiredRelayNode),
                false,
                layerId);
            var secondCycleNode = graph.AddNode(
                typeof(RequiredRelayNode),
                false,
                layerId);
            output.OutputKind = LayerOutputKind.Masks;
            graph.AddConnection(
                source.NodeId,
                0,
                output.NodeId,
                OutputNode.MaskInputIndex);
            graph.AddConnection(
                firstCycleNode.NodeId,
                0,
                secondCycleNode.NodeId,
                0);
            graph.AddConnection(
                secondCycleNode.NodeId,
                0,
                firstCycleNode.NodeId,
                0);

            var context = new NodeContext(1)
            {
                MapSize = new Vector2Int(11, 5)
            };
            var result = new GraphRunner().Execute(
                graph.CreateExecutionScope(layerId),
                context);
            var report = new GraphValidator().ValidateDetailed(graph);

            Assert.That(result.Success, Is.True, result.ErrorMessage);
            Assert.NotNull(result.GetArtifact<LayerOutputSnapshot>(output.NodeId));
            Assert.That(result.Logs.Any(log =>
                !log.IsConnectedToOutput
                && log.Status == NodeStatus.Warning
                && log.Message.Contains("Not connected to Output")), Is.True);
            Assert.That(report.Issues.Any(issue =>
                issue.Code == "GRAPH_CYCLE"
                && issue.Severity == ValidationSeverity.Warning
                && issue.Message.Contains("Not connected to Output")), Is.True);
        }

        [Test]
        public void Wfc_UsesStableSeedAndExactContextSize()
        {
            var node = CreateTransient<WaveFunctionCollapseNode>();
            node.NodeId = "wfc-contract";
            var sample = new string[3, 3];
            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                sample[x, y] = "A";
            var context = new NodeContext(4455)
            {
                MapSize = new Vector2Int(8, 3)
            };

            NodeOutput first = node.Execute(new object[] { sample }, context);
            NodeOutput second = node.Execute(new object[] { sample }, context);

            Assert.That(first.Status, Is.EqualTo(NodeStatus.Success), first.Message);
            Assert.That(second.Status, Is.EqualTo(NodeStatus.Success), second.Message);
            var firstMap = first.Values[0] as string[,];
            var secondMap = second.Values[0] as string[,];
            AssertExactSize(firstMap, context.MapSize);
            Assert.That(Hash(firstMap), Is.EqualTo(Hash(secondMap)));
        }

        [Test]
        public void TwcModifier_OneByOneInputNeverExpandsToLegacyFiftyByFifty()
        {
            var node = CreateTransient<TwcModifierNode>();
            node.NodeId = "twc-one-cell";
            node.ConfigureModifier(typeof(IdentityBlueprintModifier));
            var source = new bool[1, 1];
            source[0, 0] = true;

            NodeOutput output = node.Execute(
                new object[] { source },
                new NodeContext(77) { MapSize = Vector2Int.one });

            Assert.That(output.Status, Is.EqualTo(NodeStatus.Success), output.Message);
            var mask = output.Values[0] as bool[,];
            Assert.NotNull(mask);
            Assert.That(mask.GetLength(0), Is.EqualTo(1));
            Assert.That(mask.GetLength(1), Is.EqualTo(1));
            Assert.That(mask[0, 0], Is.True);
        }

        [Test]
        public void Subgraph_MigratesOnlyUnambiguousOutputLayer()
        {
            GraphAsset child = CreateGraph(SubgraphPath);
            string childLayerId = child.EnsureDefaultLayer();
            child.AddNode(typeof(OutputNode), false, childLayerId);
            var node = CreateTransient<SubgraphNode>();
            node.Subgraph = child;

            Assert.That(node.TryMigrateOutputLayerId(out string error), Is.True, error);
            Assert.That(node.OutputLayerId, Is.EqualTo(childLayerId));

            var secondLayer = child.AddLayer("Second");
            child.AddNode(typeof(OutputNode), false, secondLayer.Id);
            node.OutputLayerId = string.Empty;

            Assert.That(node.TryMigrateOutputLayerId(out error), Is.False);
            Assert.That(error, Does.Contain("exactly one Output"));
            Assert.That(node.OutputLayerId, Is.Empty);
        }

        [Test]
        public async Task Subgraph_AsyncExecutionMatchesSynchronousSnapshot()
        {
            GraphAsset child = CreateGraph(SubgraphPath);
            string childLayerId = child.EnsureDefaultLayer();
            var input = child.AddNode(
                typeof(SubgraphInputNode),
                false,
                childLayerId);
            var output = child.AddNode(
                typeof(OutputNode),
                false,
                childLayerId) as OutputNode;
            output.OutputKind = LayerOutputKind.InternalData;
            child.AddConnection(
                input.NodeId,
                0,
                output.NodeId,
                OutputNode.BiomeMapInputIndex);

            var node = CreateTransient<SubgraphNode>();
            node.Subgraph = child;
            Assert.That(
                node.TryMigrateOutputLayerId(out string error),
                Is.True,
                error);

            var biomeMap = new string[9, 4];
            biomeMap[2, 1] = "grass";
            biomeMap[7, 3] = "sand";
            var inputs = new object[] { biomeMap, null, null, null };
            var context = new NodeContext(991)
            {
                MapSize = new Vector2Int(9, 4)
            };

            NodeOutput synchronous = node.Execute(inputs, context);
            NodeOutput asynchronous =
                await node.ExecuteAsync(inputs, context);

            Assert.That(synchronous.Status, Is.EqualTo(NodeStatus.Success));
            Assert.That(asynchronous.Status, Is.EqualTo(NodeStatus.Success));
            Assert.That(
                Hash(synchronous.Values[0] as Array),
                Is.EqualTo(Hash(asynchronous.Values[0] as Array)));
            Assert.That(
                Hash(asynchronous.Values[0] as Array),
                Is.EqualTo(Hash(biomeMap)));
        }

        [Test]
        public void Catalog_HasStableOrderedEnglishEntriesAndOnlyStandardAdd()
        {
            var entries = GraphNodeCatalog.Entries;
            var ids = entries
                .Select(entry => entry.Descriptor.StableId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToArray();
            Assert.That(ids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(ids.Length));

            var creatable = entries
                .Where(entry => entry.Descriptor.IsCreatable)
                .ToArray();
            Assert.That(creatable, Is.Not.Empty);
            Assert.That(creatable.All(entry =>
                !ContainsCyrillic(entry.Descriptor.Title)
                && !ContainsCyrillic(entry.Descriptor.Category)), Is.True);
            Assert.That(creatable.All(entry =>
                ContainsCyrillic(entry.Descriptor.Description)), Is.True);
            Assert.That(
                creatable.Select(entry =>
                    NodeCategoryOrder.Get(entry.Descriptor.Category)),
                Is.Ordered);
            Assert.That(
                creatable.Count(entry => entry.Descriptor.Title == "Add"),
                Is.EqualTo(1));
            Assert.That(
                creatable.Single(entry => entry.Descriptor.Title == "Add")
                    .Descriptor.NodeType,
                Is.EqualTo(typeof(AddNode)));

            string[] expectedNewIds =
            {
                "moyva.generators.noise-map",
                "moyva.masks.shape",
                "moyva.masks.texture",
                "moyva.masks.threshold",
                "moyva.masks.range",
                "moyva.modifiers.distance-field",
                "moyva.masks.slope",
                "moyva.height.normalize-map",
                "moyva.height.remap-map",
                "moyva.height.smooth-map",
                "moyva.height.terrace-map",
                "moyva.modifiers.mask-morphology"
            };
            foreach (string stableId in expectedNewIds)
            {
                Assert.That(
                    creatable.Any(entry =>
                        entry.Descriptor.StableId == stableId),
                    Is.True,
                    stableId);
            }

            Assert.That(TwcModifierCatalog.MenuItems.Any(item =>
                Canonical(item.DisplayName) == "add"
                || Canonical(item.ModifierType?.Name) == "add"), Is.False);
        }

        [Test]
        public void GraphPresetIo_MigratesVersionTwoWithoutLosingCompatibility()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                $"moyva-preset-{Guid.NewGuid():N}.graphpreset");
            try
            {
                File.WriteAllText(
                    path,
                    "{\"preset\":{\"version\":2,\"nodes\":[],\"connections\":[]," +
                    "\"scriptableObjects\":[]}}");

                GraphPreset preset = GraphPresetIO.ReadFromFile(path);

                Assert.That(
                    preset.version,
                    Is.EqualTo(GraphPresetIO.CurrentVersion));
                GraphPresetIO.WriteToFile(preset, path);
                Assert.That(
                    File.ReadAllText(path),
                    Does.Contain($"\"version\": {GraphPresetIO.CurrentVersion}"));
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [Test]
        public void SubassetAudit_ProtectsExternalReferences()
        {
            GraphAsset graph = CreateGraph(AuditGraphPath);
            var detached = ScriptableObject.CreateInstance<BoolValueNode>();
            detached.name = "Detached audit node";
            AssetDatabase.AddObjectToAsset(detached, graph);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(
                AuditGraphPath,
                ImportAssetOptions.ForceUpdate);
            graph = AssetDatabase.LoadAssetAtPath<GraphAsset>(AuditGraphPath);
            detached = AssetDatabase.LoadAllAssetsAtPath(AuditGraphPath)
                .OfType<BoolValueNode>()
                .Single();

            GraphSubassetAuditReport beforeReference =
                GraphSubassetAuditUtility.Audit(graph, writeReport: false);
            Assert.That(beforeReference.CleanupSafe, Is.True);
            Assert.That(
                beforeReference.Orphaned.Any(entry =>
                    entry.Asset == detached),
                Is.True);

            var holder = ScriptableObject.CreateInstance<GraphAsset>();
            AssetDatabase.CreateAsset(holder, AuditReferencePath);
            var serializedHolder = new SerializedObject(holder);
            SerializedProperty nodes =
                serializedHolder.FindProperty("_nodes");
            nodes.arraySize = 1;
            nodes.GetArrayElementAtIndex(0).objectReferenceValue = detached;
            serializedHolder.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(
                AuditReferencePath,
                ImportAssetOptions.ForceUpdate);
            graph = AssetDatabase.LoadAssetAtPath<GraphAsset>(AuditGraphPath);
            detached = AssetDatabase.LoadAllAssetsAtPath(AuditGraphPath)
                .OfType<BoolValueNode>()
                .Single();

            GraphSubassetAuditReport afterReference =
                GraphSubassetAuditUtility.Audit(graph, writeReport: false);
            Assert.That(afterReference.CleanupSafe, Is.True);
            Assert.That(
                afterReference.Reachable.Any(entry =>
                    entry.Asset == detached
                    && entry.Reason.Contains("External reference")),
                Is.True);
            Assert.That(
                afterReference.Orphaned.Any(entry =>
                    entry.Asset == detached),
                Is.False);
            Assert.That(
                afterReference.ExternalReferences.Any(reference =>
                    reference.Contains(AuditReferencePath)),
                Is.True);
        }

        [Test]
        public void GraphAssetMigration_PreservesLegacyAndActiveGuids()
        {
            Assert.That(
                AssetDatabase.GUIDToAssetPath(LegacyGraphGuid),
                Is.EqualTo(
                    "Assets/Moyva/SO/Generation/Legacy/GeneratorGraph.asset"));
            Assert.That(
                AssetDatabase.GUIDToAssetPath(ActiveGraphGuid),
                Is.EqualTo(
                    "Assets/Moyva/SO/Generation/Prototype/TestGeneratorGraph.asset"));
            Assert.That(
                AssetDatabase.LoadAssetAtPath<GraphAsset>(
                    AssetDatabase.GUIDToAssetPath(ActiveGraphGuid)),
                Is.Not.Null);
        }

        private void AssertRunnerFails<TNode>(string expectedMessage)
            where TNode : NodeBase
        {
            var node = CreateTransient<TNode>();
            node.NodeId = typeof(TNode).Name;
            var scope = new GraphExecutionScope(
                null,
                "test-layer",
                "test-graph",
                new[] { node },
                Array.Empty<Connection>());
            var result = new GraphRunner().Execute(
                scope,
                new NodeContext(1) { MapSize = new Vector2Int(4, 3) });

            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain(expectedMessage));
        }

        private object[] BuildInputs(NodeBase node, int width, int height)
        {
            if (node is TextureMaskNode textureMask)
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.SetPixels(new[]
                {
                    Color.black,
                    Color.white,
                    Color.red,
                    Color.green
                });
                texture.Apply(false, false);
                _transientObjects.Add(texture);
                var serialized = new SerializedObject(textureMask);
                serialized.FindProperty("_texture").objectReferenceValue = texture;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            var values = new object[node.Inputs.Length];
            for (int i = 0; i < node.Inputs.Length; i++)
            {
                Type type = node.Inputs[i].ValueType;
                if (type == typeof(float[,]))
                {
                    var map = new float[width, height];
                    for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        map[x, y] = (x * 0.17f + y * 0.31f) % 1f;
                    values[i] = map;
                }
                else if (type == typeof(bool[,]))
                {
                    var mask = new bool[width, height];
                    for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        mask[x, y] = (x + y) % 3 == 0;
                    values[i] = mask;
                }
                else if (type == typeof(Texture2D))
                {
                    values[i] = null;
                }
                else
                {
                    throw new AssertionException(
                        $"No contract fixture for input type {type} on {node.Title}.");
                }
            }

            return values;
        }

        private GraphAsset CreateGraph(string path)
        {
            var graph = ScriptableObject.CreateInstance<GraphAsset>();
            AssetDatabase.CreateAsset(graph, path);
            return graph;
        }

        private T CreateTransient<T>() where T : NodeBase
        {
            var node = ScriptableObject.CreateInstance<T>();
            _transientObjects.Add(node);
            return node;
        }

        private NodeBase CreateTransient(Type type)
        {
            var node = ScriptableObject.CreateInstance(type) as NodeBase;
            Assert.NotNull(node);
            _transientObjects.Add(node);
            return node;
        }

        private static void AssertExactSize(Array map, Vector2Int size)
        {
            Assert.NotNull(map);
            Assert.That(map.Rank, Is.EqualTo(2));
            Assert.That(map.GetLength(0), Is.EqualTo(size.x));
            Assert.That(map.GetLength(1), Is.EqualTo(size.y));
        }

        private static ulong HashOutputs(IReadOnlyList<object> values)
        {
            ulong hash = 1469598103934665603UL;
            for (int i = 0; i < values.Count; i++)
                hash = Mix(hash, Hash(values[i] as Array));
            return hash;
        }

        private static ulong Hash(Array map)
        {
            if (map == null)
                return 0UL;

            ulong hash = 1469598103934665603UL;
            hash = Mix(hash, (ulong)map.GetLength(0));
            hash = Mix(hash, (ulong)map.GetLength(1));
            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                object value = map.GetValue(x, y);
                ulong cell = value switch
                {
                    bool boolean => boolean ? 1UL : 0UL,
                    float number => unchecked((uint)BitConverter.SingleToInt32Bits(number)),
                    int integer => unchecked((uint)integer),
                    string text => unchecked((uint)GlobalSeed.StableHash(text)),
                    _ => 0UL
                };
                hash = Mix(hash, cell);
            }

            return hash;
        }

        private static ulong Mix(ulong hash, ulong value)
        {
            hash ^= value;
            return hash * 1099511628211UL;
        }

        private static bool ContainsCyrillic(string value)
        {
            return !string.IsNullOrEmpty(value)
                && value.Any(character =>
                    character >= '\u0400'
                    && character <= '\u04ff');
        }

        private static string Canonical(string value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : new string(value
                    .Where(char.IsLetterOrDigit)
                    .Select(char.ToLowerInvariant)
                    .ToArray());
        }

        public sealed class RequiredRelayNode : NodeBase
        {
            public override string Title => "Required Relay";
            public override PortDefinition[] Inputs =>
                new[] { PortDefinition.Input<bool[,]>("Source", "in.source") };
            public override PortDefinition[] Outputs =>
                new[] { PortDefinition.Output<bool[,]>("Mask", "out.mask") };
            public override NodeOutput Execute(object[] inputs, NodeContext context) =>
                NodeOutput.Success(inputs[0]);
        }

        public sealed class WrongOutputCountNode : NodeBase
        {
            public override string Title => "Wrong Output Count";
            public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
            public override PortDefinition[] Outputs =>
                new[] { PortDefinition.Output<int>("Value", "out.value") };
            public override NodeOutput Execute(object[] inputs, NodeContext context) =>
                NodeOutput.Success(1, 2);
        }

        public sealed class WrongOutputTypeNode : NodeBase
        {
            public override string Title => "Wrong Output Type";
            public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
            public override PortDefinition[] Outputs =>
                new[] { PortDefinition.Output<float[,]>("Map", "out.map") };
            public override NodeOutput Execute(object[] inputs, NodeContext context) =>
                NodeOutput.Success(new bool[4, 3]);
        }

        public sealed class WrongOutputSizeNode : NodeBase
        {
            public override string Title => "Wrong Output Size";
            public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
            public override PortDefinition[] Outputs =>
                new[] { PortDefinition.Output<float[,]>("Map", "out.map") };
            public override NodeOutput Execute(object[] inputs, NodeContext context) =>
                NodeOutput.Success(new float[2, 2]);
        }

        public sealed class NullOutputNode : NodeBase
        {
            public override string Title => "Null Output";
            public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();
            public override PortDefinition[] Outputs =>
                new[] { PortDefinition.Output<string>("Value", "out.value") };
            public override NodeOutput Execute(object[] inputs, NodeContext context) =>
                NodeOutput.Success((object)null);
        }

        public sealed class IdentityBlueprintModifier : BlueprintModifier
        {
            public override HashSet<Vector2> Execute(
                HashSet<Vector2> positions,
                BlueprintLayer layer) =>
                positions;
        }
    }
}
