#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Kruty1918.JsonConfig;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Removes JsonBindingMarker records whose config type or JSON id no longer
/// resolves (e.g. deleted generator-graph types or renamed audio presets).
/// Invoke via CLI: -executeMethod StaleMoyvaJsonBindingCleanup.Run
/// </summary>
public static class StaleMoyvaJsonBindingCleanup
{
    public static void Run()
    {
        int removed = 0;
        int touchedScenes = 0;
        SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            JsonConfigRuntime.EnsureLoaded();
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Moyva" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                int sceneRemoved = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (JsonBindingMarker marker in root.GetComponentsInChildren<JsonBindingMarker>(true))
                    {
                        var kept = new List<JsonBindingRecord>();
                        foreach (JsonBindingRecord record in marker.Bindings)
                        {
                            if (record != null && record.Target != null &&
                                JsonConfigRuntime.GetByTypeName(record.ConfigType, record.ConfigId) != null)
                            {
                                kept.Add(record);
                                continue;
                            }

                            sceneRemoved++;
                            Debug.Log($"[StaleMoyvaJsonBindingCleanup] Removed stale binding " +
                                      $"{path}: {record?.ConfigType}.{record?.PropertyPath} id={record?.ConfigId}");
                        }

                        if (kept.Count != marker.Bindings.Count)
                        {
                            marker.ReplaceBindings(kept);
                            EditorUtility.SetDirty(marker);
                        }
                    }
                }

                if (sceneRemoved > 0)
                {
                    // Some hosts (e.g. ReactUnity UGUI) generate UI children in edit mode.
                    // Destroy those generated subtrees so SaveScene cannot persist them.
                    RemoveGeneratedUiArtifacts(scene);
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    touchedScenes++;
                    removed += sceneRemoved;
                }
            }
        }
        finally
        {
            if (setup != null && setup.Length > 0)
                EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        Debug.Log($"[StaleMoyvaJsonBindingCleanup] Done. removed={removed} scenes={touchedScenes}");
    }

    /// <summary>Destroy GameObjects that carry ReactUnity generated-element components.</summary>
    private static void RemoveGeneratedUiArtifacts(Scene scene)
    {
        var generated = new List<GameObject>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Component component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null) continue;
                // ReactUnity marks every generated node with a ReactElement component;
                // host components (ReactUnityUGUI etc.) are authored and must stay.
                string fullName = component.GetType().FullName ?? string.Empty;
                if (fullName.StartsWith("ReactUnity.", StringComparison.Ordinal) &&
                    fullName.EndsWith(".ReactElement", StringComparison.Ordinal))
                {
                    generated.Add(component.gameObject);
                }
            }
        }

        var generatedSet = new HashSet<GameObject>(generated);
        foreach (GameObject go in generated)
        {
            if (go == null) continue;
            Transform parent = go.transform.parent;
            bool parentIsGenerated = false;
            while (parent != null)
            {
                if (generatedSet.Contains(parent.gameObject)) { parentIsGenerated = true; break; }
                parent = parent.parent;
            }

            if (!parentIsGenerated)
                UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
#endif
