using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Generator
{
    [TestFixture]
    public sealed class TileWorldCreatorTerrainBuildPolicyTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _createdObjects.Count; i++)
            {
                if (_createdObjects[i] != null)
                    Object.DestroyImmediate(_createdObjects[i]);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void Apply_MergedMode_ForcesChunkSizeAndMergedTerrainLayers()
        {
            var config = ScriptableObject.CreateInstance<Configuration>();
            _createdObjects.Add(config);
            config.width = 16;
            config.height = 16;
            config.mergeTiles = false;
            config.clusterCellSize = 5;
            config.buildLayerFolders = new List<BuildLayerFolder> { new BuildLayerFolder("Root") };

            var layer = ScriptableObject.CreateInstance<MoyvaTerrainHeightAwareTilesBuildLayer>();
            _createdObjects.Add(layer);
            layer.isEnabled = true;
            layer.meshGenerationOverride = false;
            layer.mergeTiles = false;
            config.buildLayerFolders[0].buildLayers.Add(layer);

            var policy = TileWorldCreatorTerrainBuildPolicy.Resolve(new TileWorldCreatorBuildOptions(), 8);
            TileWorldCreatorTerrainBuildPolicy.Apply(config, policy, "test");

            Assert.AreEqual(8, config.clusterCellSize);
            Assert.IsTrue(config.mergeTiles);
            Assert.IsTrue(layer.meshGenerationOverride);
            Assert.IsTrue(layer.mergeTiles);
            Assert.IsTrue(policy.UsesPrecomputedHeights);
            Assert.IsFalse(policy.UsesLegacyHeightProjection);
        }

        [Test]
        public void HeightContext_UsesMaxContributingLevelForDualGridTiles()
        {
            var gameObject = new GameObject("Height Context");
            _createdObjects.Add(gameObject);
            var context = gameObject.AddComponent<MoyvaTerrainHeightContext>();
            context.Configure(
                new[,]
                {
                    { 1, 2 },
                    { 3, 4 }
                },
                2);

            bool found = context.TryGetTileHeight(
                new Vector2(0.5f, 0.5f),
                true,
                new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) },
                out float height);

            Assert.IsTrue(found);
            Assert.AreEqual(8f, height);
        }
    }
}
