using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Builds a deterministic validation scene for the simple-stable-v1
    /// profile: a strip of generated solid beveled terrain tiles at varied
    /// heights (checks continuous tops, bevels and closure), a water strip of
    /// flat quads over bed columns (checks the single-quad surface and the
    /// sealed underwater void), and one instance of every sw-* decoration
    /// wrapper (checks pivot, grounding and materials).
    /// </summary>
    internal static class MoyvaSimpleStableValidationSceneBuilder
    {
        private const string ScenePath = "Assets/Moyva/Scenes/SimpleStableValidation.unity";
        private const string WrapperFolder = "Assets/Moyva/Prefabs/Environment/SimpleStable";
        private const float CellSize = 1f;

        private static readonly string[] WrapperIds =
        {
            "sw-tree-single-a", "sw-tree-single-b",
            "sw-rock-single-a", "sw-rock-single-c", "sw-rock-single-e",
            "sw-grass-cross-a", "sw-grass-tri-a", "sw-grass-tall-a",
            "sw-fern-a", "sw-flower-a", "sw-flower-b", "sw-sedge-a",
            "sw-waterlily-a",
        };

        private static readonly float[] TerrainHeights = { 0f, 0f, 1f, 1f, 2f, 1f, 0f, 0f };

        [MenuItem("Tools/Moyva/Simple Stable/Create Validation Scene")]
        public static void CreateScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("SimpleStable_Validation");

            BuildTerrainStrip(root.transform);
            BuildWaterStrip(root.transform);
            BuildWrapperRow(root.transform);
            SetupLightingAndCamera();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[SimpleStableValidation] Saved {ScenePath}. " +
                      "Terrain: continuous tops, no centre seams, closed walls. " +
                      "Water: one flat quad per cell, bed sealed below. " +
                      "Props: grounded, no floaters.");
        }

        /// <summary>
        /// One solid beveled tile per cell in two height-stepped rows — the
        /// exact mesh the simple-stable terrain path emits per cell.
        /// </summary>
        private static void BuildTerrainStrip(Transform root)
        {
            var section = new GameObject("Terrain_SolidBeveled").transform;
            section.SetParent(root, false);

            Mesh mesh = SolidBeveledTileMeshUtility.GetOrCreate();
            Material material = LoadMaterial("SW_Stone", new Color(0.5f, 0.48f, 0.42f));

            for (int i = 0; i < TerrainHeights.Length; i++)
            {
                var go = new GameObject($"tile_{i}_h{TerrainHeights[i]}");
                go.transform.SetParent(section, false);
                go.transform.position = new Vector3(i * CellSize, TerrainHeights[i], 0f);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = material;
            }
        }

        /// <summary>
        /// Water quads at a shared level plus sealed bed columns — mirrors
        /// CollectSimpleWaterSource + CollectWaterBedSource output.
        /// </summary>
        private static void BuildWaterStrip(Transform root)
        {
            var section = new GameObject("Water_SimpleQuads").transform;
            section.SetParent(root, false);

            Mesh quad = SimpleWaterQuadMeshUtility.GetOrCreate();
            Material water = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Moyva/Art/Materials/Water_SimpleStable.mat");
            Material bed = LoadMaterial("SW_Stone", new Color(0.5f, 0.48f, 0.42f));

            const float waterY = -0.35f;
            const int count = 4;
            for (int i = 0; i < count; i++)
            {
                var surface = new GameObject($"water_{i}");
                surface.transform.SetParent(section, false);
                surface.transform.position = new Vector3(i * CellSize, waterY, 3f);
                surface.transform.localScale = new Vector3(CellSize, 1f, CellSize);
                surface.AddComponent<MeshFilter>().sharedMesh = quad;
                surface.AddComponent<MeshRenderer>().sharedMaterial = water;

                var column = GameObject.CreatePrimitive(PrimitiveType.Cube);
                column.name = $"waterbed_{i}";
                Object.DestroyImmediate(column.GetComponent<Collider>());
                column.transform.SetParent(section, false);
                column.transform.position = new Vector3(i * CellSize, waterY - 1f, 3f);
                column.transform.localScale = new Vector3(CellSize, 2f, CellSize);
                column.GetComponent<MeshRenderer>().sharedMaterial = bed;
            }
        }

        private static void BuildWrapperRow(Transform root)
        {
            var section = new GameObject("Decoration_Wrappers").transform;
            section.SetParent(root, false);

            int missing = 0;
            for (int i = 0; i < WrapperIds.Length; i++)
            {
                string path = $"{WrapperFolder}/{WrapperIds[i]}.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogWarning($"[SimpleStableValidation] Missing wrapper {path}");
                    missing++;
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = WrapperIds[i];
                instance.transform.SetParent(section, false);
                instance.transform.position = new Vector3(i * 1.25f, 0f, 6f);
            }

            if (missing > 0)
                Debug.LogWarning(
                    $"[SimpleStableValidation] {missing} wrapper prefabs missing — " +
                    "run Tools/Moyva/Simple Stable/Build Decoration Wrappers first.");
        }

        private static Material LoadMaterial(string name, Color fallbackColor)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(
                $"Assets/Moyva/Art/Materials/{name}.mat");
            if (material != null)
                return material;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            var generated = new Material(shader) { name = name + "_Validation" };
            generated.SetColor("_BaseColor", fallbackColor);
            return generated;
        }

        private static void SetupLightingAndCamera()
        {
            var light = new GameObject("Sun");
            var directional = light.AddComponent<Light>();
            directional.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var cameraGo = new GameObject("ValidationCamera");
            var camera = cameraGo.AddComponent<UnityEngine.Camera>();
            camera.transform.position = new Vector3(7f, 6f, -8f);
            camera.transform.LookAt(new Vector3(6f, 0f, 3f));
        }
    }
}
