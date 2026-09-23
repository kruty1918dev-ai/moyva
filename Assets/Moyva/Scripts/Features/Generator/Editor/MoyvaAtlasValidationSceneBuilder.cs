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
