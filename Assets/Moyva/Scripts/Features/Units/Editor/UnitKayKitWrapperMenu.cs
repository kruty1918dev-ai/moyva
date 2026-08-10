using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Editor
{
    internal static class UnitKayKitWrapperMenu
    {
        private const string OutputDirectory =
            "Assets/Moyva/Prefabs/Units/KayKit";
        private const string SourceDirectory =
            "Assets/ThirdParty/KayKit/Packs/KayKit - Medieval Hexagon Pack (for Unity)/Prefabs/units/blue";

        private readonly struct WrapperSpec
        {
            public WrapperSpec(
                string id,
                float riderHeight,
                params string[] parts)
            {
                Id = id;
                RiderHeight = riderHeight;
                Parts = parts;
            }

            public string Id { get; }
            public float RiderHeight { get; }
            public string[] Parts { get; }
        }

        private static readonly WrapperSpec[] Specs =
        {
            new WrapperSpec(
                "warrior",
                0f,
                "unit_blue_full",
                "sword_blue_full",
                "shield_blue_full",
                "helmet_blue_full"),
            new WrapperSpec(
                "spearman",
                0f,
                "unit_blue_full",
                "spear_blue_full",
                "shield_blue_full",
                "helmet_blue_full"),
            new WrapperSpec(
                "archer",
                0f,
                "unit_blue_full",
                "bow_blue_full",
                "helmet_blue_full"),
            new WrapperSpec(
                "light-cavalry",
                0.48f,
                "horse_blue_full",
                "unit_blue_full",
                "sword_blue_full",
                "shield_blue_full",
                "helmet_blue_full"),
        };

        [MenuItem("Moyva/Units/Content/Apply KayKit Unit Wrappers")]
        private static void ApplyAll()
        {
            EnsureOutputDirectory();
            for (int index = 0; index < Specs.Length; index++)
                CreateWrapper(Specs[index]);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ValidateAll();
            Debug.Log(
                $"[MoyvaUnitContent] Applied {Specs.Length} KayKit unit wrappers.");
        }

        [MenuItem("Moyva/Units/Content/Validate KayKit Unit Wrappers")]
        private static void ValidateAll()
        {
            var failures = new List<string>();
            for (int index = 0; index < Specs.Length; index++)
            {
                WrapperSpec spec = Specs[index];
                string path = GetOutputPath(spec.Id);
                GameObject prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    failures.Add($"{spec.Id}: prefab missing");
                    continue;
                }

                if (prefab.GetComponentsInChildren<Renderer>(true).Length == 0)
                    failures.Add($"{spec.Id}: no renderer");
                if (prefab.GetComponent<BoxCollider>() == null)
                    failures.Add($"{spec.Id}: root collider missing");
                if (prefab.transform.Find("Visual") == null)
                    failures.Add($"{spec.Id}: Visual root missing");
                if (spec.RiderHeight > 0f)
                    ValidateRiderContact(prefab, spec.Id, failures);
            }

            if (failures.Count > 0)
            {
                throw new InvalidOperationException(
                    "KayKit unit wrapper validation failed: "
                    + string.Join("; ", failures));
            }

            Debug.Log(
                $"[MoyvaUnitContent] VALIDATION_OK wrappers={Specs.Length}");
        }

        private static void ValidateRiderContact(
            GameObject prefab,
            string id,
            ICollection<string> failures)
        {
            Transform horse = prefab.transform.Find(
                "Visual/horse_blue_full");
            Transform rider = prefab.transform.Find(
                "Visual/unit_blue_full");
            Renderer horseRenderer = horse != null
                ? horse.GetComponentInChildren<Renderer>(true)
                : null;
            Renderer riderRenderer = rider != null
                ? rider.GetComponentInChildren<Renderer>(true)
                : null;
            if (horseRenderer == null || riderRenderer == null)
            {
                failures.Add($"{id}: rider/horse renderer missing");
                return;
            }

            float verticalGap = riderRenderer.bounds.min.y
                - horseRenderer.bounds.max.y;
            if (verticalGap > 0.08f)
            {
                failures.Add(
                    $"{id}: rider floats {verticalGap:0.###} units above horse");
            }
            else if (verticalGap < -0.2f)
            {
                failures.Add(
                    $"{id}: rider penetrates horse by {-verticalGap:0.###} units");
            }
        }

        private static void CreateWrapper(WrapperSpec spec)
        {
            string outputPath = GetOutputPath(spec.Id);
            CreateExternalBackupIfPresent(outputPath);

            var root = new GameObject($"Unit-{spec.Id}-KayKit");
            Undo.RegisterCreatedObjectUndo(
                root,
                $"Create KayKit wrapper: {spec.Id}");
            try
            {
                var visualRoot = new GameObject("Visual");
                visualRoot.transform.SetParent(root.transform, false);

                for (int index = 0; index < spec.Parts.Length; index++)
                {
                    string partName = spec.Parts[index];
                    string sourcePath =
                        $"{SourceDirectory}/{partName}.prefab";
                    GameObject source =
                        AssetDatabase.LoadAssetAtPath<GameObject>(
                            sourcePath);
                    if (source == null)
                    {
                        throw new InvalidOperationException(
                            $"KayKit unit part is missing: {sourcePath}");
                    }

                    GameObject part = (GameObject)
                        PrefabUtility.InstantiatePrefab(
                            source,
                            visualRoot.transform);
                    part.name = partName;
                    part.transform.localPosition = ResolvePartOffset(
                        spec,
                        partName);
                    part.transform.localRotation = Quaternion.identity;
                    part.transform.localScale = Vector3.one;
                }

                FitRootCollider(root);
                PrefabUtility.SaveAsPrefabAsset(root, outputPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Vector3 ResolvePartOffset(
            WrapperSpec spec,
            string partName)
        {
            if (spec.RiderHeight <= 0f
                || string.Equals(
                    partName,
                    "horse_blue_full",
                    StringComparison.Ordinal))
            {
                return Vector3.zero;
            }

            return Vector3.up * spec.RiderHeight;
        }

        private static void FitRootCollider(GameObject root)
        {
            Renderer[] renderers =
                root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException(
                    $"Wrapper '{root.name}' contains no renderers.");

            Bounds localBounds = default;
            bool initialized = false;
            for (int index = 0; index < renderers.Length; index++)
            {
                Bounds bounds = renderers[index].bounds;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 world = new Vector3(
                        (corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                        (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                        (corner & 4) == 0 ? bounds.min.z : bounds.max.z);
                    Vector3 local =
                        root.transform.InverseTransformPoint(world);
                    if (!initialized)
                    {
                        localBounds = new Bounds(local, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(local);
                    }
                }
            }

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = localBounds.center;
            collider.size = Vector3.Max(
                localBounds.size,
                Vector3.one * 0.1f);
        }

        private static string GetOutputPath(string id)
            => $"{OutputDirectory}/{id}.prefab";

        private static void EnsureOutputDirectory()
        {
            if (AssetDatabase.IsValidFolder(OutputDirectory))
                return;

            string current = "Assets";
            string[] parts = OutputDirectory.Split('/');
            for (int index = 1; index < parts.Length; index++)
            {
                string next = $"{current}/{parts[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }

        private static void CreateExternalBackupIfPresent(
            string assetPath)
        {
            string projectRoot =
                Directory.GetParent(Application.dataPath)?.FullName;
            string sourcePath = string.IsNullOrWhiteSpace(projectRoot)
                ? null
                : Path.Combine(projectRoot, assetPath);
            if (string.IsNullOrWhiteSpace(sourcePath)
                || !File.Exists(sourcePath))
            {
                return;
            }

            string backupRoot = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "moyva-cli",
                "backups",
                "unit-content",
                DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));
            string backupPath = Path.Combine(backupRoot, assetPath);
            Directory.CreateDirectory(
                Path.GetDirectoryName(backupPath));
            File.Copy(sourcePath, backupPath, overwrite: false);
        }
    }
}
