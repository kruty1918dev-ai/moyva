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

            public TestGraphBindingContext(
                TileWorldCreatorManager manager,
                GraphAsset graph)
            {
                Manager = manager;
                GraphAsset = graph;
            }

            public TileWorldCreatorManager Manager { get; }
            public GraphAsset GraphAsset { get; }
            public int EditorSeed => 1;
            public bool CompileBeforeGenerate => true;
            public bool GenerateBuildLayersAfterCompile => false;
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

    }
}
