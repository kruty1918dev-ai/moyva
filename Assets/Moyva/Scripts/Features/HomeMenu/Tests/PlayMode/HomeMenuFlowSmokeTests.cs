using System;
using System.Collections;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.HomeMenu.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Kruty1918.Moyva.Tests.HomeMenu.PlayMode
{
    /// <summary>
    /// End-to-end smoke: реальні flows меню, запуск gameplay-сцени, Escape-шари,
    /// дедуплікація швидких кліків та multiplayer host flow (LAN, без хмари).
    /// </summary>
    public sealed class HomeMenuFlowSmokeTests : HomeMenuSmokeFixture
    {
        private const string GameplaySceneName = "Gamplay_Scene";

        [UnityTest]
        public IEnumerator SoloFlowLaunchesGameplayScene()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);
            yield return Capture("flow-main", 0.2f);

            Bridge.Play();
            yield return WaitRealtime(0.5f);
            yield return Capture("flow-playmode", 0f);
            AssertCurrentRoute("PlayModePanel");

            Bridge.Solo();
            yield return WaitRealtime(0.5f);
            AssertCurrentRoute("WorldSetupPanel");
            yield return Capture("flow-worldsetup", 0f);

            View.SetWorldName("Smoke Realm");
            View.RandomizeSeed();
            yield return WaitRealtime(0.2f);
            Assert.IsTrue(View.CreateWorldButton == null || View.CreateWorldButton.interactable,
                "WorldSetup start must be enabled once a name and seed are set.");

            Bridge.CreateWorld();
            yield return WaitUntil(
                () => SceneManager.GetActiveScene().name == GameplaySceneName,
                TimeSpan.FromSeconds(60),
                "gameplay scene load");

            // Let the gameplay scene boot a few frames.
            yield return WaitRealtime(2f);

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        [UnityTest]
        public IEnumerator BotFlowLaunchesGameplayScene()
        {
            yield return LoadMenu();

            Bridge.Play();
            yield return WaitRealtime(0.5f);
            Bridge.PlayVsBot();
            yield return WaitRealtime(0.5f);
            AssertCurrentRoute("WorldSetupPanel");

            View.SetWorldName("Smoke Bot Realm");
            View.RandomizeSeed();
            yield return WaitRealtime(0.2f);

            Bridge.CreateWorld();
            yield return WaitUntil(
                () => SceneManager.GetActiveScene().name == GameplaySceneName,
                TimeSpan.FromSeconds(60),
                "gameplay scene load (vs bot)");

            yield return WaitRealtime(2f);

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        [UnityTest]
        public IEnumerator MultiplayerLanHostCreatesLobby()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);

            Bridge.Multiplayer();
            yield return WaitRealtime(0.5f);
            AssertCurrentRoute("SelectMultiplayerType");

            Bridge.CreateLan();
            yield return WaitRealtime(0.8f);
            AssertCurrentRoute("CreateRoomPanel");
            yield return Capture("flow-createroom-lan", 0f);

            Bridge.SetRoomName("Smoke LAN Lobby");
            Bridge.CreateRoom();
            yield return WaitRealtime(0.6f);
            AssertCurrentRoute("WorldSetupPanel");

            Bridge.CreateWorld();
            // LAN lobby creation is local; may take a few seconds for transport bind.
            yield return WaitUntil(
                () => State.CurrentRoute == "LobbyPanel" || View.InfoVisible,
                TimeSpan.FromSeconds(45),
                "LAN lobby creation or a graceful info error");

            if (View.InfoVisible)
            {
                TestContext.Out.WriteLine("[smoke] LAN lobby creation failed gracefully with info panel — acceptable offline behaviour.");
                yield return Capture("flow-lobby-create-failed", 0.3f);
                Bridge.AcknowledgeInfo();
            }
            else
            {
                yield return WaitRealtime(0.6f);
                AssertCurrentRoute("LobbyPanel");
                Assert.That(View.LobbyStatus.IsHost, Is.True, "Local player must be host of a freshly created LAN lobby.");
                yield return Capture("flow-lobby-lan", 0f);

                // Leaving the lobby must restore a usable menu.
                Bridge.LeaveLobby();
                yield return WaitRealtime(0.6f);
                if (View.ConfirmationVisible)
                {
                    Bridge.Confirm();
                    yield return WaitRealtime(0.5f);
                }
            }

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        [UnityTest]
        public IEnumerator EscapeRoutesThroughModalLayersThenNavigation()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);

            // Navigation level: open a panel, Escape closes it.
            yield return OpenRoute("SettingsPanel", 0.5f);
            PressEscape();
            yield return WaitRealtime(0.5f);
            Assert.That(string.IsNullOrEmpty(Nav.CurrentMenu) || Nav.CurrentMenu != "SettingsPanel",
                Is.True, "Escape must close the current panel.");

            // Modal level: info modal swallows the first Escape, navigation stays put.
            yield return OpenRoute("PlayModePanel", 0.5f);
            View.Show(new InfoMessage("Info", "Modal layer check"));
            yield return WaitRealtime(0.3f);
            PressEscape();
            yield return WaitRealtime(0.4f);
            Assert.IsFalse(View.InfoVisible, "Escape must dismiss the info modal.");
            AssertCurrentRoute("PlayModePanel");

            // Confirmation level.
            View.Show(new ConfirmationRequest { LabelText = "Confirm", MessageText = "Layer check" });
            yield return WaitRealtime(0.3f);
            PressEscape();
            yield return WaitRealtime(0.4f);
            Assert.IsFalse(View.ConfirmationVisible, "Escape must cancel the confirmation modal.");
            AssertCurrentRoute("PlayModePanel");

            // Password modal level.
            View.Show("Private Room", string.Empty);
            yield return WaitRealtime(0.3f);
            PressEscape();
            yield return WaitRealtime(0.4f);
            Assert.IsFalse(View.PasswordVisible, "Escape must cancel the password modal.");
            AssertCurrentRoute("PlayModePanel");

            // Busy overlay swallows Escape entirely — navigation must not move.
            var overlay = View.LoadOverlay(10f, 100f, "%");
            overlay?.SetStatus("Joining the lobby...");
            yield return WaitRealtime(0.3f);
            PressEscape();
            yield return WaitRealtime(0.4f);
            AssertCurrentRoute("PlayModePanel");
            Assert.IsTrue(View.OverlayVisible, "Overlay must stay visible after Escape (busy op is non-cancellable).");
            View.StopOverlay(true);
            yield return WaitRealtime(0.3f);
            PressEscape();
            yield return WaitRealtime(0.5f);
            Assert.That(Nav.CurrentMenu, Is.Not.EqualTo("PlayModePanel"),
                "Escape after overlay close must navigate back.");

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        [UnityTest]
        public IEnumerator RapidRefreshAndNavigationDoNotDuplicateOrBreak()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);

            yield return OpenRoute("JoinRoomPanel", 0.5f);

            // Hammer refresh — must not throw or corrupt state.
            for (var i = 0; i < 6; i++)
                Bridge.RefreshRooms();
            yield return WaitRealtime(0.8f);
            yield return Capture("rapid-refresh", 0f);

            // Hammer navigation between two panels quickly.
            for (var i = 0; i < 4; i++)
            {
                Nav.Open("SettingsPanel");
                Nav.Open("JoinRoomPanel");
            }
            yield return WaitRealtime(0.6f);
            AssertCurrentRoute("JoinRoomPanel");

            // Rapid Escape spam — must settle back to main, not crash.
            for (var i = 0; i < 3; i++)
            {
                PressEscape();
                yield return WaitRealtime(0.12f);
            }
            yield return WaitRealtime(0.5f);

            ReportLogSummary();
            AssertNoFatalLogs();
            Assert.That(UnexpectedErrorCount(), Is.EqualTo(0), "Rapid input produced unexpected console errors.");
        }

        [UnityTest]
        public IEnumerator ControlsCaptureEscapeCancelsCaptureNotNavigation()
        {
            yield return LoadMenu();
            yield return SetViewport(1366, 768);

            yield return OpenRoute("SettingsPanel", 0.5f);
            Debug.Log("[smoke-step] controls: select Controls section");
            State.SetSettingsSection(HomeMenuSettingsSection.Controls);
            yield return WaitRealtime(0.45f);
            LogOverflowingElements();
            yield return Capture("controls-section", 0f);

            // Enter capture mode on the first binding card. Assert synchronously:
            // in batchmode Application.isFocused is false, so the editor's Tick()
            // auto-cancels capture on the next frame — the same way a real app
            // cancels capture when the window loses focus.
            Debug.Log("[smoke-step] controls: SelectAction + StartCapture");
            View.Controls.SelectAction(0, true);
            View.Controls.StartCapture();
            Assert.IsTrue(View.Controls.IsCapturing, "Controls editor must enter capture mode.");

            // Escape must cancel capture, not navigate. HandleEscape checks
            // IsCapturing first, so call it before any frame elapses.
            Debug.Log("[smoke-step] controls: press Escape");
            Bridge.HandleEscape();
            Assert.IsFalse(View.Controls.IsCapturing, "Escape during capture must cancel capture.");
            AssertCurrentRoute("SettingsPanel");
            yield return WaitRealtime(0.3f);
            AssertCurrentRoute("SettingsPanel");

            ReportLogSummary();
            AssertNoFatalLogs();
        }

        private void PressEscape()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null)
            {
                TestContext.Out.WriteLine("[smoke] No keyboard device — routing via bridge.HandleEscape() directly.");
                Bridge.HandleEscape();
                return;
            }

            UnityEngine.InputSystem.InputSystem.QueueDeltaStateEvent(keyboard.escapeKey, 1f);
            UnityEngine.InputSystem.InputSystem.QueueDeltaStateEvent(keyboard.escapeKey, 0f);
        }

        private void AssertCurrentRoute(string expected)
        {
            Assert.That(State.CurrentRoute, Is.EqualTo(expected),
                $"Expected route '{expected}' but current route is '{State.CurrentRoute}' (nav '{Nav.CurrentMenu}').");
        }
    }
}
