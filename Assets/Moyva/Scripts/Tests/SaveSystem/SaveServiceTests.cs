using System;
using System.IO;
using System.Collections.Generic;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    /// <summary>
    /// Unit tests for SaveService using ZenjectUnitTestFixture.
    /// All tests write to Application.persistentDataPath/saves/testNN.mvs
    /// and clean up via Delete() in TearDown.
    /// </summary>
    [TestFixture]
    public class SaveServiceTests : ZenjectUnitTestFixture
    {
        // ─── Inline stubs ─────────────────────────────────────────────────

        private class AlwaysWriteModule : ISaveModule
        {
            public int   Value     = 42;
            public int   LoadedValue;
            public bool  OnSaveCalled;
            public bool  OnLoadCalled;

            public void OnSave(ISaveContext ctx)
            {
                OnSaveCalled = true;
                ctx.Writer.Write(Value);
            }

            public void OnLoad(ISaveContext ctx)
            {
                OnLoadCalled = true;
                LoadedValue  = ctx.Reader.ReadInt32();
            }
        }

        private class ThrowOnSaveModule : ISaveModule
        {
            public void OnSave(ISaveContext ctx) => throw new InvalidOperationException("Save error");
            public void OnLoad(ISaveContext ctx) { }
        }

        private class ThrowOnLoadModule : ISaveModule
        {
            public void OnSave(ISaveContext ctx) => ctx.Writer.Write(1);
            public void OnLoad(ISaveContext ctx) => throw new InvalidOperationException("Load error");
        }

        // ─── Fields ───────────────────────────────────────────────────────

        private SaveService    _service;
        private SignalBus      _signalBus;
        private const int      TestSlot = 95; // high slot to avoid collision

        // ─── Setup / Teardown ─────────────────────────────────────────────

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<SaveRequestedSignal>();
            Container.DeclareSignal<LoadRequestedSignal>();
            Container.DeclareSignal<SaveCompletedSignal>();
        }

        public override void Teardown()
        {
            _service?.Delete(TestSlot);
            _service?.Dispose();
            base.Teardown();
        }

        private SaveService BuildService(List<ISaveModule> modules = null)
        {
            _signalBus = Container.Resolve<SignalBus>();
            _service   = new SaveService(modules ?? new List<ISaveModule>(), _signalBus);
            _service.Initialize();
            return _service;
        }

        // ─── 1. Invalid slot fires failure signal ─────────────────────────

        [Test]
        public void Save_InvalidSlot_FiresFailureSignal()
        {
            BuildService();
            SaveCompletedSignal? received = null;
            _signalBus.Subscribe<SaveCompletedSignal>(s => received = s);

            _service.Save(-1);

            Assert.IsNotNull(received, "Signal should be fired");
            Assert.IsFalse(received.Value.Success);
        }

        // ─── 2. No modules → creates valid file ───────────────────────────

        [Test]
        public void Save_NoModules_CreatesValidFile()
        {
            BuildService();
            SaveCompletedSignal? received = null;
            _signalBus.Subscribe<SaveCompletedSignal>(s => received = s);

            _service.Save(TestSlot);

            Assert.IsNotNull(received);
            Assert.IsTrue(received.Value.Success, "No-module save should succeed");
            Assert.IsTrue(_service.HasSave(TestSlot));
        }

        // ─── 3. Single module save+load roundtrip ─────────────────────────

        [Test]
        public void SaveLoad_SingleModule_Roundtrip()
        {
            var module = new AlwaysWriteModule { Value = 99 };
            BuildService(new List<ISaveModule> { module });

            _service.Save(TestSlot);
            module.Value = 0; // reset to verify load

            _service.Load(TestSlot);

            Assert.IsTrue(module.OnSaveCalled);
            Assert.IsTrue(module.OnLoadCalled);
            Assert.AreEqual(99, module.LoadedValue);
        }

        [Test]
        public void SaveLoad_ModuleRegisteredAfterServiceConstruction_Roundtrip()
        {
            var registry = new SaveModuleRegistry();
            var module = new AlwaysWriteModule { Value = 321 };

            _signalBus = Container.Resolve<SignalBus>();
            _service = new SaveService(new List<ISaveModule>(), _signalBus, moduleRegistry: registry);
            _service.Initialize();

            registry.Register(module);
            _service.Save(TestSlot);
            module.Value = 0;

            _service.Load(TestSlot);

            Assert.IsTrue(module.OnSaveCalled);
            Assert.IsTrue(module.OnLoadCalled);
            Assert.AreEqual(321, module.LoadedValue);
        }

        // ─── 4. Module exception on save doesn't stop others ─────────────

        [Test]
        public void Save_ModuleThrows_OtherModulesStillSave()
        {
            var good   = new AlwaysWriteModule { Value = 7 };
            var thrower = new ThrowOnSaveModule();
            BuildService(new List<ISaveModule> { thrower, good });

            SaveCompletedSignal? received = null;
            _signalBus.Subscribe<SaveCompletedSignal>(s => received = s);

            Assert.DoesNotThrow(() => _service.Save(TestSlot));

            // File should exist — good module wrote a block
            Assert.IsTrue(_service.HasSave(TestSlot));
        }

        // ─── 5. Corrupted file load does not throw ────────────────────────

        [Test]
        public void Load_CorruptedFile_DoesNotThrow()
        {
            BuildService();
            _service.Save(TestSlot);

            // Corrupt the save file
            string path = SaveService.GetPath(TestSlot);
            byte[] bytes = File.ReadAllBytes(path);
            for (int i = 4; i < bytes.Length - 4; i++)
                bytes[i] ^= 0xFF;
            File.WriteAllBytes(path, bytes);

            // SaveService logs warnings when CRC fails and no backup exists
            Assert.DoesNotThrow(() => _service.Load(TestSlot));
        }

        // ─── 6. Unknown block id is skipped ──────────────────────────────

        [Test]
        public void Load_UnknownBlockId_DoesNotThrow()
        {
            // Save with one module, then load with empty module list
            var module = new AlwaysWriteModule { Value = 5 };
            BuildService(new List<ISaveModule> { module });
            _service.Save(TestSlot);

            // Rebuild service with no modules (unknown blockId on load)
            _service.Dispose();
            _service = new SaveService(new List<ISaveModule>(), _signalBus);
            _service.Initialize();

            Assert.DoesNotThrow(() => _service.Load(TestSlot));
        }

        // ─── 7. OnLoad exception doesn't stop others ──────────────────────

        [Test]
        public void Load_ModuleThrowsOnLoad_OtherModulesStillLoad()
        {
            var good    = new AlwaysWriteModule { Value = 123 };
            var thrower = new ThrowOnLoadModule();

            // Build in same order for consistent blockIds
            var modules = new List<ISaveModule> { thrower, good };
            BuildService(modules);
            _service.Save(TestSlot);

            good.LoadedValue = 0;

            Assert.DoesNotThrow(() => _service.Load(TestSlot));
            Assert.AreEqual(123, good.LoadedValue, "Good module should still receive OnLoad");
        }

        // ─── 8. HasSave and Delete basics ────────────────────────────────

        [Test]
        public void HasSave_Delete_Basics()
        {
            BuildService();

            Assert.IsFalse(_service.HasSave(TestSlot), "Should not exist yet");

            _service.Save(TestSlot);
            Assert.IsTrue(_service.HasSave(TestSlot), "Should exist after save");

            _service.Delete(TestSlot);
            Assert.IsFalse(_service.HasSave(TestSlot), "Should not exist after delete");
        }

        // ─── 9. GetSlotInfo returns correct metadata ──────────────────────

        [Test]
        public void GetSlotInfo_AfterSave_ReturnsCorrectInfo()
        {
            BuildService();
            _service.Save(TestSlot);

            SaveSlotInfo info = _service.GetSlotInfo(TestSlot);

            Assert.IsTrue(info.Exists);
            Assert.AreEqual(TestSlot, info.Slot);
            Assert.Greater(info.FileSizeBytes, 0L);
            Assert.AreNotEqual(DateTime.MinValue, info.LastWriteTimeUtc);
        }

        // ─── 10. GetSlotInfo for non-existent slot ────────────────────────

        [Test]
        public void GetSlotInfo_NoSave_ExistsFalse()
        {
            BuildService();
            // Make sure slot is clean
            _service.Delete(TestSlot);

            SaveSlotInfo info = _service.GetSlotInfo(TestSlot);

            Assert.IsFalse(info.Exists);
        }

        // ─── 11. SignalBus SaveRequested fires Save ────────────────────────

        [Test]
        public void SignalBus_SaveRequested_TriggersSave()
        {
            BuildService();
            SaveCompletedSignal? received = null;
            _signalBus.Subscribe<SaveCompletedSignal>(s => received = s);

            _signalBus.Fire(new SaveRequestedSignal { Slot = TestSlot });

            Assert.IsNotNull(received);
            Assert.AreEqual(TestSlot, received.Value.Slot);
        }
    }
}
