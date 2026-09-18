using System.IO;
using Kruty1918.Moyva.Marketing.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Marketing.EditorTools
{
    /// <summary>
    /// Creates/updates the MarketingStudio scene: a single MarketingStudio
    /// object carrying the controller (the rig is built at runtime, keeping
    /// the scene minimal and merge-friendly).
    /// </summary>
    public static class MarketingSceneBuilder
    {
        public const string ScenePath = "Assets/Moyva/Scenes/MarketingStudio.unity";

        [MenuItem("Moyva/Marketing/Create or Repair Studio Scene")]
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("MarketingStudio");
            go.AddComponent<MarketingStudioController>();

            var dir = Path.GetDirectoryName(ScenePath);
            Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[MarketingStudio] Scene saved: {ScenePath}");
        }

        public static void EnsureSceneExists()
        {
            if (File.Exists(Path.GetFullPath(ScenePath)))
                return;
            CreateScene();
        }
    }
}
