using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Builds the generated low-poly vegetation set under
    /// <c>Assets/Moyva/Generated/Vegetation</c>: a shared palette texture,
    /// procedural meshes (grass tufts, sedges, ferns, litter, saplings,
    /// logs, pebbles), card prefabs using the authored bush/grass PNGs, and
    /// per-variant materials on <c>Moyva/3D/Decor Shared Stylized</c>.
    ///
    /// All asset GUIDs are deterministic (md5 of the asset path via
    /// <see cref="MoyvaAtlasPackImporter.DeterministicGuid"/>), so JSON
    /// <c>$asset</c> keys in the map-object registry stay stable across
    /// machines and rebuilds. Re-running updates content in place.
    /// </summary>
    public static class MoyvaVegetationAssetBuilder
    {
        public const string OutputRoot = "Assets/Moyva/Generated/Vegetation";
        public const string MeshesFolder = OutputRoot + "/Meshes";
        public const string MaterialsFolder = OutputRoot + "/Materials";
        public const string PrefabsFolder = OutputRoot + "/Prefabs";
        public const string TexturesFolder = OutputRoot + "/Textures";
        public const string PalettePath = TexturesFolder + "/VegPalette.png";

        private const string DecorShaderName = "Moyva/3D/Decor Shared Stylized";
        private const string BarkTexturePath =
            "Assets/Moyva/Art/World/Tiles/AtlasV3/Textures/Sources/T_MOYVA_Bark_BaseColor.png";

        private const string GrassFolder = "Assets/Moyva/Generated/Grass";
        private const string GrassTexturePath = GrassFolder + "/ChatGPT Image 17 черв. 2026 р., 23_15_19(1).png";

        /// <summary>
        /// Registry id -> generated prefab name. Consumed when authoring
        /// mapobjectregistry.json + the runtime asset catalog.
        /// </summary>
        public static readonly string[] RegistryIds =
        {
            // grass cards (existing grass texture, new silhouette meshes)
            "veg-grass-cross-a", "veg-grass-tri-a", "veg-grass-tall-a",
            "veg-grass-clump-a", "veg-grass-tall-b", "veg-grass-clump-b",
            // bushes from authored cards
            "veg-bush-a", "veg-bush-b", "veg-bush-c", "veg-bush-d", "veg-bush-hedge",
            // procedural undergrowth
            "veg-fern-a", "veg-fern-b",
            "veg-sedge-a", "veg-sedge-b",
            "veg-flower-a", "veg-flower-b",
            "veg-twig-a", "veg-twig-b",
            "veg-leafpatch-a", "veg-leafpatch-b", "veg-moss",
            "veg-log", "veg-pebble",
            "veg-sapling-a", "veg-sapling-b", "veg-sapling-c"
        };

        [MenuItem("Moyva/Vegetation/Rebuild Generated Assets")]
        public static void BuildFromMenu() => BuildAll();

        /// <summary>Entry point usable from -executeMethod. Returns prefab count.</summary>
        public static int BuildAll()
        {
            EnsureFolder(OutputRoot);
            EnsureFolder(MeshesFolder);
            EnsureFolder(MaterialsFolder);
            EnsureFolder(PrefabsFolder);
            EnsureFolder(TexturesFolder);

            Texture2D palette = BuildPalette();

            Material flat = BuildFlatMaterial(palette);
            Material bark = BuildBarkMaterial(LoadTexture(BarkTexturePath));
            Material grassCardA = BuildCardMaterial("VegGrass_A", LoadTexture(GrassTexturePath), billboard: false, greenTint: true);
            Material grassCardB = BuildCardMaterial("VegGrass_B", LoadTexture(GrassTexturePath), billboard: false, greenTint: true);
            Material[] bushMats =
            {
                BuildCardMaterial("VegBush_A", LoadTexture(GrassFolder + "/bush_001.png"), billboard: false, greenTint: false),
                BuildCardMaterial("VegBush_B", LoadTexture(GrassFolder + "/bush_002.png"), billboard: false, greenTint: false),
                BuildCardMaterial("VegBush_C", LoadTexture(GrassFolder + "/bush_003.png"), billboard: false, greenTint: false),
                BuildCardMaterial("VegBush_D", LoadTexture(GrassFolder + "/bush_004.png"), billboard: false, greenTint: false),
                BuildCardMaterial("VegBush_Hedge", LoadTexture(GrassFolder + "/bush_005.png"), billboard: false, greenTint: false),
            };

            int created = 0;
            created += BuildCardPrefabs(grassCardA, grassCardB);
            created += BuildBushPrefabs(bushMats);
            created += BuildFlatPrefabs(flat, bark);

            AssetDatabase.SaveAssets();
            NormalizeDeterministicGuids();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"[VegetationAssetBuilder] Built {created} prefabs under {OutputRoot}");
            return created;
        }

        // ------------------------------------------------------------------
        // Palette texture
        // ------------------------------------------------------------------

        // 4x4 muted low-poly palette; mesh UVs aim at cell centres.
        private static readonly Color32[] PaletteColors =
        {
            C(0x8C, 0xA6, 0x60), C(0x61, 0x80, 0x4D), C(0xB8, 0xC7, 0x73), C(0xBF, 0xB8, 0x6B),
            C(0x73, 0x54, 0x38), C(0x61, 0x47, 0x33), C(0x4D, 0x6B, 0x47), C(0x8C, 0x8D, 0x8A),
            C(0xEB, 0xE6, 0xD1), C(0xF2, 0xCC, 0x4D), C(0xD1, 0x59, 0x4D), C(0x9E, 0x80, 0xB8),
            C(0xD9, 0xC9, 0xA3), C(0x6B, 0x8C, 0x54), C(0xB3, 0x8C, 0x4D), C(0x52, 0x3D, 0x2B),
        };

        private static Color32 C(byte r, byte g, byte b) => new Color32(r, g, b, 255);

        // cell indices into the 4x4 palette
        private const int Leaf = 0, LeafDark = 1, LeafLight = 2, LitterYellow = 3;
        private const int Trunk = 4, Twig = 5, Moss = 6, Stone = 7;
        private const int FlWhite = 8, FlYellow = 9, FlRed = 10, FlViolet = 11;
        private const int Sand = 12, Reed = 13, LitterOrange = 14, BarkDark = 15;

        private static Texture2D BuildPalette()
        {
            const int size = 256, cell = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int cx = Mathf.Clamp(x / cell, 0, 3);
                int cy = Mathf.Clamp(y / cell, 0, 3);
                px[y * size + x] = PaletteColors[(3 - cy) * 4 + cx]; // row0 = uv v=1
            }
            tex.SetPixels32(px);
            tex.Apply();

            File.WriteAllBytes(PalettePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(PalettePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(PalettePath);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(PalettePath);
        }

        /// <summary>UV of a palette cell centre (cell = row-major 0..15).</summary>
        private static Vector2 PaletteUV(int cell)
        {
            int cx = cell % 4;
            int cy = cell / 4;
            // palette rows written top->bottom mapped to v 1->0
            return new Vector2((cx + 0.5f) / 4f, 1f - (cy + 0.5f) / 4f);
        }

        // ------------------------------------------------------------------
        // Materials
        // ------------------------------------------------------------------

        private static Material BuildFlatMaterial(Texture2D palette)
        {
            var mat = new Material(Shader.Find(DecorShaderName));
            mat.name = "VegFlat";
            mat.SetTexture("_BaseMap", palette);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_AlphaClipEnabled", 0f);
            mat.SetFloat("_BillboardEnabled", 0f);
            mat.SetFloat("_CullMode", 2f);
            mat.SetFloat("_TextureVolumeStrength", 0.15f);
            mat.SetFloat("_LeafPlaneShading", 0.3f);
            mat.SetFloat("_ContactBlobMode", 1f);
            mat.SetFloat("_ContactRadius", 0.35f);
            mat.SetFloat("_ContactProjectionScale", 1.2f);
            mat.enableInstancing = true;
            return CreateOrUpdateAsset(mat, MaterialsFolder + "/VegFlat.mat");
        }

        /// <summary>
        /// Real bark texture for wooden props (logs, twigs). The supplied
        /// base-color texture is sampled with grain-along-length UVs authored
        /// by PrismXBark/StripUv; other wood props keep the palette material.
        /// </summary>
        private static Material BuildBarkMaterial(Texture2D bark)
        {
            var mat = new Material(Shader.Find(DecorShaderName));
            mat.name = "VegBark";
            mat.SetTexture("_BaseMap", bark);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_AlphaClipEnabled", 0f);
            mat.SetFloat("_BillboardEnabled", 0f);
            mat.SetFloat("_CullMode", 2f);
            mat.SetFloat("_TextureVolumeStrength", 0.15f);
            mat.SetFloat("_LeafPlaneShading", 0.3f);
            mat.SetFloat("_ContactBlobMode", 1f);
            mat.SetFloat("_ContactRadius", 0.35f);
            mat.SetFloat("_ContactProjectionScale", 1.2f);
            mat.enableInstancing = true;
            return CreateOrUpdateAsset(mat, MaterialsFolder + "/VegBark.mat");
        }

        private static Material BuildCardMaterial(string name, Texture2D texture, bool billboard, bool greenTint)
        {
            var mat = new Material(Shader.Find(DecorShaderName));
            mat.name = name;
            mat.SetTexture("_BaseMap", texture);
            mat.SetColor("_BaseColor", greenTint ? new Color(0.72f, 0.92f, 0.62f) : Color.white);
            mat.SetFloat("_AlphaClipEnabled", 1f);
            mat.SetFloat("_AlphaClipThreshold", 0.35f);
            mat.SetFloat("_BillboardEnabled", billboard ? 1f : 0f);
            mat.SetFloat("_CullMode", 0f);
            mat.SetFloat("_ZWrite", 1f);
            mat.SetFloat("_AmbientStrength", 0.75f);
            mat.SetFloat("_MinimumBrightness", 0.85f);
            mat.SetFloat("_LeafPlaneShading", 1f);
            mat.SetFloat("_TextureVolumeStrength", 0.2f);
            mat.SetFloat("_ContactBlobMode", 1f);
            mat.SetFloat("_ContactRadius", 0.46f);
            mat.SetFloat("_ContactProjectionScale", 1.2f);
            mat.SetFloat("_OutlineEnabled", 1f);
            mat.SetFloat("_OutlineScreenWidthPx", 1f);
            mat.renderQueue = 2490;
            mat.enableInstancing = true;
            return CreateOrUpdateAsset(mat, MaterialsFolder + "/" + name + ".mat");
        }

        private static Texture2D LoadTexture(string path)
        {
            var t = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (t == null) throw new InvalidOperationException("Missing texture " + path);
            return t;
        }

        // ------------------------------------------------------------------
        // Prefab assembly
        // ------------------------------------------------------------------

        private static int BuildCardPrefabs(Material grassA, Material grassB)
        {
            int n = 0;
            // silhouettes x texture => distinct grass-clump variants
            n += CardPrefab("veg-grass-cross-a", BuildCrossMesh("veg_card_cross", 2, 0.66f, 0.56f, 0f), grassA, castShadows: false);
            n += CardPrefab("veg-grass-tri-a", BuildCrossMesh("veg_card_tri", 3, 0.72f, 0.58f, 0f), grassB, castShadows: false);
            n += CardPrefab("veg-grass-tall-a", BuildCrossMesh("veg_card_tall", 2, 0.50f, 0.84f, 0.14f), grassB, castShadows: false);
            n += CardPrefab("veg-grass-clump-a", BuildClumpMesh("veg_card_clump", 3, 1.10f, 0.44f), grassA, castShadows: false);
            n += CardPrefab("veg-grass-tall-b", BuildCrossMesh("veg_card_tall_b", 3, 0.58f, 0.76f, 0.10f), grassA, castShadows: false);
            n += CardPrefab("veg-grass-clump-b", BuildClumpMesh("veg_card_clump_b", 4, 1.25f, 0.40f), grassB, castShadows: false);
            return n;
        }

        private static int BuildBushPrefabs(Material[] mats)
        {
            int n = 0;
            string[] names = { "veg-bush-a", "veg-bush-b", "veg-bush-c", "veg-bush-d" };
            for (int i = 0; i < names.Length; i++)
            {
                n += CardPrefab(names[i],
                    BuildCrossMesh("veg_card_bush_" + (char)('a' + i), 2, 0.80f, 0.68f, 0f),
                    mats[i], castShadows: false);
            }
            // wide hedge strip (bush_005 is a ~3:1 silhouette)
            n += CardPrefab("veg-bush-hedge",
                BuildCrossMesh("veg_card_hedge", 2, 1.25f, 0.40f, 0f),
                mats[4], castShadows: false);
            return n;
        }

        private static int BuildFlatPrefabs(Material flat, Material bark)
        {
            int n = 0;
            n += FlatPrefab("veg-fern-a", BuildFernMesh("veg_fern_a", 5, 0.42f, 0.10f, upright: 0), flat, true);
            n += FlatPrefab("veg-fern-b", BuildFernMesh("veg_fern_b", 7, 0.50f, 0.09f, upright: 2), flat, true);
            n += FlatPrefab("veg-sedge-a", BuildSedgeMesh("veg_sedge_a", 6, 0.58f, 0.045f), flat, false);
            n += FlatPrefab("veg-sedge-b", BuildSedgeMesh("veg_sedge_b", 8, 0.46f, 0.04f), flat, false);
            n += FlatPrefab("veg-flower-a", BuildFlowerMesh("veg_flower_a", 4, FlYellow), flat, false);
            n += FlatPrefab("veg-flower-b", BuildFlowerMesh("veg_flower_b", 3, FlWhite), flat, false);
            n += FlatPrefab("veg-twig-a", BuildTwigMesh("veg_twig_a", false, barkUv: true), bark, false);
            n += FlatPrefab("veg-twig-b", BuildTwigMesh("veg_twig_b", true, barkUv: true), bark, false);
            n += FlatPrefab("veg-leafpatch-a", BuildDiscMesh("veg_leafpatch_a", 0.26f, 7, LitterYellow, 0.02f), flat, false);
            n += FlatPrefab("veg-leafpatch-b", BuildDiscMesh("veg_leafpatch_b", 0.22f, 6, LitterOrange, 0.02f), flat, false);
            n += FlatPrefab("veg-moss", BuildDiscMesh("veg_moss", 0.30f, 8, Moss, 0.025f), flat, false);
            n += FlatPrefab("veg-log", BuildLogMesh("veg_log", 0.72f, 0.09f, barkUv: true), bark, true);
            n += FlatPrefab("veg-pebble", BuildPebbleMesh("veg_pebble", 0.16f), flat, false);
            n += FlatPrefab("veg-sapling-a", BuildSaplingMesh("veg_sapling_a", 0.55f, 0.18f, Leaf, blobSquash: 0.75f), flat, true);
            n += FlatPrefab("veg-sapling-b", BuildSaplingMesh("veg_sapling_b", 0.70f, 0.22f, LeafDark, blobSquash: 1.1f), flat, true);
            n += FlatPrefab("veg-sapling-c", BuildSaplingMesh("veg_sapling_c", 0.48f, 0.15f, LeafLight, blobSquash: 0.85f), flat, true);
            return n;
        }

        private static int CardPrefab(string id, Mesh mesh, Material mat, bool castShadows)
            => WritePrefab(id, mesh, mat, castShadows);

        private static int FlatPrefab(string id, Mesh mesh, Material mat, bool castShadows)
            => WritePrefab(id, mesh, mat, castShadows);

        private static int WritePrefab(string id, Mesh mesh, Material mat, bool castShadows)
        {
            string path = PrefabsFolder + "/" + id + ".prefab";
            var go = new GameObject(id);
            try
            {
                var mf = go.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                var mr = go.AddComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                mr.shadowCastingMode = castShadows
                    ? UnityEngine.Rendering.ShadowCastingMode.On
                    : UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = true;
                var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (existing != null)
                {
                    PrefabUtility.SaveAsPrefabAsset(go, path);
                }
                else
                {
                    PrefabUtility.SaveAsPrefabAsset(go, path);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
            return 1;
        }

        private static T CreateOrUpdateAsset<T>(T asset, string path) where T : UnityEngine.Object
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(asset);
                return existing;
            }
            AssetDatabase.CreateAsset(asset, path);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        // ------------------------------------------------------------------
        // Mesh builders
        // ------------------------------------------------------------------

        private sealed class MB
        {
            public readonly List<Vector3> V = new List<Vector3>();
            public readonly List<Vector3> N = new List<Vector3>();
            public readonly List<Vector2> UV = new List<Vector2>();
            public readonly List<int> T = new List<int>();

            public int Vert(Vector3 p, Vector3 n, Vector2 uv)
            {
                V.Add(p); N.Add(n.normalized); UV.Add(uv);
                return V.Count - 1;
            }

            public void Tri(int a, int b, int c) { T.Add(a); T.Add(b); T.Add(c); }

            /// <summary>Vertical quad: origin at base-centre, yawed by angle.</summary>
            public void Card(float w, float h, float yawDeg, Vector3 offset, float leanDeg)
            {
                float yaw = yawDeg * Mathf.Deg2Rad;
                Vector3 right = new Vector3(Mathf.Cos(yaw), 0f, -Mathf.Sin(yaw)) * (w * 0.5f);
                Vector3 normal = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw));
                Vector3 up = Quaternion.AngleAxis(leanDeg, right.normalized) * Vector3.up;
                int i0 = Vert(offset - right, normal, new Vector2(0, 0));
                int i1 = Vert(offset + right, normal, new Vector2(1, 0));
                int i2 = Vert(offset + right + up * h, normal, new Vector2(1, 1));
                int i3 = Vert(offset - right + up * h, normal, new Vector2(0, 1));
                Tri(i0, i1, i2); Tri(i0, i2, i3);
            }

            /// <summary>Flat-coloured quad with uv on a palette cell.</summary>
            public void PQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, int cell)
            {
                Vector2 uv = PaletteUV(cell);
                Vector3 n = Vector3.Cross(b - a, d - a);
                int i0 = Vert(a, n, uv); int i1 = Vert(b, n, uv);
                int i2 = Vert(c, n, uv); int i3 = Vert(d, n, uv);
                Tri(i0, i1, i2); Tri(i0, i2, i3);
            }

            /// <summary>Tapered strip along points (blade/frond segment chain).</summary>
            public void Strip(Vector3[] points, float[] widths, int cell)
            {
                Vector2 uv = PaletteUV(cell);
                for (int i = 0; i < points.Length - 1; i++)
                {
                    Vector3 dir = (points[i + 1] - points[i]).normalized;
                    Vector3 side = Vector3.Cross(dir, Vector3.forward);
                    if (side.sqrMagnitude < 0.001f) side = Vector3.right;
                    side.Normalize();
                    float w0 = widths[i] * 0.5f, w1 = widths[i + 1] * 0.5f;
                    Vector3 n = Vector3.Cross(dir, side);
                    int i0 = Vert(points[i] - side * w0, n, uv);
                    int i1 = Vert(points[i] + side * w0, n, uv);
                    int i2 = Vert(points[i + 1] + side * w1, n, uv);
                    int i3 = Vert(points[i + 1] - side * w1, n, uv);
                    Tri(i0, i1, i2); Tri(i0, i2, i3);
                }
            }

            /// <summary>Flat irregular disc on the ground.</summary>
            public void Disc(float radius, int sides, int cell, float dome, int seed)
            {
                Vector2 uv = PaletteUV(cell);
                int center = Vert(new Vector3(0, dome, 0), Vector3.up, uv);
                var rng = new System.Random(seed);
                int[] ring = new int[sides];
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2f / sides;
                    float r = radius * (0.75f + 0.25f * (float)rng.NextDouble());
                    ring[i] = Vert(new Vector3(Mathf.Cos(a) * r, 0.005f, Mathf.Sin(a) * r), Vector3.up, uv);
                }
                for (int i = 0; i < sides; i++)
                    Tri(center, ring[i], ring[(i + 1) % sides]);
            }

            /// <summary>Squashed octahedron crown/rock blob with flat faces.</summary>
            public void Blob(Vector3 c, float rx, float ry, float rz, int cell)
            {
                Vector2 uv = PaletteUV(cell);
                Vector3[] p =
                {
                    c + new Vector3(rx, 0, 0), c + new Vector3(-rx, 0, 0),
                    c + new Vector3(0, ry, 0), c + new Vector3(0, -ry, 0),
                    c + new Vector3(0, 0, rx), c + new Vector3(0, 0, -rx),
                };
                int[][] f =
                {
                    new[] { 2, 0, 4 }, new[] { 2, 4, 1 }, new[] { 2, 1, 5 }, new[] { 2, 5, 0 },
                    new[] { 3, 4, 0 }, new[] { 3, 1, 4 }, new[] { 3, 5, 1 }, new[] { 3, 0, 5 },
                };
                foreach (var face in f)
                {
                    Vector3 n = Vector3.Cross(p[face[1]] - p[face[0]], p[face[2]] - p[face[0]]);
                    int i0 = Vert(p[face[0]], n, uv); int i1 = Vert(p[face[1]], n, uv); int i2 = Vert(p[face[2]], n, uv);
                    Tri(i0, i1, i2);
                }
            }

            /// <summary>Horizontal tapered prism (log/trunk segment along X).</summary>
            public void PrismX(Vector3 origin, float len, float r0, float r1, int sides, int cell, int capCell)
            {
                Vector2 uv = PaletteUV(cell), capUv = PaletteUV(capCell);
                var ringA = new int[sides]; var ringB = new int[sides];
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2f / sides + Mathf.PI / sides;
                    var dir = new Vector3(0, Mathf.Cos(a), Mathf.Sin(a));
                    ringA[i] = Vert(origin + dir * r0, dir, uv);
                    ringB[i] = Vert(origin + new Vector3(len, 0, 0) + dir * r1, dir, uv);
                }
                for (int i = 0; i < sides; i++)
                {
                    int j = (i + 1) % sides;
                    Tri(ringA[i], ringB[i], ringB[j]); Tri(ringA[i], ringB[j], ringA[j]);
                }
                // end caps
                int ca = Vert(origin, Vector3.left, capUv);
                int cb = Vert(origin + new Vector3(len, 0, 0), Vector3.right, capUv);
                for (int i = 0; i < sides; i++)
                {
                    int j = (i + 1) % sides;
                    Tri(ca, ringA[j], ringA[i]);
                    Tri(cb, ringB[i], ringB[j]);
                }
            }

            /// <summary>
            /// Horizontal tapered prism with real texture UVs: u wraps the
            /// circumference (seam-duplicated), v runs along the length so a
            /// bark texture's grain follows the trunk.
            /// </summary>
            public void PrismXBark(Vector3 origin, float len, float r0, float r1, int sides, int capCell)
            {
                Vector2 capUv = PaletteUV(capCell);
                var ringA = new int[sides + 1]; var ringB = new int[sides + 1];
                for (int i = 0; i <= sides; i++)
                {
                    float a = (i % sides) * Mathf.PI * 2f / sides + Mathf.PI / sides;
                    var dir = new Vector3(0, Mathf.Cos(a), Mathf.Sin(a));
                    float u = (float)i / sides;
                    ringA[i] = Vert(origin + dir * r0, dir, new Vector2(u, 0f));
                    ringB[i] = Vert(origin + new Vector3(len, 0, 0) + dir * r1, dir, new Vector2(u, 1f));
                }
                for (int i = 0; i < sides; i++)
                {
                    int j = i + 1;
                    Tri(ringA[i], ringB[i], ringB[j]); Tri(ringA[i], ringB[j], ringA[j]);
                }
                int ca = Vert(origin, Vector3.left, capUv);
                int cb = Vert(origin + new Vector3(len, 0, 0), Vector3.right, capUv);
                for (int i = 0; i < sides; i++)
                {
                    int j = (i + 1) % sides;
                    Tri(ca, ringA[j], ringA[i]);
                    Tri(cb, ringB[i], ringB[j]);
                }
            }

            /// <summary>Tapered strip with real UVs: u across width, v along length.</summary>
            public void StripUv(Vector3[] points, float[] widths)
            {
                float total = 0f;
                for (int i = 0; i + 1 < points.Length; i++)
                    total += (points[i + 1] - points[i]).magnitude;
                total = Mathf.Max(total, 0.0001f);
                float v0 = 0f;
                for (int i = 0; i + 1 < points.Length; i++)
                {
                    Vector3 dir = (points[i + 1] - points[i]).normalized;
                    Vector3 side = Vector3.Cross(dir, Vector3.forward);
                    if (side.sqrMagnitude < 0.001f) side = Vector3.right;
                    side.Normalize();
                    float w0 = widths[i] * 0.5f, w1 = widths[i + 1] * 0.5f;
                    float v1 = v0 + (points[i + 1] - points[i]).magnitude / total;
                    Vector3 n = Vector3.Cross(dir, side);
                    int i0 = Vert(points[i] - side * w0, n, new Vector2(0f, v0));
                    int i1 = Vert(points[i] + side * w0, n, new Vector2(1f, v0));
                    int i2 = Vert(points[i + 1] + side * w1, n, new Vector2(1f, v1));
                    int i3 = Vert(points[i + 1] - side * w1, n, new Vector2(0f, v1));
                    Tri(i0, i1, i2); Tri(i0, i2, i3);
                    v0 = v1;
                }
            }

            /// <summary>Vertical tapering prism (trunk).</summary>
            public void TrunkY(float h, float r0, float r1, int sides, int cell)
            {
                Vector2 uv = PaletteUV(cell);
                var ringA = new int[sides]; var ringB = new int[sides];
                for (int i = 0; i < sides; i++)
                {
                    float a = i * Mathf.PI * 2f / sides;
                    var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                    ringA[i] = Vert(dir * r0, dir, uv);
                    ringB[i] = Vert(dir * r1 + Vector3.up * h, dir, uv);
                }
                for (int i = 0; i < sides; i++)
                {
                    int j = (i + 1) % sides;
                    Tri(ringA[i], ringB[i], ringB[j]); Tri(ringA[i], ringB[j], ringA[j]);
                }
            }

            public Mesh Bake(string name)
            {
                var m = new Mesh { name = name };
                m.SetVertices(V);
                m.SetNormals(N);
                m.SetUVs(0, UV);
                m.SetTriangles(T, 0);
                m.RecalculateBounds();
                return CreateOrUpdateAsset(m, MeshesFolder + "/" + name + ".asset");
            }
        }

        private static Mesh BuildCrossMesh(string name, int planes, float w, float h, float leanDeg)
        {
            var mb = new MB();
            for (int i = 0; i < planes; i++)
                mb.Card(w, h, 180f / planes * i, Vector3.zero, leanDeg);
            return mb.Bake(name);
        }

        private static Mesh BuildClumpMesh(string name, int planes, float spread, float h)
        {
            var mb = new MB();
            mb.Card(spread * 0.55f, h, 0f, Vector3.zero, 0f);
            mb.Card(spread * 0.5f, h * 0.92f, 70f, new Vector3(-spread * 0.22f, 0, 0.05f), 6f);
            mb.Card(spread * 0.5f, h * 0.85f, -65f, new Vector3(spread * 0.22f, 0, -0.04f), -6f);
            if (planes > 3)
                mb.Card(spread * 0.4f, h * 0.7f, 30f, new Vector3(0, 0, spread * 0.18f), 4f);
            return mb.Bake(name);
        }

        private static Mesh BuildFernMesh(string name, int fronds, float len, float width, int upright)
        {
            var mb = new MB();
            int total = fronds + upright;
            for (int i = 0; i < total; i++)
            {
                float yaw = 360f / total * i + (i % 2) * 13f;
                bool up = i >= fronds;
                float l = up ? len * 0.8f : len;
                // arched frond: rises then droops outward
                var pts = new Vector3[4];
                var ws = new float[4];
                for (int s = 0; s < 4; s++)
                {
                    float t = s / 3f;
                    float arc = Mathf.Sin(t * Mathf.PI * (up ? 0.42f : 0.55f));
                    float reach = t * l * (up ? 0.35f : 1f);
                    pts[s] = new Vector3(0, arc * l * (up ? 1.0f : 0.75f) + t * l * (up ? 0.85f : 0.12f), reach);
                    ws[s] = width * (1f - t * 0.8f);
                }
                var rot = Quaternion.Euler(0, yaw, 0);
                for (int s = 0; s < 4; s++) pts[s] = rot * pts[s];
                mb.Strip(pts, ws, i % 3 == 0 ? LeafDark : Leaf);
            }
            return mb.Bake(name);
        }

        private static Mesh BuildSedgeMesh(string name, int blades, float h, float w)
        {
            var mb = new MB();
            var rng = new System.Random(name.GetHashCode());
            for (int i = 0; i < blades; i++)
            {
                float yaw = 360f / blades * i + (float)rng.NextDouble() * 25f;
                float lean = 6f + (float)rng.NextDouble() * 14f;
                float bh = h * (0.8f + 0.4f * (float)rng.NextDouble());
                var pts = new Vector3[3];
                var ws = new float[3];
                for (int s = 0; s < 3; s++)
                {
                    float t = s / 2f;
                    float bend = Mathf.Sin(t * Mathf.PI * 0.5f) * lean * 0.01f * bh * 60f;
                    pts[s] = new Vector3(bend, t * bh, 0);
                    ws[s] = w * (1f - t * 0.75f);
                }
                var rot = Quaternion.Euler(0, yaw, 0);
                for (int s = 0; s < 3; s++) pts[s] = rot * pts[s];
                mb.Strip(pts, ws, i % 2 == 0 ? Reed : Leaf);
            }
            return mb.Bake(name);
        }

        private static Mesh BuildFlowerMesh(string name, int stems, int petalCell)
        {
            var mb = new MB();
            // small ground tuft
            mb.Disc(0.14f, 6, LeafDark, 0.05f, name.GetHashCode());
            var rng = new System.Random(name.GetHashCode() + 1);
            for (int i = 0; i < stems; i++)
            {
                float yaw = 360f / stems * i + (float)rng.NextDouble() * 40f;
                var dir = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
                float h = 0.16f + 0.08f * (float)rng.NextDouble();
                Vector3 baseP = dir * 0.05f;
                Vector3 topP = dir * 0.09f + Vector3.up * h;
                mb.Strip(new[] { baseP, (baseP + topP) * 0.5f + dir * 0.01f, topP },
                    new[] { 0.02f, 0.018f, 0.012f }, Reed);
                // petal star: two crossed small quads at the top
                mb.PQuad(topP + new Vector3(-0.035f, 0.01f, 0), topP + new Vector3(0.035f, 0.01f, 0),
                    topP + new Vector3(0.035f, 0.05f, 0), topP + new Vector3(-0.035f, 0.05f, 0), petalCell);
                mb.PQuad(topP + new Vector3(0, 0.01f, -0.035f), topP + new Vector3(0, 0.01f, 0.035f),
                    topP + new Vector3(0, 0.05f, 0.035f), topP + new Vector3(0, 0.05f, -0.035f), petalCell);
            }
            return mb.Bake(name);
        }

        private static Mesh BuildTwigMesh(string name, bool forked, bool barkUv = false)
        {
            var mb = new MB();
            if (barkUv)
                mb.PrismXBark(new Vector3(-0.12f, 0.02f, 0), 0.22f, 0.014f, 0.009f, 4, Twig);
            else
                mb.PrismX(new Vector3(-0.12f, 0.02f, 0), 0.22f, 0.014f, 0.009f, 4, Twig, Twig);
            if (forked)
            {
                // second branch angled off
                var p0 = new Vector3(0.02f, 0.025f, 0f);
                var pts = new[]
                {
                    p0,
                    p0 + new Vector3(0.06f, 0.05f, 0.07f),
                    p0 + new Vector3(0.11f, 0.08f, 0.12f),
                };
                if (barkUv) mb.StripUv(pts, new[] { 0.02f, 0.014f, 0.006f });
                else mb.Strip(pts, new[] { 0.02f, 0.014f, 0.006f }, Twig);
            }
            return mb.Bake(name);
        }

        private static Mesh BuildDiscMesh(string name, float r, int sides, int cell, float dome)
        {
            var mb = new MB();
            mb.Disc(r, sides, cell, dome, name.GetHashCode());
            return mb.Bake(name);
        }

        private static Mesh BuildLogMesh(string name, float len, float r, bool barkUv = false)
        {
            var mb = new MB();
            var origin = new Vector3(-len * 0.5f, r * 0.9f, 0);
            if (barkUv)
            {
                mb.PrismXBark(origin, len, r, r * 0.85f, 6, Sand);
                mb.TrunkY(0.10f, 0.03f, 0.02f, 4, Twig);
            }
            else
            {
                mb.PrismX(origin, len, r, r * 0.85f, 6, Trunk, Sand);
                mb.TrunkY(0.10f, 0.03f, 0.02f, 4, Twig);
            }
            return mb.Bake(name);
        }

        private static Mesh BuildPebbleMesh(string name, float r)
        {
            var mb = new MB();
            mb.Blob(new Vector3(0, r * 0.55f, 0), r, r * 0.6f, r * 0.8f, Stone);
            mb.Blob(new Vector3(r * 0.7f, r * 0.4f, r * 0.25f), r * 0.6f, r * 0.45f, r * 0.55f, Stone);
            return mb.Bake(name);
        }

        private static Mesh BuildSaplingMesh(string name, float trunkH, float crownR, int crownCell, float blobSquash)
        {
            var mb = new MB();
            mb.TrunkY(trunkH, 0.022f, 0.014f, 5, Trunk);
            mb.Blob(new Vector3(0, trunkH + crownR * 0.55f, 0), crownR, crownR * blobSquash, crownR, crownCell);
            return mb.Bake(name);
        }

        // ------------------------------------------------------------------
        // Deterministic GUID normalization (mirrors MoyvaAtlasPackImporter)
        // ------------------------------------------------------------------

        private static void NormalizeDeterministicGuids()
        {
            var files = new List<string>();
            CollectGeneratedFiles(OutputRoot, files);

            var remap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in files)
            {
                string metaPath = path + ".meta";
                if (!File.Exists(metaPath)) continue;
                string meta = File.ReadAllText(metaPath);
                var match = System.Text.RegularExpressions.Regex.Match(meta, @"guid:\s*([0-9a-fA-F]{32})");
                if (!match.Success) continue;

                string wanted = MoyvaAtlasPackImporter.DeterministicGuid(path);
                string actual = match.Groups[1].Value;
                if (string.Equals(actual, wanted, StringComparison.OrdinalIgnoreCase)) continue;

                string claimed = AssetDatabase.GUIDToAssetPath(wanted);
                if (!string.IsNullOrEmpty(claimed) &&
                    !string.Equals(claimed, path, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Deterministic guid collision: {path} vs {claimed}.");
                }
                remap[actual] = wanted;
            }

            if (remap.Count == 0) return;
            foreach (string path in files)
            {
                PatchGuids(path, remap);
                PatchGuids(path + ".meta", remap);
            }
        }

        private static void PatchGuids(string file, Dictionary<string, string> remap)
        {
            if (!File.Exists(file)) return;
            string text = File.ReadAllText(file);
            string updated = text;
            foreach (var pair in remap)
                updated = updated.Replace(pair.Key, pair.Value);
            if (!ReferenceEquals(updated, text))
                File.WriteAllText(file, updated);
        }

        private static void CollectGeneratedFiles(string folder, List<string> results)
        {
            if (!Directory.Exists(folder)) return;
            foreach (string file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                results.Add(file.Replace('\\', '/'));
            }
            foreach (string dir in Directory.GetDirectories(folder))
                CollectGeneratedFiles(dir.Replace('\\', '/'), results);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
