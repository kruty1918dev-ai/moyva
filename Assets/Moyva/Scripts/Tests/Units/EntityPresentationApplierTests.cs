using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Tests.Units
{
    [TestFixture]
    public sealed class EntityPresentationApplierTests
    {
        private GameObject _instance;

        [TearDown]
        public void TearDown()
        {
            if (_instance != null)
                Object.DestroyImmediate(_instance);
        }

        [Test]
        public void ApplyTransform_UsesScaleRotationAndOffsets()
        {
            _instance = new GameObject("presented-entity");
            var presentation = new EntityPresentationConfig
            {
                ScaleMultiplier = 2f,
                PositionOffset = new Vector3(1f, 2f, 3f),
                RotationOffset = new Vector3(0f, 90f, 0f),
                GroundOffsetY = 0.25f,
            };

            Quaternion baseRotation = Quaternion.Euler(0f, 90f, 0f);
            Vector3 canonicalPosition = new Vector3(10f, 20f, 30f);

            EntityPresentationApplier.ApplyTransform(
                _instance,
                presentation,
                canonicalPosition,
                baseRotation,
                new Vector3(1f, 2f, 3f),
                fallbackGroundOffsetY: 0.5f);

            AssertVectorApproximately(new Vector3(2f, 4f, 6f), _instance.transform.localScale);
            Assert.That(
                Quaternion.Angle(
                    baseRotation * Quaternion.Euler(0f, 90f, 0f),
                    _instance.transform.rotation),
                Is.LessThan(0.01f));
            AssertVectorApproximately(
                canonicalPosition
                + baseRotation * presentation.PositionOffset
                + Vector3.up * 0.25f,
                _instance.transform.position);
            AssertVectorApproximately(
                canonicalPosition + baseRotation * presentation.PositionOffset,
                EntityPresentationApplier.ResolvePositionOffset(
                    canonicalPosition,
                    baseRotation,
                    presentation));
        }

        [Test]
        public void ApplyStyleAndShadows_WritesRendererProperties()
        {
            _instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Renderer renderer = _instance.GetComponent<Renderer>();
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;

            var tint = new Color(0.2f, 0.4f, 0.6f, 0.8f);
            var teamColor = new Color(1f, 0.1f, 0.2f, 1f);
            var outlineColor = new Color(0.8f, 0.7f, 0.1f, 1f);
            var presentation = new EntityPresentationConfig
            {
                Tint = tint,
                TeamColorEnabled = true,
                Outline = new EntityOutlineConfig
                {
                    Enabled = true,
                    Width = 0.07f,
                    Color = outlineColor,
                },
                Shadows = new EntityShadowConfig
                {
                    Cast = false,
                    Receive = false,
                },
            };

            EntityPresentationApplier.ApplyStyleAndShadows(
                _instance,
                presentation,
                teamColor);

            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);

            AssertColorApproximately(tint, block.GetColor("_Color"));
            AssertColorApproximately(tint, block.GetColor("_BaseColor"));
            Assert.AreEqual(1f, block.GetFloat("_TeamColorEnabled"));
            AssertColorApproximately(teamColor, block.GetColor("_TeamColor"));
            Assert.AreEqual(1f, block.GetFloat("_OutlineEnabled"));
            Assert.AreEqual(0.07f, block.GetFloat("_OutlineWidth"), 0.0001f);
            AssertColorApproximately(outlineColor, block.GetColor("_OutlineColor"));
            Assert.AreEqual(ShadowCastingMode.Off, renderer.shadowCastingMode);
            Assert.IsFalse(renderer.receiveShadows);
        }

        private static void AssertVectorApproximately(
            Vector3 expected,
            Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 0.0001f);
            Assert.AreEqual(expected.y, actual.y, 0.0001f);
            Assert.AreEqual(expected.z, actual.z, 0.0001f);
        }

        private static void AssertColorApproximately(
            Color expected,
            Color actual)
        {
            Assert.AreEqual(expected.r, actual.r, 0.0001f);
            Assert.AreEqual(expected.g, actual.g, 0.0001f);
            Assert.AreEqual(expected.b, actual.b, 0.0001f);
            Assert.AreEqual(expected.a, actual.a, 0.0001f);
        }
    }
}
