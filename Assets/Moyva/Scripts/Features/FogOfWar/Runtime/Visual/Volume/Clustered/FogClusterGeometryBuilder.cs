using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class FogClusterGeometryBuilder : IFogClusterGeometryBuilder
    {
        private readonly List<Vector3> _vertices = new List<Vector3>(1024);
        private readonly List<int> _unexploredTriangles = new List<int>(1536);
        private readonly List<int> _exploredTriangles = new List<int>(1536);
        private readonly List<Vector2> _uvs = new List<Vector2>(1024);

        public int VertexCount => _vertices.Count;

        public void Clear()
        {
            _vertices.Clear();
            _unexploredTriangles.Clear();
            _exploredTriangles.Clear();
            _uvs.Clear();
        }

        public void AddCellQuad(Vector2Int cell, float y, float cellSize, Vector3 origin, int subMeshIndex)
        {
            int vertexStart = _vertices.Count;
            float safeCellSize = Mathf.Max(0.0001f, cellSize);
            float x0 = origin.x + cell.x * safeCellSize;
            float z0 = origin.z + cell.y * safeCellSize;
            float x1 = x0 + safeCellSize;
            float z1 = z0 + safeCellSize;

            _vertices.Add(new Vector3(x0, y, z0));
            _vertices.Add(new Vector3(x0, y, z1));
            _vertices.Add(new Vector3(x1, y, z1));
            _vertices.Add(new Vector3(x1, y, z0));

            var triangles = subMeshIndex == 1 ? _exploredTriangles : _unexploredTriangles;
            triangles.Add(vertexStart);
            triangles.Add(vertexStart + 1);
            triangles.Add(vertexStart + 2);
            triangles.Add(vertexStart);
            triangles.Add(vertexStart + 2);
            triangles.Add(vertexStart + 3);

            _uvs.Add(new Vector2(0f, 0f));
            _uvs.Add(new Vector2(0f, 1f));
            _uvs.Add(new Vector2(1f, 1f));
            _uvs.Add(new Vector2(1f, 0f));
        }

        public void ApplyTo(Mesh mesh)
        {
            if (mesh == null)
                return;

            mesh.Clear();
            if (_vertices.Count == 0)
                return;

            mesh.SetVertices(_vertices);
            mesh.SetUVs(0, _uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(_unexploredTriangles, 0);
            mesh.SetTriangles(_exploredTriangles, 1);
            mesh.RecalculateBounds();
        }
    }
}
