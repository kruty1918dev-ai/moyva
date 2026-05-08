using System;
using System.Linq;
using Kruty1918.Moyva.WorldCreation.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.WorldCreation
{
    [TestFixture]
    public sealed class WorldCreationConfigTests
    {
        [Test]
        public void Default_WorldName_IsNotEmpty()
        {
            var cfg = new WorldCreationConfig();
            Assert.IsFalse(string.IsNullOrWhiteSpace(cfg.WorldName));
        }

        [Test]
        public void Default_Seed_IsZero()
        {
            Assert.AreEqual(0, new WorldCreationConfig().Seed);
        }

        [Test]
        public void Default_SizePreset_IsMedium()
        {
            Assert.AreEqual(WorldSizePreset.Medium, new WorldCreationConfig().SizePreset);
        }

        [Test]
        public void Default_HumanPlayerCount_IsOne()
        {
            Assert.AreEqual(1, new WorldCreationConfig().HumanPlayerCount);
        }

        [Test]
        public void Default_BotCount_IsOne()
        {
            Assert.AreEqual(1, new WorldCreationConfig().BotCount);
        }

        [Test]
        public void TotalFactions_SumsHumansAndBots()
        {
            var cfg = new WorldCreationConfig { HumanPlayerCount = 2, BotCount = 3 };
            Assert.AreEqual(5, cfg.TotalFactions);
        }

        [Test]
        public void ResolvedWidth_Small_Returns32()
        {
            var cfg = new WorldCreationConfig { SizePreset = WorldSizePreset.Small };
            Assert.AreEqual(32, cfg.ResolvedWidth);
        }

        [Test]
        public void ResolvedWidth_Medium_Returns64()
        {
            var cfg = new WorldCreationConfig { SizePreset = WorldSizePreset.Medium };
            Assert.AreEqual(64, cfg.ResolvedWidth);
        }

        [Test]
        public void ResolvedWidth_Large_Returns128()
        {
            var cfg = new WorldCreationConfig { SizePreset = WorldSizePreset.Large };
            Assert.AreEqual(128, cfg.ResolvedWidth);
        }

        [Test]
        public void ResolvedWidth_Custom_ReturnsCustomWidth()
        {
            var cfg = new WorldCreationConfig { SizePreset = WorldSizePreset.Custom, CustomWidth = 200 };
            Assert.AreEqual(200, cfg.ResolvedWidth);
        }

        [Test]
        public void ResolvedHeight_Small_Returns32()
        {
            var cfg = new WorldCreationConfig { SizePreset = WorldSizePreset.Small };
            Assert.AreEqual(32, cfg.ResolvedHeight);
        }

        [Test]
        public void ResolvedHeight_Custom_ReturnsCustomHeight()
        {
            var cfg = new WorldCreationConfig { SizePreset = WorldSizePreset.Custom, CustomHeight = 300 };
            Assert.AreEqual(300, cfg.ResolvedHeight);
        }

        [Test]
        public void Default_EnableBots_IsTrue()
        {
            Assert.IsTrue(new WorldCreationConfig().EnableBots);
        }

        [Test]
        public void Default_StartingGold_Is200()
        {
            Assert.AreEqual(200, new WorldCreationConfig().StartingGold);
        }

        [Test]
        public void Default_StartingFood_Is100()
        {
            Assert.AreEqual(100, new WorldCreationConfig().StartingFood);
        }

        [Test]
        public void Default_GenerateRivers_IsTrue()
        {
            Assert.IsTrue(new WorldCreationConfig().GenerateRivers);
        }

        [Test]
        public void Default_GenerateBiomes_IsTrue()
        {
            Assert.IsTrue(new WorldCreationConfig().GenerateBiomes);
        }

        [Test]
        public void Default_ApplyWFC_IsTrue()
        {
            Assert.IsTrue(new WorldCreationConfig().ApplyWFC);
        }
    }

    [TestFixture]
    public sealed class WorldCreationServiceTests
    {
        private IWorldCreationService CreateService(WorldCreationDefaultsSO defaults = null)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Kruty1918.Moyva.WorldCreation.Runtime.WorldCreationService"))
                .FirstOrDefault(t => t != null);
            Assert.IsNotNull(type, "WorldCreationService type not found in loaded assemblies.");
            return (IWorldCreationService)Activator.CreateInstance(type, new object[] { defaults });
        }

        private WorldCreationConfig MakeValid()
        {
            return new WorldCreationConfig
            {
                WorldName = "Test World",
                Seed = 42,
                HumanPlayerCount = 1,
                BotCount = 1,
                EnableBots = true,
                StartingGold = 100,
                StartingFood = 50
            };
        }

        // --- Constructor ---
        [Test]
        public void Constructor_NullDefaults_CreatesDefaultConfig()
        {
            var svc = CreateService(null);
            Assert.IsNotNull(svc.CurrentConfig);
        }

        // --- UpdateConfig ---
        [Test]
        public void UpdateConfig_SetsCurrentConfig()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            svc.UpdateConfig(cfg);
            Assert.AreSame(cfg, svc.CurrentConfig);
        }

        [Test]
        public void UpdateConfig_Null_ThrowsArgumentNull()
        {
            var svc = CreateService(null);
            Assert.Throws<ArgumentNullException>(() => svc.UpdateConfig(null));
        }

        // --- ResetToDefaults ---
        [Test]
        public void ResetToDefaults_NullDefaults_CreatesNewConfig()
        {
            var svc = CreateService(null);
            svc.UpdateConfig(MakeValid());
            svc.ResetToDefaults();
            Assert.IsNotNull(svc.CurrentConfig);
            Assert.AreNotSame(MakeValid(), svc.CurrentConfig);
        }

        // --- GenerateRandomSeed ---
        [Test]
        public void GenerateRandomSeed_ReturnsNonZero()
        {
            var svc = CreateService(null);
            int seed = svc.GenerateRandomSeed();
            Assert.AreNotEqual(0, seed);
        }

        [Test]
        public void GenerateRandomSeed_UpdatesConfigSeed()
        {
            var svc = CreateService(null);
            int seed = svc.GenerateRandomSeed();
            Assert.AreEqual(seed, svc.CurrentConfig.Seed);
        }

        // --- ValidateConfig ---
        [Test]
        public void Validate_NullConfig_ReturnsFalse()
        {
            var svc = CreateService(null);
            Assert.IsFalse(svc.ValidateConfig(null, out var err));
            Assert.IsNotNull(err);
        }

        [Test]
        public void Validate_EmptyWorldName_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.WorldName = "";
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_WhitespaceWorldName_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.WorldName = "   ";
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_WorldNameTooLong_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.WorldName = new string('A', 65);
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_WorldName64Chars_ReturnsTrue()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.WorldName = new string('A', 64);
            Assert.IsTrue(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_CustomSizeTooSmall_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.SizePreset = WorldSizePreset.Custom;
            cfg.CustomWidth = 8;
            cfg.CustomHeight = 8;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_CustomSizeTooLarge_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.SizePreset = WorldSizePreset.Custom;
            cfg.CustomWidth = 1024;
            cfg.CustomHeight = 1024;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_CustomSize16x16_ReturnsTrue()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.SizePreset = WorldSizePreset.Custom;
            cfg.CustomWidth = 16;
            cfg.CustomHeight = 16;
            Assert.IsTrue(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_CustomSize512x512_ReturnsTrue()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.SizePreset = WorldSizePreset.Custom;
            cfg.CustomWidth = 512;
            cfg.CustomHeight = 512;
            Assert.IsTrue(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_HumanLessThan1_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.HumanPlayerCount = 0;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_HumanMoreThan4_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.HumanPlayerCount = 5;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_BotNegative_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.BotCount = -1;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_BotMoreThan4_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.BotCount = 5;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_BotsDisabledButCountNonZero_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.EnableBots = false;
            cfg.BotCount = 1;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_BotsDisabledCountZero_ReturnsTrue()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.EnableBots = false;
            cfg.BotCount = 0;
            // TotalFactions = 1 which fails minimum 2 check
            cfg.HumanPlayerCount = 2;
            Assert.IsTrue(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_TotalFactionsLessThan2_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.HumanPlayerCount = 1;
            cfg.BotCount = 0;
            cfg.EnableBots = false;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_NegativeGold_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.StartingGold = -1;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_NegativeFood_ReturnsFalse()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.StartingFood = -1;
            Assert.IsFalse(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_ZeroGold_ReturnsTrue()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.StartingGold = 0;
            Assert.IsTrue(svc.ValidateConfig(cfg, out _));
        }

        [Test]
        public void Validate_ValidConfig_ReturnsTrue()
        {
            var svc = CreateService(null);
            Assert.IsTrue(svc.ValidateConfig(MakeValid(), out var err));
            Assert.IsNull(err);
        }

        // --- ToSignalData ---
        [Test]
        public void ToSignalData_MapsWorldName()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            var data = svc.ToSignalData(cfg);
            Assert.AreEqual(cfg.WorldName, data.WorldName);
        }

        [Test]
        public void ToSignalData_MapsExistingSeed()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.Seed = 42;
            var data = svc.ToSignalData(cfg);
            Assert.AreEqual(42, data.Seed);
        }

        [Test]
        public void ToSignalData_ZeroSeed_GeneratesNewSeed()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.Seed = 0;
            var data = svc.ToSignalData(cfg);
            Assert.AreNotEqual(0, data.Seed);
        }

        [Test]
        public void ToSignalData_MapsSizePreset()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.SizePreset = WorldSizePreset.Large;
            var data = svc.ToSignalData(cfg);
            Assert.AreEqual((int)WorldSizePreset.Large, data.SizePresetIndex);
        }

        [Test]
        public void ToSignalData_MapsPlayerCounts()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.HumanPlayerCount = 3;
            cfg.BotCount = 2;
            var data = svc.ToSignalData(cfg);
            Assert.AreEqual(3, data.HumanPlayerCount);
            Assert.AreEqual(2, data.BotCount);
        }

        [Test]
        public void ToSignalData_MapsDensities()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.ForestDensity = 0.9f;
            cfg.MountainDensity = 0.1f;
            var data = svc.ToSignalData(cfg);
            Assert.AreEqual(0.9f, data.ForestDensity, 0.001f);
            Assert.AreEqual(0.1f, data.MountainDensity, 0.001f);
        }

        [Test]
        public void ToSignalData_MapsFlags()
        {
            var svc = CreateService(null);
            var cfg = MakeValid();
            cfg.GenerateRivers = false;
            cfg.GenerateBiomes = true;
            cfg.ApplyWFC = false;
            var data = svc.ToSignalData(cfg);
            Assert.IsFalse(data.GenerateRivers);
            Assert.IsTrue(data.GenerateBiomes);
            Assert.IsFalse(data.ApplyWFC);
        }
    }
}
