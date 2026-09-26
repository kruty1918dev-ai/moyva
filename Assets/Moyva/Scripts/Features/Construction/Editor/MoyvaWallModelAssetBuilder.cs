using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Construction.Editor
{
    /// <summary>
    /// Rewrites the wall/gate collection prefabs onto the authored OBJ meshes
    /// (Assets/Moyva/Art/Models/Walls) with the shared gray-stone material.
    /// Prefab files keep their paths and GUIDs, so every serialized reference
    /// (building registry, building definitions, asset catalog) stays stable.
    /// </summary>
    public static class MoyvaWallModelAssetBuilder
    {
        private const string SourceFolder = "Assets/Moyva/Art/Models/Walls";
        private const string AtlasTexturePath = "Assets/Moyva/Art/Textures/gray_stone_atlas.png";
        private const string MaterialPath = "Assets/Moyva/Art/Materials/StoneWall_Gray.mat";
        private const string WallsFolder = "Assets/Moyva/Prefabs/Buildings/Walls/wall-stone-collection-0";
        private const string ResolverFolder = "Assets/Moyva/Prefabs/Buildings/ResolverGenerated/wall-stone-collection-0";

        private const string MeshWallStraight = "Wall_Straight";
        private const string MeshWallCorner = "Wall_Corner";
        private const string MeshGateClosed = "Gate_Closed";
        private const string MeshGateOpen = "Gate_Open";

        /// <summary>Prefab file name → source mesh + model yaw.</summary>
        private readonly struct WallPrefabSpec
        {
            public readonly string PrefabPath;
            public readonly string MeshName;
            public readonly int YawDegrees;

            public WallPrefabSpec(string path, string mesh, int yaw)
            {
                PrefabPath = path;
                MeshName = mesh;
                YawDegrees = yaw;
            }
        }

        [MenuItem("Tools/Moyva/Walls/Build Wall & Gate Assets")]
        public static void BuildMenu() => Build();

        public static void BuildForBatch() => Build();

        public static int Build()
        {
            var meshes = LoadSourceMeshes();
            Material material = EnsureMaterial();
            var (cornerArmA, cornerArmB) = DetectCornerArms(meshes[MeshWallCorner]);

            int rewritten = 0;
            foreach (var spec in BuildSpecs(cornerArmA, cornerArmB))
            {
                if (RewriteWallPrefab(spec, meshes, material))
                    rewritten++;
            }

            rewritten += RewriteGatePrefab(meshes, material) ? 1 : 0;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[MoyvaWallModelAssetBuilder] Rewrote {rewritten} prefabs; " +
                      $"corner arms detected: {DescribeDir(cornerArmA)}/{DescribeDir(cornerArmB)}");
            return rewritten;
        }

        // ---------------------------------------------------------------
        // Prefab specs
        // ---------------------------------------------------------------

        /// <summary>
        /// Returns the yaw that rotates the imported corner's detected arm pair
        /// onto the requested pair. Works for either importer chirality.
        /// </summary>
        private static int CornerYaw(Vector2Int detectedA, Vector2Int detectedB, Vector2Int wantA, Vector2Int wantB)
        {
            for (int yaw = 0; yaw < 360; yaw += 90)
            {
                if (Rotate(detectedA, yaw) == wantA && Rotate(detectedB, yaw) == wantB)
                    return yaw;
                if (Rotate(detectedA, yaw) == wantB && Rotate(detectedB, yaw) == wantA)
                    return yaw;
            }
            throw new InvalidOperationException(
                $"Cannot rotate corner arms {DescribeDir(detectedA)}/{DescribeDir(detectedB)} onto {DescribeDir(wantA)}/{DescribeDir(wantB)}");
        }

        /// <summary>Yaws a cardinal direction by 90° steps around +Y.</summary>
        private static Vector2Int Rotate(Vector2Int dir, int yaw)
        {
            // Unity yaw +90: +X → -Z, +Z → +X.
            for (int i = 0; i < ((yaw % 360) + 360) % 360 / 90; i++)
                dir = new Vector2Int(dir.y, -dir.x);
            return dir;
        }

        private static string DescribeDir(Vector2Int dir)
        {
            if (dir == Vector2Int.right) return "+X";
            if (dir == Vector2Int.left) return "-X";
            if (dir == Vector2Int.up) return "+Z";
            if (dir == Vector2Int.down) return "-Z";
            return dir.ToString();
        }

        private static IEnumerable<WallPrefabSpec> BuildSpecs(Vector2Int detectedA, Vector2Int detectedB)
        {
            // Neighbor-connection pairs each corner prefab must satisfy
            // (world axes: +X = East, +Z = North).
            int yawNE = CornerYaw(detectedA, detectedB, Vector2Int.right, Vector2Int.up);
            int yawSE = CornerYaw(detectedA, detectedB, Vector2Int.right, Vector2Int.down);
            int yawSW = CornerYaw(detectedA, detectedB, Vector2Int.left, Vector2Int.down);
            int yawNW = CornerYaw(detectedA, detectedB, Vector2Int.left, Vector2Int.up);

            yield return new WallPrefabSpec($"{WallsFolder}/wall-stone-collection-0_Horizontal.prefab", MeshWallStraight, 0);
            yield return new WallPrefabSpec($"{WallsFolder}/wall-stone-collection-0_Vertical.prefab", MeshWallStraight, 90);
            yield return new WallPrefabSpec($"{WallsFolder}/wall-stone-collection-0_Corner_NE.prefab", MeshWallCorner, yawNE);
            yield return new WallPrefabSpec($"{WallsFolder}/wall-stone-collection-0_Corner_NW.prefab", MeshWallCorner, yawNW);
            yield return new WallPrefabSpec($"{WallsFolder}/wall-stone-collection-0_Corner_SE.prefab", MeshWallCorner, yawSE);
            yield return new WallPrefabSpec($"{WallsFolder}/wall-stone-collection-0_Corner_SW.prefab", MeshWallCorner, yawSW);

            // Topology-binding variants: ends and edge-strip pieces reuse the
            // straight segment — the cap silhouette already reads as an end.
            yield return new WallPrefabSpec($"{ResolverFolder}/wall_stone_collection_0_end_east.prefab", MeshWallStraight, 0);
            yield return new WallPrefabSpec($"{ResolverFolder}/wall_stone_collection_0_end_west.prefab", MeshWallStraight, 0);
            yield return new WallPrefabSpec($"{ResolverFolder}/wall_stone_collection_0_end_north.prefab", MeshWallStraight, 90);
            yield return new WallPrefabSpec($"{ResolverFolder}/wall_stone_collection_0_end_south.prefab", MeshWallStraight, 90);
            yield return new WallPrefabSpec($"{ResolverFolder}/wall_stone_collection_0_horizontal_top.prefab", MeshWallStraight, 0);
            yield return new WallPrefabSpec($"{ResolverFolder}/wall_stone_collection_0_horizontal_bottom.prefab", MeshWallStraight, 0);
            yield return new WallPrefabSpec($"{ResolverFolder}/wall_stone_collection_0_vertical_left.prefab", MeshWallStraight, 90);
            yield return new WallPrefabSpec($"{ResolverFolder}/wall_stone_collection_0_vertical_right.prefab", MeshWallStraight, 90);
        }

        // ---------------------------------------------------------------
        // Prefab rewrite
        // ---------------------------------------------------------------

        private static bool RewriteWallPrefab(
            WallPrefabSpec spec,
            IReadOnlyDictionary<string, Mesh> meshes,
            Material material)
        {
            if (!meshes.TryGetValue(spec.MeshName, out Mesh mesh) || mesh == null)
            {
                Debug.LogError($"[MoyvaWallModelAssetBuilder] Missing mesh '{spec.MeshName}' for {spec.PrefabPath}");
                return false;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(spec.PrefabPath);
            try
            {
                StripLegacyComponents(root);
                GameObject model = CreateModelChild(root.transform, "Model", spec.YawDegrees);
                AttachMesh(model, mesh, material);
            }
            finally
            {
                PrefabUtility.SaveAsPrefabAsset(root, spec.PrefabPath);
                PrefabUtility.UnloadPrefabContents(root);
            }
            return true;
        }

        private static bool RewriteGatePrefab(
            IReadOnlyDictionary<string, Mesh> meshes,
            Material material)
        {
            string path = $"{WallsFolder}/wall-stone-collection-0_Gate.prefab";
            if (!meshes.TryGetValue(MeshGateClosed, out Mesh closed) || closed == null
                || !meshes.TryGetValue(MeshGateOpen, out Mesh open) || open == null)
            {
                Debug.LogError("[MoyvaWallModelAssetBuilder] Missing gate meshes.");
                return false;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                StripLegacyComponents(root);

                GameObject closedChild = CreateModelChild(root.transform, "Closed", 0);
                AttachMesh(closedChild, closed, material);
                GameObject openChild = CreateModelChild(root.transform, "Open", 0);
                AttachMesh(openChild, open, material);
                openChild.SetActive(false);

                var view = root.GetComponent<GateVisualStateView>();
                if (view == null)
                    view = root.AddComponent<GateVisualStateView>();

                var so = new SerializedObject(view);
                so.FindProperty("_closedRoot").objectReferenceValue = closedChild;
                so.FindProperty("_openRoot").objectReferenceValue = openChild;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            finally
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
            return true;
        }

        /// <summary>Removes SpriteRenderer and any legacy children; keeps the root.</summary>
        private static void StripLegacyComponents(GameObject root)
        {
            for (int i = root.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);

            foreach (var component in root.GetComponents<Component>())
            {
                if (component is Transform)
                    continue;
                UnityEngine.Object.DestroyImmediate(component);
            }
        }

        private static GameObject CreateModelChild(Transform parent, string name, int yawDegrees)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, worldPositionStays: false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localEulerAngles = new Vector3(0f, yawDegrees, 0f);
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static void AttachMesh(GameObject target, Mesh mesh, Material material)
        {
            var filter = target.GetComponent<MeshFilter>();
            if (filter == null)
                filter = target.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;

            var renderer = target.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = target.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        // ---------------------------------------------------------------
        // Source meshes / material
        // ---------------------------------------------------------------

        private static Dictionary<string, Mesh> LoadSourceMeshes()
        {
            var result = new Dictionary<string, Mesh>(StringComparer.Ordinal);
            foreach (string path in Directory.GetFiles(SourceFolder, "*.obj"))
            {
                PrepareObjImporter(path);
                Mesh mesh = null;
                foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is Mesh candidate && mesh == null)
                        mesh = candidate;
                }
                if (mesh == null)
                    throw new InvalidDataException("No mesh in " + path);
                result[Path.GetFileNameWithoutExtension(path)] = mesh;
            }
            return result;
        }

        private static void PrepareObjImporter(string path)
        {
            if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                importer = AssetImporter.GetAtPath(path) as ModelImporter;
            }
            if (importer == null)
                throw new FileNotFoundException("ModelImporter unavailable for " + path);

            importer.isReadable = true;
            importer.useFileScale = false;
            importer.globalScale = 1f;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Infers the corner's arm pair from the empty (concave) quadrant of
        /// face centres: an L connecting +X/+Z leaves the -X/-Z quadrant empty.
        /// Robust to importer axis flips.
        /// </summary>
        private static (Vector2Int armA, Vector2Int armB) DetectCornerArms(Mesh cornerMesh)
        {
            Vector3[] verts = cornerMesh.vertices;
            int[] tris = cornerMesh.triangles;

            int pp = 0, pn = 0, np = 0, nn = 0;
            for (int t = 0; t < tris.Length; t += 3)
            {
                Vector3 c = (verts[tris[t]] + verts[tris[t + 1]] + verts[tris[t + 2]]) / 3f;
                if (c.x >= 0f && c.z >= 0f) pp++;
                else if (c.x >= 0f) pn++;
                else if (c.z >= 0f) np++;
                else nn++;
            }

            Debug.Log($"[MoyvaWallModelAssetBuilder] corner face-centre quadrants: " +
                      $"++={pp} +-={pn} -+={np} --={nn}");

            int min = Mathf.Min(pp, Mathf.Min(pn, Mathf.Min(np, nn)));
            if (pp == min) return (Vector2Int.left, Vector2Int.down);   // empty ++ → arms -X,-Z (SW)
            if (pn == min) return (Vector2Int.left, Vector2Int.up);     // empty +- → arms -X,+Z (NW)
            if (np == min) return (Vector2Int.right, Vector2Int.down);  // empty -+ → arms +X,-Z (SE)
            return (Vector2Int.right, Vector2Int.up);                   // empty -- → arms +X,+Z (NE)
        }

        private static Material EnsureMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            Texture2D atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasTexturePath);
            if (atlas == null)
            {
                AssetDatabase.ImportAsset(AtlasTexturePath, ImportAssetOptions.ForceSynchronousImport);
                atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasTexturePath);
            }
            if (atlas == null)
                throw new FileNotFoundException("Atlas texture missing: " + AtlasTexturePath);

            material.SetTexture("_BaseMap", atlas);
            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
