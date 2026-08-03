using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Editor;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kruty1918.Moyva.Tests.Generator
{
    public sealed class GraphPreviewParityTests
    {
        private const string TestGraphPath =
            "Assets/Moyva/Scripts/Tests/Generator/GraphPreviewParityTestGraph.asset";
        private const string TestConfigurationPath =
            "Assets/Moyva/Scripts/Tests/Generator/GraphPreviewParityTestConfiguration.asset";

        private int _previousGlobalSeed;
        private Random.State _previousRandomState;

        [SetUp]
        public void SetUp()
        {
            _previousGlobalSeed = GlobalSeed.Current;
            _previousRandomState = Random.state;
            GameLaunchContext.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            GameLaunchContext.Reset();
            AssetDatabase.DeleteAsset(TestGraphPath);
            AssetDatabase.DeleteAsset(TestConfigurationPath);
            GlobalSeed.Set(_previousGlobalSeed);
            Random.state = _previousRandomState;
        }

        [Test]
        public void DeterministicSeed_InitializesGlobalAndUnityRandomTogether()
        {
            const int seed = 17391;
            Random.InitState(seed);
            float expectedFirstValue = Random.value;

            GlobalSeed.Set(991);
            Random.InitState(992);

            int effectiveSeed = GlobalSeed.InitializeDeterministic(seed);
            float actualFirstValue = Random.value;

            Assert.AreEqual(seed, effectiveSeed);
            Assert.AreEqual(seed, GlobalSeed.Current);
            Assert.AreEqual(expectedFirstValue, actualFirstValue);
        }

        [Test]
        public void CompilerEntryPoint_RestoresGlobalAndUnityRandomState()
        {
            GlobalSeed.Set(444);
            Random.InitState(445);
            Random.State expectedRandomState = Random.state;

            IReadOnlyList<CompiledLayerMap> result =
                GraphToConfigurationCompiler.Compile(null, null, 0);

            Assert.NotNull(result);
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(444, GlobalSeed.Current);
            Assert.AreEqual(expectedRandomState, Random.state);
        }

        [Test]
        public void FullPreview_PreservesRectangularMapAtOnePixelPerTile()
        {
            Vector2Int dimensions = GraphEditorWindow.ResolvePreviewSize(2, 317, 113);
            var mask = new bool[dimensions.x, dimensions.y];
            mask[dimensions.x - 1, dimensions.y - 1] = true;

            Texture2D texture = NodePreviewTextureFactory.TryBuild(
                new object[] { mask },
                dimensions.x,
                dimensions.y,
                out bool ownsTexture,
                out _);

            try
            {
                Assert.NotNull(texture);
                Assert.IsTrue(ownsTexture);
                Assert.AreEqual(317, texture.width);
                Assert.AreEqual(113, texture.height);
                Assert.Greater(
                    texture.GetPixel(dimensions.x - 1, dimensions.y - 1).g,
                    texture.GetPixel(0, 0).g);
            }
            finally
            {
                if (texture != null)
                    Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void SceneParityPreview_UsesTransientTwcPathAndPreservesExactTileCount()
        {
            GraphAsset graph = CreateRenderableGraph(out string layerId);
            var requestedSize = new Vector2Int(17, 9);

            GlobalSeed.Set(2468);
            Random.InitState(1357);
            Random.State randomStateBeforePreview = Random.state;

            bool success = SceneParityLayerPreviewBuilder.TryBuildLayerMatrices(
                graph,
                97531,
                requestedSize,
                null,
                out Dictionary<string, bool[,]> matrices,
                out _,
                out int width,
                out int height,
                out string status);

            Assert.IsTrue(success, status);
            Assert.AreEqual(requestedSize.x, width);
            Assert.AreEqual(requestedSize.y, height);
            Assert.That(matrices, Does.ContainKey(layerId));

            bool[,] layerMatrix = matrices[layerId];
            Assert.AreEqual(requestedSize.x, layerMatrix.GetLength(0));
            Assert.AreEqual(requestedSize.y, layerMatrix.GetLength(1));
            Assert.AreEqual(
                requestedSize.x * requestedSize.y,
                layerMatrix.Cast<bool>().Count(value => value));

            Assert.AreEqual(2468, GlobalSeed.Current);
            Assert.AreEqual(randomStateBeforePreview, Random.state);
        }

        [Test]
        public void RuntimeGraphBindingCompile_UsesLaunchDimensionsInsteadOfFiftyByFiftyFallback()
        {
            GraphAsset graph = CreateRenderableGraph(out _);
            var configuration = ScriptableObject.CreateInstance<Configuration>();
            configuration.width = 7;
            configuration.height = 5;
            AssetDatabase.CreateAsset(configuration, TestConfigurationPath);

            var managerObject = new GameObject("Graph Preview Runtime Parity Test");
            try
            {
                var manager = managerObject.AddComponent<TileWorldCreatorManager>();
                manager.configuration = configuration;
                var context = new TestGraphBindingContext(manager, graph);

                GameLaunchContext.ConfigureMenuNewGame(
                    0,
                    "Preview parity",
                    86420,
                    0,
                    0,
                    0,
                    1,
                    false,
                    23,
                    11);

                var resolver = new MoyvaTwcGraphBindingResolver();
                Assert.AreEqual(new Vector2Int(23, 11), resolver.ResolveMapSize(context));

                var compiler = new MoyvaTwcGraphCompileService(
                    resolver,
                    new MoyvaTwcGraphValidationService());
                IReadOnlyList<CompiledLayerMap> compiled = compiler.Compile(
                    context,
                    86420,
                    false);

                Assert.IsNotEmpty(compiled);
                Assert.AreEqual(23, configuration.width);
                Assert.AreEqual(11, configuration.height);
            }
            finally
            {
                Object.DestroyImmediate(managerObject);
            }
        }

        [Test]
        public void GraphBindingGeneration_GlobalValidationErrorNeverCompilesOrBuildsCompanion()
        {
            var graph = ScriptableObject.CreateInstance<GraphAsset>();
            graph.EnsureDefaultLayer();
            var configuration = ScriptableObject.CreateInstance<Configuration>();
            var managerObject = new GameObject("Graph Binding Validation Failure Test");
            try
            {
                var manager =
                    managerObject.AddComponent<TileWorldCreatorManager>();
                manager.configuration = configuration;
                var context = new TestGraphBindingContext(
                    manager,
                    graph,
                    generateBuildLayersAfterCompile: true);
                var compiler = new StubGraphCompileService();
                var validation = new StubGraphValidationService(
                    hasGlobalErrors: true);
                var worldBuild = new CountingWorldBuildBridge();
                var service = new MoyvaTwcGraphBindingGenerationService(
                    new StubGraphBindingResolver(),
                    compiler,
                    validation,
                    worldBuild);

                LogAssert.Expect(
                    LogType.Error,
                    "[Moyva TWC Graph Binding] Generation stopped before compilation: graph validation contains 1 global error(s). Native or stale companion output was not built.");

                bool generated = service.GenerateFromGraph(context, 17);

                Assert.IsFalse(generated);
                Assert.AreEqual(0, compiler.CompileCalls);
                Assert.AreEqual(0, worldBuild.BuildCalls);
                Assert.IsFalse(context.IsGenerating);
            }
            finally
            {
                Object.DestroyImmediate(managerObject);
                Object.DestroyImmediate(configuration);
                Object.DestroyImmediate(graph);
            }
        }

        [Test]
        public void GraphBindingGeneration_EmptyCompileResultNeverBuildsStaleCompanion()
        {
            var graph = ScriptableObject.CreateInstance<GraphAsset>();
            string layerId = graph.EnsureDefaultLayer();
            var configuration = ScriptableObject.CreateInstance<Configuration>();
            var managerObject = new GameObject("Graph Binding Empty Compile Test");
            try
            {
                var manager =
                    managerObject.AddComponent<TileWorldCreatorManager>();
                manager.configuration = configuration;
                var context = new TestGraphBindingContext(
                    manager,
                    graph,
                    generateBuildLayersAfterCompile: true);
                context.SetLastCompiledLayers(new[]
                {
                    new CompiledLayerMap
                    {
                        GraphLayerId = layerId,
                        BlueprintLayerGuid = "stale-blueprint",
                        HasRenderableTileOutput = true
                    }
                });
                var compiler = new StubGraphCompileService(
                    System.Array.Empty<CompiledLayerMap>());
                var worldBuild = new CountingWorldBuildBridge();
                var service = new MoyvaTwcGraphBindingGenerationService(
                    new StubGraphBindingResolver(),
                    compiler,
                    new StubGraphValidationService(),
                    worldBuild);

                LogAssert.Expect(
                    LogType.Error,
                    "[Moyva TWC Graph Binding] Generation stopped: graph compilation produced no enabled authoritative renderable layer. Existing companion output was not built.");

                bool generated = service.GenerateFromGraph(context, 23);

                Assert.IsFalse(generated);
                Assert.AreEqual(1, compiler.CompileCalls);
                Assert.AreEqual(0, worldBuild.BuildCalls);
                Assert.IsEmpty(context.LastCompiledLayers);
                Assert.IsFalse(context.IsGenerating);
            }
            finally
            {
                Object.DestroyImmediate(managerObject);
                Object.DestroyImmediate(configuration);
                Object.DestroyImmediate(graph);
            }
        }

        private static GraphAsset CreateRenderableGraph(out string layerId)
        {
            AssetDatabase.DeleteAsset(TestGraphPath);
            var graph = ScriptableObject.CreateInstance<GraphAsset>();
            AssetDatabase.CreateAsset(graph, TestGraphPath);

            layerId = graph.EnsureDefaultLayer();
            var mask = graph.AddNode(typeof(GraphPreviewFullMapMaskNode), false, layerId);
            var tileSettings = graph.AddNode(typeof(TileSettingsNode), false, layerId) as TileSettingsNode;
            var output = graph.AddNode(typeof(OutputNode), false, layerId) as OutputNode;

            Assert.NotNull(mask);
            Assert.NotNull(tileSettings);
            Assert.NotNull(output);

            var preset = ScriptableObject.CreateInstance<TilePreset>();
            preset.name = "Graph Preview Parity Test Tile";
            preset.tileId = "graph-preview-parity-test";
            preset.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(preset, graph);

            FieldInfo variantsField = typeof(TileSettingsNode).GetField(
                "_tileVariants",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(variantsField);
            variantsField.SetValue(
                tileSettings,
                new List<TilePresetVariant>
                {
                    new()
                    {
                        Preset = preset,
                        Slot = TilePresetSlot.Top,
                        Weight = 1f
                    }
                });

            output.OutputKind = LayerOutputKind.Tiles;
            graph.AddConnection(mask.NodeId, 0, tileSettings.NodeId, 0);
            graph.AddConnection(tileSettings.NodeId, 0, output.NodeId, OutputNode.MaskInputIndex);
            EditorUtility.SetDirty(tileSettings);
            EditorUtility.SetDirty(output);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
            return graph;
        }

        private sealed class TestGraphBindingContext : IMoyvaTwcGraphBindingContext
        {
            private IReadOnlyList<CompiledLayerMap> _compiled = new List<CompiledLayerMap>();
            private readonly bool _compileBeforeGenerate;
            private readonly bool _generateBuildLayersAfterCompile;

            public TestGraphBindingContext(
                TileWorldCreatorManager manager,
                GraphAsset graph,
                bool compileBeforeGenerate = true,
                bool generateBuildLayersAfterCompile = false)
            {
                Manager = manager;
                GraphAsset = graph;
                _compileBeforeGenerate = compileBeforeGenerate;
                _generateBuildLayersAfterCompile =
                    generateBuildLayersAfterCompile;
            }

            public TileWorldCreatorManager Manager { get; }
            public GraphAsset GraphAsset { get; }
            public int EditorSeed => 1;
            public bool CompileBeforeGenerate => _compileBeforeGenerate;
            public bool GenerateBuildLayersAfterCompile =>
                _generateBuildLayersAfterCompile;
            public bool IsGenerating { get; private set; }
            public IReadOnlyList<CompiledLayerMap> LastCompiledLayers => _compiled;
            public Object LogContext => Manager;

            public void SetLastCompiledLayers(IReadOnlyList<CompiledLayerMap> layers)
            {
                _compiled = layers ?? new List<CompiledLayerMap>();
            }

            public void SetGenerating(bool value)
            {
                IsGenerating = value;
            }
        }

        private sealed class StubGraphBindingResolver :
            IMoyvaTwcGraphBindingResolver
        {
            public int ResolveSeed(IMoyvaTwcGraphBindingContext context) => 1;

            public Vector2Int ResolveMapSize(
                IMoyvaTwcGraphBindingContext context) => Vector2Int.one;

            public int NormalizeSeed(int seed) => GlobalSeed.Normalize(seed);
        }

        private sealed class StubGraphCompileService :
            IMoyvaTwcGraphCompileService
        {
            private readonly IReadOnlyList<CompiledLayerMap> _result;

            public StubGraphCompileService(
                IReadOnlyList<CompiledLayerMap> result = null)
            {
                _result = result ?? System.Array.Empty<CompiledLayerMap>();
            }

            public int CompileCalls { get; private set; }

            public IReadOnlyList<CompiledLayerMap> Compile(
                IMoyvaTwcGraphBindingContext context) =>
                Compile(context, 1, true);

            public IReadOnlyList<CompiledLayerMap> Compile(
                IMoyvaTwcGraphBindingContext context,
                int seed) => Compile(context, seed, true);

            public IReadOnlyList<CompiledLayerMap> Compile(
                IMoyvaTwcGraphBindingContext context,
                int seed,
                bool emitLayerLog)
            {
                CompileCalls++;
                context.SetLastCompiledLayers(_result);
                return _result;
            }
        }

        private sealed class StubGraphValidationService :
            IMoyvaTwcGraphValidationService
        {
            private readonly bool _hasGlobalErrors;

            public StubGraphValidationService(bool hasGlobalErrors = false)
            {
                _hasGlobalErrors = hasGlobalErrors;
            }

            public bool CanCompile(
                IMoyvaTwcGraphBindingContext context,
                out string reason)
            {
                reason = null;
                return true;
            }

            public GraphValidationReport Validate(GraphAsset graph) => null;

            public List<GraphValidationIssue> GetGlobalErrors(
                GraphValidationReport report) => _hasGlobalErrors
                ? new List<GraphValidationIssue> { null }
                : new List<GraphValidationIssue>();

            public HashSet<string> GetInvalidLayerIds(
                GraphValidationReport report) => new HashSet<string>();
        }

        private sealed class CountingWorldBuildBridge :
            ITileWorldCreatorWorldBuildBridge
        {
            public int BuildCalls { get; private set; }

            public TileWorldCreatorWorldBuildResult Build(
                GeneratedWorldData worldData)
            {
                BuildCalls++;
                return TileWorldCreatorWorldBuildResult.Disabled;
            }
        }

    }
}
