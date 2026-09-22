using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.HomeMenu.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;

namespace Kruty1918.Moyva.Tests.Smoke.PlayMode
{
    /// <summary>
    /// Supported runtime smoke paths (AGENTS.md): a direct Gameplay scene load
    /// and the real menu -> Gameplay startup pipeline. Both assert the scene
    /// becomes active, the Zenject scene context exists, and no Error/Exception
    /// log lines were emitted — headless-only noise (audio devices, service
    /// auth) is whitelisted explicitly so real regressions still fail.
    /// </summary>
    public class GameplaySmokeTests
    {
        private const string GameplaySceneName = "Gamplay_Scene";
        private const string HomeMenuSceneName = "HomeMenu";

        private sealed class ErrorCollector : IDisposable
        {
            private static readonly string[] HeadlessNoise =
            {
                "audio", "alsa", "pulseaudio", "input system", "inputsystem",
                "unity services", "authentication", "vivox", "license",
                "graphics device", "dlss", "fsr", "virtualtexturing",
            };
            public readonly List<string> Failures = new List<string>();
            public ErrorCollector() => Application.logMessageReceived += OnLog;
            public void Dispose() => Application.logMessageReceived -= OnLog;
            private void OnLog(string condition, string stackTrace, LogType type)
            {
                if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
                var lower = (condition ?? string.Empty).ToLowerInvariant();
                if (HeadlessNoise.Any(lower.Contains)) return;
                Failures.Add(condition);
            }
            public void AssertClean()
                => Assert.IsEmpty(Failures, "Unexpected errors during smoke path:\n" + string.Join("\n", Failures));
        }

        private static IEnumerator LoadScene(string path)
        {
            var op = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
            Assert.NotNull(op, "LoadSceneAsync returned null for " + path);
            while (!op.isDone) yield return null;
            // Installers and scene-context composition complete in the first frames.
            for (int i = 0; i < 15; i++) yield return null;
        }

        private static SceneContext FindSceneContext()
            => UnityEngine.Object.FindObjectsByType<SceneContext>().FirstOrDefault();

        [TearDown]
        public void TearDown()
        {
            // Single-mode loads leave the last scene active; destroy its roots so
            // canvas/event systems do not leak into unrelated fixtures.
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
                return;
            foreach (var root in scene.GetRootGameObjects())
                if (root != null)
                    UnityEngine.Object.Destroy(root);
        }

        [UnityTest]
        public IEnumerator GameplayScene_LoadsDirectly_WithoutErrors()
        {
            using var errors = new ErrorCollector();
            yield return LoadScene("Assets/Moyva/Scenes/Gamplay_Scene.unity");
            Assert.AreEqual(GameplaySceneName, SceneManager.GetActiveScene().name);
            Assert.NotNull(FindSceneContext(), "Gameplay scene must compose a Zenject SceneContext.");
            errors.AssertClean();
        }

        [UnityTest, Timeout(180000)]
        public IEnumerator BootScene_LoadsThenReachesHomeMenu_WithoutErrors()
        {
            using var errors = new ErrorCollector();
            yield return LoadScene("Assets/Moyva/Scenes/Boot.unity");
            Assert.AreEqual("Boot", SceneManager.GetActiveScene().name);

            // Boot screen holds scene activation until the load completes and the
            // minimum display time passes; the controller then activates HomeMenu.
            var deadline = DateTime.UtcNow.AddSeconds(120);
            while (SceneManager.GetActiveScene().name != HomeMenuSceneName
                   && DateTime.UtcNow < deadline)
                yield return null;
            for (int i = 0; i < 10; i++) yield return null;

            Assert.AreEqual(HomeMenuSceneName, SceneManager.GetActiveScene().name,
                "Boot screen never reached the HomeMenu scene.");
            Assert.NotNull(FindSceneContext(), "HomeMenu scene must compose a Zenject SceneContext.");
            errors.AssertClean();
        }

        [UnityTest, Timeout(180000)]
        public IEnumerator HomeMenu_StartGame_ReachesGameplay_WithoutErrors()
        {
            using var errors = new ErrorCollector();
            yield return LoadScene("Assets/Moyva/Scenes/HomeMenu.unity");

            var context = FindSceneContext();
            Assert.NotNull(context, "HomeMenu scene must compose a Zenject SceneContext.");
            var starter = context.Container.Resolve<IHomeMenuGameStarter>();
            var task = starter.StartGameAsync();

            var deadline = DateTime.UtcNow.AddSeconds(150);
            while (SceneManager.GetActiveScene().name != GameplaySceneName
                   && DateTime.UtcNow < deadline && !task.IsFaulted)
                yield return null;
            for (int i = 0; i < 10; i++) yield return null;

            if (task.IsFaulted)
                Assert.Fail("Startup pipeline faulted: " + task.Exception?.GetBaseException());
            Assert.AreEqual(GameplaySceneName, SceneManager.GetActiveScene().name,
                "Menu -> Gameplay startup never reached the gameplay scene.");
            Assert.NotNull(FindSceneContext(), "Gameplay scene must compose a Zenject SceneContext.");
            errors.AssertClean();
        }
    }
}
