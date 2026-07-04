using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogClusterGeometryBuilder
    {
        int VertexCount { get; }
        void Clear();
        void AddCellQuad(Vector2Int cell, float y, float cellSize, Vector3 origin, int subMeshIndex);
        void ApplyTo(Mesh mesh);
    }
}
