using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Generator
{
    [TestFixture]
    public sealed class ObjectPlacementAdapterTests
    {
        private const string TestAssetPath = "Assets/Moyva/Scripts/Tests/Generator/ObjectPlacementAdapterTestConfig.asset";

        private readonly List<Object> _createdObjects = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TestAssetPath);

            for (int index = 0; index < _createdObjects.Count; index++)
            {
                if (_createdObjects[index] != null)
                    Object.DestroyImmediate(_createdObjects[index]);
            }

            _createdObjects.Clear();
        }

        [Test]
        public void Apply_CreatesTWCObjectBuildLayerFromPlacementLayer()
        {
            var configuration = CreateConfigurationAsset();
            configuration.width = 4;
            configuration.height = 4;
            configuration.cellSize = 2f;

            var managerObject = new GameObject("TWC Manager");
            _createdObjects.Add(managerObject);
            var manager = managerObject.AddComponent<TileWorldCreatorManager>();
            manager.configuration = configuration;

            var prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = "Grass Card";
            _createdObjects.Add(prefab);

            var placementLayer = new ObjectPlacementLayer("Grass")
            {
                Rule = new ObjectPlacementRule
                {
                    Jitter = 0.25f,
                    RotationRandomization = 90f,
                    ScaleRandomization = new Vector2(0.8f, 1.2f),
                    UseTWCObjectLayer = true
                }
            };
            placementLayer.Prefabs.Add(new ObjectPrefabEntry
            {
                Prefab = prefab,
                Weight = 1f,
                MinScale = 0.9f,
                MaxScale = 1.1f
            });
            placementLayer.Candidates.Add(new ScatterCandidate(new Vector2Int(1, 2), Vector2.zero, 1f, 0f, 1f));
            placementLayer.Candidates.Add(new ScatterCandidate(new Vector2Int(20, 20), Vector2.zero, 1f, 0f, 1f));

            TWCObjectPlacementAdapter.Apply(configuration, manager, new[] { placementLayer });

            string generatedName = TWCObjectPlacementAdapter.BuildGeneratedLayerName("Grass");
            var blueprint = configuration.GetBlueprintLayerByGuid(configuration.GetBlueprintLayerGuid(generatedName));
            Assert.NotNull(blueprint);
            Assert.That(blueprint.allPositions, Is.EquivalentTo(new[] { new Vector2(1, 2) }));

            var buildLayer = configuration.GetBuildLayerByGuid(configuration.GetBuildLayerGuid(generatedName)) as ObjectBuildLayer;
            Assert.NotNull(buildLayer);
            Assert.AreEqual(blueprint.guid, buildLayer.assignedBlueprintLayerGuid);
            Assert.AreEqual(1, buildLayer.prefabObjects.Count);
            Assert.AreEqual(prefab, buildLayer.prefabObjects[0].prefabObject);
            Assert.AreEqual(0.5f, buildLayer.objectRNDPositionOffsetRadius);
            Assert.IsTrue(buildLayer.useRndRotation);
            Assert.IsTrue(buildLayer.useRndScale);
        }

        [Test]
        public void Apply_RemovesStaleGeneratedObjectLayers()
        {
            var configuration = CreateConfigurationAsset();
            var managerObject = new GameObject("TWC Manager");
            _createdObjects.Add(managerObject);
            var manager = managerObject.AddComponent<TileWorldCreatorManager>();
            manager.configuration = configuration;

            var generatedBlueprint = manager.AddNewBlueprintLayer(
                TWCObjectPlacementAdapter.BuildGeneratedLayerName("Old Grass"));
            var generatedBuildLayer = manager.AddNewBuildLayer<ObjectBuildLayer>(
                TWCObjectPlacementAdapter.BuildGeneratedLayerName("Old Grass"));

            TWCObjectPlacementAdapter.Apply(configuration, manager, System.Array.Empty<ObjectPlacementLayer>());

            Assert.IsFalse(configuration.blueprintLayerFolders[0].blueprintLayers.Contains(generatedBlueprint));
            Assert.IsFalse(configuration.buildLayerFolders[0].buildLayers.Contains(generatedBuildLayer));
        }

        private Configuration CreateConfigurationAsset()
        {
            AssetDatabase.DeleteAsset(TestAssetPath);
            var configuration = ScriptableObject.CreateInstance<Configuration>();
            _createdObjects.Add(configuration);
            AssetDatabase.CreateAsset(configuration, TestAssetPath);
            configuration.blueprintLayerFolders = new List<BlueprintLayerFolder> { new BlueprintLayerFolder("Root") };
            configuration.buildLayerFolders = new List<BuildLayerFolder> { new BuildLayerFolder("Root") };
            return configuration;
        }
    }
}
