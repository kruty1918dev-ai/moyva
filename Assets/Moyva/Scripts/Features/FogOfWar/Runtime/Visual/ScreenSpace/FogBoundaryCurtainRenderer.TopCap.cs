using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogBoundaryCurtainRenderer
    {
        private void AddTopCapQuad(
            Vector3 innerA,
            Vector3 innerB,
            Vector3 outerB,
            Vector3 outerA)
        {
            int vertexStart =
                _vertices.Count;

            _vertices.Add(innerA);
            _vertices.Add(innerB);
            _vertices.Add(outerB);
            _vertices.Add(outerA);

            /*
             * UV.y = 1 для всіх вершин:
             * top-cap завжди використовує темний TopColor.
             */
            _uvs.Add(
                new Vector2(
                    0f,
                    1f));

            _uvs.Add(
                new Vector2(
                    1f,
                    1f));

            _uvs.Add(
                new Vector2(
                    1f,
                    1f));

            _uvs.Add(
                new Vector2(
                    0f,
                    1f));

            Vector3 candidateNormal =
                Vector3.Cross(
                    innerB - innerA,
                    outerB - innerA);

            bool candidateFacesUp =
                Vector3.Dot(
                    candidateNormal,
                    Vector3.up)
                >= 0f;

            if (candidateFacesUp)
            {
                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 1);
                _triangles.Add(vertexStart + 2);

                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 2);
                _triangles.Add(vertexStart + 3);
            }
            else
            {
                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 2);
                _triangles.Add(vertexStart + 1);

                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 3);
                _triangles.Add(vertexStart + 2);
            }
        }

    }
}
