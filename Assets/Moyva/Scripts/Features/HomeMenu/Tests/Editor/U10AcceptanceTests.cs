using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.WorldCreation.API;
using Kruty1918.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    /// <summary>
    /// U10 acceptance invariants for the Continue panel: the slot list shows
    /// only existing saves, a stale slot never starts a game and surfaces a
    /// localized reason, a valid slot configures a MenuLoadGame launch and an
    /// offline session, double-select is guarded while a load is in flight,
    /// and a failed/cancelled load reports an error without silently falling
    /// back to New Game.
    /// </summary>
    public sealed class U10AcceptanceTests
    {
        private sealed class FakeContinueView : IContinueViewController
        {
            public readonly List<GameSlotInfo> Slots = new();
            public event Action<GameSlotInfo> OnSlotSelected;
            public void AddSlot(GameSlotInfo slot) => Slots.Add(slot);
            public void RemoveSlot(string slotName)
                => Slots.RemoveAll(s => s.SlotName == slotName);
            public void ClearSlots() => Slots.Clear();
            public void RefreshSlots() { }
            public void Select(GameSlotInfo slot) => OnSlotSelected?.Invoke(slot);
        }

        private sealed class FakeSaveService : ISaveService
        {
            public readonly Dictionary<int, SaveSlotInfo> Slots = new();
            public void Save(int slot = 0) { }
            public void Load(int slot = 0) { }
            public bool HasSave(int slot = 0) => Slots.TryGetValue(slot, out var i) && i.Exists;
            public void Delete(int slot = 0) => Slots.Remove(slot);
            public SaveSlotInfo GetSlotInfo(int slot = 0)
                => Slots.TryGetValue(slot, out var i)
                    ? i
                    : new SaveSlotInfo(slot, false, 0, DateTime.MinValue);
        }

        private sealed class FakeGameStarter : IHomeMenuGameStarter
        {
            public int Calls;
            public TaskCompletionSource<bool> Gate;
            public Exception ThrowOnStart;

            public Task StartGameAsync(CancellationToken ct = default)
            {
                Calls++;
                if (ThrowOnStart != null)
                    return Task.FromException(ThrowOnStart);
                return Gate?.Task ?? Task.CompletedTask;
            }
        }

        private sealed class FakeGameplaySession : IGameplaySession
        {
            public NetworkProviderType? AppliedMode;
            public WorldSettingsDto? AppliedWorld;
            public string AppliedLocalPlayerId;
            public int ApplyCalls;

            public bool IsHost => true;
            public NetworkProviderType Mode => AppliedMode ?? NetworkProviderType.Offline;
            public WorldSettingsDto WorldSettings => AppliedWorld ?? default;
            public IReadOnlyList<GameplayPlayer> Players => null;
            public GameplayPlayer LocalPlayer => default;
            public GameplayPlayer Host => default;

            public void Apply(NetworkProviderType mode, WorldSettingsDto worldSettings,
                IReadOnlyList<GameplayPlayer> players, string localPlayerId)
            {
                AppliedMode = mode;
                AppliedWorld = worldSettings;
                AppliedLocalPlayerId = localPlayerId;
                ApplyCalls++;
            }
            public void Clear() { }
        }

        private sealed class FakeInfoPanel : IInfoPanelService
        {
            public readonly List<InfoMessage> Messages = new();
            public bool IsShown => Messages.Count > 0;
            public void Show(InfoMessage message) => Messages.Add(message);
            public void ForceHide() => Messages.Clear();
        }

        private DiContainer _container;
        private FakeContinueView _view;
        private FakeSaveService _saves;
        private FakeGameStarter _starter;
        private FakeGameplaySession _session;
        private FakeInfoPanel _info;
        private ContinuePanelService _service;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            global::Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<SaveCompletedSignal>().OptionalSubscriber();
            _view = new FakeContinueView();
            _saves = new FakeSaveService();
            _starter = new FakeGameStarter();
            _session = new FakeGameplaySession();
            _info = new FakeInfoPanel();

            _container.Bind<IContinueViewController>().FromInstance(_view);
            _container.Bind<ISaveService>().FromInstance(_saves);
            _container.Bind<IGameplaySession>().FromInstance(_session);
            _container.Bind<IHomeMenuGameStarter>().FromInstance(_starter);
            _container.Bind<IInfoPanelService>().FromInstance(_info);

            _service = _container.Instantiate<ContinuePanelService>();
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            // Never leak launch-context state into other tests.
            GameLaunchContext.Reset();
        }

        private void SeedSlot(int index, string worldName)
            => _saves.Slots[index] = new SaveSlotInfo(
                index, true, 1024, DateTime.UtcNow, worldName);

        [Test]
        public void Initialize_ListsOnlyExistingSlots_WithWorldNames()
        {
            SeedSlot(2, "River Realm");
            SeedSlot(7, "Dust March");
            _service.Initialize();

            Assert.AreEqual(2, _view.Slots.Count,
                "only existing save slots may be listed");
            Assert.AreEqual(2, _view.Slots[0].SlotIndex);
            Assert.AreEqual("River Realm", _view.Slots[0].SlotName);
            Assert.AreEqual(7, _view.Slots[1].SlotIndex);
            Assert.AreEqual("Dust March", _view.Slots[1].SlotName);
        }

        [Test]
        public void StaleSlot_ShowsReason_AndNeverStartsGame()
        {
            _service.Initialize(); // no saves exist

            _view.Select(new GameSlotInfo { SlotIndex = 3, SlotName = "ghost" });
            // allow the async-void handler to finish synchronously
            Assert.AreEqual(0, _starter.Calls,
                "a missing save must never launch the game");
            Assert.AreEqual(0, _session.ApplyCalls,
                "a missing save must not touch the session");
            Assert.IsTrue(_info.IsShown,
                "the UI must surface a reason instead of silently failing");
            Assert.AreNotEqual(GameLaunchMode.MenuLoadGame, GameLaunchContext.Mode,
                "a rejected load must not configure a load-game launch");
        }

        [Test]
        public void ValidSlot_ConfiguresMenuLoad_AndOfflineSession()
        {
            SeedSlot(4, "Iron Fields");
            _service.Initialize();

            _view.Select(new GameSlotInfo { SlotIndex = 4, SlotName = "Iron Fields" });

            Assert.AreEqual(GameLaunchMode.MenuLoadGame, GameLaunchContext.Mode,
                "continue must route through the save-load launch path");
            Assert.AreEqual(4, GameLaunchContext.SaveSlot,
                "the chosen slot index must reach the launch context");
            Assert.AreEqual(1, _starter.Calls);
            Assert.AreEqual(1, _session.ApplyCalls);
            Assert.AreEqual(NetworkProviderType.Offline, _session.AppliedMode,
                "a loaded local save must run as an offline session");
            Assert.IsFalse(_info.IsShown);
        }

        [Test]
        public void DoubleSelect_WhileStarting_IsGuarded()
        {
            SeedSlot(1, "World A");
            SeedSlot(2, "World B");
            _service.Initialize();
            _starter.Gate = new TaskCompletionSource<bool>();

            _view.Select(new GameSlotInfo { SlotIndex = 1, SlotName = "World A" });
            _view.Select(new GameSlotInfo { SlotIndex = 2, SlotName = "World B" });

            Assert.AreEqual(1, _starter.Calls,
                "a second selection while loading must be ignored");
            Assert.AreEqual(1, GameLaunchContext.SaveSlot);

            _starter.Gate.SetResult(true);
        }

        [Test]
        public void FailedLoad_ReportsError_WithoutFallbackToNewGame()
        {
            SeedSlot(5, "Broken World");
            _service.Initialize();
            _starter.ThrowOnStart = new InvalidOperationException("corrupt payload");
            UnityEngine.TestTools.LogAssert.Expect(
                UnityEngine.LogType.Error,
                new System.Text.RegularExpressions.Regex(
                    @"\[ContinuePanelService\] Failed to load slot 05"));

            _view.Select(new GameSlotInfo { SlotIndex = 5, SlotName = "Broken World" });

            Assert.IsTrue(_info.IsShown,
                "load failure must surface a visible error");
            Assert.IsTrue(_info.Messages[0].Title.Contains("Load Failed")
                          || _info.Messages[0].Message.Contains("corrupt payload"),
                "error text must identify the failure");
            Assert.AreEqual(1, _starter.Calls);
            Assert.AreNotEqual(GameLaunchMode.MenuNewGame, GameLaunchContext.Mode,
                "a failed load must never fall back to a fresh game");
        }

        [Test]
        public void SaveCompletion_RefreshesSlotList()
        {
            _service.Initialize();
            Assert.AreEqual(0, _view.Slots.Count);

            SeedSlot(9, "Late Save");
            _container.Resolve<SignalBus>().Fire(new SaveCompletedSignal());
            Assert.AreEqual(1, _view.Slots.Count,
                "a fresh save must appear in the list without a restart");
            Assert.AreEqual("Late Save", _view.Slots[0].SlotName);
        }
    }
}
