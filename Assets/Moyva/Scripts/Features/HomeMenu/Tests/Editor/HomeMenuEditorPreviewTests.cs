using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.Runtime;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    /// <summary>
    /// Reproduces the editor live-preview mount path that logs
    /// "[HomeMenuMoyvaUI] Editor preview mount failed" in the Console.
    /// </summary>
    public sealed class HomeMenuEditorPreviewTests
    {
        [Test]
        public void EditorPreview_Mount_SucceedsWithoutErrors()
        {
            var errors = new List<string>();
            void Capture(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Error || type == LogType.Exception)
                    errors.Add(condition + "\n" + stackTrace);
            }

            Application.logMessageReceived += Capture;
            try
            {
                var scene = EditorSceneManager.OpenScene(
                    "Assets/Moyva/Scenes/HomeMenu.unity",
                    OpenSceneMode.Single);
                Assert.IsTrue(scene.IsValid(), "HomeMenu scene failed to open");

                var anchor = Object.FindObjectsByType<HomeMenuMoyvaUiAnchor>(
                    FindObjectsInactive.Include)[0];
                Assert.NotNull(anchor, "HomeMenuMoyvaUiAnchor missing in HomeMenu scene");
                Assert.IsTrue(
                    anchor.TryGetEditorAuthoringTargets(out var mountRoot, out var html, out var css),
                    "Anchor has no authoring targets");
                Assert.NotNull(mountRoot);
                Assert.NotNull(html);
                Assert.NotNull(css);

                anchor.EditorRefreshPreview();
            }
            finally
            {
                Application.logMessageReceived -= Capture;
            }

            Assert.IsEmpty(errors, string.Join("\n---\n", errors));
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
    }
}
