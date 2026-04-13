using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Signals
{
    [TestFixture]
    public sealed class UnitCreatedSignalTests
    {
        [Test]
        public void Fields_Default_AreNullOrZero()
        {
            var s = new UnitCreatedSignal();
            Assert.IsNull(s.UnitId);
            Assert.IsNull(s.UnitTypeId);
            Assert.AreEqual(Vector2Int.zero, s.Position);
            Assert.AreEqual(0, s.VisionRange);
            Assert.IsNull(s.UnitObject);
            Assert.IsNull(s.OwnerId);
        }

        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new UnitCreatedSignal
            {
                UnitId = "w1",
                UnitTypeId = "warrior",
                Position = new Vector2Int(5, 10),
                VisionRange = 3,
                OwnerId = "p0"
            };
            Assert.AreEqual("w1", s.UnitId);
            Assert.AreEqual("warrior", s.UnitTypeId);
            Assert.AreEqual(new Vector2Int(5, 10), s.Position);
            Assert.AreEqual(3, s.VisionRange);
            Assert.AreEqual("p0", s.OwnerId);
        }
    }

    [TestFixture]
    public sealed class UnitMovedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new UnitMovedSignal
            {
                UnitId = "u1",
                NewPosition = new Vector2Int(3, 4),
                Cost = 1.5f,
                SourceFactionId = "p0"
            };
            Assert.AreEqual("u1", s.UnitId);
            Assert.AreEqual(new Vector2Int(3, 4), s.NewPosition);
            Assert.AreEqual(1.5f, s.Cost, 0.001f);
            Assert.AreEqual("p0", s.SourceFactionId);
        }
    }

    [TestFixture]
    public sealed class UnitDestroyedSignalTests
    {
        [Test]
        public void UnitId_SetCorrectly()
        {
            var s = new UnitDestroyedSignal { UnitId = "u1" };
            Assert.AreEqual("u1", s.UnitId);
        }

        [Test]
        public void UnitId_Default_IsNull()
        {
            Assert.IsNull(new UnitDestroyedSignal().UnitId);
        }
    }

    [TestFixture]
    public sealed class InterruptMovementSignalTests
    {
        [Test]
        public void UnitId_SetCorrectly()
        {
            var s = new InterruptMovementSignal { UnitId = "u1" };
            Assert.AreEqual("u1", s.UnitId);
        }
    }

    [TestFixture]
    public sealed class FactionEliminatedSignalTests
    {
        [Test]
        public void FactionId_SetCorrectly()
        {
            var s = new FactionEliminatedSignal { FactionId = "bot_0" };
            Assert.AreEqual("bot_0", s.FactionId);
        }
    }

    [TestFixture]
    public sealed class OnMapObjectSpawnedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new OnMapObjectSpawnedSignal
            {
                ObjectId = "mountain",
                Position = new Vector2Int(10, 20)
            };
            Assert.AreEqual("mountain", s.ObjectId);
            Assert.AreEqual(new Vector2Int(10, 20), s.Position);
        }
    }

    [TestFixture]
    public sealed class OnObjectsMapChangedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new OnObjectsMapChangedSignal
            {
                Position = new Vector2Int(1, 2),
                OccupantId = "unit_5"
            };
            Assert.AreEqual(new Vector2Int(1, 2), s.Position);
            Assert.AreEqual("unit_5", s.OccupantId);
        }

        [Test]
        public void OccupantId_Null_WhenCleared()
        {
            var s = new OnObjectsMapChangedSignal { OccupantId = null };
            Assert.IsNull(s.OccupantId);
        }
    }

    [TestFixture]
    public sealed class FogStateChangedSignalTests
    {
        [Test]
        public void ChangedTilesCount_SetCorrectly()
        {
            var s = new FogStateChangedSignal { ChangedTilesCount = 42 };
            Assert.AreEqual(42, s.ChangedTilesCount);
        }
    }

    [TestFixture]
    public sealed class WorldBuiltSignalTests
    {
        [Test]
        public void CanBeConstructed()
        {
            Assert.DoesNotThrow(() => { var s = new WorldBuiltSignal(); });
        }
    }

    [TestFixture]
    public sealed class WorldGeneratedDataSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var tiles = new string[2, 2];
            var objects = new string[2, 2];
            var heights = new float[2, 2];

            var s = new WorldGeneratedDataSignal
            {
                Width = 2,
                Height = 2,
                TileMap = tiles,
                ObjectMap = objects,
                HeightMap = heights
            };
            Assert.AreEqual(2, s.Width);
            Assert.AreEqual(2, s.Height);
            Assert.AreSame(tiles, s.TileMap);
            Assert.AreSame(objects, s.ObjectMap);
            Assert.AreSame(heights, s.HeightMap);
        }
    }

    // --- Construction signals ---

    [TestFixture]
    public sealed class BuildingPlacedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new BuildingPlacedSignal
            {
                BuildingId = "townhall",
                Position = new Vector2Int(5, 5),
                OwnerId = "p0",
                SourceFactionId = "f0"
            };
            Assert.AreEqual("townhall", s.BuildingId);
            Assert.AreEqual(new Vector2Int(5, 5), s.Position);
            Assert.AreEqual("p0", s.OwnerId);
            Assert.AreEqual("f0", s.SourceFactionId);
        }
    }

    [TestFixture]
    public sealed class BuildingCancelledSignalTests
    {
        [Test]
        public void CanBeConstructed()
        {
            Assert.DoesNotThrow(() => { var s = new BuildingCancelledSignal(); });
        }
    }

    [TestFixture]
    public sealed class BuildingPreviewChangedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new BuildingPreviewChangedSignal
            {
                Position = new Vector2Int(3, 4),
                BuildingId = "barracks",
                PreviewState = BuildingPreviewState.Blocked
            };
            Assert.AreEqual(new Vector2Int(3, 4), s.Position);
            Assert.AreEqual("barracks", s.BuildingId);
            Assert.AreEqual(BuildingPreviewState.Blocked, s.PreviewState);
        }

        [Test]
        public void PreviewState_Valid()
        {
            var s = new BuildingPreviewChangedSignal { PreviewState = BuildingPreviewState.Valid };
            Assert.AreEqual(BuildingPreviewState.Valid, s.PreviewState);
        }

        [Test]
        public void PreviewState_None()
        {
            var s = new BuildingPreviewChangedSignal { PreviewState = BuildingPreviewState.None };
            Assert.AreEqual(BuildingPreviewState.None, s.PreviewState);
        }
    }

    [TestFixture]
    public sealed class BuildingDemolishedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new BuildingDemolishedSignal
            {
                BuildingId = "wall",
                Position = new Vector2Int(1, 2),
                OwnerId = "p0",
                SourceFactionId = "f0"
            };
            Assert.AreEqual("wall", s.BuildingId);
            Assert.AreEqual(new Vector2Int(1, 2), s.Position);
        }
    }

    [TestFixture]
    public sealed class ShowWallHandlesSignalTests
    {
        [Test]
        public void Show_SetCorrectly()
        {
            var s = new ShowWallHandlesSignal { Center = new Vector2Int(5, 5), Hide = false };
            Assert.AreEqual(new Vector2Int(5, 5), s.Center);
            Assert.IsFalse(s.Hide);
        }

        [Test]
        public void Hide_SetCorrectly()
        {
            var s = new ShowWallHandlesSignal { Hide = true };
            Assert.IsTrue(s.Hide);
        }
    }

    // --- Economy signals ---

    [TestFixture]
    public sealed class EconomyTickCompletedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new EconomyTickCompletedSignal
            {
                SettlementId = "s1",
                OwnerId = "p0",
                Turn = 5,
                TotalPopulation = 100,
                Arrivals = 10,
                Deaths = 2,
                ProductionCyclesCompleted = 3
            };
            Assert.AreEqual("s1", s.SettlementId);
            Assert.AreEqual("p0", s.OwnerId);
            Assert.AreEqual(5, s.Turn);
            Assert.AreEqual(100, s.TotalPopulation);
            Assert.AreEqual(10, s.Arrivals);
            Assert.AreEqual(2, s.Deaths);
            Assert.AreEqual(3, s.ProductionCyclesCompleted);
        }
    }

    [TestFixture]
    public sealed class SettlementCreatedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new SettlementCreatedSignal
            {
                SettlementId = "s1",
                OwnerId = "p0",
                TownHallPosition = new Vector2Int(10, 10)
            };
            Assert.AreEqual("s1", s.SettlementId);
            Assert.AreEqual(new Vector2Int(10, 10), s.TownHallPosition);
        }
    }

    [TestFixture]
    public sealed class SettlementDeactivatedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new SettlementDeactivatedSignal
            {
                SettlementId = "s1",
                OwnerId = "p0",
                Reason = "no_population"
            };
            Assert.AreEqual("s1", s.SettlementId);
            Assert.AreEqual("no_population", s.Reason);
        }
    }

    [TestFixture]
    public sealed class SettlementResourceChangedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new SettlementResourceChangedSignal
            {
                SettlementId = "s1",
                OwnerId = "p0",
                ResourceId = "gold",
                NewAmount = 150f,
                Delta = 50f
            };
            Assert.AreEqual("gold", s.ResourceId);
            Assert.AreEqual(150f, s.NewAmount, 0.001f);
            Assert.AreEqual(50f, s.Delta, 0.001f);
        }
    }

    [TestFixture]
    public sealed class ResourceDeficitSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new ResourceDeficitSignal
            {
                SettlementId = "s1",
                OwnerId = "p0",
                ResourceId = "food"
            };
            Assert.AreEqual("food", s.ResourceId);
        }
    }

    // --- Game Mode/State signals ---

    [TestFixture]
    public sealed class GameModeChangedSignalTests
    {
        [Test]
        public void Normal_Mode()
        {
            var s = new GameModeChangedSignal { NewMode = GameModeType.Normal };
            Assert.AreEqual(GameModeType.Normal, s.NewMode);
        }

        [Test]
        public void Construction_Mode()
        {
            var s = new GameModeChangedSignal { NewMode = GameModeType.Construction };
            Assert.AreEqual(GameModeType.Construction, s.NewMode);
        }

        [Test]
        public void Lobby_Mode()
        {
            var s = new GameModeChangedSignal { NewMode = GameModeType.Lobby };
            Assert.AreEqual(GameModeType.Lobby, s.NewMode);
        }
    }

    [TestFixture]
    public sealed class GameModeChangeRequestedSignalTests
    {
        [Test]
        public void RequestedMode_SetCorrectly()
        {
            var s = new GameModeChangeRequestedSignal { RequestedMode = GameModeType.Construction };
            Assert.AreEqual(GameModeType.Construction, s.RequestedMode);
        }
    }

    [TestFixture]
    public sealed class GameStartedSignalTests
    {
        [Test]
        public void CanBeConstructed()
        {
            Assert.DoesNotThrow(() => { var s = new GameStartedSignal(); });
        }
    }

    [TestFixture]
    public sealed class GameEndedSignalTests
    {
        [Test]
        public void WinnerId_SetCorrectly()
        {
            var s = new GameEndedSignal { WinnerId = "player_0" };
            Assert.AreEqual("player_0", s.WinnerId);
        }

        [Test]
        public void WinnerId_Null_ForDraw()
        {
            var s = new GameEndedSignal { WinnerId = null };
            Assert.IsNull(s.WinnerId);
        }
    }

    [TestFixture]
    public sealed class GamePausedSignalTests
    {
        [Test]
        public void IsPaused_True()
        {
            var s = new GamePausedSignal { IsPaused = true };
            Assert.IsTrue(s.IsPaused);
        }

        [Test]
        public void IsPaused_False()
        {
            var s = new GamePausedSignal { IsPaused = false };
            Assert.IsFalse(s.IsPaused);
        }
    }

    // --- Save signals ---

    [TestFixture]
    public sealed class SaveRequestedSignalTests
    {
        [Test]
        public void Slot_SetCorrectly()
        {
            var s = new SaveRequestedSignal { Slot = 3 };
            Assert.AreEqual(3, s.Slot);
        }
    }

    [TestFixture]
    public sealed class LoadRequestedSignalTests
    {
        [Test]
        public void Slot_SetCorrectly()
        {
            var s = new LoadRequestedSignal { Slot = 1 };
            Assert.AreEqual(1, s.Slot);
        }
    }

    [TestFixture]
    public sealed class SaveCompletedSignalTests
    {
        [Test]
        public void Success_SetCorrectly()
        {
            var s = new SaveCompletedSignal { Slot = 2, Success = true, ErrorMessage = null };
            Assert.AreEqual(2, s.Slot);
            Assert.IsTrue(s.Success);
            Assert.IsNull(s.ErrorMessage);
        }

        [Test]
        public void Failure_SetCorrectly()
        {
            var s = new SaveCompletedSignal { Slot = 1, Success = false, ErrorMessage = "disk full" };
            Assert.IsFalse(s.Success);
            Assert.AreEqual("disk full", s.ErrorMessage);
        }
    }

    // --- World Creation signals ---

    [TestFixture]
    public sealed class WorldCreationConfigDataTests
    {
        [Test]
        public void AllFields_CanBeSet()
        {
            var d = new WorldCreationConfigData
            {
                WorldName = "Test",
                Seed = 42,
                SizePresetIndex = 1,
                CustomWidth = 64,
                CustomHeight = 64,
                MapTypePresetIndex = 2,
                DifficultyIndex = 3,
                EnableBots = true,
                HumanPlayerCount = 2,
                BotCount = 2,
                StartingGold = 500,
                StartingFood = 300,
                ForestDensity = 0.5f,
                MountainDensity = 0.3f,
                WaterDensity = 0.2f,
                VillageDensity = 0.1f,
                GenerateRivers = true,
                GenerateBiomes = false,
                ApplyWFC = true
            };
            Assert.AreEqual("Test", d.WorldName);
            Assert.AreEqual(42, d.Seed);
            Assert.AreEqual(1, d.SizePresetIndex);
            Assert.AreEqual(64, d.CustomWidth);
            Assert.AreEqual(64, d.CustomHeight);
            Assert.AreEqual(2, d.MapTypePresetIndex);
            Assert.AreEqual(3, d.DifficultyIndex);
            Assert.IsTrue(d.EnableBots);
            Assert.AreEqual(2, d.HumanPlayerCount);
            Assert.AreEqual(2, d.BotCount);
            Assert.AreEqual(500, d.StartingGold);
            Assert.AreEqual(300, d.StartingFood);
            Assert.AreEqual(0.5f, d.ForestDensity, 0.001f);
            Assert.AreEqual(0.3f, d.MountainDensity, 0.001f);
            Assert.AreEqual(0.2f, d.WaterDensity, 0.001f);
            Assert.AreEqual(0.1f, d.VillageDensity, 0.001f);
            Assert.IsTrue(d.GenerateRivers);
            Assert.IsFalse(d.GenerateBiomes);
            Assert.IsTrue(d.ApplyWFC);
        }
    }

    [TestFixture]
    public sealed class WorldCreationConfirmedSignalTests
    {
        [Test]
        public void Config_SetCorrectly()
        {
            var data = new WorldCreationConfigData { WorldName = "World1" };
            var s = new WorldCreationConfirmedSignal { Config = data };
            Assert.AreEqual("World1", s.Config.WorldName);
        }
    }

    [TestFixture]
    public sealed class WorldCreationCancelledSignalTests
    {
        [Test]
        public void CanBeConstructed()
        {
            Assert.DoesNotThrow(() => { var s = new WorldCreationCancelledSignal(); });
        }
    }

    // --- Building Info signals ---

    [TestFixture]
    public sealed class WorldInfoPanelRequestedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new WorldInfoPanelRequestedSignal
            {
                Title = "T",
                Subtitle = "S",
                Content = "C"
            };
            Assert.AreEqual("T", s.Title);
            Assert.AreEqual("S", s.Subtitle);
            Assert.AreEqual("C", s.Content);
        }
    }

    [TestFixture]
    public sealed class WorldInfoPanelClosedSignalTests
    {
        [Test]
        public void CanBeConstructed()
        {
            Assert.DoesNotThrow(() => { var s = new WorldInfoPanelClosedSignal(); });
        }
    }

    [TestFixture]
    public sealed class BuildingInfoPanelRequestedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new BuildingInfoPanelRequestedSignal
            {
                BuildingId = "b1",
                Position = new Vector2Int(2, 3)
            };
            Assert.AreEqual("b1", s.BuildingId);
            Assert.AreEqual(new Vector2Int(2, 3), s.Position);
        }
    }

    [TestFixture]
    public sealed class UnitInfoPanelRequestedSignalTests
    {
        [Test]
        public void Fields_SetCorrectly()
        {
            var s = new UnitInfoPanelRequestedSignal
            {
                UnitId = "u1",
                Position = new Vector2Int(5, 6)
            };
            Assert.AreEqual("u1", s.UnitId);
            Assert.AreEqual(new Vector2Int(5, 6), s.Position);
        }
    }

    [TestFixture]
    public sealed class WorldInfoSelectionChangedSignalTests
    {
        [Test]
        public void Building_Selection()
        {
            var s = new WorldInfoSelectionChangedSignal
            {
                Kind = WorldInfoSelectionKind.Building,
                ObjectId = "townhall",
                Position = new Vector2Int(10, 10)
            };
            Assert.AreEqual(WorldInfoSelectionKind.Building, s.Kind);
            Assert.AreEqual("townhall", s.ObjectId);
        }

        [Test]
        public void Unit_Selection()
        {
            var s = new WorldInfoSelectionChangedSignal
            {
                Kind = WorldInfoSelectionKind.Unit,
                ObjectId = "warrior-01"
            };
            Assert.AreEqual(WorldInfoSelectionKind.Unit, s.Kind);
        }

        [Test]
        public void None_Selection()
        {
            var s = new WorldInfoSelectionChangedSignal { Kind = WorldInfoSelectionKind.None };
            Assert.AreEqual(WorldInfoSelectionKind.None, s.Kind);
        }
    }

    // --- TileClickedSignal ---

    [TestFixture]
    public sealed class TileClickedSignalTests
    {
        [Test]
        public void Position_SetCorrectly()
        {
            var s = new TileClickedSignal { Position = new Vector2Int(7, 8) };
            Assert.AreEqual(new Vector2Int(7, 8), s.Position);
        }

        [Test]
        public void Position_Default_IsZero()
        {
            var s = new TileClickedSignal();
            Assert.AreEqual(Vector2Int.zero, s.Position);
        }
    }

    // --- Enum coverage ---

    [TestFixture]
    public sealed class BuildingPreviewStateEnumTests
    {
        [Test]
        public void None_IsZero()
        {
            Assert.AreEqual(0, (int)BuildingPreviewState.None);
        }

        [Test]
        public void Valid_IsOne()
        {
            Assert.AreEqual(1, (int)BuildingPreviewState.Valid);
        }

        [Test]
        public void Blocked_IsTwo()
        {
            Assert.AreEqual(2, (int)BuildingPreviewState.Blocked);
        }
    }

    [TestFixture]
    public sealed class GameModeTypeEnumTests
    {
        [Test]
        public void Normal_IsZero()
        {
            Assert.AreEqual(0, (int)GameModeType.Normal);
        }

        [Test]
        public void Construction_IsOne()
        {
            Assert.AreEqual(1, (int)GameModeType.Construction);
        }

        [Test]
        public void Lobby_IsTen()
        {
            Assert.AreEqual(10, (int)GameModeType.Lobby);
        }
    }

    [TestFixture]
    public sealed class WorldInfoSelectionKindEnumTests
    {
        [Test]
        public void None_IsZero()
        {
            Assert.AreEqual(0, (int)WorldInfoSelectionKind.None);
        }

        [Test]
        public void Building_IsOne()
        {
            Assert.AreEqual(1, (int)WorldInfoSelectionKind.Building);
        }

        [Test]
        public void Unit_IsTwo()
        {
            Assert.AreEqual(2, (int)WorldInfoSelectionKind.Unit);
        }
    }
}
