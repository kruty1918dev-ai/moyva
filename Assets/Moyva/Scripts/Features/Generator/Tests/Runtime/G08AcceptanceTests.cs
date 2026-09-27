using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G08 acceptance invariants for the TWC shore band: shore cells exist
    /// exactly where land touches logical water (8-way — inner/outer corners,
    /// diagonals, narrow straits), water and untouched land are never
    /// converted, pre-existing sand joins the band for level correction, and
    /// banded cells rise to their water neighbour's terrain level.
    /// </summary>
    public sealed class G08AcceptanceTests
    {
        private sealed class FakeEnvironment : ITileWorldCreatorBuildEnvironment
        {
            public TileWorldCreatorManager Manager => null;
            public TileWorldCreatorIdMappingSO Mapping => null;
            public TileWorldCreatorBuildOptions Options { get; } =
                new TileWorldCreatorBuildOptions();
        }

        private static TileWorldCreatorShoreBandService Service() =>
            new TileWorldCreatorShoreBandService(new FakeEnvironment());

        private static GeneratedWorldData World(string[,] biomes, int[,] levels = null)
        {
            return new GeneratedWorldData
            {
                Width = biomes.GetLength(0),
                Height = biomes.GetLength(1),
                BiomeMap = biomes,
                TerrainLevelMap = levels,
            };
        }

        [Test]
        public void Land_TouchingWater_ConvertsToShoreBand_OrthogonalAndDiagonal()
        {
            var biomes = new string[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                biomes[x, y] = "grass";
            biomes[0, 0] = "water-ocean";

            var world = World(biomes);
            string shoreId = new FakeEnvironment().Options.ShoreBandTileId;
            Service().Expand(world);

            Assert.AreEqual(shoreId, biomes[1, 0], "orthogonal water neighbour must band");
            Assert.AreEqual(shoreId, biomes[1, 1], "diagonal water neighbour must band");
            Assert.AreEqual("water-ocean", biomes[0, 0], "water is never converted");
            Assert.AreEqual("grass", biomes[7, 7], "interior land untouched");
        }

        [Test]
        public void NarrowStrait_BothBanksBand()
        {
            var biomes = new string[12, 4];
            for (int x = 0; x < 12; x++)
            for (int y = 0; y < 4; y++)
                biomes[x, y] = "grass";
            for (int y = 0; y < 4; y++)
                biomes[6, y] = "water";

            var world = World(biomes);
            string shoreId = new FakeEnvironment().Options.ShoreBandTileId;
            Service().Expand(world);

            for (int y = 0; y < 4; y++)
            {
                Assert.AreEqual(shoreId, biomes[5, y], "west strait bank");
                Assert.AreEqual(shoreId, biomes[7, y], "east strait bank");
                Assert.AreEqual("water", biomes[6, y]);
            }
        }

        [Test]
        public void InteriorCorner_LandDiagonalToWater_Bands()
        {
            var biomes = new string[8, 8];
            for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
                biomes[x, y] = "water";
            biomes[3, 3] = "grass"; // land cell fully surrounded by water
            biomes[4, 4] = "grass";

            var world = World(biomes);
            string shoreId = new FakeEnvironment().Options.ShoreBandTileId;
            Service().Expand(world);

            Assert.AreEqual(shoreId, biomes[3, 3]);
            Assert.AreEqual(shoreId, biomes[4, 4]);
        }

        [Test]
        public void ExistingSand_IsBanded_AndLevelRaisedToWater()
        {
            var biomes = new string[6, 6];
            var levels = new int[6, 6];
            for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
            {
                biomes[x, y] = "grass";
                levels[x, y] = 4;
            }
            biomes[0, 0] = "water";
            levels[0, 0] = 0;
            biomes[1, 1] = "sand"; // diagonal water contact, pre-existing sand

            var world = World(biomes, levels);
            Service().Expand(world);

            Assert.AreEqual("sand", biomes[1, 1], "existing sand keeps its visual id");
            Assert.AreEqual(4, levels[1, 1],
                "band level = max(own, water neighbour) — own already higher stays");
        }

        [Test]
        public void BandedCell_RisesToWaterNeighbourLevel()
        {
            var biomes = new string[6, 6];
            var levels = new int[6, 6];
            for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
            {
                biomes[x, y] = "grass";
                levels[x, y] = 0;
            }
            biomes[0, 0] = "water";
            levels[0, 0] = 2; // elevated water level
            levels[1, 0] = 0;

            var world = World(biomes, levels);
            Service().Expand(world);

            Assert.AreEqual(2, levels[1, 0],
                "banded cell rises to its highest water neighbour level");
        }

        [Test]
        public void NullOrEmptyMaps_AreNoOp()
        {
            var service = Service();
            Assert.DoesNotThrow(() => service.Expand(null));
            Assert.DoesNotThrow(() => service.Expand(new GeneratedWorldData()));
            var empty = new string[0, 0];
            Assert.DoesNotThrow(() => service.Expand(World(empty)));
        }

        [Test]
        public void LandWithoutWaterContact_StaysUnchanged()
        {
            var biomes = new string[6, 6];
            for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
                biomes[x, y] = "grass";
            var world = World(biomes);
            Service().Expand(world);
            for (int x = 0; x < 6; x++)
            for (int y = 0; y < 6; y++)
                Assert.AreEqual("grass", biomes[x, y]);
        }
    }
}
