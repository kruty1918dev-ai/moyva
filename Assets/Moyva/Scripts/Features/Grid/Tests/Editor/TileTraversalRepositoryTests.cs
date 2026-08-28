using System;
using System.Collections.Generic;
using System.Reflection;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.Jsonization;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Grid
{
    public sealed class TileTraversalRepositoryTests
    {
        private TilePreset _dualPreset;

        [SetUp]
        public void SetUp()
        {
            _dualPreset = ScriptableObject.CreateInstance<TilePreset>();
            _dualPreset.gridtype = TilePreset.GridType.dual;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_dualPreset);
        }

        [Test]
        public void TileRepository_ResolvesOnlyCanonicalIdsAndExactAliases()
        {
            TileTypeRepository repository = BuildTiles(
                Tile("grass", "grass", aliases: new[] { "TileGrass" }));

            Assert.That(repository.TryResolveId("grass", out string canonical), Is.True);
            Assert.That(canonical, Is.EqualTo("grass"));
            Assert.That(repository.TryResolveId("TileGrass", out canonical), Is.True);
            Assert.That(canonical, Is.EqualTo("grass"));
            Assert.That(repository.TryResolveId("grass-generated-42", out _), Is.False);
        }

        [Test]
        public void TraversalResolver_UsesTileOverrideThenClassAndVariesByProfile()
        {
            TileTypeRepository tiles = BuildTiles(
                Tile("grass", "grass"),
                Tile("swamp", "swamp"));
            var profiles = new MovementProfileRepository(new[]
            {
                Profile(
                    "infantry",
                    ("grass", true, 1f),
                    ("swamp", true, 2.5f),
                    tileOverride: ("grass", true, 1.25f)),
                Profile(
                    "cavalry",
                    ("grass", true, 0.75f),
                    ("swamp", true, 4f)),
            });
            var resolver = new TraversalCostResolver(tiles, profiles);

            Assert.That(resolver.TryResolve("infantry", "grass", out float infantryGrass, out _), Is.True);
            Assert.That(infantryGrass, Is.EqualTo(1.25f));
            Assert.That(resolver.TryResolve("infantry", "swamp", out float infantrySwamp, out _), Is.True);
            Assert.That(infantrySwamp, Is.EqualTo(2.5f));
            Assert.That(resolver.TryResolve("cavalry", "swamp", out float cavalrySwamp, out _), Is.True);
            Assert.That(cavalrySwamp, Is.EqualTo(4f));
        }

        [Test]
        public void MovementProfile_PassableRuleRejectsZeroCostWithJsonPointer()
        {
            MovementProfileConfig invalid = Profile("invalid", ("grass", true, 0f));

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => new MovementProfileRepository(new[] { invalid }));

            StringAssert.Contains("/classRules/0/staminaCost", exception.Message);
        }

        [Test]
        public void TileRepository_RejectsDeclaredDualModeWithNormalPreset()
        {
            TileTypeConfig tile = Tile("grass", "grass");
            tile.Visual.Variants[0].Preset.gridtype = TilePreset.GridType.standard;

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => BuildTiles(tile));

            StringAssert.Contains("/visual/variants/0/preset", exception.Message);
        }

        private TileTypeRepository BuildTiles(params TileTypeConfig[] configs)
            => new(configs);

        private TileTypeConfig Tile(
            string id,
            string traversalClass,
            IEnumerable<string> aliases = null)
        {
            var config = new TileTypeConfig
            {
                DisplayName = id,
                TraversalClassId = traversalClass,
                Aliases = aliases != null ? new List<string>(aliases) : new List<string>(),
                Tags = new List<string> { "land" },
                Visual = new TileVisualConfig
                {
                    GridMode = TileGridMode.Dual,
                    Variants = new List<TileVisualVariantConfig>
                    {
                        new()
                        {
                            Preset = _dualPreset,
                            Slot = TileVisualSlot.Top,
                            Weight = 1f,
                        },
                    },
                },
            };
            SetJsonId(config, id);
            return config;
        }

        private static MovementProfileConfig Profile(
            string id,
            (string classId, bool passable, float cost) first,
            (string classId, bool passable, float cost)? second = null,
            (string tileId, bool passable, float cost)? tileOverride = null)
        {
            var config = new MovementProfileConfig
            {
                Fallback = new MovementFallbackConfig
                {
                    Passable = false,
                    StaminaCost = 0f,
                },
                ClassRules = new List<MovementClassRuleConfig>
                {
                    Rule(first),
                },
                TileOverrides = new List<MovementTileOverrideConfig>(),
            };
            if (second.HasValue)
                config.ClassRules.Add(Rule(second.Value));
            if (tileOverride.HasValue)
            {
                config.TileOverrides.Add(new MovementTileOverrideConfig
                {
                    TileTypeId = tileOverride.Value.tileId,
                    Passable = tileOverride.Value.passable,
                    StaminaCost = tileOverride.Value.cost,
                });
            }

            SetJsonId(config, id);
            return config;
        }

        private static MovementClassRuleConfig Rule(
            (string classId, bool passable, float cost) value)
            => new()
            {
                ClassId = value.classId,
                Passable = value.passable,
                StaminaCost = value.cost,
            };

        private static void SetJsonId(MoyvaJsonConfigObject config, string id)
        {
            PropertyInfo property = typeof(MoyvaJsonConfigObject).GetProperty(
                nameof(MoyvaJsonConfigObject.JsonId),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            property.SetValue(config, id);
        }
    }
}
