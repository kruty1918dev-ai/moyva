using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Generator
{
    [TestFixture]
    public sealed class BlueprintLayerExpansionTests
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
        public void GeneratorLayerDefinition_ClampsExtraSizeCells()
        {
            var layer = new GeneratorLayerDefinition("Water");

            layer.ExtraWidthCells = -10;
            layer.ExtraLengthCells = -5;

            Assert.AreEqual(0, layer.ExtraWidthCells);
            Assert.AreEqual(0, layer.ExtraLengthCells);
        }

        [Test]
        public void Configuration_UsesSeparateBlueprintLayerExpansionAxes()
        {
            var configuration = CreateScriptableObject<Configuration>();
            configuration.width = 20;
            configuration.height = 12;

            var layer = CreateScriptableObject<BlueprintLayer>();
            layer.borderPaddingWidthCells = 8;
            layer.borderPaddingHeightCells = 4;

            Assert.AreEqual(28, configuration.GetBlueprintLayerWidth(layer));
            Assert.AreEqual(16, configuration.GetBlueprintLayerHeight(layer));
            Assert.AreEqual(8, configuration.GetBlueprintLayerPaddingCells(layer));
        }

        [Test]
        public void ExecuteLayer_WithExpansion_CentersGeneratedPositionsAroundMap()
        {
            var configuration = CreateScriptableObject<Configuration>();
            configuration.width = 4;
            configuration.height = 4;
            configuration.useGlobalRandomSeed = true;
            configuration.globalRandomSeed = 1;

            var layer = CreateScriptableObject<BlueprintLayer>();
            layer.borderPaddingWidthCells = 4;
            layer.borderPaddingHeightCells = 2;
            layer.tileMapModifiers.Add(CreateScriptableObject<FillCurrentMapModifier>());

            layer.ExecuteLayer(configuration, null);

            Assert.That(layer.allPositions, Does.Contain(new Vector2(-2, -1)));
            Assert.That(layer.allPositions, Does.Contain(new Vector2(5, 4)));
            Assert.That(layer.allPositions, Does.Contain(new Vector2(0, 0)));
            Assert.AreEqual(48, layer.allPositions.Count);
            Assert.AreEqual(4, configuration.width);
            Assert.AreEqual(4, configuration.height);
        }

        private T CreateScriptableObject<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            _createdObjects.Add(asset);
            return asset;
        }

        private sealed class FillCurrentMapModifier : BlueprintModifier
        {
            public override HashSet<Vector2> Execute(HashSet<Vector2> positions, BlueprintLayer layer)
            {
                var result = new HashSet<Vector2>();
                for (int x = 0; x < asset.width; x++)
                {
                    for (int y = 0; y < asset.height; y++)
                        result.Add(new Vector2(x, y));
                }

                return result;
            }
        }
    }
}
