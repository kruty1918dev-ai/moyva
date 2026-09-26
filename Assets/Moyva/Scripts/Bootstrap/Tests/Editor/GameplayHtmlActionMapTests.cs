using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using NUnit.Framework;
using ReactUnity.UGUI.Behaviours;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityHTML.Runtime;
using Zenject;
using Kruty1918.InputRouting.API;
using Kruty1918.Notifications.API;
using Kruty1918.UIActions.API;

namespace Kruty1918.Moyva.Tests.Bootstrap
{
    /// <summary>
    /// P070: gameplay action map. Mounts each HUD screen's real markup against
    /// the real Globals.gameplay bridge, clicks every enabled button, and
    /// requires an observable effect — a routed domain action, a recorded
    /// service call, or a UI-state change. A control that looks active but
    /// produces nothing fails the sweep unless the markup itself marks it
    /// (e.g. the currently active tab carries the "active" class).
    /// </summary>
    [TestFixture]
    internal sealed class GameplayHtmlActionMapTests
    {
        private static string HudCss => File.ReadAllText(Path.Combine(
            Application.dataPath, "Moyva", "UI", "Gameplay", "GameplayHud.css.txt"));

        private static GameObject CreateRoot()
        {
            var root = new GameObject("P070 Action Map Root", typeof(RectTransform), typeof(Canvas));
            var rect = root.GetComponent<RectTransform>();
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1280f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 720f);
            return root;
        }

        // ---------- recording fakes ----------

        private sealed class RecordingRouter : IUiActionRouter
        {
            public readonly List<string> Executed = new();

            public UiActionResult Execute(in UiActionRequest request)
                => Execute(request.ActionId, request.Source, request.ContextId, request.TargetId);

            public UiActionResult Execute(UiActionId actionId,
                UiActionSource source = UiActionSource.Programmatic,
                string contextId = null, string targetId = null)
            {
                Executed.Add(actionId.ToString());
                return UiActionResult.Performed();
            }
        }

        private sealed class RecordingConstruction : IConstructionSessionCommands
        {
            public int Calls;

            public BuildingPlacementState State => default;
            public bool IsDemolishMode => false;
            public int PendingDemolitionCount => 0;
            public void SelectBuilding(string buildingId) => Calls++;
            public string GetSelectedBuildingId() => null;
            public void SetActiveOwner(string ownerId) => Calls++;
            public string GetActiveOwner() => "player_0";
            public bool TryPreviewAt(Vector2Int position) => false;
            public bool HasPendingPlacementAt(Vector2Int position) => false;
            public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
            { buildingId = null; return false; }
            public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
                => new Dictionary<Vector2Int, string> { { new Vector2Int(3, 4), "lumber-camp" } };
            public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition) => false;
            public bool RemovePendingAt(Vector2Int position) => false;
            public bool TryGetPendingPlacementStatus(Vector2Int position,
                out ConstructionPendingPlacementStatus status)
            {
                status = new ConstructionPendingPlacementStatus(
                    position, "lumber-camp", "northhold", "Northhold", true, true, null);
                return true;
            }
            public ConstructionResourceProjection GetResourceProjection(Vector2Int position) => null;
            public IReadOnlyDictionary<string, float> GetBuildingResourceCosts(string buildingId)
                => new Dictionary<string, float> { ["wood"] = 35f };
            public void Confirm() => Calls++;
            public void Cancel() => Calls++;
            public void UndoLast() => Calls++;
            public void RedoLast() => Calls++;
            public void ToggleDemolishMode() => Calls++;
            public bool TryDemolishAt(Vector2Int position) { Calls++; return false; }
            public string GetLastActionMessage() => null;
        }

        private sealed class FakeRoles : ILocalGameplayRoleResolver
        {
            public LocalGameplayRoleSnapshot Resolve()
                => new LocalGameplayRoleSnapshot(LocalGameplayRole.Host, "player_0");
        }

        private sealed class FakeTurns : ITurnService
        {
            public int Calls;
#pragma warning disable CS0067 // event required by the contract; never raised in tests
            public event Action StateChanged;
#pragma warning restore CS0067
            public TurnPhase Phase => TurnPhase.AwaitingInput;
            public int Round => 4;
            public long GlobalTurn => 11;
            public int ActionsThisTurn => 2;
            public string ActiveOwnerId => "player_0";
            public string LocalOwnerId => "player_0";
            public IReadOnlyList<TurnFaction> Factions { get; }
                = new[] { new TurnFaction("player_0", new Vector2Int(0, 0)) };
            public bool IsOwnerActive(string ownerId) => ownerId == "player_0";
            public bool CanOwnerAct(string ownerId, out string reason) { reason = null; return true; }
            public bool TryRecordAction(string ownerId, string actionId) => true;
            public bool TryEndTurn(string requesterOwnerId, out string reason)
            { Calls++; reason = null; return true; }
        }

        private sealed class FakeBuildings : IBuildingRegistry
        {
            public BuildingDefinition[] GetAll() => Array.Empty<BuildingDefinition>();
            public BuildingDefinition GetById(string id) => null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId) => null;
        }

        private sealed class RecordingCameraFocus : IGameplayCameraFocusService
        {
            public int Calls;
            public void FocusGridPosition(Vector2Int gridPosition, string targetId = null) => Calls++;
            public void FocusSelected() => Calls++;
            public void FocusCapital() => Calls++;
        }

        private sealed class RecordingExit : IExitMatchCoordinator
        {
            public int Calls;
            public bool IsExiting => false;
            public Task<ExitMatchResult> ExitToMenuAsync(CancellationToken cancellationToken = default)
            { Calls++; return Task.FromResult(ExitMatchResult.Cancellation()); }
            public Task SaveBeforeExitAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public Task DisconnectBeforeExitAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            public Task LoadHomeMenuAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        }

        private sealed class RecordingClock : IGameplayProgressClock
        {
            public int Calls;
#pragma warning disable CS0067
            public event Action<GameplayProgressTick> Progressed;
#pragma warning restore CS0067
            public GameplayProgressMode Mode => GameplayProgressMode.SandboxRealtime;
            public bool IsRealtime => true;
            public float SandboxRoundSeconds => 30f;
            public float Speed => 1f;
            public long CurrentSequence => 0;
            public double ElapsedGameplaySeconds => 0;
            public float SecondsUntilNextProgress => 30f;
            public void Configure(GameplayProgressMode mode, float sandboxRoundSeconds, float speed) => Calls++;
            public void SetSpeed(float speed) => Calls++;
        }

        private sealed class RecordingSupplyService : IConstructionSupplyService
        {
            public int Calls;
#pragma warning disable CS0067
            public event Action Changed;
#pragma warning restore CS0067
            public ConstructionSupplyEvaluation Evaluate(string ownerId, string buildingId,
                Vector2Int position, IReadOnlyDictionary<string, float> requiredCosts)
                => new ConstructionSupplyEvaluation(true, "northhold", "Northhold", position, null,
                    new[] { new ConstructionSupplyResourceLine("wood", 35f, 0f, 0f, 35f) },
                    new[]
                    {
                        new ConstructionSupplySourceSnapshot("northhold", "Northhold", "wh-1",
                            new Dictionary<string, float> { ["wood"] = 76f }, 12f),
                    },
                    new[]
                    {
                        new ConstructionSupplyWagonSnapshot("wagon-1", new Vector2Int(40, 26),
                            50f, 10f, false, "Idle"),
                    });
            public CaravanTransferResult DispatchSupply(ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts)
            { Calls++; return CaravanTransferResult.Success(); }
            public IReadOnlyDictionary<string, float> PreviewShipment(
                ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts)
                => new Dictionary<string, float> { ["wood"] = 35f };
            public IReadOnlyList<ConstructionSupplyOrderSnapshot> GetOrders(string ownerId)
                => Array.Empty<ConstructionSupplyOrderSnapshot>();
            public bool TryGetOrderAt(Vector2Int position, out ConstructionSupplyOrderSnapshot snapshot)
            { snapshot = default; return false; }
            public IReadOnlyDictionary<string, float> GetResourcesForPlacement(
                string settlementId, Vector2Int position)
                => new Dictionary<string, float>();
            public void CancelOrderAt(Vector2Int position) => Calls++;
            public CaravanTransferResult ApplyConfirmedDispatch(ConstructionSupplyDispatchRequest request,
                IReadOnlyDictionary<string, float> requiredCosts) => CaravanTransferResult.Success();
            public void ApplyConfirmedCancelOrder(string ownerId, string unitId, Vector2Int position) { }
        }

        private sealed class RecordingCaravans : ICaravanService
        {
            public int Calls;
#pragma warning disable CS0067
            public event Action Changed;
            public event Action<CaravanRouteTransferCommitted> RouteTransferCommitted;
#pragma warning restore CS0067
            public bool TryGetCargo(string ownerId, string unitId, out CaravanCargoSnapshot snapshot)
            {
                snapshot = new CaravanCargoSnapshot(
                    new CaravanUnitSnapshot(ownerId, new Vector2Int(41, 27), 50f),
                    new Dictionary<string, float> { ["wood"] = 20f, ["food"] = 10f });
                return true;
            }
            public IReadOnlyDictionary<string, float> GetLootAtWagon(string ownerId, string unitId)
                => new Dictionary<string, float>();
            public CaravanTransferResult CanExecute(CaravanCargoRequest request)
                => CaravanTransferResult.Success();
            public CaravanTransferResult Execute(CaravanCargoRequest request)
            { Calls++; return CaravanTransferResult.Success(); }
            public CaravanTransferResult SetRoute(CaravanRouteRequest request)
            { Calls++; return CaravanTransferResult.Success(); }
            public CaravanTransferResult CanSetRoute(CaravanRouteRequest request)
                => CaravanTransferResult.Success();
            public CaravanTransferResult StopRoute(string ownerId, string unitId)
            { Calls++; return CaravanTransferResult.Success(); }
            public bool TryGetRoute(string ownerId, string unitId, out CaravanRouteSnapshot snapshot)
            { snapshot = default; return false; }
            public IReadOnlyList<CaravanRouteSnapshot> GetRoutes(string ownerId)
                => Array.Empty<CaravanRouteSnapshot>();
            public CaravanTransferResult CanFoundSettlement(string ownerId, string unitId,
                string buildingId, out Vector2Int position)
            { position = new Vector2Int(44, 30); return CaravanTransferResult.Success(); }
            public CaravanTransferResult FoundSettlement(string ownerId, string unitId, string buildingId)
            { Calls++; return CaravanTransferResult.Success(); }
            public CaravanTransferResult ApplyConfirmedTransfer(CaravanCargoRequest request)
                => CaravanTransferResult.Success();
            public CaravanTransferResult ApplyConfirmedRouteTransfer(CaravanCargoRequest request,
                CaravanRoutePhase nextPhase) => CaravanTransferResult.Success();
            public CaravanTransferResult ApplyConfirmedSetRoute(CaravanRouteRequest request)
                => CaravanTransferResult.Success();
            public CaravanTransferResult ApplyConfirmedStopRoute(string ownerId, string unitId)
                => CaravanTransferResult.Success();
            public CaravanTransferResult ApplyConfirmedFoundSettlement(string ownerId, string unitId,
                string buildingId, Vector2Int position) => CaravanTransferResult.Success();
        }

        private sealed class FakeEconomy : IEconomyRuntimeApi
        {
            public IReadOnlyList<string> GetSettlementIdsForOwner(string ownerId)
                => new[] { "northhold", "rivergate" };
            public EconomyCategoryTotals GetOwnerCategoryTotals(string ownerId) => default;
            public EconomyFormattedCategoryTotals GetFormattedOwnerCategoryTotals(string ownerId) => default;
            public Dictionary<string, float> GetOwnerResourceTotals(string ownerId)
                => new Dictionary<string, float>();
            public EconomyCategoryTotals GetSettlementCategoryTotals(string settlementId) => default;
            public EconomyFormattedCategoryTotals GetFormattedSettlementCategoryTotals(string settlementId) => default;
            public Dictionary<string, float> GetSettlementResourceTotals(string settlementId)
                => new Dictionary<string, float>();
            public IReadOnlyList<EconomyWarehouseSnapshot> GetOwnerWarehouseSnapshots(string ownerId)
                => new[]
                {
                    new EconomyWarehouseSnapshot("wh-1", "warehouse", "northhold", "Northhold",
                        new Vector2Int(42, 27), 186f, 250,
                        new Dictionary<string, float> { ["food"] = 110f, ["wood"] = 76f }),
                    new EconomyWarehouseSnapshot("wh-2", "warehouse", "rivergate", "Rivergate",
                        new Vector2Int(67, 31), 98f, 200,
                        new Dictionary<string, float> { ["food"] = 44f, ["gold"] = 54f }),
                };
            public IReadOnlyList<EconomySettlementSnapshot> GetOwnerSettlementSnapshots(string ownerId)
                => Array.Empty<EconomySettlementSnapshot>();
            public EconomyProductionReadSnapshot GetOwnerProductionSnapshot(string ownerId) => default;
        }

        // ---------- fixture ----------

        private sealed class Fixture : IDisposable
        {
            public readonly GameplayHtmlState State = new();
            public readonly RecordingRouter Router = new();
            public readonly RecordingConstruction Construction = new();
            public readonly RecordingCameraFocus CameraFocus = new();
            public readonly RecordingExit Exit = new();
            public readonly RecordingClock Clock = new();
            public readonly RecordingSupplyService SupplyService = new();
            public readonly RecordingCaravans Caravans = new();
            public readonly FakeEconomy Economy = new();
            public readonly FakeTurns Turns = new();
            public readonly SignalBus Signals;
            public readonly GameplayHudReadModel ReadModel;
            public readonly GameplaySupplyPanel SupplyPanel;
            public readonly GameplayCargoPanel CargoPanel;
            public readonly GameplayHtmlBridge Bridge;
            public int Changes;
            public int PanelClosedSignals;

            public Fixture()
            {
                var container = new DiContainer();
                Zenject.SignalBusInstaller.Install(container);
                container.DeclareSignal<WorldInfoSelectionChangedSignal>().OptionalSubscriber();
                container.DeclareSignal<UnitMovedSignal>().OptionalSubscriber();
                container.DeclareSignal<WorldInfoPanelClosedSignal>().OptionalSubscriber();
                container.DeclareSignal<BuildingInfoPanelRequestedSignal>().OptionalSubscriber();
                Signals = container.Resolve<SignalBus>();

                var roles = new FakeRoles();
                SupplyPanel = new GameplaySupplyPanel(
                    Construction, SupplyService, roles, Turns, State);
                CargoPanel = new GameplayCargoPanel(
                    Caravans, Economy, roles, State, Signals, Turns);
                SupplyPanel.Initialize();
                CargoPanel.Initialize();
                ReadModel = new GameplayHudReadModel(
                    Turns, Economy, Construction, new FakeBuildings(),
                    roleResolver: roles, progressClock: Clock,
                    cargoPanel: CargoPanel, supplyPanel: SupplyPanel,
                    supply: SupplyService, caravans: Caravans);
                container.Bind<IUiActionRouter>().FromInstance(Router);
                var lazy = new LazyInject<IUiActionRouter>(container,
                    new InjectContext(container, typeof(IUiActionRouter)));
                Bridge = new GameplayHtmlBridge(
                    State, ReadModel, lazy, Construction, roles,
                    CameraFocus, Exit, Clock, CargoPanel, SupplyPanel, Signals);
                State.Changed += () => Changes++;
                Signals.Subscribe<WorldInfoPanelClosedSignal>(() => PanelClosedSignals++);
            }

            /// <summary>Anything a user would notice: a routed domain action,
            /// a service call, a fired signal or a UI-state mutation.</summary>
            public int EffectCount => Changes + Router.Executed.Count + Construction.Calls
                + CameraFocus.Calls + Exit.Calls + Clock.Calls
                + SupplyService.Calls + Caravans.Calls + Turns.Calls + PanelClosedSignals;

            /// <summary>Selection panels are not OpenPanelId-based; the read model
            /// must believe a world object is selected for close/dismiss flows.</summary>
            public void SelectWorldObject(WorldInfoSelectionKind kind, string objectId)
            {
                var signal = new WorldInfoSelectionChangedSignal
                {
                    Kind = kind,
                    ObjectId = objectId,
                    Position = new Vector2Int(42, 27),
                };
                ReadModel.SetSelection(signal);
                Signals.Fire(signal);
            }

            public void Dispose()
            {
                SupplyPanel.Dispose();
                CargoPanel.Dispose();
            }
        }

        private static string Describe(Button button)
        {
            var label = button.GetComponentInChildren<TMP_Text>(true);
            string text = label != null ? label.text?.Trim() : null;
            var element = button.GetComponent<ReactElement>();
            string classes = element?.Component?.ClassName ?? string.Empty;
            return string.IsNullOrWhiteSpace(text)
                ? $"{button.name} [{classes}]"
                : $"\"{text}\" [{classes}]";
        }

        /// <summary>Re-selecting the active tab/filter is a legit no-op; the
        /// markup marks it with the "active" class, which is the explanation.</summary>
        private static bool HasActiveMarker(Button button)
        {
            var element = button.GetComponent<ReactElement>();
            string classes = element?.Component?.ClassName ?? string.Empty;
            return classes.Split(' ').Any(token => token == "active");
        }

        /// <summary>Mounts the markup once to count enabled controls, then
        /// re-mounts a fresh fixture per button so every control is clicked in
        /// exactly the state it was rendered in — earlier clicks can never make
        /// a later button look dead (e.g. CLEAR ALL invalidating a stale VIEW).</summary>
        private static void SweepEnabledButtons(string scenario, GameplayHtmlSnapshot snapshot,
            Action<Fixture, GameplayHtmlSnapshot> configure)
        {
            int enabled;
            var labels = new List<string>();
            using (var probe = new Fixture())
            {
                configure?.Invoke(probe, snapshot);
                string probeHtml = GameplayHtmlMarkup.Build(snapshot, probe.State, "vp-wide");
                var probeRoot = CreateRoot();
                using (var probeHost = new UnityHtmlHost())
                {
                    try
                    {
                        UnityHtmlMountResult result = probeHost.Mount(
                            probeRoot.GetComponent<RectTransform>(),
                            new UnityHtmlDocument(probeHtml, HudCss, $"P070 {scenario} probe"),
                            new Dictionary<string, object> { ["gameplay"] = probe.Bridge });
                        Assert.IsTrue(result.Succeeded, result.ErrorMessage);
                        foreach (Button button in probeRoot.GetComponentsInChildren<Button>(true))
                            if (button.interactable)
                                labels.Add(Describe(button));
                    }
                    finally
                    {
                        UnityEngine.Object.DestroyImmediate(probeRoot);
                    }
                }
            }
            enabled = labels.Count;
            Assert.Greater(enabled, 0, $"{scenario}: no enabled buttons rendered.");

            var dead = new List<string>();
            int explainedNoops = 0;
            for (int index = 0; index < enabled; index++)
            {
                using var fixture = new Fixture();
                configure?.Invoke(fixture, snapshot);
                string html = GameplayHtmlMarkup.Build(snapshot, fixture.State, "vp-wide");
                var root = CreateRoot();
                using var host = new UnityHtmlHost();
                try
                {
                    UnityHtmlMountResult result = host.Mount(
                        root.GetComponent<RectTransform>(),
                        new UnityHtmlDocument(html, HudCss, $"P070 {scenario}"),
                        new Dictionary<string, object> { ["gameplay"] = fixture.Bridge });
                    Assert.IsTrue(result.Succeeded, result.ErrorMessage);

                    var buttons = root.GetComponentsInChildren<Button>(true)
                        .Where(b => b.interactable).ToArray();
                    Button button = buttons[index];
                    int before = fixture.EffectCount;
                    button.onClick.Invoke();
                    if (fixture.EffectCount == before)
                    {
                        if (HasActiveMarker(button)) explainedNoops++;
                        else dead.Add($"{labels[index]} @click#{index}");
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }

            Assert.IsEmpty(dead,
                $"{scenario}: {dead.Count} enabled button(s) produced no routed action, " +
                $"service call or state change: {string.Join(", ", dead)}");
            TestContext.Out.WriteLine(
                $"[P070] {scenario}: {enabled} enabled clicked in isolation, " +
                $"{explainedNoops} explained no-op (active tab/filter).");
        }

        // ---------- screens ----------

        private static GameplayHtmlAnchor.PreviewScreen Screen(
            string name) => (GameplayHtmlAnchor.PreviewScreen)Enum.Parse(
                typeof(GameplayHtmlAnchor.PreviewScreen), name);

        [Test]
        public void ActionMap_NormalHud_EveryEnabledButtonActs()
            => SweepEnabledButtons("Normal",
                GameplayHtmlSnapshot.CreatePreview(Screen("Normal")), null);

        [Test]
        public void ActionMap_FirstCastlePlacement_EveryEnabledButtonActs()
            => SweepEnabledButtons("FirstCastle",
                GameplayHtmlSnapshot.CreatePreview(Screen("FirstCastle")), null);

        [Test]
        public void ActionMap_ConstructionPanel_EveryEnabledButtonActs()
            => SweepEnabledButtons("Construction",
                GameplayHtmlSnapshot.CreatePreview(Screen("Construction")),
                (fixture, _) => fixture.State.OpenPanel(GameplayHtmlPanel.Construction));

        [Test]
        public void ActionMap_BuildingSelection_EveryEnabledButtonActs()
            => SweepEnabledButtons("Selection",
                GameplayHtmlSnapshot.CreatePreview(Screen("Selection")),
                (fixture, _) => fixture.SelectWorldObject(
                    WorldInfoSelectionKind.Building, "castle"));

        [Test]
        public void ActionMap_RecruitmentSelection_EveryEnabledButtonActs()
            => SweepEnabledButtons("Recruitment",
                GameplayHtmlSnapshot.CreatePreview(Screen("Recruitment")),
                (fixture, _) => fixture.SelectWorldObject(
                    WorldInfoSelectionKind.Building, "barracks"));

        [Test]
        public void ActionMap_KingdomDashboard_EveryEnabledButtonActs()
            => SweepEnabledButtons("Dashboard",
                GameplayHtmlSnapshot.CreatePreview(Screen("Dashboard")),
                (fixture, _) => fixture.State.OpenPanel(GameplayHtmlPanel.Kingdom));

        [Test]
        public void ActionMap_PauseOverlay_EveryEnabledButtonActs()
            => SweepEnabledButtons("Pause",
                GameplayHtmlSnapshot.CreatePreview(Screen("Pause")),
                (fixture, _) => fixture.State.SetPaused(true));

        [Test]
        public void ActionMap_CargoPanel_EveryEnabledButtonActs()
            => SweepEnabledButtons("Cargo",
                GameplayHtmlSnapshot.CreatePreview(Screen("Cargo")),
                (fixture, _) =>
                {
                    fixture.SelectWorldObject(
                        WorldInfoSelectionKind.Unit, "caravan-wagon-preview");
                    fixture.CargoPanel.Capture("caravan-wagon-preview");
                });

        [Test]
        public void ActionMap_RoutePanel_EveryEnabledButtonActs()
            => SweepEnabledButtons("Route",
                GameplayHtmlSnapshot.CreatePreview(Screen("Route")),
                (fixture, _) =>
                {
                    fixture.SelectWorldObject(
                        WorldInfoSelectionKind.Unit, "caravan-wagon-preview");
                    fixture.CargoPanel.Capture("caravan-wagon-preview");
                });

        [Test]
        public void ActionMap_SupplyPanel_EveryEnabledButtonActs()
            => SweepEnabledButtons("Supply",
                GameplayHtmlSnapshot.CreatePreview(Screen("Construction")),
                (fixture, snapshot) =>
                {
                    fixture.State.OpenSupplyPanel(new Vector2Int(3, 4), "lumber-camp");
                    snapshot.Supply = fixture.SupplyPanel.Capture();
                    Assert.NotNull(snapshot.Supply,
                        "Supply capture must succeed so the panel renders real controls.");
                });

        [Test]
        public void ActionMap_NotificationsPanel_EveryEnabledButtonActs()
            => SweepEnabledButtons("Notifications",
                GameplayHtmlSnapshot.CreatePreview(Screen("Normal")),
                (fixture, _) =>
                {
                    fixture.State.AddNotification(
                        "Lumber camp finished.", "Info",
                        new Vector2Int(42, 27), "lumber-camp");
                    fixture.State.OpenPanel(GameplayHtmlPanel.Notifications);
                });

        /// <summary>P071: the blocked-action guidance popup — every rendered
        /// control (close, blocker focus, next/prev, per-option action buttons,
        /// resume-goal) must produce a routed action, a service call or a UI
        /// state change; dead popups would silently strand the player.</summary>
        [Test]
        public void ActionMap_GuidancePopup_EveryEnabledButtonActs()
            => SweepEnabledButtons("Guidance",
                GameplayHtmlSnapshot.CreatePreview(Screen("Construction")),
                (fixture, snapshot) =>
                {
                    var goal = new GuidanceGoal
                    {
                        Kind = GuidanceGoalKind.Placement,
                        BuildingId = "lumber-camp",
                        Position = new Vector2Int(3, 4),
                        PlacementCount = 1,
                    };
                    fixture.State.OpenGuidance(goal);
                    snapshot.Guidance = new GameplayGuidanceViewSnapshot
                    {
                        GoalKind = GuidanceGoalKind.Placement,
                        GoalLabel = "lumber-camp",
                        GoalBuildingId = "lumber-camp",
                        GoalPosition = new Vector2Int(3, 4),
                        PlacementCount = 1,
                        FocusIndex = 0,
                        TotalBlockers = 2,
                        PendingBlockers = 2,
                        Blockers = new[]
                        {
                            new GameplayGuidanceBlockerSnapshot(
                                GuidanceBlockerKind.Resource, "wood", string.Empty, "wood",
                                10f, 2f, 10f, false,
                                new[]
                                {
                                    new GameplayGuidanceOptionSnapshot(
                                        GuidanceOptionKind.BuildProducer,
                                        "sawmill", "wood", null, default, false),
                                    new GameplayGuidanceOptionSnapshot(
                                        GuidanceOptionKind.FocusProducer,
                                        "sawmill", "wood", "built",
                                        new Vector2Int(9, 9), true),
                                    new GameplayGuidanceOptionSnapshot(
                                        GuidanceOptionKind.OpenQueue,
                                        string.Empty, string.Empty, "reserved",
                                        default, false),
                                    new GameplayGuidanceOptionSnapshot(
                                        GuidanceOptionKind.Unobtainable,
                                        string.Empty, "res-x", "no producer",
                                        default, false),
                                }),
                            new GameplayGuidanceBlockerSnapshot(
                                GuidanceBlockerKind.Population, "Population",
                                "Housing is full", string.Empty,
                                3f, 0f, 0f, false,
                                new[]
                                {
                                    new GameplayGuidanceOptionSnapshot(
                                        GuidanceOptionKind.BuildHousing,
                                        "house", string.Empty, null, default, false),
                                }),
                        },
                    };
                });
    }
}
