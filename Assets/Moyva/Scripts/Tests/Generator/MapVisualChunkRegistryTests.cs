using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Generator
{
    [TestFixture]
    public sealed class MapVisualChunkRegistryTests
    {
        [Test]
        public void SetCameraVisible_DoesNotIncrementVersionForEquivalentSet()
        {
            var registry = new MapVisualChunkRegistry();
            var first = new HashSet<MapChunkCoord>
            {
                new(0, 0),
                new(1, 0),
            };
            var equivalent = new HashSet<MapChunkCoord>
            {
                new(1, 0),
                new(0, 0),
            };

            registry.SetCameraVisible(first);
            int versionAfterFirstUpdate = registry.CameraVisibilityVersion;
            registry.SetCameraVisible(equivalent);

            Assert.AreEqual(versionAfterFirstUpdate, registry.CameraVisibilityVersion);

            equivalent.Add(new MapChunkCoord(2, 0));
            registry.SetCameraVisible(equivalent);
            Assert.AreEqual(versionAfterFirstUpdate + 1, registry.CameraVisibilityVersion);
        }

        [Test]
        public void VisualDiscovery_RunsOnlyWhenRequestedOrInsideTrackingWindow()
        {
            Assert.IsTrue(MapVisualChunkDiscoveryService.ShouldRunDiscovery(
                requested: true,
                currentTime: 10f,
                nextDiscoveryAt: 20f,
                discoveryUntil: 5f));
            Assert.IsTrue(MapVisualChunkDiscoveryService.ShouldRunDiscovery(
                requested: false,
                currentTime: 10f,
                nextDiscoveryAt: 9f,
                discoveryUntil: 11f));
            Assert.IsFalse(MapVisualChunkDiscoveryService.ShouldRunDiscovery(
                requested: false,
                currentTime: 12f,
                nextDiscoveryAt: 9f,
                discoveryUntil: 11f));
        }
    }
}
