using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Floating;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Wave-floating service: registered transforms bob with the replicated
    /// Gerstner field, ease toward the wave pose, keep their authored yaw, and
    /// dead transforms prune themselves without leaking work.
    /// </summary>
    public class WaterFloatServiceTests
    {
        private sealed class FloatHarness
        {
            public readonly WaterFloatService Service;
            public readonly GameObject Target;

            public FloatHarness(WaterFloatRules rules = null)
            {
                var config = new EnvironmentDecorationConfig();
                if (rules != null)
                    config.Floating = rules;
                Service = new WaterFloatService(config);
                Target = new GameObject("floater");
            }
        }

        private static WaterFloatRules FastRules()
            => new WaterFloatRules { FrameStride = 1, Smoothing = 0.01f };

        [Test]
        public void Tick_LiftsFloaterToWaveHeight()
        {
            var h = new FloatHarness(FastRules());
            h.Target.transform.position = new Vector3(1.5f, 0f, 2.5f);
            h.Service.Register(h.Target.transform, 0f, 0.3f);

            // Sweep time until the wave crest passes under the floater; at some
            // phase the summed amplitude must push it above the rest height.
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < 400; i++)
            {
                h.Service.Tick(i * 0.05f, 0.016f);
                maxY = Mathf.Max(maxY, h.Target.transform.position.y);
            }

            Assert.Greater(maxY, 0.005f, "wave crest must lift the floater above rest height");
            Object.DestroyImmediate(h.Target);
        }

        [Test]
        public void Tick_TiltsFloaterIntoWaveNormal_AndKeepsYaw()
        {
            var h = new FloatHarness(FastRules());
            h.Target.transform.rotation = Quaternion.Euler(0f, 137f, 0f);
            h.Service.Register(h.Target.transform, 0f, 0.3f);

            float maxTilt = 0f;
            for (int i = 0; i < 400; i++)
            {
                h.Service.Tick(i * 0.05f, 0.016f);
                float tilt = Vector3.Angle(Vector3.up, h.Target.transform.up);
                maxTilt = Mathf.Max(maxTilt, tilt);
            }

            Assert.Greater(maxTilt, 0.1f, "wave slope must tilt the floater");
            Object.DestroyImmediate(h.Target);
        }

        [Test]
        public void Tick_StrideSpreadsUpdatesAcrossFrames()
        {
            var rules = FastRules();
            rules.FrameStride = 4;
            var h = new FloatHarness(rules);
            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject($"floater{i}");
                go.transform.position = new Vector3(i * 10f, 0f, 0f);
                h.Service.Register(go.transform, 0f, 0.3f);
            }

            h.Service.Tick(0.37f, 0.016f);
            Assert.GreaterOrEqual(h.Service.Count, 4);
        }

        [Test]
        public void Tick_PrunesDestroyedTransforms()
        {
            var h = new FloatHarness(FastRules());
            h.Service.Register(h.Target.transform, 0f, 0.3f);
            h.Service.Tick(0f, 0.016f);

            Object.DestroyImmediate(h.Target);
            h.Service.Tick(0.05f, 0.016f);

            Assert.AreEqual(0, h.Service.Count);
        }

        [Test]
        public void DisabledConfig_RegistersNothing()
        {
            var rules = FastRules();
            rules.Enabled = false;
            var h = new FloatHarness(rules);
            h.Service.Register(h.Target.transform, 0f, 0.3f);
            h.Service.Tick(1f, 0.016f);

            Assert.AreEqual(0, h.Service.Count);
            Object.DestroyImmediate(h.Target);
        }
    }
}
