using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogClusterGeometryBuilder
    {
        int VertexCount { get; }
        void Clear();
        void AddCellQuad(Vector2Int cell, float y, float cellSize, Vector3 origin, int subMeshIndex);
        void AddCellSide(Vector3 topStart, Vector3 topEnd, Vector3 bottomEnd, Vector3 bottomStart, int subMeshIndex);
        void ApplyTo(Mesh mesh);
    }
}
