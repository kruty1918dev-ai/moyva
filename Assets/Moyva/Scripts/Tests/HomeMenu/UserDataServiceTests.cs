using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class UserDataServiceTests
    {
        [Test]
        public void HasAnyUserData_ReturnsTrue_WhenSaveExists()
        {
            var save = new FakeSaveService { SavedSlots = { 3 } };
            var cfg  = new FakeConfigService();
            var sut  = new UserDataService(save, cfg);

            Assert.IsTrue(sut.HasAnyUserData());
        }

        [Test]
        public void HasAnyUserData_ReturnsTrue_WhenConfigExists()
        {
            var save = new FakeSaveService();
            var cfg  = new FakeConfigService { Exists = true };
            var sut  = new UserDataService(save, cfg);

            Assert.IsTrue(sut.HasAnyUserData());
        }

        [Test]
        public void HasAnyUserData_ReturnsFalse_WhenEmpty()
        {
            var sut = new UserDataService(new FakeSaveService(), new FakeConfigService());
            Assert.IsFalse(sut.HasAnyUserData());
        }

        [Test]
        public void DeleteAllUserData_RemovesEverything_FiresEvent()
        {
            var save = new FakeSaveService { SavedSlots = { 0, 5, 42 } };
            var cfg  = new FakeConfigService { Exists = true };
            var sut  = new UserDataService(save, cfg);

            int eventCount = 0;
            sut.UserDataDeleted += () => eventCount++;

            int deleted = sut.DeleteAllUserData();

            Assert.AreEqual(4, deleted);              // 3 слоти + 1 конфіг
            Assert.IsEmpty(save.SavedSlots);
            Assert.IsFalse(cfg.Exists);
            Assert.AreEqual(1, eventCount);
            Assert.IsFalse(sut.HasAnyUserData());
        }

        [Test]
        public void DeleteAllUserData_OnEmpty_ReturnsZero_ButStillFiresEvent()
        {
            var sut = new UserDataService(new FakeSaveService(), new FakeConfigService());
            int eventCount = 0;
            sut.UserDataDeleted += () => eventCount++;

            int deleted = sut.DeleteAllUserData();

            Assert.AreEqual(0, deleted);
            Assert.AreEqual(1, eventCount);
        }

        // ── Fakes ────────────────────────────────────────────────────────

        private sealed class FakeSaveService : ISaveService
        {
            public readonly HashSet<int> SavedSlots = new HashSet<int>();
            public void Save(int slot = 0) => SavedSlots.Add(slot);
            public void Load(int slot = 0) { }
            public bool HasSave(int slot = 0) => SavedSlots.Contains(slot);
            public void Delete(int slot = 0) => SavedSlots.Remove(slot);
            public SaveSlotInfo GetSlotInfo(int slot = 0) => default;
        }

        private sealed class FakeConfigService : IConfigService
        {
            public bool Exists;
            public void Initialize() { }
            public void Dispose() { }
            public void SaveConfig(List<ISaveModule> modules) => Exists = true;
            public void LoadConfig(List<ISaveModule> modules) { }
            public bool HasConfig() => Exists;
            public void DeleteConfig() => Exists = false;
            public SaveSlotInfo GetConfigInfo() => default;
        }
    }
}
