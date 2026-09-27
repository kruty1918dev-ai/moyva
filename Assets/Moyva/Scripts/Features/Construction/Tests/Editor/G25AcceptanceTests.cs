using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// G25 acceptance invariants for the wall/gate prototype meshes: every OBJ
    /// keeps a ground-center pivot inside a ~1×1 tile footprint, references
    /// exactly one material from gray_stone.mtl, ships valid UVs and face
    /// indices, Gate_Open leaves a walkable corridor through the wall line and
    /// Gate_Closed seals it.
    /// </summary>
    public sealed class G25AcceptanceTests
    {
        private static readonly string WallsDir =
            Path.Combine(Application.dataPath, "Moyva/Art/Models/Walls");

        private sealed class ObjModel
        {
            public readonly List<Vector3> Vertices = new();
            public readonly List<Vector2> Uvs = new();
            public readonly List<int[]> Faces = new();
            public string MaterialLib;
            public string UsedMaterial;
        }

        private static ObjModel Parse(string fileName)
        {
            var model = new ObjModel();
            string path = Path.Combine(WallsDir, fileName);
            Assert.IsTrue(File.Exists(path), $"missing model {fileName}");

            foreach (string raw in File.ReadAllLines(path))
            {
                string line = raw.Trim();
                if (line.StartsWith("v "))
                {
                    string[] p = line.Split(' ');
                    model.Vertices.Add(new Vector3(
                        float.Parse(p[1], CultureInfo.InvariantCulture),
                        float.Parse(p[2], CultureInfo.InvariantCulture),
                        float.Parse(p[3], CultureInfo.InvariantCulture)));
                }
                else if (line.StartsWith("vt "))
                {
                    string[] p = line.Split(' ');
                    model.Uvs.Add(new Vector2(
                        float.Parse(p[1], CultureInfo.InvariantCulture),
                        float.Parse(p[2], CultureInfo.InvariantCulture)));
                }
                else if (line.StartsWith("f "))
                {
                    string[] p = line.Split(' ');
                    var face = new int[p.Length - 1];
                    for (int i = 1; i < p.Length; i++)
                        face[i - 1] = int.Parse(p[i].Split('/')[0], CultureInfo.InvariantCulture);
                    model.Faces.Add(face);
                }
                else if (line.StartsWith("mtllib "))
                {
                    Assert.IsNull(model.MaterialLib, "only one material lib allowed");
                    model.MaterialLib = line.Substring(7).Trim();
                }
                else if (line.StartsWith("usemtl "))
                {
                    Assert.IsNull(model.UsedMaterial, "extra materials are not allowed");
                    model.UsedMaterial = line.Substring(7).Trim();
                }
            }
            return model;
        }

        private static void Bounds(ObjModel m, out Vector3 min, out Vector3 max)
        {
            min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var v in m.Vertices)
            {
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }
        }

        private static int CorridorBlockingFaces(ObjModel m, float corridorHalfWidth, float ceiling)
        {
            int count = 0;
            foreach (var face in m.Faces)
            {
                Vector3 c = Vector3.zero;
                foreach (int i in face)
                    c += m.Vertices[i - 1];
                c /= face.Length;
                // Corridor runs through the wall along Z centered on 0.
                if (Mathf.Abs(c.x) < corridorHalfWidth && c.y < ceiling && Mathf.Abs(c.z) < 0.2f)
                    count++;
            }
            return count;
        }

        [Test]
        public void AllModels_SingleGrayStoneMaterial_AndAtlasExists()
        {
            foreach (string name in new[]
                     { "Wall_Straight.obj", "Wall_Corner.obj", "Gate_Open.obj", "Gate_Closed.obj" })
            {
                ObjModel m = Parse(name);
                Assert.AreEqual("gray_stone.mtl", m.MaterialLib, name);
                Assert.AreEqual("gray_stone", m.UsedMaterial,
                    $"{name} must use a single shared material");
            }

            string mtl = File.ReadAllText(Path.Combine(WallsDir, "gray_stone.mtl"));
            StringAssert.Contains("newmtl gray_stone", mtl);
            StringAssert.Contains("map_Kd", mtl);
            Assert.IsTrue(
                File.Exists(Path.Combine(Application.dataPath,
                    "Moyva/Art/Textures/gray_stone_atlas.png")),
                "the material atlas texture must resolve");
        }

        [Test]
        public void AllModels_GroundPivot_AndFitTileFootprint()
        {
            foreach (string name in new[]
                     { "Wall_Straight.obj", "Wall_Corner.obj", "Gate_Open.obj", "Gate_Closed.obj" })
            {
                ObjModel m = Parse(name);
                Bounds(m, out Vector3 min, out Vector3 max);
                Assert.AreEqual(0f, min.y, 0.001f,
                    $"{name}: pivot must sit on the ground (minY=0)");
                Assert.LessOrEqual(Mathf.Abs(min.x), 0.56f, name);
                Assert.LessOrEqual(Mathf.Abs(max.x), 0.56f, name);
                Assert.LessOrEqual(Mathf.Abs(min.z), 0.56f, name);
                Assert.LessOrEqual(Mathf.Abs(max.z), 0.56f, name);
                Assert.Greater(max.y, 0.5f,
                    $"{name}: wall/gate must have visible height");
            }
        }

        [Test]
        public void AllModels_ValidFaces_AndUvs()
        {
            foreach (string name in new[]
                     { "Wall_Straight.obj", "Wall_Corner.obj", "Gate_Open.obj", "Gate_Closed.obj" })
            {
                ObjModel m = Parse(name);
                Assert.Greater(m.Vertices.Count, 0, name);
                Assert.Greater(m.Faces.Count, 0, name);
                Assert.Greater(m.Uvs.Count, 0,
                    $"{name}: faces reference uvs, so vt records must exist");
                foreach (var face in m.Faces)
                {
                    Assert.GreaterOrEqual(face.Length, 3,
                        $"{name}: degenerate face");
                    foreach (int i in face)
                        Assert.IsTrue(i >= 1 && i <= m.Vertices.Count,
                            $"{name}: face index {i} out of range");
                }
                foreach (var uv in m.Uvs)
                {
                    Assert.IsTrue(uv.x >= -0.001f && uv.x <= 1.001f
                                  && uv.y >= -0.001f && uv.y <= 1.001f,
                        $"{name}: uv {uv} escapes the atlas");
                }
            }
        }

        [Test]
        public void GateOpen_LeavesWalkableCorridor_GateClosed_Seals()
        {
            // Open state: nothing blocks the ground corridor between pillars.
            ObjModel open = Parse("Gate_Open.obj");
            Assert.AreEqual(0, CorridorBlockingFaces(open, 0.21f, 1.0f),
                "open gate must leave the passage corridor empty");

            // Closed state: leaves fill the opening, sealing the corridor.
            ObjModel closed = Parse("Gate_Closed.obj");
            Assert.Greater(CorridorBlockingFaces(closed, 0.21f, 1.0f), 0,
                "closed gate must block the passage corridor");
        }

        [Test]
        public void WallStraight_SpanMatchesTile_WallCorner_TurnsBothAxes()
        {
            ObjModel straight = Parse("Wall_Straight.obj");
            Bounds(straight, out Vector3 smin, out Vector3 smax);
            Assert.AreEqual(1.04f, smax.x - smin.x, 0.01f,
                "straight wall spans the tile along X (posts overlap neighbors)");
            Assert.Less(smax.z - smin.z, 0.45f,
                "straight wall stays thin along Z");

            ObjModel corner = Parse("Wall_Corner.obj");
            Bounds(corner, out Vector3 cmin, out Vector3 cmax);
            Assert.Greater(cmax.x - cmin.x, 0.9f, "corner extends along X");
            Assert.Greater(cmax.z - cmin.z, 0.9f, "corner extends along Z");
        }
    }
}
