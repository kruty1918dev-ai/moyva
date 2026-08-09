using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Generator
{
    [TestFixture]
    public sealed class FullChunkMapPolicyTests
    {
        [TestCase(50, 50, 48, 48)]
        [TestCase(64, 64, 64, 64)]
        [TestCase(63, 33, 48, 32)]
        [TestCase(16, 16, 16, 16)]
        [TestCase(15, 9, 16, 16)]
        public void CropMapSize_ProducesWholeSixteenTileChunks(
            int requestedWidth, int requestedHeight,
            int expectedWidth, int expectedHeight)
        {
            Vector2Int size = MapChunkSizePolicy.CropMapSize(
                requestedWidth, requestedHeight);
            Assert.AreEqual(expectedWidth, size.x);
            Assert.AreEqual(expectedHeight, size.y);
            Assert.IsTrue(MapChunkSizePolicy.IsFullChunkMap(size.x, size.y));
        }

        [Test]
        public void Layout_50x50Request_BecomesNineFull16x16Chunks()
        {
            var settings = ScriptableObject.CreateInstance<MapChunkSettingsSO>();
            try
            {
                settings.ChunkSize = 8;
                var layout = new MapChunkLayoutService(settings);
                layout.Configure(50, 50, 1f, false, default);

                Assert.AreEqual(16, layout.ChunkSize);
                Assert.AreEqual(48, layout.Width);
                Assert.AreEqual(48, layout.Height);
                Assert.AreEqual(9, layout.Chunks.Count);
                foreach (MapChunkDescriptor d in layout.Chunks)
                {
                    Assert.AreEqual(16, d.TileRect.width);
                    Assert.AreEqual(16, d.TileRect.height);
                }
                Assert.IsTrue(layout.TryGetChunkCoord(
                    new Vector2Int(47, 47), out MapChunkCoord owner));
                Assert.AreEqual(2, owner.X);
                Assert.AreEqual(2, owner.Y);
                Assert.IsFalse(layout.TryGetChunkCoord(
                    new Vector2Int(48, 48), out _));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }
    }
}
