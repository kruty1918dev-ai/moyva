using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// Starter-pack grant must tolerate an empty settlement id: before the
    /// first settlement exists the resources go into the owner pool, which the
    /// economy layer later routes to the first warehouse. Previously the grant
    /// returned false for an empty settlement id, so the initializer marked the
    /// pack granted while the player received nothing — a funding deadlock.
    /// </summary>
    [TestFixture]
    public sealed class BootstrapStarterPackGrantServiceTests
    {
        private DiContainer _container;
        private SignalBus _signalBus;
        private List<GrantStarterPackResourcesSignal> _received;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<GrantStarterPackResourcesSignal>().OptionalSubscriber();
            _signalBus = _container.Resolve<SignalBus>();
            _received = new List<GrantStarterPackResourcesSignal>();
            _signalBus.Subscribe<GrantStarterPackResourcesSignal>(
                signal => _received.Add(signal));
        }

        [TearDown]
        public void TearDown() => _signalBus = null;

        [Test]
        public void TryGrant_EmptySettlementId_GrantsIntoOwnerPool()
        {
            var settings = new BootstrapGameSettings();
            settings.InitialResources.Clear();
            settings.InitialResources.Add(
                new InitialResourceEntry("steak-food-resources", 140f));
            var service = new BootstrapStarterPackGrantService(settings, _signalBus);

            bool granted = service.TryGrant(string.Empty, "p1");

            Assert.IsTrue(granted);
            Assert.AreEqual(1, _received.Count);
            Assert.AreEqual(string.Empty, _received[0].SettlementId);
            Assert.AreEqual("p1", _received[0].OwnerId);
            Assert.AreEqual(1, _received[0].Entries.Length);
            Assert.AreEqual("steak-food-resources", _received[0].Entries[0].ResourceId);
            Assert.AreEqual(140f, _received[0].Entries[0].Amount);
        }

        [Test]
        public void TryGrant_WithSettlementId_PassesSettlementThrough()
        {
            var settings = new BootstrapGameSettings();
            settings.InitialResources.Clear();
            settings.InitialResources.Add(
                new InitialResourceEntry("stone-materials-resources", 25f));
            var service = new BootstrapStarterPackGrantService(settings, _signalBus);

            Assert.IsTrue(service.TryGrant("settlement-1", "p1"));
            Assert.AreEqual("settlement-1", _received[0].SettlementId);
        }

        [Test]
        public void TryGrant_EmptyOwner_Rejected()
        {
            var service = new BootstrapStarterPackGrantService(
                new BootstrapGameSettings(), _signalBus);

            Assert.IsFalse(service.TryGrant(string.Empty, " "));
            Assert.AreEqual(0, _received.Count);
        }

        [Test]
        public void TryGrant_EmptyEntryList_SucceedsSilently()
        {
            var settings = new BootstrapGameSettings();
            settings.InitialResources.Clear();
            var service = new BootstrapStarterPackGrantService(settings, _signalBus);

            Assert.IsTrue(service.TryGrant(string.Empty, "p1"));
            Assert.AreEqual(0, _received.Count);
        }

        [Test]
        public void TryGrant_SkipsInvalidEntries()
        {
            var settings = new BootstrapGameSettings();
            settings.InitialResources.Clear();
            settings.InitialResources.Add(new InitialResourceEntry(" ", 10f));
            settings.InitialResources.Add(new InitialResourceEntry("iron", -5f));
            settings.InitialResources.Add(new InitialResourceEntry(" valid-resource ", 30f));
            var service = new BootstrapStarterPackGrantService(settings, _signalBus);

            Assert.IsTrue(service.TryGrant(string.Empty, "p1"));

            Assert.AreEqual(1, _received.Count);
            Assert.AreEqual(1, _received[0].Entries.Length);
            Assert.AreEqual("valid-resource", _received[0].Entries[0].ResourceId);
            Assert.AreEqual(30f, _received[0].Entries[0].Amount);
        }

        [Test]
        public void TryGrant_OnlyInvalidEntries_NoSignal()
        {
            var settings = new BootstrapGameSettings();
            settings.InitialResources.Clear();
            settings.InitialResources.Add(new InitialResourceEntry(null, 0f));
            var service = new BootstrapStarterPackGrantService(settings, _signalBus);

            Assert.IsTrue(service.TryGrant(string.Empty, "p1"));
            Assert.AreEqual(0, _received.Count);
        }
    }
}
