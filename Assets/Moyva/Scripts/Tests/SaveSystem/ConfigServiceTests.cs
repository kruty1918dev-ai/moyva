using System;
using System.IO;
using System.Collections.Generic;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    /// <summary>
    /// Unit tests for ConfigService.
    /// Тестує окремий конфіг-сервіс (один файл config.mvs).
    /// </summary>
    [TestFixture]
    public class ConfigServiceTests : ZenjectUnitTestFixture
    {
        // ─── Inline stubs ─────────────────────────────────────────────────

        private class TestModule : ISaveModule
        {
            public int Value = 42;
            public int LoadedValue;

            public void OnSave(ISaveContext ctx)
                => ctx.Writer.Write(Value);

            public void OnLoad(ISaveContext ctx)
                => LoadedValue = ctx.Reader.ReadInt32();
        }

        private class SettingsModule : ISaveModule
        {
            public float Volume = 0.8f;
            public float LoadedVolume;

            public void OnSave(ISaveContext ctx)
                => ctx.Writer.Write(Volume);

            public void OnLoad(ISaveContext ctx)
                => LoadedVolume = ctx.Reader.ReadSingle();
        }

        // ─── Fields ───────────────────────────────────────────────────────

        private ConfigService _config;

        // ─── Setup / Teardown ─────────────────────────────────────────────

        public override void Teardown()
        {
            _config?.DeleteConfig();
            base.Teardown();
        }

        private ConfigService BuildService()
        {
            _config = new ConfigService();
            _config.Initialize();
            return _config;
        }

        // ─── Tests ────────────────────────────────────────────────────────

        [Test]
        public void SaveConfig_SingleModule_CreatesFile()
        {
            BuildService();
            var module = new TestModule { Value = 123 };

            _config.SaveConfig(new List<ISaveModule> { module });

            Assert.IsTrue(_config.HasConfig(), "Config file should exist");
        }

        [Test]
        public void SaveLoad_SingleModule_Roundtrip()
        {
            BuildService();
            var module = new TestModule { Value = 999 };

            _config.SaveConfig(new List<ISaveModule> { module });
            module.Value = 0;
            module.LoadedValue = 0;

            _config.LoadConfig(new List<ISaveModule> { module });

            Assert.AreEqual(999, module.LoadedValue, "Value should be restored from config");
        }

        [Test]
        public void SaveLoad_MultipleModules_RoundtripAll()
        {
            BuildService();
            var test = new TestModule { Value = 11 };
            var settings = new SettingsModule { Volume = 0.5f };

            var modules = new List<ISaveModule> { test, settings };
            _config.SaveConfig(modules);

            test.LoadedValue = 0;
            settings.LoadedVolume = 0f;

            _config.LoadConfig(modules);

            Assert.AreEqual(11, test.LoadedValue);
            Assert.AreEqual(0.5f, settings.LoadedVolume, 0.001f);
        }

        [Test]
        public void GetConfigInfo_AfterSave_ReturnsMetadata()
        {
            BuildService();
            _config.SaveConfig(new List<ISaveModule> { new TestModule() });

            SaveSlotInfo info = _config.GetConfigInfo();

            Assert.IsTrue(info.Exists);
            Assert.Greater(info.FileSizeBytes, 0L);
            Assert.AreNotEqual(DateTime.MinValue, info.LastWriteTimeUtc);
        }

        [Test]
        public void DeleteConfig_AfterSave_RemovesFile()
        {
            BuildService();
            _config.SaveConfig(new List<ISaveModule> { new TestModule() });
            Assert.IsTrue(_config.HasConfig());

            _config.DeleteConfig();

            Assert.IsFalse(_config.HasConfig());
        }

        [Test]
        public void SaveConfig_EmptyList_DoesNotThrow()
        {
            BuildService();
            Assert.DoesNotThrow(() => _config.SaveConfig(new List<ISaveModule>()));
        }

        [Test]
        public void LoadConfig_NoFile_DoesNotThrow()
        {
            BuildService();
            Assert.DoesNotThrow(() => _config.LoadConfig(new List<ISaveModule> { new TestModule() }));
        }
    }
}
