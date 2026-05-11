using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.FogOfWar.Runtime;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.FogOfWar
{
    /// <summary>
    /// Чисті C# тести для FogOfWarServiceRegistry (без Zenject).
    /// </summary>
    [TestFixture]
    public class FogOfWarServiceRegistryTests
    {
        private sealed class StubFogOfWarService : IFogOfWarService
        {
            public void Initialize(int width, int height) { }
            public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }
            public void UpdateUnitVisionRange(string unitId, int visionRange) { }
            public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
            public void UnregisterUnit(string unitId) { }
            public FogStateType GetFogState(Vector2Int position) => FogStateType.Visible;
            public bool IsVisible(Vector2Int position) => true;
            public bool IsExplored(Vector2Int position) => true;
            public bool[,] GetExploredSnapshot() => new bool[0, 0];
            public void LoadFromSnapshot(bool[,] explored) { }
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => System.Array.Empty<Vector2Int>();
        }

        private FogOfWarServiceRegistry CreateRegistry() => new FogOfWarServiceRegistry();

        [Test]
        public void Register_ThenTryGetFor_ReturnsRegisteredService()
        {
            var registry = CreateRegistry();
            var service  = new StubFogOfWarService();

            registry.Register("faction1", service);
            var found = registry.TryGetFor("faction1", out var result);

            Assert.IsTrue(found);
            Assert.AreSame(service, result);
        }

        [Test]
        public void TryGetFor_UnknownFaction_ReturnsFalse()
        {
            var registry = CreateRegistry();

            var found = registry.TryGetFor("unknown", out var result);

            Assert.IsFalse(found);
            Assert.IsNull(result);
        }

        [Test]
        public void Register_NullService_IsIgnored()
        {
            var registry = CreateRegistry();

            registry.Register("faction1", null);
            var found = registry.TryGetFor("faction1", out _);

            Assert.IsFalse(found, "Null сервіс не повинен реєструватись.");
        }

        [Test]
        public void Register_EmptyFactionId_IsIgnored()
        {
            var registry = CreateRegistry();
            var service  = new StubFogOfWarService();

            registry.Register("", service);
            var found = registry.TryGetFor("", out _);

            Assert.IsFalse(found, "Порожній factionId не повинен реєструватись.");
        }

        [Test]
        public void Register_Twice_OverwritesPreviousService()
        {
            var registry  = CreateRegistry();
            var service1  = new StubFogOfWarService();
            var service2  = new StubFogOfWarService();

            registry.Register("faction1", service1);
            registry.Register("faction1", service2);

            registry.TryGetFor("faction1", out var result);
            Assert.AreSame(service2, result, "Другий Register повинен перезаписати перший.");
        }
    }
}
