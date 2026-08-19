using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Tests.Units
{
    [TestFixture]
    public sealed class UnitPresentationConfigTests
    {
        private GameObject _legacyPrefab;
        private GameObject _presentationPrefab;
        private GameObject _previewPrefab;

        [TearDown]
        public void TearDown()
        {
            Destroy(_legacyPrefab);
            Destroy(_presentationPrefab);
            Destroy(_previewPrefab);
        }

        [Test]
        public void ResolvePrefab_UsesPresentationPrefabBeforeLegacyPrefab()
        {
            _legacyPrefab = new GameObject("legacy-prefab");
            _presentationPrefab = new GameObject("presentation-prefab");

            var config = new UnitClassConfig
            {
                Prefab = _legacyPrefab,
                Presentation = new UnitPresentationConfig
                {
                    Prefab = _presentationPrefab,
                },
            };

            Assert.AreSame(_presentationPrefab, config.ResolvePrefab());
        }

        [Test]
        public void ResolvePreviewPrefab_UsesPreviewThenPresentationThenLegacy()
        {
            _legacyPrefab = new GameObject("legacy-prefab");
            _presentationPrefab = new GameObject("presentation-prefab");
            _previewPrefab = new GameObject("preview-prefab");

            var config = new UnitClassConfig
            {
                Prefab = _legacyPrefab,
                Presentation = new UnitPresentationConfig
                {
                    Prefab = _presentationPrefab,
                    PreviewPrefab = _previewPrefab,
                },
            };

            Assert.AreSame(_previewPrefab, config.ResolvePreviewPrefab());

            config.Presentation.PreviewPrefab = null;
            Assert.AreSame(_presentationPrefab, config.ResolvePreviewPrefab());

            config.Presentation.Prefab = null;
            Assert.AreSame(_legacyPrefab, config.ResolvePreviewPrefab());
        }

        [Test]
        public void ResolveMarkerScale_UsesSelectionPresentation()
        {
            var config = new UnitClassConfig
            {
                Presentation = new UnitPresentationConfig
                {
                    Selection = new EntitySelectionPresentationConfig
                    {
                        MarkerScale = 1.75f,
                    },
                },
            };

            Assert.AreEqual(
                1.75f,
                UnitSelectionVisualService.ResolveMarkerScale(config),
                0.0001f);
        }

        private static void Destroy(GameObject obj)
        {
            if (obj != null)
                Object.DestroyImmediate(obj);
        }
    }
}
