using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Editor;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Generator
{
    public sealed class MoyvaOverlayGenerationIntegrationTests
    {
        private const string TestFolder =
            "Assets/Moyva/Scripts/Tests/Generator/__OverlayGenerationIntegrationTemp";
        private const string GraphPath = TestFolder + "/OverlayGraph.asset";
        private const string MaterialPath = TestFolder + "/OverlayMaterial.mat";
        private const string PrefabPath = TestFolder + "/OverlayTile.prefab";
        private const string ChunkRootName = "MapVisualChunks";

        [Test]
        public void GenerateMoyvaMap_BuildsChunks_CleansLegacyOutput_AndRestoresBindingToggles()
        {
            GameObject managerObject = null;
            GameObject generatedChunkRoot = null;
            GameObject prefabSource = null;
            Object previousSelection = Selection.activeObject;
            int previousGlobalSeed = GlobalSeed.Current;
            Random.State previousRandomState = Random.state;
            GameObject preservedChunkRoot = GameObject.Find(ChunkRootName);
            string preservedChunkRootName = null;

            try
            {
                GameLaunchContext.Reset();
                GameLaunchContext.ConfigureMenuNewGame(
                    0,
                    "Overlay integration",
                    314159,
                    0,
                    0,
                    0,
                    1,
                    false,
                    1,
                    1);
                if (preservedChunkRoot != null)
                {
                    preservedChunkRootName = preservedChunkRoot.name;
                    preservedChunkRoot.name =
                        ChunkRootName + "__PreservedDuringOverlayIntegrationTest";
                }

                RecreateTestFolder();
                Material material = CreateMaterial();
                GameObject prefab = CreateTilePrefab(material, out prefabSource);
                GraphAsset graph = CreateGraph(prefab);
                ConfigureCompanionMapSize(graph);

                managerObject = new GameObject("Moyva Overlay Integration Manager");
                var manager =
                    managerObject.AddComponent<TileWorldCreatorManager>();
                var binding = managerObject
                    .AddComponent<MoyvaTileWorldCreatorGraphBinding>();
                binding.SetGraphAsset(graph);
                binding.SetEditorSeed(314159);
                binding.SetCompileBeforeGenerate(false);
                binding.SetGenerateBuildLayersAfterCompile(false);

                SeedLegacyOutput(manager.transform);
                Assert.AreEqual(
                    1,
                    manager.GetComponentsInChildren<LayerIdentifier>(true).Length);
                Assert.AreEqual(
                    1,
                    manager.GetComponentsInChildren<ClusterIdentifier>(true).Length);

                bool generated =
                    MoyvaTileWorldCreatorManagerInspectorBridge
                        .TryGenerateFromOverlay(manager);

                Assert.IsTrue(generated);
                Assert.IsFalse(binding.CompileBeforeGenerate);
                Assert.IsFalse(binding.GenerateBuildLayersAfterCompile);

                generatedChunkRoot = GameObject.Find(ChunkRootName);
                Assert.IsNotNull(generatedChunkRoot);
                Assert.AreNotSame(preservedChunkRoot, generatedChunkRoot);

                MeshFilter terrainFilter = generatedChunkRoot
                    .GetComponentsInChildren<MeshFilter>(true)
                    .FirstOrDefault(filter =>
                        filter != null
                        && filter.name == "TerrainMesh"
                        && filter.sharedMesh != null);
                Assert.IsNotNull(
                    terrainFilter,
                    "Generate Moyva Map must emit a chunk-first TerrainMesh.");
                Assert.Greater(terrainFilter.sharedMesh.vertexCount, 0);

                Assert.IsEmpty(
                    manager.GetComponentsInChildren<LayerIdentifier>(true),
                    "Legacy TWC layer output must be removed from the graph-managed manager.");
                Assert.IsEmpty(
                    manager.GetComponentsInChildren<ClusterIdentifier>(true),
                    "Legacy TWC cluster output must be removed from the graph-managed manager.");
                Assert.IsEmpty(
                    generatedChunkRoot.GetComponentsInChildren<LayerIdentifier>(true));
                Assert.IsEmpty(
                    generatedChunkRoot.GetComponentsInChildren<ClusterIdentifier>(true));
            }
            finally
            {
                if (prefabSource != null)
                    Object.DestroyImmediate(prefabSource);

                generatedChunkRoot ??= GameObject.Find(ChunkRootName);
                DestroyGeneratedChunkRoot(generatedChunkRoot);

                if (managerObject != null)
                    Object.DestroyImmediate(managerObject);

                if (preservedChunkRoot != null)
                    preservedChunkRoot.name = preservedChunkRootName;

                GameLaunchContext.Reset();
                GlobalSeed.Set(previousGlobalSeed);
                Random.state = previousRandomState;
                Selection.activeObject = previousSelection;
                AssetDatabase.DeleteAsset(TestFolder);
                AssetDatabase.SaveAssets();
            }
        }

        private static void RecreateTestFolder()
        {
            AssetDatabase.DeleteAsset(TestFolder);
            string guid = AssetDatabase.CreateFolder(
                "Assets/Moyva/Scripts/Tests/Generator",
                "__OverlayGenerationIntegrationTemp");
            Assert.IsFalse(string.IsNullOrWhiteSpace(guid));
        }

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                            ?? Shader.Find("Standard");
            Assert.IsNotNull(shader, "A test shader is required.");

            var material = new Material(shader)
            {
                name = "Overlay Integration Material"
            };
            AssetDatabase.CreateAsset(material, MaterialPath);
            return material;
        }

        private static GameObject CreateTilePrefab(
            Material material,
            out GameObject prefabSource)
        {
            prefabSource = GameObject.CreatePrimitive(PrimitiveType.Plane);
            prefabSource.name = "Overlay Integration Tile";
            prefabSource.transform.localScale = Vector3.one * 0.1f;
            prefabSource.GetComponent<MeshRenderer>().sharedMaterial = material;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(
                prefabSource,
                PrefabPath);
            Assert.IsNotNull(prefab);
            Object.DestroyImmediate(prefabSource);
            prefabSource = null;
            return prefab;
        }

        private static GraphAsset CreateGraph(GameObject prefab)
        {
            var graph = ScriptableObject.CreateInstance<GraphAsset>();
            graph.name = "Overlay Generation Integration Graph";
            AssetDatabase.CreateAsset(graph, GraphPath);

            string layerId = graph.EnsureDefaultLayer();
            var mask = graph.AddNode(
                typeof(GraphPreviewFullMapMaskNode),
                false,
                layerId);
            var tileSettings = graph.AddNode(
                typeof(TileSettingsNode),
                false,
                layerId) as TileSettingsNode;
            var output = graph.AddNode(
                typeof(OutputNode),
                false,
                layerId) as OutputNode;

            Assert.IsNotNull(mask);
            Assert.IsNotNull(tileSettings);
            Assert.IsNotNull(output);

            TilePreset preset = CreatePreset(graph, prefab);
            FieldInfo variantsField = typeof(TileSettingsNode).GetField(
                "_tileVariants",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(variantsField);
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
            graph.AddConnection(
                tileSettings.NodeId,
                0,
                output.NodeId,
                OutputNode.MaskInputIndex);

            EditorUtility.SetDirty(tileSettings);
            EditorUtility.SetDirty(output);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
            return graph;
        }

        private static TilePreset CreatePreset(
            GraphAsset graph,
            GameObject prefab)
        {
            var preset = ScriptableObject.CreateInstance<TilePreset>();
            preset.name = "Overlay Generation Integration Preset";
            preset.tileId = "overlay-integration-tile";
            preset.gridtype = TilePreset.GridType.standard;
            preset.NRMGRD_singleTile = prefab;

            AssetDatabase.AddObjectToAsset(preset, graph);
            EditorUtility.SetDirty(preset);
            return preset;
        }

        private static void ConfigureCompanionMapSize(GraphAsset graph)
        {
            Configuration configuration =
                GraphBuildLayerStore.GetCompanionConfiguration(graph, true);
            Assert.IsNotNull(configuration);
            configuration.width = 1;
            configuration.height = 1;
            configuration.cellSize = 1f;
            EditorUtility.SetDirty(configuration);
            AssetDatabase.SaveAssets();
        }

        private static void SeedLegacyOutput(Transform manager)
        {
            var layer = new GameObject("Legacy Layer");
            layer.transform.SetParent(manager, false);
            layer.AddComponent<LayerIdentifier>().guid = "legacy-layer";

            var cluster = new GameObject("Legacy Orphan Cluster");
            cluster.transform.SetParent(manager, false);
            cluster.AddComponent<ClusterIdentifier>().layerGuid =
                "legacy-layer";
        }

        private static void DestroyGeneratedChunkRoot(GameObject root)
        {
            if (root == null)
                return;

            MeshFilter[] filters =
                root.GetComponentsInChildren<MeshFilter>(true);
            var meshes = new HashSet<Mesh>();
            for (int i = 0; i < filters.Length; i++)
            {
                Mesh mesh = filters[i] != null
                    ? filters[i].sharedMesh
                    : null;
                if (mesh != null && !AssetDatabase.Contains(mesh))
                    meshes.Add(mesh);
                if (filters[i] != null)
                    filters[i].sharedMesh = null;
            }

            MeshCollider[] colliders =
                root.GetComponentsInChildren<MeshCollider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                    colliders[i].sharedMesh = null;
            }

            Object.DestroyImmediate(root);
            foreach (Mesh mesh in meshes)
            {
                if (mesh != null)
                    Object.DestroyImmediate(mesh);
            }
        }
    }
}
