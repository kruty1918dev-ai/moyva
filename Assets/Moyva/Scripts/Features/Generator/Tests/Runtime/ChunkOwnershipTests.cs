using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Prop renderers must register to the single chunk that owns them via the
    /// MapChunk_X_Y hierarchy, not to every chunk their bounds overlap —
    /// otherwise a canopy-sized prop stays visible after its support chunk is
    /// hidden or culled.
    /// </summary>
    public sealed class ChunkOwnershipTests
    {
        private MapVisualChunkRootService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new MapVisualChunkRootService();
        }

        [TearDown]
        public void TearDown()
        {
            var root = GameObject.Find("MapVisualChunks");
            if (root != null)
                Object.DestroyImmediate(root);
        }

        [Test]
        public void PropUnderChunkRoot_ResolvesOwnerCoord()
        {
            Transform chunkRoot = _service.GetOrCreateRoot(new MapChunkCoord(2, 3));
            var objects = new GameObject("Objects");
            objects.transform.SetParent(chunkRoot, false);
            var prop = new GameObject("prop");
            prop.transform.SetParent(objects.transform, false);

            Assert.IsTrue(_service.TryGetOwnedChunk(prop.transform, out MapChunkCoord coord));
            Assert.AreEqual(new MapChunkCoord(2, 3), coord);
        }

        [Test]
        public void PropOutsideChunkHierarchy_HasNoOwner()
        {
            _service.GetOrCreateRoot(new MapChunkCoord(0, 0));
            var loose = new GameObject("looseProp");

            Assert.IsFalse(_service.TryGetOwnedChunk(loose.transform, out _));
            Object.DestroyImmediate(loose);
        }

        [Test]
        public void SceneAuthoredChunkRoot_ResolvesByName()
        {
            // Ensure the shared root exists.
            _service.GetOrCreateRoot(new MapChunkCoord(0, 0));
            var sceneChunk = new GameObject("MapChunk_5_7");
            sceneChunk.transform.SetParent(GameObject.Find("MapVisualChunks").transform, false);
            var prop = new GameObject("prop");
            prop.transform.SetParent(sceneChunk.transform, false);

            Assert.IsTrue(_service.TryGetOwnedChunk(prop.transform, out MapChunkCoord coord));
            Assert.AreEqual(new MapChunkCoord(5, 7), coord);
        }

        [Test]
        public void NonChunkNamedChildOfRoot_HasNoOwner()
        {
            _service.GetOrCreateRoot(new MapChunkCoord(1, 1));
            var named = new GameObject("NotAChunk");
            named.transform.SetParent(GameObject.Find("MapVisualChunks").transform, false);
            var prop = new GameObject("prop");
            prop.transform.SetParent(named.transform, false);

            Assert.IsFalse(_service.TryGetOwnedChunk(prop.transform, out _));
        }
    }
}
