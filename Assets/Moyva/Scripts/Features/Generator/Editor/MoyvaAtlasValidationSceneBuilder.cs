using System;
using System.IO;
using Kruty1918.Moyva.Generator.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Builds a deterministic validation scene for the AtlasV3 tile pack:
    /// every dual-grid mask for all nine themes (high and low variants),
    /// an assembled 6x6 platform per theme, and stair flights of 1-4 modules
    /// in all four directions. Open the scene and inspect seams, pivots,
    /// scale and stair alignment against the pack contract.
    /// </summary>
    internal static class MoyvaAtlasValidationSceneBuilder
    {
        private const string ScenePath = "Assets/Moyva/Scenes/AtlasTilesValidation.unity";
        private const string StairTheme = "stone";

        // Gap between single-fragment cells so open borders stay visible.
        private const float FragmentSpacing = 1.5f;
        private const float ThemeRowSpacing = 4f;
        private const int PlatformSize = 6;

        [MenuItem("Tools/Moyva/Atlas V3/Create Validation Scene")]
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("AtlasV3_Validation");
            BuildMaskSections(root.transform);
            BuildAssembledPlatforms(root.transform);
            BuildStairSection(root.transform);
            BuildBeveledSourceRow(root.transform);
            BuildVariedHeightMap(root.transform);
            SetupLightingAndCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[MoyvaAtlasValidation] Saved {ScenePath}. Inspect all forms, " +
                      "stair flights and platform seams; run the game world to verify traversal.");
        }

        /// <summary>All 16 corner masks per theme, high row then low row.</summary>
        private static void BuildMaskSections(Transform root)
        {
            var section = new GameObject("Masks_All16").transform;
            section.SetParent(root, false);

            for (int themeIndex = 0; themeIndex < MoyvaAtlasPackImporter.Themes.Length; themeIndex++)
            {
                string theme = MoyvaAtlasPackImporter.Themes[themeIndex];
                float zBase = themeIndex * ThemeRowSpacing;
                Label(section, $"{theme} masks", new Vector3(-2.2f, 0f, zBase + 0.5f));

                for (int mask = 0; mask < 16; mask++)
                {
                    float x = mask * FragmentSpacing;
                    SpawnForm(section, theme, mask, lowVariant: false,
                        new Vector3(x, 0f, zBase));
                    SpawnForm(section, theme, mask, lowVariant: true,
                        new Vector3(x, 0f, zBase + FragmentSpacing));
                }
            }
        }

        /// <summary>
        /// A filled 6x6 platform per theme built from dual-grid fragments.
        /// A fragment at dual-centre (x+0.5, z+0.5) overlaps platform cells
        /// (x,z) SW, (x+1,z) SE, (x,z+1) NW, (x+1,z+1) NE.
        /// </summary>
        private static void BuildAssembledPlatforms(Transform root)
        {
            var section = new GameObject("Assembled_6x6").transform;
            section.SetParent(root, false);
            const float zBase = -10f;

            for (int themeIndex = 0; themeIndex < MoyvaAtlasPackImporter.Themes.Length; themeIndex++)
            {
                string theme = MoyvaAtlasPackImporter.Themes[themeIndex];
                float xBase = themeIndex * (PlatformSize + 3f);
                Label(section, $"{theme} platform", new Vector3(xBase, 0f, zBase - 1.5f));

                for (int x = -1; x <= PlatformSize; x++)
                for (int z = -1; z <= PlatformSize; z++)
                {
                    int mask = AtlasDualGridShapes.BuildMask(
                        northWest: InPlatform(x, z + 1),
                        northEast: InPlatform(x + 1, z + 1),
                        southWest: InPlatform(x, z),
                        southEast: InPlatform(x + 1, z));
                    if (mask == 0)
                        continue;

                    SpawnForm(section, theme, mask, lowVariant: false,
                        new Vector3(xBase + x + 0.5f, 0f, zBase + z + 0.5f));
                }
            }
        }

        private static bool InPlatform(int x, int z)
            => x >= 0 && x < PlatformSize && z >= 0 && z < PlatformSize;

        /// <summary>Stair flights: 1-4 modules rising +Z, plus one per direction.</summary>
        private static void BuildStairSection(Transform root)
        {
            var section = new GameObject("Stairs").transform;
            section.SetParent(root, false);
            const float zBase = -18f;
            const float xBase = -6f;

            Label(section, "stairs 1-4 modules +Z", new Vector3(xBase - 1f, 0f, zBase));

            // Each module is 1 m deep, rises 0.25 m, pivot at module top surface.
            for (int n = 1; n <= 4; n++)
            {
                float rowX = xBase + n * 7f;
                for (int k = 0; k < n; k++)
                {
                    SpawnPrefab(section, StairPrefabPath, $"stair_{StairTheme}_n{n}_{k}",
                        new Vector3(rowX + (k + 0.5f), (k + 1) * 0.25f, zBase),
                        Quaternion.identity);
                }

                // Landing plateaus at both ends so the flight reads correctly.
                SpawnForm(section, StairTheme, 15, lowVariant: false,
                    new Vector3(rowX - 0.5f, 0f, zBase));
                SpawnForm(section, StairTheme, 15, lowVariant: false,
                    new Vector3(rowX + n + 0.5f, n * 0.25f, zBase));
            }

            // One module per direction descending from a hub platform.
            float hubX = xBase - 4f;
            float hubZ = zBase + 3f;
            Label(section, "stair directions", new Vector3(hubX - 1f, 0f, hubZ));
            SpawnForm(section, StairTheme, 15, lowVariant: false,
                new Vector3(hubX, 0.25f, hubZ));
            for (int dir = 0; dir < 4; dir++)
            {
                Vector3 offset = dir switch
                {
                    0 => new Vector3(0f, 0f, -1f),
                    1 => new Vector3(1f, 0f, 0f),
                    2 => new Vector3(0f, 0f, 1f),
                    _ => new Vector3(-1f, 0f, 0f),
                };
                SpawnPrefab(section, StairPrefabPath, $"stair_dir{dir}",
                    new Vector3(hubX + offset.x, 0.25f, hubZ + offset.z),
                    Quaternion.Euler(0f, dir * 90f, 0f));
            }
        }

        private static string StairPrefabPath
            => $"{MoyvaAtlasPackImporter.PrefabsFolder}/{StairTheme}_stair_025.prefab";

        private static void SpawnForm(
            Transform parent, string theme, int mask, bool lowVariant, Vector3 position)
        {
            if (!AtlasDualGridShapes.TryResolve(mask, out AtlasTileForm form, out int yRotation))
            {
                if (mask == 0)
                    return;
                form = AtlasTileForm.Fill;
                yRotation = 0;
            }

            string kind = form switch
            {
                AtlasTileForm.Corner => "corner",
                AtlasTileForm.Edge => "edge",
                AtlasTileForm.Interior => "interior",
                AtlasTileForm.Merged => "merged",
                _ => "fill",
            };
            string name = $"{theme}_{kind}{(lowVariant ? "_low" : string.Empty)}";
            SpawnPrefab(parent,
                $"{MoyvaAtlasPackImporter.PrefabsFolder}/{name}.prefab",
                $"{name}_m{mask:00}_r{yRotation}",
                position,
                Quaternion.Euler(0f, yRotation, 0f));
        }

        private static GameObject SpawnPrefab(
            Transform parent, string path, string name, Vector3 position, Quaternion rotation)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[MoyvaAtlasValidation] Missing prefab {path} — run " +
                                 "Tools/Moyva/Atlas V3/Import Atlas Pack Assets first.");
                return null;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            return go;
        }

        /// <summary>
        /// All 19 beveled source OBJs in two rows at their authored scale,
        /// so every shipped model can be eyeballed against the pack renders.
        /// </summary>
        private static void BuildBeveledSourceRow(Transform root)
        {
            var section = new GameObject("Beveled_Sources").transform;
            section.SetParent(root, false);
            const float zBase = 44f;

            string[] files = Directory.GetFiles(
                MoyvaCliffTileAssetBuilder.SourceObjFolder, "*.obj");
            Array.Sort(files, StringComparer.Ordinal);
            Material preview = AssetDatabase.LoadAssetAtPath<Material>(
                $"{MoyvaAtlasPackImporter.CliffMaterialsFolder}/moyva_theme_stone.mat");

            for (int i = 0; i < files.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(files[i]);
                if (prefab == null)
                {
                    Debug.LogWarning($"[MoyvaAtlasValidation] Cannot load {files[i]}");
                    continue;
                }

                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.name = Path.GetFileNameWithoutExtension(files[i]);
                go.transform.SetParent(section, false);
                go.transform.SetPositionAndRotation(
                    new Vector3((i % 7) * 3f, 0f, zBase + (i / 7) * 3f),
                    Quaternion.identity);
                if (preview != null)
                    foreach (var r in go.GetComponentsInChildren<Renderer>())
                        r.sharedMaterial = preview;
            }
        }

        /// <summary>
        /// A small two-level terrain patch built from real mask dispatch:
        /// a raised platform beside a lower plateau plus a stepped edge, so
        /// high/high, low/low and low/high neighbour seams are all covered.
        /// </summary>
        private static void BuildVariedHeightMap(Transform root)
        {
            var section = new GameObject("Varied_Heights").transform;
            section.SetParent(root, false);
            const float zBase = 30f;
            const int w = 8, h = 6;

            // Logical heights: right half raised one step, upper-left bump.
            var heights = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int z = 0; z < h; z++)
                heights[x, z] = x >= 4 ? 1 : 0;
            for (int x = 1; x <= 2; x++)
            for (int z = 3; z <= 4; z++)
                heights[x, z] = 1;

            // Dual-grid fragment at vertex (x,z) sees corner cells
            // (x,z) SW, (x+1,z) SE, (x,z+1) NW, (x+1,z+1) NE.
            for (int x = -1; x <= w; x++)
            for (int z = -1; z <= h; z++)
            {
                int sw = HeightAt(heights, x, z);
                int se = HeightAt(heights, x + 1, z);
                int nw = HeightAt(heights, x, z + 1);
                int ne = HeightAt(heights, x + 1, z + 1);
                int top = Mathf.Max(Mathf.Max(sw, se), Mathf.Max(nw, ne));
                if (top == 0)
                    continue; // ground plane is not part of the tile set

                int mask = AtlasDualGridShapes.BuildMask(
                    northWest: nw == top,
                    northEast: ne == top,
                    southWest: sw == top,
                    southEast: se == top);
                if (mask == 0)
                    continue;

                SpawnForm(section, "grass", mask, lowVariant: false,
                    new Vector3(x + 0.5f, top * 0.5f, zBase + z + 0.5f));
            }
        }

        private static int HeightAt(int[,] heights, int x, int z)
            => x < 0 || z < 0 || x >= heights.GetLength(0) || z >= heights.GetLength(1)
                ? -1
                : heights[x, z];

        /// <summary>
        /// Renders the saved validation scene from a few fixed viewpoints
        /// into PNGs so batch runs leave inspectable evidence.
        /// </summary>
        public static void CaptureForBatch()
        {
            try
            {
                EditorSceneManager.OpenScene(ScenePath);
                string dir = Path.GetFullPath(
                    Path.Combine(Application.dataPath, "../Library/ai/beveled-validation"));
                Directory.CreateDirectory(dir);

                Shot(new Vector3(11f, 26f, 4f), Quaternion.Euler(60f, 0f, 0f),
                    dir + "/masks_overview.png", 1600, 1200);
                Shot(new Vector3(4f, 6f, 26f), Quaternion.Euler(35f, -12f, 0f),
                    dir + "/varied_heights.png", 1600, 1200);
                Shot(new Vector3(9f, 8f, 38f),
                    Quaternion.LookRotation(new Vector3(0f, -8f, 9f), Vector3.up),
                    dir + "/beveled_sources.png", 1600, 900);
                Shot(new Vector3(8f, 8f, -12f), Quaternion.Euler(30f, 0f, 0f),
                    dir + "/platforms_stairs.png", 1600, 1200);
                Shot(new Vector3(2.5f, 4f, -15f),
                    Quaternion.LookRotation(new Vector3(0f, -4f, 8f), Vector3.up),
                    dir + "/seam_closeup.png", 1600, 900);
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoyvaAtlasValidation] Capture failed: {ex}");
                EditorApplication.Exit(1);
            }
        }

        private static void Shot(
            Vector3 position, Quaternion rotation, string path, int w, int h)
        {
            var go = new GameObject("shotcam");
            try
            {
                var cam = go.AddComponent<UnityEngine.Camera>();
                go.transform.SetPositionAndRotation(position, rotation);
                cam.clearFlags = CameraClearFlags.Skybox;
                var rt = new RenderTexture(w, h, 24);
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();
                RenderTexture.active = null;
                cam.targetTexture = null;
                rt.Release();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Debug.Log($"[MoyvaAtlasValidation] Wrote {path}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static void Label(Transform parent, string text, Vector3 position)
        {
            var go = new GameObject(text);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
        }

        private static void SetupLightingAndCamera()
        {
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var camGo = new GameObject("Validation Camera");
            var cam = camGo.AddComponent<UnityEngine.Camera>();
            camGo.transform.position = new Vector3(11f, 22f, -14f);
            camGo.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
        }
    }
}
