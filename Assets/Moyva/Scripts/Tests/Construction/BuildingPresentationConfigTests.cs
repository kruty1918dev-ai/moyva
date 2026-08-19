using Kruty1918.Moyva.Construction.API;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class BuildingPresentationConfigTests
    {
        private GameObject _prefab;
        private GameObject _previewPrefab;
        private GameObject _constructionPrefab;

        [TearDown]
        public void TearDown()
        {
            if (_prefab != null)
                Object.DestroyImmediate(_prefab);
            if (_previewPrefab != null)
                Object.DestroyImmediate(_previewPrefab);
            if (_constructionPrefab != null)
                Object.DestroyImmediate(_constructionPrefab);
        }

        [Test]
        public void ToRuntimeDefinition_CopiesPresentationIntoRuntimeSnapshot()
        {
            _prefab = new GameObject("placed-prefab");
            _previewPrefab = new GameObject("preview-prefab");
            _constructionPrefab = new GameObject("construction-prefab");
            var asset = new BuildingDefinitionAsset
            {
                Identity =
                    new BuildingIdentity
                    {
                        Id = "test-building",
                        DisplayName = "Test Building",
                    },
                Presentation =
                    new BuildingPresentation
                    {
                        Prefab = _prefab,
                        Variants = new BuildingPresentationVariants
                        {
                            ConstructionPrefab = _constructionPrefab,
                        },
                        PreviewPrefab = _previewPrefab,
                        VisualYOffset = 0.25f,
                        GroundOffsetY = 0.75f,
                        ScaleMultiplier = 1.5f,
                        PositionOffset = new Vector3(1f, 2f, 3f),
                    },
            };

            BuildingDefinition runtime = asset.ToRuntimeDefinition();

            Assert.AreSame(_prefab, runtime.Prefab);
            Assert.AreSame(_constructionPrefab, runtime.ResolveConstructionPrefab());
            Assert.AreSame(_previewPrefab, runtime.ResolvePreviewPrefab());
            Assert.AreEqual(0.75f, runtime.ResolveVisualYOffset(), 0.0001f);
            Assert.AreNotSame(asset.Presentation, runtime.Presentation);
            Assert.AreEqual(1.5f, runtime.Presentation.ScaleMultiplier, 0.0001f);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), runtime.Presentation.PositionOffset);
        }

        [Test]
        public void ResolveVisualYOffset_FallsBackToLegacyVisualYOffset()
        {
            var runtime = new BuildingDefinition
            {
                VisualYOffset = 0.35f,
            };
            runtime.Presentation.GroundOffsetY = null;

            Assert.AreEqual(0.35f, runtime.ResolveVisualYOffset(), 0.0001f);
        }

        [Test]
        public void ResolveConstructionPrefab_FallsBackToPlacedPrefab()
        {
            _prefab = new GameObject("placed-prefab");
            var runtime = new BuildingDefinition
            {
                Prefab = _prefab,
            };

            Assert.AreSame(_prefab, runtime.ResolveConstructionPrefab());
        }
    }
}
