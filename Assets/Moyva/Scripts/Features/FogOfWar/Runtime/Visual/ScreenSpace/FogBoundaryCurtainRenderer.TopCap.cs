using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogBoundaryCurtainRenderer
    {
        private void AddTopCapQuad(
            Vector3 innerA,
            Vector3 innerB,
            Vector3 outerB,
            Vector3 outerA,
            Color32 debugColor)
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

            _colors.Add(debugColor);
            _colors.Add(debugColor);
            _colors.Add(debugColor);
            _colors.Add(debugColor);

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

        private Color32 ResolveDebugColor(
            FogScreenSpaceSettings settings,
            Vector2Int cell,
            int directionIndex,
            float topY,
            float revealedHeight,
            float hiddenHeight)
        {
            switch (settings.CurtainDebugVisualMode)
            {
                case FogCurtainDebugVisualMode
                    .BoundaryDirection:

                    switch (directionIndex)
                    {
                        case 0:
                            return new Color32(
                                255,
                                60,
                                60,
                                255);

                        case 1:
                            return new Color32(
                                60,
                                255,
                                80,
                                255);

                        case 2:
                            return new Color32(
                                70,
                                120,
                                255,
                                255);

                        default:
                            return new Color32(
                                255,
                                220,
                                60,
                                255);
                    }

                case FogCurtainDebugVisualMode
                    .HeightBands:

                    Color heightColor =
                        Color.HSVToRGB(
                            Mathf.Repeat(
                                topY * 0.173f,
                                1f),
                            0.9f,
                            1f);

                    return heightColor;

                case FogCurtainDebugVisualMode
                    .HeightRelation:

                    if (hiddenHeight
                        > revealedHeight + 0.001f)
                    {
                        return new Color32(
                            255,
                            70,
                            200,
                            255);
                    }

                    if (revealedHeight
                        > hiddenHeight + 0.001f)
                    {
                        return new Color32(
                            40,
                            220,
                            255,
                            255);
                    }

                    return new Color32(
                        255,
                        255,
                        255,
                        255);

                case FogCurtainDebugVisualMode
                    .SegmentParity:

                    return ((cell.x
                             + cell.y
                             + directionIndex)
                            & 1) == 0
                        ? new Color32(
                            255,
                            255,
                            255,
                            255)
                        : new Color32(
                            30,
                            30,
                            30,
                            255);

                default:
                    return new Color32(
                        255,
                        255,
                        255,
                        255);
            }
        }

    }
}
