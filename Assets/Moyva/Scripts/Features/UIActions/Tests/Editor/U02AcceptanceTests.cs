using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kruty1918.UIActions.API;
using Kruty1918.UIActions.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kruty1918.Moyva.Tests.UIActions
{
    /// <summary>
    /// U02 acceptance — while a text field owns the keyboard no gameplay
    /// hotkey may fire, focus detection matches the input policy's ancestor
    /// reach (HTML inputs render as TMP_InputField with child visuals), and
    /// remapped bindings never double-fire on one chord. InputSystem and
    /// TMPro types are resolved by name: this assembly intentionally does not
    /// reference them — see the U02 integration request.
    /// </summary>
    [TestFixture]
    internal sealed class U02AcceptanceTests
    {
        private static readonly Type KeyType =
            Type.GetType("UnityEngine.InputSystem.Key, Unity.InputSystem");
        private static readonly Type KeyboardType =
            Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
        private static readonly Type KeyboardStateType =
            Type.GetType("UnityEngine.InputSystem.LowLevel.KeyboardState, Unity.InputSystem");
        private static readonly Type InputSystemType =
            Type.GetType("UnityEngine.InputSystem.InputSystem, Unity.InputSystem");
        private static readonly Type TmpInputFieldType =
            Type.GetType("TMPro.TMP_InputField, Unity.TextMeshPro");

        private static readonly UiActionId ActionA = new("test.hotkey-a");
        private static readonly UiActionId ActionB = new("test.hotkey-b");
        private static readonly UiActionId ActionC = new("test.hotkey-c");

        private sealed class FakeRouter : IUiActionRouter
        {
            public readonly List<UiActionId> Executed = new();
            public UiActionResult Execute(in UiActionRequest request)
                => Execute(request.ActionId, request.Source, request.ContextId, request.TargetId);
            public UiActionResult Execute(UiActionId actionId,
                UiActionSource source = UiActionSource.Programmatic,
                string contextId = null, string targetId = null)
            {
                Executed.Add(actionId);
                return UiActionResult.Performed();
            }
        }

        private UiContextStack _contexts;
        private FakeRouter _actions;
        private object _keyboard;
        private readonly List<GameObject> _objects = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i] != null)
                    UnityEngine.Object.DestroyImmediate(_objects[i]);
            }
            _objects.Clear();

            // DestroyImmediate may skip OnDisable in EditMode; clear the
            // static registry so no dead EventSystem leaks into other tests.
            (typeof(EventSystem)
                .GetField("m_EventSystems", BindingFlags.Static | BindingFlags.NonPublic)
                ?.GetValue(null) as System.Collections.IList)
                ?.Clear();

            if (_keyboard != null)
            {
                InputSystemType?.GetMethod("RemoveDevice", new[] { Type.GetType(
                    "UnityEngine.InputSystem.InputDevice, Unity.InputSystem") })
                    ?.Invoke(null, new[] { _keyboard });
                _keyboard = null;
            }
        }

        private GameObject Track(GameObject go)
        {
            _objects.Add(go);
            return go;
        }

        private static object ToKey(string name)
            => name == null
                ? Enum.ToObject(KeyType, 0) // Key.None
                : Enum.Parse(KeyType, name);

        private static UiHotkeyBinding MakeBinding(
            UiActionId actionId, string primary, string secondary = null,
            bool ctrl = false, bool shift = false, bool alt = false,
            UiHotkeyTriggerMode mode = UiHotkeyTriggerMode.TriggeredOnce,
            IReadOnlyCollection<string> contexts = null)
        {
            ConstructorInfo ctor = typeof(UiHotkeyBinding).GetConstructors().Single();
            return (UiHotkeyBinding)ctor.Invoke(new object[]
            {
                actionId, ToKey(primary), ToKey(secondary),
                ctrl, shift, alt, mode, contexts
            });
        }

        private UiHotkeyService CreateService(params UiHotkeyBinding[] defaults)
        {
            _contexts = new UiContextStack();
            _actions = new FakeRouter();
            return new UiHotkeyService(_actions, _contexts, defaultBindings: defaults);
        }

        // --- InputSystem plumbing via reflection (assembly has no ref) ---

        private object AddKeyboard()
        {
            Assert.NotNull(InputSystemType, "Unity.InputSystem must be resolvable for key injection.");
            MethodInfo addDevice = InputSystemType
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(m => m.Name == "AddDevice" && m.IsGenericMethodDefinition);
            object device = addDevice.MakeGenericMethod(KeyboardType)
                .Invoke(null, new object[] { null });
            device.GetType().GetMethod("MakeCurrent").Invoke(device, null);
            _keyboard = device;
            return device;
        }

        private void InjectKeys(params string[] keyNames)
        {
            Array keys = Array.CreateInstance(KeyType, keyNames.Length);
            for (int i = 0; i < keyNames.Length; i++)
                keys.SetValue(Enum.Parse(KeyType, keyNames[i]), i);

            object state = Activator.CreateInstance(
                KeyboardStateType, new object[] { keys });
            MethodInfo queue = InputSystemType
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Single(m => m.Name == "QueueStateEvent"
                    && m.IsGenericMethodDefinition
                    && m.GetParameters().Length == 3);
            queue.MakeGenericMethod(KeyboardStateType)
                .Invoke(null, new object[] { _keyboard, state, -1.0 });
            InputSystemType.GetMethod("Update", Type.EmptyTypes).Invoke(null, null);
        }

        // Guard against a silently-dead injection path turning "suppressed"
        // assertions into false positives. Batch EditMode never reports
        // wasPressedThisFrame for injected state — isPressed is the live check.
        private void AssertKeyPressed(string keyPropertyName)
        {
            object current = KeyboardType.GetProperty("current").GetValue(null);
            Assert.NotNull(current, "Keyboard.current is null — injection is not live.");
            object keyControl = KeyboardType.GetProperty(keyPropertyName).GetValue(current);
            bool pressed = (bool)keyControl.GetType()
                .GetProperty("isPressed").GetValue(keyControl);
            Assert.IsTrue(pressed, $"Injected '{keyPropertyName}' press never landed.");
        }

        private void NextFrame()
            => InputSystemType.GetMethod("Update", Type.EmptyTypes).Invoke(null, null);

        private static void SetInstanceField(object target, string name, object value)
        {
            FieldInfo field = target.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field, $"Field '{name}' missing on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private GameObject CreateEventSystem()
        {
            var go = Track(new GameObject("EventSystem", typeof(EventSystem)));
            typeof(EventSystem)
                .GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(go.GetComponent<EventSystem>(), null);
            EventSystem.current = go.GetComponent<EventSystem>();
            return go;
        }

        private Component CreateFocusedField()
        {
            Assert.NotNull(TmpInputFieldType,
                "Unity.TextMeshPro must be resolvable — the service checks TMP_InputField.");
            var fieldGo = Track(new GameObject("Input", typeof(RectTransform)));
            var field = fieldGo.AddComponent(TmpInputFieldType);
            SetInstanceField(field, "m_AllowInput", true); // isFocused
            return field;
        }

        // --- Conflict detection / remapping ---

        [Test]
        public void DetectConflicts_RemappedSecondaryOntoOtherPrimary_IsCaught()
        {
            // Remapping is the only user-facing way to create this overlap;
            // a miss lets one physical press fire two actions.
            var service = CreateService(
                MakeBinding(ActionA, "B", "Enter"),
                MakeBinding(ActionB, "Enter"));

            var conflicts = service.DetectConflicts();

            Assert.AreEqual(1, conflicts.Count,
                "A.Secondary == B.Primary is the same physical chord.");
        }

        [Test]
        public void DetectConflicts_SecondaryVsSecondary_IsCaught()
        {
            var service = CreateService(
                MakeBinding(ActionA, "B", "Enter"),
                MakeBinding(ActionB, "R", "Enter"));

            Assert.AreEqual(1, service.DetectConflicts().Count);
        }

        [Test]
        public void DetectConflicts_DifferentModifiers_AreDistinctChords()
        {
            var service = CreateService(
                MakeBinding(ActionA, "B"),
                MakeBinding(ActionB, "B", ctrl: true));

            Assert.AreEqual(0, service.DetectConflicts().Count,
                "Ctrl+B and B must not conflict.");
        }

        [Test]
        public void DetectConflicts_SameChordDisjointContexts_NoConflict()
        {
            var service = CreateService(
                MakeBinding(ActionA, "B", contexts: new[] { "ConstructionMode" }),
                MakeBinding(ActionB, "B", contexts: new[] { "DeploymentMode" }));

            Assert.AreEqual(0, service.DetectConflicts().Count,
                "Same chord in disjoint contexts is a legal remap.");
        }

        [Test]
        public void DetectConflicts_UnboundSlots_NeverConflict()
        {
            var service = CreateService(
                MakeBinding(ActionA, null),
                MakeBinding(ActionB, null),
                MakeBinding(ActionC, "B", "B"));

            Assert.AreEqual(0, service.DetectConflicts().Count,
                "Two unbound actions and a duplicated own-slot must not be conflicts.");
        }

        [Test]
        public void SetBinding_ConflictingRemap_IsRejectedAndRolledBack()
        {
            var original = MakeBinding(ActionA, "B");
            var service = CreateService(original, MakeBinding(ActionB, "Enter"));

            bool accepted = service.SetBinding(MakeBinding(ActionA, "B", "Enter"));

            Assert.IsFalse(accepted, "A remap onto an occupied chord must fail.");
            Assert.AreEqual(2, service.Bindings.Count);
            Assert.AreEqual(original.PrimaryKey, service.Bindings[0].PrimaryKey);
            Assert.AreEqual(0, service.DetectConflicts().Count,
                "A rejected remap must leave no conflict behind.");
        }

        [Test]
        public void SetBinding_FreeChord_AcceptsAndUpdatesLabel()
        {
            var service = CreateService(MakeBinding(ActionA, "B"));

            Assert.IsTrue(service.SetBinding(MakeBinding(ActionA, "K", ctrl: true)));
            Assert.AreEqual("Ctrl+K", service.GetBindingLabel(ActionA),
                "The label must describe the remapped chord.");
        }

        // --- Typing gate through the real Tick + injected keyboard ---
        // Batch EditMode cannot produce wasPressedThisFrame, so TriggeredOnce
        // bindings can never fire here regardless of the gate — these tests
        // use Repeatable (held + latch) so a broken gate would really execute.

        [Test]
        public void Tick_WhileTypingInFocusedField_NoHotkeyFires()
        {
            AddKeyboard();
            CreateEventSystem();
            var field = CreateFocusedField();
            EventSystem.current.SetSelectedGameObject(field.gameObject);

            var service = CreateService(
                MakeBinding(ActionA, "B", mode: UiHotkeyTriggerMode.Repeatable),
                // even a remapped Escape chord
                MakeBinding(ActionB, "Escape", mode: UiHotkeyTriggerMode.Repeatable));

            InjectKeys("B");
            AssertKeyPressed("bKey");
            service.Tick();
            InjectKeys("Escape");
            AssertKeyPressed("escapeKey");
            service.Tick();

            Assert.AreEqual(0, _actions.Executed.Count,
                "While a text field owns the keyboard, no binding may fire — "
                + "Escape unfocus is owned by UiEscapeRouter, not hotkeys.");
        }

        [Test]
        public void Tick_HotkeyOnDescendantOfFocusedField_StillSuppressed()
        {
            // Selection may sit on a child visual of the field; the input
            // policy already treats that as text input — hotkeys must too.
            AddKeyboard();
            CreateEventSystem();
            var field = CreateFocusedField();
            var child = Track(new GameObject("Caret", typeof(RectTransform)));
            child.transform.SetParent(field.gameObject.transform, false);
            EventSystem.current.SetSelectedGameObject(child);

            var service = CreateService(
                MakeBinding(ActionA, "B", mode: UiHotkeyTriggerMode.Repeatable));

            InjectKeys("B");
            AssertKeyPressed("bKey");
            service.Tick();

            Assert.AreEqual(0, _actions.Executed.Count,
                "Selection inside a focused field must still suppress hotkeys.");
        }

        [Test]
        public void Tick_AfterBlur_HotkeyFiresAgain()
        {
            AddKeyboard();
            CreateEventSystem();
            var field = CreateFocusedField();
            EventSystem.current.SetSelectedGameObject(field.gameObject);
            var service = CreateService(
                MakeBinding(ActionA, "B", mode: UiHotkeyTriggerMode.Repeatable));

            InjectKeys("B");
            AssertKeyPressed("bKey");
            service.Tick();
            Assert.AreEqual(0, _actions.Executed.Count);

            // Blur: EventSystem deselect runs TMP's OnDeselect → unfocused.
            EventSystem.current.SetSelectedGameObject(null);
            SetInstanceField(field, "m_AllowInput", false);

            InjectKeys("B");
            service.Tick();

            Assert.AreEqual(1, _actions.Executed.Count,
                "After blur the same press must reach the hotkey action.");
            Assert.AreEqual(ActionA, _actions.Executed[0]);
        }

        [Test]
        public void Tick_HeldLatchReleasedWhileTyping_RetriggersAfterBlur()
        {
            AddKeyboard();
            var service = CreateService(
                MakeBinding(ActionA, "B", mode: UiHotkeyTriggerMode.Repeatable));

            InjectKeys("B");
            AssertKeyPressed("bKey");
            service.Tick();
            Assert.AreEqual(1, _actions.Executed.Count);

            NextFrame(); // still held
            service.Tick();
            Assert.AreEqual(1, _actions.Executed.Count,
                "A repeatable binding fires once per hold.");

            // Focus a field mid-hold, then release the key while typing.
            CreateEventSystem();
            var field = CreateFocusedField();
            EventSystem.current.SetSelectedGameObject(field.gameObject);
            InjectKeys(); // release all
            service.Tick();
            Assert.AreEqual(1, _actions.Executed.Count, "Typing suppresses.");

            EventSystem.current.SetSelectedGameObject(null);
            SetInstanceField(field, "m_AllowInput", false);
            InjectKeys("B");
            service.Tick();

            Assert.AreEqual(2, _actions.Executed.Count,
                "A release consumed by typing must not wedge the latch — "
                + "the next press after blur must fire.");
        }
    }
}
