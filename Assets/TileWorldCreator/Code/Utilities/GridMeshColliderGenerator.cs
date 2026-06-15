/*

  _____ _ _    __        __         _     _  ____                _             
 |_   _(_) | __\ \      / /__  _ __| | __| |/ ___|_ __ ___  __ _| |_ ___  _ __ 
   | | | | |/ _ \ \ /\ / / _ \| '__| |/ _` | |   | '__/ _ \/ _` | __/ _ \| '__|
   | | | | |  __/\ V  V / (_) | |  | | (_| | |___| | |  __/ (_| | || (_) | |   
   |_| |_|_|\___| \_/\_/ \___/|_|  |_|\__,_|\____|_|  \___|\__,_|\__\___/|_|   
                                                                               
	TileWorldCreator (c) by Giant Grey
	Author: Marc Egli

	www.giantgrey.com

*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GiantGrey.TileWorldCreator.Utilities
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
    public static class GridMeshGenerator
    {

        public static Mesh GenerateMesh(HashSet<Vector2> cellPositions, HashSet<Vector2> allCells, float cellSize, float height, float extrusionHeight, bool invertWalls, int paddingCells = 0)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            float insetAmount = 0.0f;

            if (cellPositions == null || cellPositions.Count == 0)
            {
                return mesh;
            }

            HashSet<Vector2> workingCells = cellPositions;
            HashSet<Vector2> workingAllCells = allCells ?? new HashSet<Vector2>();

            if (paddingCells > 0)
            {
                int minX = Mathf.FloorToInt(cellPositions.Min(c => c.x));
                int maxX = Mathf.CeilToInt(cellPositions.Max(c => c.x));
                int minY = Mathf.FloorToInt(cellPositions.Min(c => c.y));
                int maxY = Mathf.CeilToInt(cellPositions.Max(c => c.y));

                workingCells = new HashSet<Vector2>(cellPositions);
                for (int x = minX - paddingCells; x <= maxX + paddingCells; x++)
                {
                    for (int y = minY - paddingCells; y <= maxY + paddingCells; y++)
                    {
                        workingCells.Add(new Vector2(x, y));
                    }
                }

                workingAllCells = new HashSet<Vector2>(workingAllCells);
                workingAllCells.UnionWith(workingCells);
            }

            // For our base quad the vertices are:
            // 0: bottom-left, 1: bottom-right, 2: top-right, 3: top-left

            // Mapping for wall edges corresponding to a missing neighbor
            // Order: left, right, up, down
            int[,] edgeIndices = new int[4, 2] {
                { 0, 3 }, // Left edge: bottom-left → top-left
                { 2, 1 }, // Right edge: top-right → bottom-right (reversed order)
                { 3, 2 }, // Up edge: top-left → top-right
                { 1, 0 }  // Down edge: bottom-right → bottom-left (reversed order)
            };

            Vector2[] directions = new Vector2[] { Vector2.left, Vector2.right, Vector2.up, Vector2.down };

            foreach (Vector2 cell in workingCells)
            {
                Vector3 basePos = new Vector3(cell.x * cellSize - cellSize * 0.5f, height, cell.y * cellSize - cellSize * 0.5f);

                // Create the base quad for the cell (floor)
                int baseIndex = vertices.Count;
                vertices.Add(basePos);                                      // 0: bottom-left
                vertices.Add(basePos + new Vector3(cellSize, 0, 0));         // 1: bottom-right
                vertices.Add(basePos + new Vector3(cellSize, 0, cellSize));  // 2: top-right
                vertices.Add(basePos + new Vector3(0, 0, cellSize));          // 3: top-left
        
                // Create floor triangles
                triangles.Add(baseIndex + 0);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);

                triangles.Add(baseIndex + 0);
                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 2);

                // Detect which walls are needed
                bool[] wallNeeded = new bool[4];
                for (int d = 0; d < 4; d++)
                {
                    Vector2 neighborPos = cell + directions[d];
                    wallNeeded[d] = !workingAllCells.Contains(neighborPos);
                }

                // Build walls, handling corners specially
                for (int d = 0; d < 4; d++)
                {
                    if (!wallNeeded[d])
                        continue;

                    int idxA = baseIndex + edgeIndices[d, 0];
                    int idxB = baseIndex + edgeIndices[d, 1];
                    Vector3 v0 = vertices[idxA];
                    Vector3 v1 = vertices[idxB];

                    Vector3 edgeDir = (v1 - v0).normalized;
                    Vector3 outward = new Vector3(-edgeDir.z, 0, edgeDir.x);

                    Vector3 v0Offset = v0 + outward * insetAmount;
                    Vector3 v1Offset = v1 + outward * insetAmount;

                    // Check if this wall is part of a corner (2 adjacent walls)
                    int prevDir = (d - 1 + 4) % 4;
                    int nextDir = (d + 1) % 4;
                    bool cornerBefore = wallNeeded[prevDir];
                    bool cornerAfter = wallNeeded[nextDir];

                    if (cornerBefore || cornerAfter)
                    {
                        // Corner wall: add extra middle vertex to avoid fan topology
                        int wallBaseIndex = vertices.Count;
                        
                        vertices.Add(v0Offset);                                     // 0 bottom offset start
                        vertices.Add(v1Offset);                                     // 1 bottom offset end
                        
                        // Middle point at bottom for better distribution
                        Vector3 midBottom = (v0Offset + v1Offset) * 0.5f;
                        vertices.Add(midBottom);                                    // 2 middle bottom
                        
                        vertices.Add(v0Offset + Vector3.up * extrusionHeight);     // 3 top offset start
                        vertices.Add(v1Offset + Vector3.up * extrusionHeight);     // 4 top offset end
                        
                        // Middle point at top
                        Vector3 midTop = (v0Offset + v1Offset) * 0.5f + Vector3.up * extrusionHeight;
                        vertices.Add(midTop);                                       // 5 middle top

                        if (!invertWalls)
                        {
                            // Bottom triangle
                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 2);
                            triangles.Add(wallBaseIndex + 1);

                            // Top triangle
                            triangles.Add(wallBaseIndex + 3);
                            triangles.Add(wallBaseIndex + 4);
                            triangles.Add(wallBaseIndex + 5);

                            // Side triangles
                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 3);
                            triangles.Add(wallBaseIndex + 5);
                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 5);
                            triangles.Add(wallBaseIndex + 2);

                            triangles.Add(wallBaseIndex + 2);
                            triangles.Add(wallBaseIndex + 5);
                            triangles.Add(wallBaseIndex + 4);
                            triangles.Add(wallBaseIndex + 2);
                            triangles.Add(wallBaseIndex + 4);
                            triangles.Add(wallBaseIndex + 1);
                        }
                        else
                        {
                            // Reverse winding
                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 1);
                            triangles.Add(wallBaseIndex + 2);

                            triangles.Add(wallBaseIndex + 3);
                            triangles.Add(wallBaseIndex + 5);
                            triangles.Add(wallBaseIndex + 4);

                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 5);
                            triangles.Add(wallBaseIndex + 3);
                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 2);
                            triangles.Add(wallBaseIndex + 5);

                            triangles.Add(wallBaseIndex + 2);
                            triangles.Add(wallBaseIndex + 4);
                            triangles.Add(wallBaseIndex + 5);
                            triangles.Add(wallBaseIndex + 2);
                            triangles.Add(wallBaseIndex + 1);
                            triangles.Add(wallBaseIndex + 4);
                        }
                    }
                    else
                    {
                        // Normal wall without corner
                        int wallBaseIndex = vertices.Count;

                        vertices.Add(v0Offset);                         
                        vertices.Add(v1Offset);                         
                        vertices.Add(v1Offset + Vector3.up * extrusionHeight);
                        vertices.Add(v0Offset + Vector3.up * extrusionHeight);

                        if (!invertWalls)
                        {
                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 1);
                            triangles.Add(wallBaseIndex + 2);

                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 2);
                            triangles.Add(wallBaseIndex + 3);
                        }
                        else
                        {
                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 2);
                            triangles.Add(wallBaseIndex + 1);

                            triangles.Add(wallBaseIndex + 0);
                            triangles.Add(wallBaseIndex + 3);
                            triangles.Add(wallBaseIndex + 2);
                        }

                        // Connector strip
                        int connectorBase = vertices.Count;
                        vertices.Add(v0);        
                        vertices.Add(v1);        
                        vertices.Add(v1Offset);  
                        vertices.Add(v0Offset);  

                        triangles.Add(connectorBase + 0);
                        triangles.Add(connectorBase + 1);
                        triangles.Add(connectorBase + 2);

                        triangles.Add(connectorBase + 0);
                        triangles.Add(connectorBase + 2);
                        triangles.Add(connectorBase + 3);
                    }
                }
            }

        
            // Assign data to the mesh
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            
            // Generate UV coordinates for proper water shader wrapping
            GeneratePlanarUVs(mesh, cellSize);
            
            // Recalculate normals and tangents for shader detail quality
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();  // Critical for water shader with normal maps
            mesh.RecalculateBounds();

            return mesh;
        }


        /// <summary>
        /// Generates planar UV coordinates for tile meshes to ensure smooth water shader wrapping.
        /// Maps vertices to normalized 0..1 space per tile cell to prevent UV stretching/artifacts.
        /// </summary>
        private static void GeneratePlanarUVs(Mesh mesh, float cellSize)
        {
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = new Vector2[vertices.Length];
            
            Vector3 meshMin = vertices[0];
            Vector3 meshMax = vertices[0];
            
            for (int i = 1; i < vertices.Length; i++)
            {
                meshMin = Vector3.Min(meshMin, vertices[i]);
                meshMax = Vector3.Max(meshMax, vertices[i]);
            }
            
            Vector3 meshSize = meshMax - meshMin;
            float scale = 1f / cellSize;  // Normalize to cell-based coordinates
            
            for (int i = 0; i < vertices.Length; i++)
            {
                // Map X-Z coordinates to UV space (0..1 per cell)
                float u = (vertices[i].x - meshMin.x) * scale;
                float v = (vertices[i].z - meshMin.z) * scale;
                
                // Fract to get local tile coordinates for seamless tiling
                uvs[i] = new Vector2(u - Mathf.Floor(u), v - Mathf.Floor(v));
            }
            
            mesh.uv = uvs;
        }

        /// <summary>
        /// A simple struct representing an edge (line segment) between two 2D points.
        /// Two edges are considered equal if they have the same endpoints (order independent).
        /// </summary>
        struct Edge
        {
            public Vector2 a, b;
            public Edge(Vector2 a, Vector2 b)
            {
                this.a = a;
                this.b = b;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Edge))
                    return false;
                Edge other = (Edge)obj;
                return (Vector2.Distance(a, other.a) < 0.001f && Vector2.Distance(b, other.b) < 0.001f) ||
                    (Vector2.Distance(a, other.b) < 0.001f && Vector2.Distance(b, other.a) < 0.001f);
            }

            public override int GetHashCode()
            {
                // Order-independent hash code.
                int hash1 = a.GetHashCode() ^ b.GetHashCode();
                int hash2 = b.GetHashCode() ^ a.GetHashCode();
                return hash1 ^ hash2;
            }
        }
    }

    /// <summary>
    /// A simple ear-clipping triangulator for 2D polygons.
    /// </summary>
    public class Triangulator
    {
        private List<Vector2> m_points = new List<Vector2>();

        public Triangulator(Vector2[] points)
        {
            m_points = new List<Vector2>(points);
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2;)
            {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V))
                {
                    int a = V[u], b = V[v], c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (int s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return A * 0.5f;
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}