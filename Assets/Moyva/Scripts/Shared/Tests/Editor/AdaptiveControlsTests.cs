using System;
using System.IO;
using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Moyva.Shared.Controls;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Kruty1918.Moyva.Tests.Controls
{
    public sealed class AdaptiveControlsTests
    {
        [Test]
        public void AutoRequiresSustainedMotionAfterDeliberateInput()
        {
            var context = new InputDeviceContext();

            context.RecordActivity(
                ControlProfile.Gamepad,
                true,
                1f,
                0f);

            context.RecordActivity(
                ControlProfile.KeyboardMouse,
                false,
                1.1f,
                0.02f);

            Assert.That(
                context.ActiveProfile,
                Is.EqualTo(ControlProfile.Gamepad));

            for (int i = 0; i < 8; i++)
            {
                context.RecordActivity(
                    ControlProfile.KeyboardMouse,
                    false,
                    2f + i * 0.02f,
                    0.02f);
            }

            Assert.That(
                context.ActiveProfile,
                Is.EqualTo(ControlProfile.KeyboardMouse));
        }

        [Test]
        public void ManualSelectionDoesNotFlickerAndAutoRetainsRecentInput()
        {
            var context = new InputDeviceContext();

            context.Configure(
                ControlProfile.KeyboardTouchpad,
                PointerInterpretation.Touchpad);

            context.RecordActivity(
                ControlProfile.Gamepad,
                true,
                1f,
                0f);

            Assert.That(
                context.ActiveProfile,
                Is.EqualTo(ControlProfile.KeyboardTouchpad));

            context.Configure(
                ControlProfile.Auto,
                PointerInterpretation.Touchpad);

            Assert.That(
                context.ActiveProfile,
                Is.EqualTo(ControlProfile.Gamepad));
        }

        [TestCase("Ctrl+<Keyboard>/p", "Ctrl+<Keyboard>/p")]
        [TestCase("alt+<mouse>/middleButton", "Alt+<Mouse>/middleButton")]
        [TestCase("<Gamepad>/leftStick/up", "<Gamepad>/leftStick/up")]
        [TestCase("<Touchpad>/spaceDrag", "<Touchpad>/spaceDrag")]
        [TestCase("<Touch>/pinchIn", "<Touch>/pinchIn")]
        public void BindingRoundTrips(string value, string expected)
        {
            Assert.That(
                PlayerControlBinding.TryParse(value, out var binding),
                Is.True);

            Assert.That(
                binding.CanonicalPath,
                Is.EqualTo(expected));
        }

        [TestCase("<Unknown>/fire")]
        [TestCase("Ctrl+Ctrl+<Keyboard>/w")]
        [TestCase("<Gamepad>/arbitraryMethod")]
        public void UnknownControlsAreRejected(string value)
        {
            Assert.That(
                PlayerControlBinding.TryParse(value, out _),
                Is.False);
        }

        [Test]
        public void PointerAliasesConflictButDistinctModifiersDoNot()
        {
            Assert.That(
                PlayerControlBinding.Conflicts(
                    "<Touchpad>/spaceDrag",
                    "<Keyboard>/space"),
                Is.True);

            Assert.That(
                PlayerControlBinding.Conflicts(
                    "<Touchpad>/scroll/up",
                    "<Mouse>/scroll/up"),
                Is.True);

            Assert.That(
                PlayerControlBinding.Conflicts(
                    "Alt+<Touchpad>/spaceDrag",
                    "<Keyboard>/space"),
                Is.False);
        }

        [Test]
        public void GamepadDeadzoneSuppressesDriftButRetainsAnalogMagnitude()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();

            try
            {
                PlayerControlBinding.TryParse(
                    "<Gamepad>/leftStick/up",
                    out var binding);

                InputSystem.QueueStateEvent(
                    gamepad,
                    new GamepadState
                    {
                        leftStick = new Vector2(0f, 0.1f)
                    });

                InputSystem.Update();

                Assert.That(
                    binding.ReadValue(0.2f),
                    Is.Zero);

                InputSystem.QueueStateEvent(
                    gamepad,
                    new GamepadState
                    {
                        leftStick = new Vector2(0f, 0.7f)
                    });

                InputSystem.Update();

                Assert.That(
                    binding.ReadValue(0.2f),
                    Is.InRange(0.1f, 0.99f));
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [Test]
        public void AdvertisedPhysicalControlsResolveWithoutDeviceSpecificLayouts()
        {
            var mouse = InputSystem.AddDevice<Mouse>();
            var gamepad = InputSystem.AddDevice<Gamepad>();

            try
            {
                foreach (var path in PlayerControlBinding.DevicePaths)
                {
                    if (!path.StartsWith("<Mouse>") &&
                        !path.StartsWith("<Gamepad>"))
                    {
                        continue;
                    }

                    using var controls = InputSystem.FindControls(path);

                    Assert.That(
                        controls.Count,
                        Is.GreaterThan(0),
                        path);
                }

                var context = new InputDeviceContext();

                Assert.That(
                    context.IsPresent(ControlProfile.Gamepad),
                    Is.True);

                Assert.That(
                    context.ActiveProfile,
                    Is.EqualTo(ControlProfile.KeyboardMouse),
                    "Presence must not select a profile.");
            }
            finally
            {
                InputSystem.RemoveDevice(mouse);
                InputSystem.RemoveDevice(gamepad);
            }
        }

        [Test]
        public void LegacyKeyboardBindingsSurviveMigrationAndProfileReset()
        {
            var scope = new TestScope();

            string file = Path.Combine(
                Application.persistentDataPath,
                scope.BuildScopedFileName("player_controls.dat"));

            try
            {
                var legacy = PlayerControlSettingsData.CreateDefault();

                legacy.Bindings.Remove(PlayerControlAction.PrimarySelect);
                legacy.Bindings.Remove(PlayerControlAction.SecondarySelect);

                legacy.Bindings[PlayerControlAction.MoveForward] =
                    "Ctrl+<Keyboard>/p";

                legacy.Bindings[PlayerControlAction.RotateRight] =
                    "<Keyboard>/space";

                using (var writer = new BinaryWriter(File.Create(file)))
                {
                    writer.Write(1);

                    writer.Write(1.4f);
                    writer.Write(1f);
                    writer.Write(1f);
                    writer.Write(1f);

                    writer.Write(legacy.Bindings.Count);

                    foreach (var pair in legacy.Bindings)
                    {
                        writer.Write((int)pair.Key);
                        writer.Write(pair.Value);
                    }
                }

                var service = CreateService(scope);

                foreach (var profile in new[]
                         {
                             ControlProfile.KeyboardMouse,
                             ControlProfile.KeyboardTouchpad
                         })
                {
                    Assert.That(
                        service.Settings
                            .Profile(profile)
                            .ToBindings()[PlayerControlAction.MoveForward],
                        Is.EqualTo("Ctrl+<Keyboard>/p"));
                }

                var laptop = service.Settings.Profile(
                    ControlProfile.KeyboardTouchpad);

                Assert.That(
                    PlayerControlBinding.Conflicts(
                        laptop.PanBinding,
                        "<Keyboard>/space"),
                    Is.False);

                Assert.That(
                    laptop.ToBindings()[PlayerControlAction.RotateRight],
                    Is.EqualTo("<Keyboard>/space"));

                service.ConfigureDevices(
                    ControlProfile.Auto,
                    PointerInterpretation.Touchpad);

                Assert.That(
                    service.TrySetProfileBinding(
                        ControlProfile.Gamepad,
                        PlayerControlAction.ZoomIn,
                        "<Gamepad>/buttonNorth",
                        out _),
                    Is.True);

                service.ResetProfile(ControlProfile.Gamepad);

                var reloaded = CreateService(scope);

                Assert.That(
                    reloaded.Settings.Devices.PointerMode,
                    Is.EqualTo(PointerInterpretation.Touchpad));

                Assert.That(
                    reloaded.Settings
                        .Profile(ControlProfile.KeyboardMouse)
                        .ToBindings()[PlayerControlAction.MoveForward],
                    Is.EqualTo("Ctrl+<Keyboard>/p"));

                Assert.That(
                    reloaded.Settings.MouseSensitivity,
                    Is.EqualTo(1.4f).Within(0.001f));
            }
            finally
            {
                foreach (var suffix in new[]
                         {
                             "",
                             ".tmp",
                             ".bak",
                             ".v1.bak"
                         })
                {
                    string path = file + suffix;

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }

        [Test]
        public void ProfileConflictDoesNotMutateSavedBindings()
        {
            var scope = new TestScope();

            string file = Path.Combine(
                Application.persistentDataPath,
                scope.BuildScopedFileName("player_controls.dat"));

            try
            {
                var service = CreateService(scope);

                var before = service.Settings
                    .Profile(ControlProfile.Gamepad)
                    .ToBindings()[PlayerControlAction.ZoomIn];

                Assert.That(
                    service.TrySetProfileBinding(
                        ControlProfile.Gamepad,
                        PlayerControlAction.ZoomIn,
                        "<Gamepad>/leftStick/up",
                        out var conflict),
                    Is.False);

                Assert.That(
                    conflict,
                    Is.EqualTo(PlayerControlAction.MoveForward));

                Assert.That(
                    service.Settings
                        .Profile(ControlProfile.Gamepad)
                        .ToBindings()[PlayerControlAction.ZoomIn],
                    Is.EqualTo(before));
            }
            finally
            {
                foreach (var suffix in new[]
                         {
                             "",
                             ".tmp",
                             ".bak"
                         })
                {
                    string path = file + suffix;

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }
        }

        private static IPlayerControlSettingsService CreateService(
            IClientInstanceScope scope)
        {
            var assembly = typeof(IPlayerControlSettingsService).Assembly;

            var type = assembly.GetType(
                "Kruty1918.Moyva.Shared.Controls.PlayerControlSettingsService");

            Assert.That(
                type,
                Is.Not.Null,
                "PlayerControlSettingsService type was not found.");

            var instance = Activator.CreateInstance(
                type,
                scope,
                new InputDeviceContext());

            Assert.That(
                instance,
                Is.Not.Null,
                "Failed to create PlayerControlSettingsService.");

            var service = instance as IPlayerControlSettingsService;

            Assert.That(
                service,
                Is.Not.Null,
                "PlayerControlSettingsService does not implement IPlayerControlSettingsService.");

            var initializeMethod = type.GetMethod("Initialize");

            Assert.That(
                initializeMethod,
                Is.Not.Null,
                "PlayerControlSettingsService.Initialize() was not found.");

            initializeMethod.Invoke(
                instance,
                null);

            return service;
        }

        private sealed class TestScope : IClientInstanceScope
        {
            public string ScopeId { get; } =
                "controls-test-" + Guid.NewGuid().ToString("N");

            public bool IsDefault => false;

            public string BuildScopedFileName(string fileName)
            {
                return ScopeId + "-" + fileName;
            }

            public string CreateDefaultPlayerName()
            {
                return ScopeId;
            }
        }
    }
}