using Kruty1918.Moyva.FogOfWar.Runtime;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    /// <summary>
    /// Regression coverage for the surface-depth prepass clear convention.
    ///
    /// Root cause of the startup gray-veil defect: the prepass cleared the
    /// depth attachment with raw reversed-Z 0 instead of the logical far
    /// depth expected by CommandBuffer.ClearRenderTarget (1.0). On D3D11
    /// every terrain fragment then failed ZTest LEqual, the surface
    /// eye-depth texture stayed empty, and the screen-space fog composite
    /// classified the whole map as surface-less (unexplored veil).
    /// </summary>
    [TestFixture]
    public sealed class FogOfWarSurfaceDepthClearTests
    {
        [Test]
        public void SurfaceDepthClearValue_IsLogicalFarPlane()
        {
            Assert.AreEqual(
                1f,
                FogOfWarScreenSpaceRendererFeature.ResolveSurfaceDepthClearValue(),
                "ClearRenderTarget expects logical depth (1.0 = far on every "
                + "platform); a raw reversed-Z 0 clears to near and breaks "
                + "the surface-depth prepass.");
        }

        [Test]
        public void SurfaceDepthClearValue_DoesNotDependOnReversedZ()
        {
            // Pins the contract: the value must not be selected via
            // SystemInfo.usesReversedZBuffer — Unity normalizes the clear
            // value internally for both depth conventions.
            Assert.AreEqual(
                1f,
                FogOfWarScreenSpaceRendererFeature.ResolveSurfaceDepthClearValue());
        }

        [Test]
        public void SurfaceDepthShader_KeepsLEqualDepthTest()
        {
            // The clear-to-far convention only works while the depth pass
            // keeps ZTest LEqual. Guard the pairing so the two cannot drift.
            var guids =
                AssetDatabase.FindAssets(
                    "t:Shader FogSurfaceDepth");

            Assert.IsNotEmpty(
                guids,
                "FogSurfaceDepth.shader not found in the project.");

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
            Assert.IsNotNull(shader);

            var source = System.IO.File.ReadAllText(path);

            StringAssert.Contains(
                "ZTest LEqual",
                source,
                "FogSurfaceDepth must keep ZTest LEqual to pair with the "
                + "logical-far depth clear.");
        }
    }
}
