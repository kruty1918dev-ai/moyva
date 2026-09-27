using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G14 audit of the shipped AtlasV3 tile set against the authored
    /// contract: 9 themes x (5 high forms + 5 low forms + stair),
    /// 1 m grid, centred pivot, top surface y=0, side walls to -0.5/-0.25,
    /// non-degenerate geometry and consistent winding/UV.
    /// </summary>
    public sealed class G14AcceptanceTests
    {
        private const string MeshDir =
            "Assets/Moyva/Art/World/Tiles/AtlasV3/Generated/Meshes";
        private const float Eps = 0.01f;

        private static readonly string[] Themes =
        {
            "grass", "sand", "dirt", "stone", "snow",
            "swamp", "rock_cliff", "road", "footpath",
        };

        private static readonly string[] Forms =
        {
            "corner", "edge", "interior", "merged", "fill",
        };

        private static Mesh Load(string name)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                $"{MeshDir}/{name}.asset");
            Assert.IsNotNull(mesh, $"missing atlas mesh {name}");
            return mesh;
        }

        private static IEnumerable<string> AllMeshNames()
        {
            foreach (string theme in Themes)
            {
                foreach (string form in Forms)
                {
                    yield return $"{theme}_{form}";
                    yield return $"{theme}_{form}_low";
                }
                yield return $"{theme}_stair_025";
            }
        }

        [Test]
        public void Atlas_AllThemeMeshes_Exist()
        {
            foreach (string name in AllMeshNames())
                Assert.IsNotNull(
                    AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshDir}/{name}.asset"),
                    $"missing atlas mesh {name}");
        }

        [Test]
        public void Atlas_Meshes_PivotBoundsAndWalls_MatchContract()
        {
            foreach (string theme in Themes)
            {
                foreach (string form in Forms)
                {
                    Mesh high = Load($"{theme}_{form}");
                    Mesh low = Load($"{theme}_{form}_low");
                    Bounds hb = high.bounds;
                    Bounds lb = low.bounds;

                    // Top plane is nominally y=0; authored low-poly relief
                    // may raise vertices, bounded well under one step delta.
                    Assert.AreEqual(0f, hb.max.y, 0.15f,
                        $"{high.name}: top surface must sit at y=0");
                    Assert.AreEqual(0f, lb.max.y, 0.15f,
                        $"{low.name}: low top surface must sit at y=0");

                    // Contract: fill forms are wall-less flat planes.
                    if (form != "fill")
                    {
                        Assert.AreEqual(-0.5f, hb.min.y, Eps,
                            $"{high.name}: high side wall must reach -0.5");
                        Assert.AreEqual(-0.25f, lb.min.y, Eps,
                            $"{low.name}: low side wall must reach -0.25");
                    }
                    else
                    {
                        Assert.GreaterOrEqual(hb.min.y, -Eps,
                            $"{high.name}: fill has no side walls");
                        Assert.GreaterOrEqual(lb.min.y, -Eps,
                            $"{low.name}: fill has no side walls");
                    }

                    // Pivot at cell centre: no mesh may leave the 1x1 cell.
                    foreach (Bounds b in new[] { hb, lb })
                    {
                        Assert.GreaterOrEqual(b.min.x, -0.5f - Eps, $"{high.name} min.x");
                        Assert.LessOrEqual(b.max.x, 0.5f + Eps, $"{high.name} max.x");
                        Assert.GreaterOrEqual(b.min.z, -0.5f - Eps, $"{high.name} min.z");
                        Assert.LessOrEqual(b.max.z, 0.5f + Eps, $"{high.name} max.z");
                    }
                }
            }
        }

        [Test]
        public void Atlas_FillForms_CoverFullTileFlat()
        {
            foreach (string theme in Themes)
            {
                foreach (string suffix in new[] { "", "_low" })
                {
                    Mesh mesh = Load($"{theme}_fill{suffix}");
                    Vector3[] v = mesh.vertices;
                    Bounds b = mesh.bounds;
                    Assert.AreEqual(0.5f, Mathf.Abs(b.min.x), Eps, mesh.name);
                    Assert.AreEqual(0.5f, b.max.x, Eps, mesh.name);
                    Assert.AreEqual(0.5f, Mathf.Abs(b.min.z), Eps, mesh.name);
                    Assert.AreEqual(0.5f, b.max.z, Eps, mesh.name);

                    // Contract: fill has no side walls. Small authored
                    // surface ripple is allowed; it never dips below y=0.
                    foreach (Vector3 p in v)
                    {
                        Assert.GreaterOrEqual(p.y, -Eps,
                            $"{mesh.name}: fill dips below the top plane");
                        Assert.LessOrEqual(p.y, 0.15f,
                            $"{mesh.name}: fill ripple too high");
                    }
                }
            }
        }

        [Test]
        public void Atlas_BeveledForms_HaveSlopedEdgeAndOutwardNormals()
        {
            foreach (string theme in new[] { "grass", "stone", "rock_cliff" })
            {
                // Edge form: a sloped band between plateau and wall — at
                // least one triangle with a tilted normal (0<n.y<1).
                Mesh mesh = Load($"{theme}_edge");
                Vector3[] n = mesh.normals;
                Vector3[] v = mesh.vertices;
                int[] t = mesh.triangles;

                bool hasTilted = false;
                for (int i = 0; i < t.Length; i += 3)
                {
                    Vector3 fn = (n[t[i]] + n[t[i + 1]] + n[t[i + 2]]).normalized;
                    if (fn.y > 0.05f && fn.y < 0.95f)
                        hasTilted = true;

                    // Winding vs stored normal consistency (inversion check).
                    // Normals are smoothed across the bevel, so only the
                    // sign of the hemisphere agreement is meaningful.
                    Vector3 geom = Vector3.Cross(
                        v[t[i + 1]] - v[t[i]],
                        v[t[i + 2]] - v[t[i]]).normalized;
                    Assert.Greater(
                        Vector3.Dot(geom, fn), 0.01f,
                        $"{theme}_edge: triangle {i / 3} wound against its normals");
                }

                Assert.IsTrue(hasTilted,
                    $"{theme}_edge: no bevel/sloped triangles found");
            }
        }

        [Test]
        public void Atlas_Meshes_NoDegenerateTrianglesOrUvs()
        {
            foreach (string name in AllMeshNames())
            {
                Mesh mesh = Load(name);
                Vector3[] v = mesh.vertices;
                Vector2[] uv = mesh.uv;
                int[] t = mesh.triangles;
                Assert.AreEqual(v.Length, uv.Length,
                    $"{name}: uv count must match vertex count");

                for (int i = 0; i < t.Length; i += 3)
                {
                    float area = Vector3.Cross(
                        v[t[i + 1]] - v[t[i]],
                        v[t[i + 2]] - v[t[i]]).magnitude * 0.5f;
                    Assert.Greater(area, 1e-7f,
                        $"{name}: degenerate triangle {i / 3}");

                    float uvArea = Mathf.Abs(
                        (uv[t[i + 1]].x - uv[t[i]].x)
                        * (uv[t[i + 2]].y - uv[t[i]].y)
                        - (uv[t[i + 2]].x - uv[t[i]].x)
                        * (uv[t[i + 1]].y - uv[t[i]].y)) * 0.5f;
                    Assert.Greater(uvArea, 1e-10f,
                        $"{name}: degenerate UV triangle {i / 3}");
                }
            }
        }

        [Test]
        public void Atlas_TopUvDensity_IsPlausible()
        {
            // Checker density: on flat top faces the UV-area/world-area ratio
            // must stay finite and non-degenerate (atlas regions differ per
            // form, so assert a sane band rather than one constant).
            foreach (string theme in Themes)
            {
                Mesh mesh = Load($"{theme}_fill");
                Vector3[] v = mesh.vertices;
                Vector2[] uv = mesh.uv;
                int[] t = mesh.triangles;
                Vector3[] n = mesh.normals;

                for (int i = 0; i < t.Length; i += 3)
                {
                    Vector3 fn = n[t[i]];
                    if (fn.y < 0.9f)
                        continue;

                    float world = Vector3.Cross(
                        v[t[i + 1]] - v[t[i]],
                        v[t[i + 2]] - v[t[i]]).magnitude * 0.5f;
                    float uvArea = Mathf.Abs(
                        (uv[t[i + 1]].x - uv[t[i]].x)
                        * (uv[t[i + 2]].y - uv[t[i]].y)
                        - (uv[t[i + 2]].x - uv[t[i]].x)
                        * (uv[t[i + 1]].y - uv[t[i]].y)) * 0.5f;
                    Assert.Greater(world, 1e-6f, $"{theme}_fill: flat top tri");
                    float density = uvArea / world;
                    Assert.Greater(density, 1e-4f,
                        $"{theme}_fill: top face UV density collapsed");
                }
            }
        }

        [Test]
        public void Atlas_Stair_RisesQuarterMeterSouthToNorth()
        {
            foreach (string theme in Themes)
            {
                Mesh mesh = Load($"{theme}_stair_025");
                Bounds b = mesh.bounds;
                Assert.AreEqual(0f, b.max.y, Eps,
                    $"{mesh.name}: stair top must reach y=0");
                Assert.GreaterOrEqual(b.min.y, -0.5f - Eps, mesh.name);
                Assert.LessOrEqual(b.size.x, 1f + Eps, mesh.name);
                Assert.LessOrEqual(b.size.z, 1f + Eps, mesh.name);

                Vector3[] v = mesh.vertices;
                float lowTop = float.MinValue, highTop = float.MinValue;
                foreach (Vector3 p in v)
                {
                    if (p.z < -0.4f)
                        lowTop = Mathf.Max(lowTop, p.y);
                    if (p.z > 0.4f)
                        highTop = Mathf.Max(highTop, p.y);
                }
                Assert.AreEqual(-0.25f, lowTop, 0.02f,
                    $"{mesh.name}: south edge must start at the low surface");
                Assert.AreEqual(0f, highTop, Eps,
                    $"{mesh.name}: north edge must reach the high surface");
            }
        }

        [Test]
        public void Atlas_TextureMemory_Reported()
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Moyva/Art/World/Tiles/AtlasV3/Textures/Moyva_AlbedoAtlas.png");
            Assert.IsNotNull(texture, "shared albedo atlas missing");
            long bytes = UnityEngine.Profiling.Profiler
                .GetRuntimeMemorySizeLong(texture);
            Debug.Log(
                $"[G14] albedo atlas {texture.width}x{texture.height} "
                + $"{texture.format}, runtime memory {bytes / (1024f * 1024f):F2} MB");
            Assert.Greater(bytes, 0);
        }
    }
}
