using System.Collections.Generic;
using System.IO;
using System.Linq;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Editor;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.SaveSystem;

namespace Kruty1918.Moyva.Tests.Generator
{
    public sealed class ChunkFirstHeightAuditIntegrationTests
    {
        private const string SourceGraphPath =
            "Assets/Moyva/SO/Generation/Prototype/TestGeneratorGraph.asset";
        private const string TestFolder =
            "Assets/Moyva/Scripts/Tests/Generator/__ChunkFirstHeightAuditTemp";
        private const string GraphCopyPath =
            TestFolder + "/TestGeneratorGraph_Audit.asset";
        private const string ChunkRootName = "MapVisualChunks";

        [Test]
        public void TestGeneratorGraph_HeightAudit_ReportsLayerPlacement()
        {
            GameObject managerObject = null;
            GameObject generatedChunkRoot = null;
            GameObject preservedChunkRoot = GameObject.Find(ChunkRootName);
            string preservedChunkRootName = null;
            int previousGlobalSeed = GlobalSeed.Current;
            Random.State previousRandomState = Random.state;

            try
            {
                GraphAsset graph = PrepareGraphCopy();
                Assert.IsNotNull(graph);

                GameLaunchContext.Reset();
                GameLaunchContext.ConfigureMenuNewGame(
                    0,
                    "Height audit",
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
                        ChunkRootName + "__PreservedDuringHeightAudit";
                }

                managerObject = new GameObject("Chunk First Height Audit Manager");
                var manager = managerObject.AddComponent<TileWorldCreatorManager>();
                var binding =
                    managerObject.AddComponent<MoyvaTileWorldCreatorGraphBinding>();
                binding.SetGraphAsset(graph);
                binding.SetEditorSeed(314159);
                binding.SetCompileBeforeGenerate(false);
                binding.SetGenerateBuildLayersAfterCompile(false);

                bool generated =
                    MoyvaTileWorldCreatorManagerInspectorBridge
                        .TryGenerateFromOverlay(manager);

                Assert.IsTrue(generated);
                generatedChunkRoot = GameObject.Find(ChunkRootName);
                Assert.IsNotNull(generatedChunkRoot);

                MeshFilter[] chunkMeshes = generatedChunkRoot
                    .GetComponentsInChildren<MeshFilter>(true)
                    .Where(filter =>
                        filter != null
                        && filter.name == "TerrainMesh"
                        && filter.sharedMesh != null)
                    .ToArray();
                Assert.AreEqual(49, chunkMeshes.Length);

                IReadOnlyList<string> lines =
                    ChunkFirstHeightAudit.SnapshotLines();
                foreach (string line in lines)
                    TestContext.Progress.WriteLine(line);

                Assert.That(lines.Any(line => line.Contains("layer=Water")));
                Assert.That(lines.Any(line => line.Contains("layer=Layer\n")));
                Assert.That(lines.Any(line => line.Contains("layer=Layer 4")));
            }
            finally
            {
                if (generatedChunkRoot != null)
                    Object.DestroyImmediate(generatedChunkRoot);
                else
                {
                    GameObject lateChunkRoot = GameObject.Find(ChunkRootName);
                    if (lateChunkRoot != null)
                        Object.DestroyImmediate(lateChunkRoot);
                }

                if (managerObject != null)
                    Object.DestroyImmediate(managerObject);

                if (preservedChunkRoot != null)
                    preservedChunkRoot.name = preservedChunkRootName;

                GameLaunchContext.Reset();
                GlobalSeed.Set(previousGlobalSeed);
                Random.state = previousRandomState;
                AssetDatabase.DeleteAsset(TestFolder);
                AssetDatabase.SaveAssets();
            }
        }

        private static GraphAsset PrepareGraphCopy()
        {
            AssetDatabase.DeleteAsset(TestFolder);
            string guid = AssetDatabase.CreateFolder(
                "Assets/Moyva/Scripts/Tests/Generator",
                Path.GetFileName(TestFolder));
            Assert.IsFalse(string.IsNullOrWhiteSpace(guid));
            Assert.IsTrue(
                AssetDatabase.CopyAsset(SourceGraphPath, GraphCopyPath));
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(
                GraphCopyPath,
                ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<GraphAsset>(GraphCopyPath);
        }
    }
}
