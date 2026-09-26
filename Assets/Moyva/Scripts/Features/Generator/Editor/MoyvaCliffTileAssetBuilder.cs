using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Imports the cliff dual-grid mesh set as the single shared
    /// dual-grid mesh set used by every atlas theme. The beveled OBJ set
    /// under <c>Models/Beveled/</c> (Moyva_DualGrid_19_Beveled_Models) is the
    /// preferred source; <c>Models/TileSet-1.fbx</c> remains as the fallback.
    /// Each source mesh is baked to the canonical contract: 1 m quad
    /// footprint, plateau top at Y = 0, pivot at quad centre, identity
    /// rotation/scale, faceted normals and regenerated UVs (planar tops
    /// and bevels, box-projected walls).
    ///
    /// The generated prefabs keep their existing paths
    /// (Generated/Prefabs/{theme}_{form}[_low].prefab), so all TilePreset and
    /// atlas-tile-set.json references survive unchanged — only the mesh and the
    /// per-theme material inside each prefab are swapped.
    /// </summary>
    internal static class MoyvaCliffTileAssetBuilder
    {
        internal const string SourceObjFolder =
            MoyvaAtlasPackImporter.PackRoot + "/Models/Beveled";
        private const string SourceFbxPath =
            MoyvaAtlasPackImporter.PackRoot + "/Models/TileSet-1.fbx";
        private const string SourcesFolder =
            MoyvaAtlasPackImporter.PackRoot + "/Textures/Sources";

        private const float TopNormalY = 0.55f;
        private const float FlatTopNormalY = 0.9f;
        private const float HighDropMeters = 0.5f;
        private const float LowDropMeters = 0.25f;

        /// <summary>Canonical quadrant bits: SW=1, SE=2, NE=4, NW=8.</summary>
        private const int Sw = 1;
        private const int Se = 2;
        private const int Ne = 4;
        private const int Nw = 8;

        /// <summary>
        /// Canonical source model per form. Names exist in both the beveled
        /// OBJ set and the legacy FBX; per Moyva_Bevel_Masks.json the chosen
        /// pieces carry pack masks corner=1 (NW high), edge=3 (N half),
        /// interior=7 (missing SE), merged=6 (NE+SW diagonal) and fill=15
        /// (all four quadrants raised). The canonical yaw is auto-derived
        /// from plateau quadrant coverage, so the pack's NW=1/NE=2/SW=4/SE=8
        /// bit order is only used to pick a representative variant.
        /// </summary>
        private static readonly Dictionary<string, string> FormModels =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["corner"] = "Cliff_Corner_Tile",
                ["edge"] = "Cliff_Edge_Tile",
                ["interior"] = "Cliff_Int_Corner_Tile",
                ["merged"] = "Cliff_Double_Corner_Tile",
                ["fill"] = "Cliff_Fill_Tile.013",
            };

        /// <summary>FBX node names that match the beveled picks.</summary>
        private static readonly Dictionary<string, string> LegacyFormModels =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["corner"] = "Cliff_Corner_Tile.003",
                ["edge"] = "Cliff_Edge_Tile.002",
                ["interior"] = "Cliff_Int_Corner_Tile.001",
                ["merged"] = "Cliff_Double_Corner_Tile.002",
                ["fill"] = "Cliff_Fill_Tile.008",
            };

        /// <summary>Plateau quadrants occupied in the canonical (yaw 0) form.</summary>
        private static readonly Dictionary<string, int> CanonicalCoverage =
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                ["corner"] = Sw,
                ["edge"] = Sw | Se,
                ["interior"] = Nw | Ne | Se,
                ["merged"] = Sw | Ne,
                ["fill"] = Sw | Se | Ne | Nw,
            };

        private static readonly Dictionary<string, string> ThemeTextures =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["grass"] = "T_MOYVA_Grass_BaseColor.png",
                ["sand"] = "T_MOYVA_Sand_BaseColor.png",
                ["dirt"] = "T_MOYVA_Earth_BaseColor.png",
                ["stone"] = "T_MOYVA_Rock_BaseColor.png",
                ["snow"] = "T_MOYVA_Snow_BaseColor.png",
                ["swamp"] = "T_MOYVA_ForestFloor_BaseColor.png",
                ["rock_cliff"] = "T_MOYVA_Rock_BaseColor.png",
                ["road"] = "T_MOYVA_Rock_BaseColor.png",
                ["footpath"] = "T_MOYVA_Earth_BaseColor.png",
            };

        private static readonly string[] Forms = { "corner", "edge", "interior", "merged", "fill" };

        [MenuItem("Tools/Moyva/Atlas V3/Build Cliff Tile Set")]
        public static void BuildMenu()
        {
            int count = Build();
            Debug.Log($"[MoyvaCliffTiles] Built {count} shared cliff meshes.");
        }

        public static void BuildForBatch()
        {
            try
            {
                int count = Build();
                Debug.Log($"[MoyvaCliffTiles] Batch build complete: {count} shared meshes.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MoyvaCliffTiles] Build failed: {ex}");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Rebuilds the shared cliff meshes, theme materials and rewires the
        /// generated per-theme prefabs in place. Returns emitted mesh count;
        /// returns 0 when the source FBX is absent so atlas-only imports keep
        /// working.
        /// </summary>
        public static int Build()
        {
            bool useObjSet = AssetDatabase.IsValidFolder(SourceObjFolder)
                && Directory.GetFiles(SourceObjFolder, "*.obj").Length > 0;
            if (!useObjSet && !File.Exists(SourceFbxPath))
            {
                Debug.LogWarning($"[MoyvaCliffTiles] No sources: {SourceObjFolder} and {SourceFbxPath} missing. Skipping.");
                return 0;
            }

            var models = useObjSet ? FormModels : LegacyFormModels;
            var sourceMeshes = useObjSet
                ? LoadObjSourceMeshes()
                : LoadFbxSourceMeshes();
            if (sourceMeshes.Count == 0)
                throw new InvalidDataException("No meshes found in " +
                    (useObjSet ? SourceObjFolder : SourceFbxPath));

            var baked = new Dictionary<string, MeshData>(StringComparer.Ordinal);
            foreach (var pair in models)
            {
                if (!sourceMeshes.TryGetValue(pair.Value, out var source)
                    || source.mesh == null)
                {
                    throw new InvalidDataException(
                        $"Missing source model '{pair.Value}' for form '{pair.Key}'.");
                }

                MeshData data = Bake(source);
                data.yaw = ResolveCanonicalYaw(pair.Key, data);
                baked[pair.Key] = data;
            }

            // The fill tile covers the whole authored quad, so its extent is the
            // authoritative source tile size.
            float scale = 1f / Mathf.Max(baked["fill"].MaxExtentXZ(), 1e-5f);

            MoyvaAtlasPackImporter.EnsureFolder(MoyvaAtlasPackImporter.CliffMeshesFolder);
            MoyvaAtlasPackImporter.EnsureFolder(MoyvaAtlasPackImporter.CliffMaterialsFolder);

            var meshes = new Dictionary<string, Mesh>(StringComparer.Ordinal);
            foreach (string form in Forms)
            {
                meshes[form] = EmitMesh(form, baked[form], scale, low: false);
                meshes[form + "_low"] = EmitMesh(form, baked[form], scale, low: true);
            }

            var materials = new Dictionary<string, Material>(StringComparer.Ordinal);
            foreach (string theme in MoyvaAtlasPackImporter.Themes)
                materials[theme] = CreateOrUpdateThemeMaterial(theme);
            Material sideMaterial = CreateOrUpdateSideMaterial();

            foreach (string theme in MoyvaAtlasPackImporter.Themes)
            {
                foreach (string form in Forms)
                {
                    RewritePrefab($"{theme}_{form}", meshes[form], materials[theme], sideMaterial);
                    RewritePrefab($"{theme}_{form}_low", meshes[form + "_low"], materials[theme], sideMaterial);
                }
            }

            AssetDatabase.SaveAssets();
            MoyvaAtlasPackImporter.NormalizeDeterministicGuids();
            return meshes.Count;
        }

        // ---------------------------------------------------------------
        // Source loading
        // ---------------------------------------------------------------

        private static void PrepareFbxImporter()
        {
            if (!(AssetImporter.GetAtPath(SourceFbxPath) is ModelImporter importer))
            {
                AssetDatabase.ImportAsset(SourceFbxPath, ImportAssetOptions.ForceSynchronousImport);
                importer = AssetImporter.GetAtPath(SourceFbxPath) as ModelImporter;
            }

            if (importer == null)
                throw new FileNotFoundException("ModelImporter unavailable for " + SourceFbxPath);

            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        private sealed class SourceMesh
        {
            public Mesh mesh;
            public Matrix4x4 bakeMatrix;
        }

        /// <summary>
        /// Loads the beveled OBJ set. Each file is authored in meters with the
        /// pivot at the footprint centre and Y up, so the bake matrix is the
        /// identity — only the OBJ importer conversion has to be neutralised
        /// (readable mesh, authored normals kept for the winding check, no
        /// generated materials).
        /// </summary>
        private static Dictionary<string, SourceMesh> LoadObjSourceMeshes()
        {
            var result = new Dictionary<string, SourceMesh>(StringComparer.Ordinal);
            foreach (string path in Directory.GetFiles(SourceObjFolder, "*.obj"))
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

                string key = Path.GetFileNameWithoutExtension(path);
                result[key] = new SourceMesh { mesh = mesh, bakeMatrix = Matrix4x4.identity };
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
        /// Maps model node names to their imported mesh plus a rotation/scale
        /// bake matrix. Translation is dropped: authored pivots are already the
        /// quad centres; scene placement is irrelevant.
        /// </summary>
        private static Dictionary<string, SourceMesh> LoadFbxSourceMeshes()
        {
            PrepareFbxImporter();
            GameObject root = AssetDatabase.LoadMainAssetAtPath(SourceFbxPath) as GameObject;
            if (root == null)
                throw new InvalidDataException("FBX main asset is not a GameObject: " + SourceFbxPath);

            var result = new Dictionary<string, SourceMesh>(StringComparer.Ordinal);
            foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null)
                    continue;

                Matrix4x4 m = mf.transform.localToWorldMatrix;
                m.m03 = 0f; m.m13 = 0f; m.m23 = 0f;
                result[mf.name] = new SourceMesh { mesh = mf.sharedMesh, bakeMatrix = m };
            }
            return result;
        }

        // ---------------------------------------------------------------
        // Normalization
        // ---------------------------------------------------------------

        private sealed class MeshData
        {
            public Vector3[] verts;
            public Vector3[] normals;
            public int[] triangles;
            public int yaw;

            public float MaxExtentXZ()
            {
                float minX = float.MaxValue, maxX = float.MinValue;
                float minZ = float.MaxValue, maxZ = float.MinValue;
                foreach (Vector3 v in verts)
                {
                    if (v.x < minX) minX = v.x;
                    if (v.x > maxX) maxX = v.x;
                    if (v.z < minZ) minZ = v.z;
                    if (v.z > maxZ) maxZ = v.z;
                }
                return Mathf.Max(maxX - minX, maxZ - minZ);
            }
        }

        private static MeshData Bake(SourceMesh source)
        {
            Vector3[] verts = source.mesh.vertices;
            var baked = new Vector3[verts.Length];
            for (int i = 0; i < verts.Length; i++)
                baked[i] = source.bakeMatrix.MultiplyPoint3x4(verts[i]);

            Vector3[] srcNormals = source.mesh.normals;
            var bakedNormals = new Vector3[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 n = i < srcNormals.Length ? srcNormals[i] : Vector3.up;
                bakedNormals[i] = source.bakeMatrix.MultiplyVector(n).normalized;
            }

            var data = new MeshData
            {
                verts = baked,
                normals = bakedNormals,
                triangles = source.mesh.triangles,
            };
            FixWinding(data);
            EnsureUpAxis(data);
            return data;
        }

        /// <summary>
        /// OBJ/FBX handedness conversion may reverse authored winding, which
        /// would leave every computed face normal pointing inward. Authored
        /// normals are the ground truth: flip any triangle whose geometric
        /// normal disagrees with its authored vertex normal.
        /// </summary>
        private static void FixWinding(MeshData data)
        {
            for (int t = 0; t < data.triangles.Length; t += 3)
            {
                int ia = data.triangles[t];
                int ib = data.triangles[t + 1];
                int ic = data.triangles[t + 2];
                Vector3 n = Vector3.Cross(
                    data.verts[ib] - data.verts[ia],
                    data.verts[ic] - data.verts[ia]);
                if (n.sqrMagnitude < 1e-12f)
                    continue;

                if (Vector3.Dot(n, data.normals[ia]) >= 0f)
                    continue;

                data.triangles[t + 1] = ic;
                data.triangles[t + 2] = ib;
            }
        }

        /// <summary>
        /// The plateau must face +Y. If the imported orientation leaves the
        /// dominant upward mass on a Z axis (mesh-space axis not yet baked by
        /// the model node rotation), rotate the data into Y-up.
        /// </summary>
        private static void EnsureUpAxis(MeshData data)
        {
            float[] up = { 0f, 0f, 0f }; // +Y, +Z, -Z scores
            for (int t = 0; t < data.triangles.Length; t += 3)
            {
                Vector3 a = data.verts[data.triangles[t]];
                Vector3 b = data.verts[data.triangles[t + 1]];
                Vector3 c = data.verts[data.triangles[t + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a);
                float area = n.magnitude;
                if (area < 1e-8f)
                    continue;
                n /= area;
                if (n.y > 0.5f) up[0] += area;
                else if (n.z > 0.5f) up[1] += area;
                else if (n.z < -0.5f) up[2] += area;
            }

            if (up[0] >= up[1] && up[0] >= up[2])
                return;

            bool flipZ = up[2] > up[1];
            for (int i = 0; i < data.verts.Length; i++)
            {
                Vector3 v = data.verts[i];
                data.verts[i] = flipZ
                    ? new Vector3(v.x, -v.z, v.y)
                    : new Vector3(v.x, v.z, -v.y);
                Vector3 n = data.normals[i];
                data.normals[i] = flipZ
                    ? new Vector3(n.x, -n.z, n.y)
                    : new Vector3(n.x, n.z, -n.y);
            }
        }

        /// <summary>
        /// Finds the yaw quarter-turn that moves the plateau coverage onto the
        /// canonical corner mask for the form, scored by per-quadrant top area
        /// share against the canonical quadrant distribution.
        /// </summary>
        private static int ResolveCanonicalYaw(string form, MeshData data)
        {
            int canonical = CanonicalCoverage[form];
            if (canonical == (Sw | Se | Ne | Nw))
                return 0;

            float[] area = QuadrantTopAreas(data);
            float total = area[0] + area[1] + area[2] + area[3];
            if (total < 1e-8f)
                return 0;

            int expectedCount = 0;
            for (int q = 0; q < 4; q++)
                if ((canonical & (1 << q)) != 0)
                    expectedCount++;
            float expectedShare = 1f / expectedCount;

            int bestYaw = 0;
            float bestScore = float.MaxValue;
            for (int yaw = 0; yaw < 4; yaw++)
            {
                float score = 0f;
                for (int q = 0; q < 4; q++)
                {
                    // A feature at quadrant i lands at RotateQuadrantIndex(i,yaw),
                    // so the coverage ending at q comes from the pre-image.
                    float share = area[RotateQuadrantIndex(q, 4 - yaw)] / total;
                    float expected = (canonical & (1 << q)) != 0 ? expectedShare : 0f;
                    score += Mathf.Abs(share - expected);
                }

                if (score < bestScore - 1e-5f)
                {
                    bestScore = score;
                    bestYaw = yaw;
                }
            }

            Debug.Log($"[MoyvaCliffTiles] {form}: canonical yaw {bestYaw * 90} " +
                      $"(shares {area[0] / total:F2}/{area[1] / total:F2}/" +
                      $"{area[2] / total:F2}/{area[3] / total:F2}, score {bestScore:F3})");
            return bestYaw;
        }

        /// <summary>
        /// Plateau-top triangle area per quadrant, indexed SW,SE,NE,NW.
        /// Only truly horizontal faces at the highest top level count — the
        /// bevel ring and the low ground plane are also up-facing and would
        /// otherwise drown out the plateau distribution.
        /// </summary>
        private static float[] QuadrantTopAreas(MeshData data)
        {
            float topY = float.MinValue;
            for (int t = 0; t < data.triangles.Length; t += 3)
            {
                Vector3 a = data.verts[data.triangles[t]];
                Vector3 b = data.verts[data.triangles[t + 1]];
                Vector3 c = data.verts[data.triangles[t + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a);
                if (n.magnitude < 1e-8f || n.y / n.magnitude < FlatTopNormalY)
                    continue;
                float y = (a.y + b.y + c.y) / 3f;
                if (y > topY)
                    topY = y;
            }

            var area = new float[4];
            for (int t = 0; t < data.triangles.Length; t += 3)
            {
                Vector3 a = data.verts[data.triangles[t]];
                Vector3 b = data.verts[data.triangles[t + 1]];
                Vector3 c = data.verts[data.triangles[t + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a);
                float doubleArea = n.magnitude;
                if (doubleArea < 1e-8f || n.y / doubleArea < FlatTopNormalY)
                    continue;

                Vector3 centroid = (a + b + c) / 3f;
                if (centroid.y < topY - 0.01f)
                    continue;
                area[QuadrantIndex(centroid)] += doubleArea * 0.5f;
            }
            return area;
        }

        private static int QuadrantIndex(Vector3 p)
        {
            if (p.z < 0f)
                return p.x < 0f ? 0 : 1; // SW, SE
            return p.x < 0f ? 3 : 2;     // NW, NE
        }

        /// <summary>Index order SW=0, SE=1, NE=2, NW=3; +90 yaw: SW-&gt;NW.</summary>
        private static int RotateQuadrantIndex(int index, int yaw)
        {
            // Unity +90 yaw: SW(-,-) -> NW(-,+); index path SW->NW->NE->SE->SW.
            int result = index;
            for (int i = 0; i < ((yaw % 4) + 4) % 4; i++)
            {
                result = result switch
                {
                    0 => 3,
                    3 => 2,
                    2 => 1,
                    1 => 0,
                    _ => result,
                };
            }
            return result;
        }

        // ---------------------------------------------------------------
        // Emission
        // ---------------------------------------------------------------

        private static Mesh EmitMesh(string form, MeshData data, float scale, bool low)
        {
            string name = $"cliff_{form}{(low ? "_low" : string.Empty)}";
            float drop = low ? LowDropMeters : HighDropMeters;

            Quaternion yaw = Quaternion.Euler(0f, data.yaw * 90f, 0f);
            var scaled = new Vector3[data.verts.Length];
            float top = float.MinValue;
            for (int i = 0; i < data.verts.Length; i++)
            {
                Vector3 v = yaw * data.verts[i];
                v *= scale;
                scaled[i] = v;
                if (v.y > top)
                    top = v.y;
            }

            for (int i = 0; i < scaled.Length; i++)
            {
                Vector3 v = scaled[i];
                v.y -= top;
                if (v.y < -drop)
                    v.y = -drop;
                scaled[i] = v;
            }

            int triCount = data.triangles.Length;
            // Submesh 0 = tops + bevels (theme material, planar UVs);
            // submesh 1 = vertical walls and undersides (earth material,
            // box-projected UVs). The split keeps the authored bevel band
            // grass-coloured while cliff sides get their own material.
            var positions = new List<Vector3>(triCount);
            var normals = new List<Vector3>(triCount);
            var uv = new List<Vector2>(triCount);
            var surfaceIndices = new List<int>();
            var sideIndices = new List<int>();

            for (int t = 0; t < triCount; t += 3)
            {
                Vector3 a = scaled[data.triangles[t]];
                Vector3 b = scaled[data.triangles[t + 1]];
                Vector3 c = scaled[data.triangles[t + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a);
                n = n.sqrMagnitude < 1e-12f ? Vector3.up : n.normalized;

                var bucket = n.y > TopNormalY ? surfaceIndices : sideIndices;
                for (int k = 0; k < 3; k++)
                {
                    Vector3 p = scaled[data.triangles[t + k]];
                    positions.Add(p);
                    normals.Add(n);
                    uv.Add(n.y > TopNormalY || n.y < -TopNormalY
                        ? new Vector2(p.x + 0.5f, p.z + 0.5f)
                        : WallUv(p, n, drop));
                    bucket.Add(positions.Count - 1);
                }
            }

            string path = $"{MoyvaAtlasPackImporter.CliffMeshesFolder}/{name}.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool created = mesh == null;
            if (created)
                mesh = new Mesh();
            else
                mesh.Clear();

            mesh.name = name;
            mesh.vertices = positions.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uv.ToArray();
            mesh.subMeshCount = 2;
            mesh.SetTriangles(surfaceIndices, 0);
            mesh.SetTriangles(sideIndices, 1);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            if (created)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return AssetDatabase.LoadAssetAtPath<Mesh>(path);
            }

            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static Vector2 WallUv(Vector3 p, Vector3 n, float drop)
        {
            float u = Mathf.Abs(n.x) >= Mathf.Abs(n.z) ? p.z + 0.5f : p.x + 0.5f;
            float v = Mathf.Clamp01(-p.y / drop);
            return new Vector2(u, v);
        }

        private static Material CreateOrUpdateThemeMaterial(string theme)
        {
            string path = $"{MoyvaAtlasPackImporter.CliffMaterialsFolder}/moyva_theme_{theme}.mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("URP Lit or Standard shader is required.");

            Texture2D albedo = PrepareThemeTexture(theme);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool created = material == null;
            if (created)
                material = new Material(shader);

            material.shader = shader;
            material.name = $"moyva_theme_{theme}";
            material.enableInstancing = true;
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", albedo);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", albedo);
            if (material.HasProperty("_BumpMap")) material.SetTexture("_BumpMap", null);
            if (material.HasProperty("_MetallicGlossMap")) material.SetTexture("_MetallicGlossMap", null);
            material.DisableKeyword("_NORMALMAP");
            material.DisableKeyword("_METALLICGLOSSMAP");
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.12f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);

            if (created)
                AssetDatabase.CreateAsset(material, path);
            else
                EditorUtility.SetDirty(material);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        /// <summary>
        /// Shared material for the second submesh: vertical walls and
        /// undersides of every theme/form. Uses the earth albedo with the
        /// same shading fields as the theme materials so cliff sides read
        /// as soil/rock instead of the top texture stretched sideways.
        /// </summary>
        private static Material CreateOrUpdateSideMaterial()
        {
            const string path =
                MoyvaAtlasPackImporter.CliffMaterialsFolder + "/moyva_cliff_side.mat";
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("URP Lit or Standard shader is required.");

            Texture2D albedo = PrepareThemeTexture("dirt");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            bool created = material == null;
            if (created)
                material = new Material(shader);

            material.shader = shader;
            material.name = "moyva_cliff_side";
            material.enableInstancing = true;
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", albedo);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", albedo);
            if (material.HasProperty("_BumpMap")) material.SetTexture("_BumpMap", null);
            if (material.HasProperty("_MetallicGlossMap")) material.SetTexture("_MetallicGlossMap", null);
            material.DisableKeyword("_NORMALMAP");
            material.DisableKeyword("_METALLICGLOSSMAP");
            material.DisableKeyword("_METALLICSPECGLOSSMAP");
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.08f);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);

            if (created)
                AssetDatabase.CreateAsset(material, path);
            else
                EditorUtility.SetDirty(material);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        private static Texture2D PrepareThemeTexture(string theme)
        {
            string path = $"{SourcesFolder}/{ThemeTextures[theme]}";
            if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                throw new FileNotFoundException("Missing theme texture.", path);

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 4;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>
        /// Rewrites the generated prefab at its stable path: shared cliff mesh
        /// plus the theme material. Editing the loaded prefab contents keeps
        /// every serialized object id stable, so TilePreset references and the
        /// atlas-tile-set.json entries survive unchanged.
        /// </summary>
        private static void RewritePrefab(
            string name, Mesh mesh, Material material, Material sideMaterial)
        {
            string path = $"{MoyvaAtlasPackImporter.PrefabsFolder}/{name}.prefab";

            bool loaded = File.Exists(path);
            GameObject root = loaded
                ? PrefabUtility.LoadPrefabContents(path)
                : new GameObject(name);
            try
            {
                root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                root.transform.localScale = Vector3.one;

                var meshFilter = root.GetComponent<MeshFilter>()
                                 ?? root.AddComponent<MeshFilter>();
                var renderer = root.GetComponent<MeshRenderer>()
                               ?? root.AddComponent<MeshRenderer>();
                var collider = root.GetComponent<MeshCollider>()
                               ?? root.AddComponent<MeshCollider>();

                meshFilter.sharedMesh = mesh;
                renderer.sharedMaterials = new[] { material, sideMaterial };
                // Open surface collision is deliberate (matches atlas prefabs).
                collider.sharedMesh = mesh;

                GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, path);
                if (saved == null)
                    throw new IOException("Could not save prefab " + path);
            }
            finally
            {
                if (loaded)
                    PrefabUtility.UnloadPrefabContents(root);
                else
                    UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
