using System;
using System.Collections.Generic;
using System.Reflection;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.UIActions.Runtime;
using Kruty1918.UIActions.API;
using Kruty1918.UIActions.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kruty1918.Moyva.Tests.UIActions
{
    /// <summary>
    /// U01 acceptance — one Escape press performs exactly one action of the
    /// top UI context (close modal → cancel local action → pause). Runs the
    /// production UiEscapeRouter over the real UiContextStack/UiActionRouter
    /// with registrations shaped like the gameplay scene.
    /// The GameplayPauseInputController fallback itself is internal to the
    /// GameMode assembly; this assembly has no reference to it, so the press
    /// driver below mirrors its documented contract. TMPro types are resolved
    /// by name for the same reason — see the U01 integration request.
    /// </summary>
    [TestFixture]
    internal sealed class U01AcceptanceTests
    {
        private static readonly Type TmpInputFieldType =
            Type.GetType("TMPro.TMP_InputField, Unity.TextMeshPro");
        private static readonly Type TmpDropdownType =
            Type.GetType("TMPro.TMP_Dropdown, Unity.TextMeshPro");

        private UiContextStack _contexts;
        private UiActionJournal _journal;
        private RecordingHandler _handler;
        private UiActionRouter _actions;
        private UiEscapeRouter _router;
        private bool _paused;
        private int _executedBefore;
        private readonly List<GameObject> _objects = new();

        [SetUp]
        public void SetUp()
        {
            _contexts = new UiContextStack();
            _journal = new UiActionJournal();
            _handler = new RecordingHandler(
                UiActionIds.Diagnostics.PanelClose,
                UiActionIds.Construction.CancelPlacement,
                UiActionIds.Pause.Open,
                UiActionIds.Pause.Close);
            _actions = new UiActionRouter(new List<IUiActionHandler> { _handler }, _journal);
            _router = new UiEscapeRouter(
                _contexts, _actions, _journal, MoyvaUiActionCatalog.CreateEscapeRoutingOptions());
            _paused = false;
        }

        [TearDown]
        public void TearDown()
        {
            // Reverse order: destroy a spawned dropdown list before its
            // TMP_Dropdown, so the component's OnDisable destroy-path sees a
            // dead reference and does not call edit-mode-illegal Destroy.
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i] != null)
                    UnityEngine.Object.DestroyImmediate(_objects[i]);
            }
            _objects.Clear();

            // DestroyImmediate is not guaranteed to run OnDisable in EditMode;
            // drop any stale entries so a destroyed EventSystem cannot leak
            // into the next test's EventSystem.current.
            (typeof(EventSystem)
                .GetField("m_EventSystems", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null) as System.Collections.IList)
                ?.Clear();
        }

        private sealed class RecordingHandler : IUiActionHandler
        {
            public readonly List<(UiActionId Action, string Context)> Executed = new();
            public Func<UiActionId, UiActionResult> Respond = _ => UiActionResult.Performed();
            public IReadOnlyCollection<UiActionId> ActionIds { get; }

            public RecordingHandler(params UiActionId[] actionIds) => ActionIds = actionIds;

            public UiActionResult Execute(in UiActionRequest request)
            {
                Executed.Add((request.ActionId, request.ContextId));
                return Respond(request.ActionId);
            }
        }

        // Mirrors the GameplayPauseInputController.Tick fallback contract:
        // when the router consumed the press nothing else runs, otherwise the
        // top-level pause toggle fires once.
        private void PressEscape()
        {
            _executedBefore = _handler.Executed.Count;
            if (_router.TryHandleEscape())
                return;

            _actions.Execute(
                _paused ? UiActionIds.Pause.Close : UiActionIds.Pause.Open,
                UiActionSource.Escape,
                "Gameplay");
        }

        private void AssertSingleExecution(UiActionId expected, string context)
        {
            int delta = _handler.Executed.Count - _executedBefore;
            Assert.AreEqual(1, delta,
                $"One Escape press must produce exactly one action, got {delta}: "
                + string.Join(", ", _handler.Executed));
            Assert.AreEqual(expected, _handler.Executed[_handler.Executed.Count - 1].Action);
            Assert.AreEqual(context, _handler.Executed[_handler.Executed.Count - 1].Context);
        }

        private void AssertJournalContains(UiActionId actionId)
        {
            var entries = _journal.GetRecent(_journal.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].ActionId == actionId)
                    return;
            }
            Assert.Fail($"Journal does not contain '{actionId}'.");
        }

        private GameObject Track(GameObject go)
        {
            _objects.Add(go);
            return go;
        }

        // EditMode never runs EventSystem.OnEnable (no ExecuteAlways), so
        // EventSystem.current stays null — activate it the way a live scene
        // would before exercising selection behaviour.
        private GameObject CreateEventSystem()
        {
            var eventSystemObject = Track(new GameObject("EventSystem", typeof(EventSystem)));
            var eventSystem = eventSystemObject.GetComponent<EventSystem>();
            typeof(EventSystem)
                .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(eventSystem, null);
            EventSystem.current = eventSystem;
            return eventSystemObject;
        }

        private static void SetInstanceField(object target, string name, object value)
        {
            FieldInfo field = target.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Field '{name}' missing on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        [Test]
        public void Escape_Sequence_ClosesOneLayerPerPress_ThenTogglesPause()
        {
            bool panelOpen = true;
            bool placing = true;
            bool constructing = true;

            // Production-shaped registrations: GameplayHtmlPresenter panel,
            // ConstructionInputService placement/mode, GameModeUiActionHandler
            // pause modal — modal > panel > mode, escape peels one per press.
            _contexts.Push(new UiContextRegistration(
                "ConstructionMode", UiContextLayer.Mode, 10,
                () => constructing, UiActionIds.Construction.CancelPlacement));
            _contexts.Push(new UiContextRegistration(
                "BuildingPlacement", UiContextLayer.Mode, 30,
                () => placing, UiActionIds.Construction.CancelPlacement));
            _contexts.Push(new UiContextRegistration(
                "GameplayHTML/Panel", UiContextLayer.Panel, 300,
                () => panelOpen, UiActionIds.Diagnostics.PanelClose));
            _contexts.Push(new UiContextRegistration(
                "PauseModal", UiContextLayer.Modal, 100,
                () => _paused, UiActionIds.Pause.Close));

            _handler.Respond = id =>
            {
                if (id == UiActionIds.Diagnostics.PanelClose)
                {
                    panelOpen = false;
                    return UiActionResult.Performed();
                }
                if (id == UiActionIds.Construction.CancelPlacement)
                {
                    if (placing) { placing = false; return UiActionResult.Performed(); }
                    if (constructing) { constructing = false; return UiActionResult.Performed(); }
                    return UiActionResult.Rejected(UiActionReason.WrongContext);
                }
                if (id == UiActionIds.Pause.Open)
                {
                    if (_paused)
                        return UiActionResult.Ignored(UiActionReason.AlreadyOpen);
                    _paused = true;
                    return UiActionResult.Performed();
                }
                if (id == UiActionIds.Pause.Close)
                {
                    if (!_paused)
                        return UiActionResult.Ignored(UiActionReason.AlreadyClosed);
                    _paused = false;
                    return UiActionResult.Performed();
                }
                return UiActionResult.Ignored(UiActionReason.ActionUnavailable);
            };

            PressEscape();
            AssertSingleExecution(UiActionIds.Diagnostics.PanelClose, "GameplayHTML/Panel");
            Assert.IsTrue(constructing && placing && !_paused,
                "The press must close the top panel only.");

            PressEscape();
            AssertSingleExecution(UiActionIds.Construction.CancelPlacement, "BuildingPlacement");
            Assert.IsTrue(constructing, "Pending placement cancels before the mode exits.");

            PressEscape();
            AssertSingleExecution(UiActionIds.Construction.CancelPlacement, "ConstructionMode");
            Assert.IsFalse(constructing);

            PressEscape();
            AssertSingleExecution(UiActionIds.Pause.Open, "Gameplay");
            Assert.IsTrue(_paused, "Bare gameplay Escape falls back to pause.");

            PressEscape();
            AssertSingleExecution(UiActionIds.Pause.Close, "PauseModal");
            Assert.IsFalse(_paused,
                "Pause modal claims Escape and resumes — never pause+resume per press.");

            PressEscape();
            AssertSingleExecution(UiActionIds.Pause.Open, "Gameplay");
            Assert.IsTrue(_paused);
        }

        [Test]
        public void Escape_InitialCastleVeto_ConsumesPress_NoLayerCloses()
        {
            bool castleRequired = true;
            bool constructing = true;

            _contexts.Push(new UiContextRegistration(
                "ConstructionMode", UiContextLayer.Mode, 10,
                () => constructing, UiActionIds.Construction.CancelPlacement));
            _contexts.Push(new UiContextRegistration(
                "InitialCastlePlacement", UiContextLayer.Modal, 250,
                () => castleRequired, UiActionIds.Construction.CancelPlacement,
                blocksLowerHotkeys: true,
                allowedHotkeyActionIds: new[] { UiActionIds.Construction.ConfirmPlacement }));

            _handler.Respond = id =>
            {
                if (id == UiActionIds.Construction.CancelPlacement && castleRequired)
                    return UiActionResult.Rejected(
                        UiActionReason.ActionUnavailable, true,
                        "Place your first castle before leaving construction mode.");
                return UiActionResult.Performed();
            };

            Assert.IsTrue(_router.TryHandleEscape());
            Assert.AreEqual(1, _handler.Executed.Count);
            Assert.AreEqual("InitialCastlePlacement", _handler.Executed[0].Context,
                "The modal veto must answer before the mode's cancel.");
            Assert.IsTrue(constructing, "A vetoed modal must not peel the layer below.");

            castleRequired = false;
            Assert.IsTrue(_router.TryHandleEscape());
            Assert.AreEqual(2, _handler.Executed.Count);
            Assert.AreEqual("ConstructionMode", _handler.Executed[1].Context,
                "Once the modal deactivates, the next press reaches the mode.");
        }

        [Test]
        public void Escape_FocusedTextInput_ReleasesFocus_WithoutContextAction()
        {
            Assert.NotNull(TmpInputFieldType,
                "Unity.TextMeshPro must be resolvable — the router checks TMP_InputField.");
            CreateEventSystem();
            var fieldGo = Track(new GameObject("SeedInput", typeof(RectTransform)));
            var field = fieldGo.AddComponent(TmpInputFieldType);
            SetInstanceField(field, "m_AllowInput", true); // isFocused
            EventSystem.current.SetSelectedGameObject(fieldGo);

            _contexts.Push(new UiContextRegistration(
                "GameplayHTML/Panel", UiContextLayer.Panel, 300,
                () => true, UiActionIds.Diagnostics.PanelClose));

            Assert.IsTrue(_router.TryHandleEscape());
            Assert.IsNull(EventSystem.current.currentSelectedGameObject,
                "Escape releases text focus instead of firing a context action.");
            Assert.AreEqual(0, _handler.Executed.Count);
            AssertJournalContains(UiActionIds.Diagnostics.InputEscape);
            AssertJournalContains(UiActionIds.Diagnostics.TextUnfocus);
        }

        [Test]
        public void Escape_BlurredTextInput_DoesNotConsume()
        {
            Assert.NotNull(TmpInputFieldType,
                "Unity.TextMeshPro must be resolvable — the router checks TMP_InputField.");
            CreateEventSystem();
            var fieldGo = Track(new GameObject("SeedInput", typeof(RectTransform)));
            fieldGo.AddComponent(TmpInputFieldType); // selected but not focused
            EventSystem.current.SetSelectedGameObject(fieldGo);

            _contexts.Push(new UiContextRegistration(
                "GameplayHTML/Panel", UiContextLayer.Panel, 300,
                () => true, UiActionIds.Diagnostics.PanelClose));

            Assert.IsTrue(_router.TryHandleEscape());
            Assert.AreEqual(1, _handler.Executed.Count);
            Assert.AreEqual(UiActionIds.Diagnostics.PanelClose, _handler.Executed[0].Action,
                "Inactive UI must not catch Escape — the context below runs.");
        }

        [Test]
        public void Escape_ExpandedDropdown_ClaimedByCancel_NoContextActionOrUnselect()
        {
            Assert.NotNull(TmpDropdownType,
                "Unity.TextMeshPro must be resolvable — the router checks TMP_Dropdown.");
            CreateEventSystem();
            var dropdownGo = Track(new GameObject("Dropdown", typeof(RectTransform)));
            var dropdown = dropdownGo.AddComponent(TmpDropdownType);
            var itemGo = Track(new GameObject("Item", typeof(RectTransform)));
            itemGo.transform.SetParent(dropdownGo.transform, false);
            // m_Dropdown non-null == IsExpanded: the input module's Cancel will
            // collapse the list on this same press, so the router must yield.
            var listGo = Track(new GameObject("Dropdown List", typeof(RectTransform)));
            SetInstanceField(dropdown, "m_Dropdown", listGo);
            EventSystem.current.SetSelectedGameObject(itemGo);

            _contexts.Push(new UiContextRegistration(
                "GameplayHTML/Panel", UiContextLayer.Panel, 300,
                () => true, UiActionIds.Diagnostics.PanelClose));

            Assert.IsTrue(_router.TryHandleEscape(),
                "An expanded dropdown owns the press via the module's Cancel.");
            Assert.AreEqual(itemGo, EventSystem.current.currentSelectedGameObject,
                "Yielding to Cancel must not disturb the EventSystem selection.");
            Assert.AreEqual(0, _handler.Executed.Count,
                "Dropdown collapse and a context action on one press = two layers closing.");
        }

        [Test]
        public void Escape_ClosedDropdown_FallsThroughToContext()
        {
            Assert.NotNull(TmpDropdownType,
                "Unity.TextMeshPro must be resolvable — the router checks TMP_Dropdown.");
            CreateEventSystem();
            var dropdownGo = Track(new GameObject("Dropdown", typeof(RectTransform)));
            dropdownGo.AddComponent(TmpDropdownType); // not expanded
            EventSystem.current.SetSelectedGameObject(dropdownGo);

            _contexts.Push(new UiContextRegistration(
                "GameplayHTML/Panel", UiContextLayer.Panel, 300,
                () => true, UiActionIds.Diagnostics.PanelClose));

            Assert.IsTrue(_router.TryHandleEscape());
            Assert.AreEqual(1, _handler.Executed.Count,
                "A closed dropdown must not swallow Escape — the context runs.");
            Assert.AreEqual(UiActionIds.Diagnostics.PanelClose, _handler.Executed[0].Action);
        }

        [Test]
        public void Escape_ContextDeclinesWrongContext_FallsThroughToPauseFallback()
        {
            _contexts.Push(new UiContextRegistration(
                "GameplayHTML/Panel", UiContextLayer.Panel, 300,
                () => true, UiActionIds.Diagnostics.PanelClose));

            _handler.Respond = id =>
            {
                if (id == UiActionIds.Diagnostics.PanelClose)
                    return UiActionResult.Rejected(UiActionReason.WrongContext);
                if (id == UiActionIds.Pause.Open)
                {
                    _paused = true;
                    return UiActionResult.Performed();
                }
                return UiActionResult.Ignored(UiActionReason.ActionUnavailable);
            };

            PressEscape();
            Assert.AreEqual(2, _handler.Executed.Count,
                "A WrongContext decline falls through to the caller's pause fallback.");
            Assert.AreEqual(UiActionIds.Pause.Open, _handler.Executed[1].Action);
            Assert.IsTrue(_paused);
        }

        [Test]
        public void Escape_AlwaysJournalsPress_EvenWhenNothingConsumes()
        {
            Assert.IsFalse(_router.TryHandleEscape(),
                "No contexts → the caller's fallback owns the press.");
            AssertJournalContains(UiActionIds.Diagnostics.InputEscape);
        }
    }
}
