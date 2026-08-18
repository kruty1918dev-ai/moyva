#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Editor.Migration
{
    internal static class BuildingContentMigrationMenu
    {
        private const string WindmillWrapperPath =
            "Assets/Moyva/Prefabs/Buildings/windmill-01.prefab";
        private const string WindmillSourcePath =
            "Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Prefabs/buildings/blue/building_windmill_blue.prefab";

        [MenuItem("Moyva/Construction/Content/Apply KayKit Windmill Wrapper")]
        private static void ApplyWindmillWrapper()
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
                WindmillSourcePath);
            if (source == null)
                throw new InvalidOperationException(
                    $"KayKit windmill source is missing: {WindmillSourcePath}");

            CreateExternalBackup(WindmillWrapperPath);
            GameObject root = PrefabUtility.LoadPrefabContents(
                WindmillWrapperPath);
            try
            {
                RemoveLegacyCube(root.transform);
                Transform visual = root.transform.Find("Visual");
                if (visual == null)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(
                        source,
                        root.transform);
                    instance.name = "Visual";
                    visual = instance.transform;
                }

                visual.localPosition = Vector3.zero;
                visual.localRotation = Quaternion.identity;
                visual.localScale = Vector3.one;
                CloseWindmillBodyGap(visual);
                FitRootCollider(root);

                PrefabUtility.SaveAsPrefabAsset(root, WindmillWrapperPath);
                Debug.Log(
                    $"[MoyvaBuildingContent] KayKit windmill wrapper applied: {WindmillWrapperPath}");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("Moyva/Construction/Content/Validate KayKit Windmill Wrapper")]
        private static void ValidateWindmillWrapper()
        {
            GameObject wrapper = AssetDatabase.LoadAssetAtPath<GameObject>(
                WindmillWrapperPath);
            bool hasLegacyCube = wrapper != null
                                 && wrapper.transform.Find("Cube") != null;
            bool hasVisual = wrapper != null
                             && wrapper.transform.Find("Visual") != null;
            float bodyGap = hasVisual
                ? CalculateWindmillBodyGap(wrapper.transform.Find("Visual"))
                : float.PositiveInfinity;
            if (wrapper == null
                || hasLegacyCube
                || !hasVisual
                || bodyGap > 0.03f)
            {
                throw new InvalidOperationException(
                    $"Windmill wrapper invalid: exists={wrapper != null}, legacyCube={hasLegacyCube}, visual={hasVisual}, bodyGap={bodyGap:0.###}.");
            }

            Debug.Log(
                "[MoyvaBuildingContent] VALIDATION_OK windmill wrapper uses KayKit visual and has no legacy Cube.");
        }

        private static void CloseWindmillBodyGap(Transform visual)
        {
            Transform top = visual.Find("building_windmill_top_blue");
            float gap = CalculateWindmillBodyGap(visual);
            if (top == null || gap <= 0f || float.IsInfinity(gap))
                return;

            // Keep the authored X/Z alignment and move only enough for the
            // top renderer to meet the base renderer.
            top.position -= Vector3.up * gap;
        }

        private static float CalculateWindmillBodyGap(Transform visual)
        {
            if (visual == null)
                return float.PositiveInfinity;

            Renderer baseRenderer = visual.GetComponent<Renderer>();
            Transform top = visual.Find("building_windmill_top_blue");
            Renderer topRenderer = top != null
                ? top.GetComponent<Renderer>()
                : null;
            if (baseRenderer == null || topRenderer == null)
                return float.PositiveInfinity;

            return topRenderer.bounds.min.y - baseRenderer.bounds.max.y;
        }

        private static void RemoveLegacyCube(Transform root)
        {
            Transform legacyCube = root.Find("Cube");
            if (legacyCube != null)
                UnityEngine.Object.DestroyImmediate(legacyCube.gameObject);
        }

        private static void FitRootCollider(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException(
                    "KayKit windmill contains no renderers.");

            bool initialized = false;
            Bounds localBounds = default;
            foreach (Renderer renderer in renderers)
            {
                Bounds bounds = renderer.bounds;
                Vector3 min = bounds.min;
                Vector3 max = bounds.max;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 worldCorner = new Vector3(
                        (corner & 1) == 0 ? min.x : max.x,
                        (corner & 2) == 0 ? min.y : max.y,
                        (corner & 4) == 0 ? min.z : max.z);
                    Vector3 localCorner =
                        root.transform.InverseTransformPoint(worldCorner);
                    if (!initialized)
                    {
                        localBounds = new Bounds(localCorner, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(localCorner);
                    }
                }
            }

            BoxCollider collider = root.GetComponent<BoxCollider>();
            if (collider == null)
                collider = root.AddComponent<BoxCollider>();
            collider.center = localBounds.center;
            collider.size = Vector3.Max(
                localBounds.size,
                Vector3.one * 0.1f);
        }

        private static void CreateExternalBackup(string assetPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)
                ?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
                throw new InvalidOperationException(
                    "Unable to resolve Unity project root.");

            string sourcePath = Path.Combine(projectRoot, assetPath);
            string backupRoot = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "moyva-cli",
                "backups",
                "building-content",
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
            string backupPath = Path.Combine(backupRoot, assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
            File.Copy(sourcePath, backupPath, overwrite: false);
        }
    }
}

#endif
