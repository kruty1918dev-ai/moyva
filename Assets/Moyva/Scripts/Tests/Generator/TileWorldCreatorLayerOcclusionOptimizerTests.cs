using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Generator
{
    [TestFixture]
    public sealed class TileWorldCreatorLayerOcclusionOptimizerTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int index = 0; index < _createdObjects.Count; index++)
            {
                if (_createdObjects[index] != null)
                    Object.DestroyImmediate(_createdObjects[index]);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void CullOccludedTileCells_UpperBuildLayerWinsSharedCells()
        {
            var lower = CreateBlueprintLayer("Lower", new Vector2(0, 0), new Vector2(1, 0), new Vector2(2, 0));
            var middle = CreateBlueprintLayer("Middle", new Vector2(1, 0), new Vector2(1, 1));
            var upper = CreateBlueprintLayer("Upper", new Vector2(1, 1), new Vector2(2, 0));
            var configuration = CreateConfiguration(lower, middle, upper);

            var result = TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(configuration);

            Assert.AreEqual(3, result.ProcessedLayerCount);
            Assert.AreEqual(3, result.RemovedCellCount);
            Assert.That(lower.allPositions, Is.EquivalentTo(new[] { new Vector2(0, 0) }));
            Assert.That(middle.allPositions, Is.EquivalentTo(new[] { new Vector2(1, 0) }));
            Assert.That(upper.allPositions, Is.EquivalentTo(new[] { new Vector2(1, 1), new Vector2(2, 0) }));
        }

        [Test]
        public void CullOccludedTileCells_PreservesNonOverlappingCells()
        {
            var lower = CreateBlueprintLayer("Lower", new Vector2(0, 0), new Vector2(1, 0));
            var upper = CreateBlueprintLayer("Upper", new Vector2(2, 0), new Vector2(3, 0));
            var configuration = CreateConfiguration(lower, upper);

            var result = TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(configuration);

            Assert.AreEqual(2, result.ProcessedLayerCount);
            Assert.AreEqual(0, result.RemovedCellCount);
            Assert.That(lower.allPositions, Is.EquivalentTo(new[] { new Vector2(0, 0), new Vector2(1, 0) }));
            Assert.That(upper.allPositions, Is.EquivalentTo(new[] { new Vector2(2, 0), new Vector2(3, 0) }));
        }

        [Test]
        public void CullOccludedTileCells_RoundsPositionsToLogicalGridCells()
        {
            var lower = CreateBlueprintLayer("Lower", new Vector2(1.04f, 1.04f));
            var upper = CreateBlueprintLayer("Upper", new Vector2(1f, 1f));
            var configuration = CreateConfiguration(lower, upper);

            var result = TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(configuration);

            Assert.AreEqual(1, result.RemovedCellCount);
            Assert.IsEmpty(lower.allPositions);
            Assert.That(upper.allPositions, Is.EquivalentTo(new[] { new Vector2(1f, 1f) }));
        }

        [Test]
        public void CullOccludedTileCells_NullOrSingleLayer_NoOps()
        {
            var nullResult = TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(null);
            Assert.AreEqual(0, nullResult.ProcessedLayerCount);
            Assert.AreEqual(0, nullResult.RemovedCellCount);

            var layer = CreateBlueprintLayer("Only", new Vector2(0, 0));
            var configuration = CreateConfiguration(layer);
            var singleResult = TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(configuration);

            Assert.AreEqual(1, singleResult.ProcessedLayerCount);
            Assert.AreEqual(0, singleResult.RemovedCellCount);
            Assert.That(layer.allPositions, Is.EquivalentTo(new[] { new Vector2(0, 0) }));
        }

        private Configuration CreateConfiguration(params BlueprintLayer[] orderedLayers)
        {
            var configuration = CreateScriptableObject<Configuration>();
            configuration.blueprintLayerFolders = new List<BlueprintLayerFolder> { new BlueprintLayerFolder("Root") };
            configuration.buildLayerFolders = new List<BuildLayerFolder> { new BuildLayerFolder("Root") };

            for (int index = 0; index < orderedLayers.Length; index++)
            {
                var blueprint = orderedLayers[index];
                var buildLayer = CreateScriptableObject<TilesBuildLayer>();
                buildLayer.layerName = $"Build {blueprint.layerName}";
                buildLayer.isEnabled = true;
                buildLayer.currentBlueprintLayer = blueprint;
                buildLayer.assignedBlueprintLayerGuid = blueprint.guid;
                buildLayer.configuration = configuration;

                configuration.blueprintLayerFolders[0].blueprintLayers.Add(blueprint);
                configuration.buildLayerFolders[0].buildLayers.Add(buildLayer);
            }

            return configuration;
        }

        private BlueprintLayer CreateBlueprintLayer(string layerName, params Vector2[] positions)
        {
            var layer = CreateScriptableObject<BlueprintLayer>();
            layer.layerName = layerName;
            layer.isEnabled = true;
            layer.allPositions = new HashSet<Vector2>(positions);
            return layer;
        }

        private T CreateScriptableObject<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            _createdObjects.Add(asset);
            return asset;
        }
    }
}
