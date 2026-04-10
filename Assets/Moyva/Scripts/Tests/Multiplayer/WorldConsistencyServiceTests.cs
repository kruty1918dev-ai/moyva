using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Persistence;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    [TestFixture]
    public class WorldConsistencyServiceTests
    {
        private sealed class FakeLogger : IMultiplayerLogger
        {
            public void Info(string msg) { }
            public void Warn(string msg) { }
            public void Error(string msg) { }
            public void Trace(string msg) { }
        }

        private WorldConsistencyService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new WorldConsistencyService(new FakeLogger());
        }

        [Test]
        public void Compare_ShouldReturnEqual_WhenChecksumAndIdMatch()
        {
            var host = new WorldSnapshot("world-1", 1, 0xDEADBEEF);
            var client = new WorldSnapshot("world-1", 1, 0xDEADBEEF);

            var result = _service.Compare(host, client);

            Assert.AreEqual(ConsistencyCheckResult.Equal, result);
        }

        [Test]
        public void Compare_ShouldReturnWorldMismatch_WhenChecksumsDiffer()
        {
            var host = new WorldSnapshot("world-1", 1, 0x11111111);
            var client = new WorldSnapshot("world-1", 1, 0x22222222);

            var result = _service.Compare(host, client);

            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, result);
        }

        [Test]
        public void Compare_ShouldReturnWorldMismatch_WhenWorldIdsDiffer()
        {
            var host = new WorldSnapshot("world-A", 1, 0x12345678);
            var client = new WorldSnapshot("world-B", 1, 0x12345678);

            var result = _service.Compare(host, client);

            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, result);
        }

        [Test]
        public void Compare_ShouldReturnWorldMismatch_WhenEitherSnapshotIsNull()
        {
            var snapshot = new WorldSnapshot("world-1", 1, 0xABCDEF01);

            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, _service.Compare(null, snapshot));
            Assert.AreEqual(ConsistencyCheckResult.WorldMismatch, _service.Compare(snapshot, null));
        }
    }
}
