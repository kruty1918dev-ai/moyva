using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogScreenSpaceTextureUpdater
    {
        private Vector2 ConvertWorldToGrid(
            Vector3 world,
            Vector3 gridOrigin,
            Vector4 worldToGrid,
            bool flipY)
        {
            Vector2 delta =
                new Vector2(
                    world.x - gridOrigin.x,
                    world.z - gridOrigin.y);

            Vector2 grid =
                new Vector2(
                    delta.x * worldToGrid.x
                        + delta.y * worldToGrid.y,

                    delta.x * worldToGrid.z
                        + delta.y * worldToGrid.w);

            if (flipY)
            {
                grid.y =
                    (_height - 1)
                    - grid.y;
            }

            return grid;
        }

        private string ResolveStateLabel(
            Vector2Int cell)
        {
            if (_pixels == null
                || !IsInBounds(
                    cell))
            {
                return "Outside";
            }

            Color32 pixel =
                _pixels[
                    cell.x
                    + cell.y * _width];

            if (pixel.g >= 128)
                return "Unexplored";

            if (pixel.r >= 128)
                return "Explored";

            return "Visible";
        }

        private static bool TryIntersectPlane(
            Ray ray,
            float planeY,
            out Vector3 point)
        {
            point =
                Vector3.zero;

            if (Mathf.Abs(
                    ray.direction.y)
                <= 0.00001f)
            {
                return false;
            }

            float distance =
                (planeY - ray.origin.y)
                / ray.direction.y;

            if (distance <= 0f)
                return false;

            point =
                ray.origin
                + ray.direction * distance;

            return true;
        }

        private static string FormatVector3(
            Vector3 value)
        {
            return "("
                   + value.x.ToString("F3")
                   + ","
                   + value.y.ToString("F3")
                   + ","
                   + value.z.ToString("F3")
                   + ")";
        }

        private float ResolveTransparentFallbackPlaneY(
            float logicalOriginY)
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            return logicalOriginY
                   + screenSettings
                       .TransparentFallbackPlaneOffsetY;
        }

        private void ResolveWorldToGridTransform(
            out Vector3 gridOrigin,
            out Vector4 worldToGrid)
        {
            if (_gridProjection != null)
            {
                Vector3 originWorld =
                    _gridProjection.GridToWorld(
                        Vector2Int.zero);

                Vector3 xWorld =
                    _gridProjection.GridToWorld(
                        Vector2Int.right);

                Vector3 yWorld =
                    _gridProjection.GridToWorld(
                        Vector2Int.up);

                Vector2 xBasis =
                    new Vector2(
                        xWorld.x
                            - originWorld.x,
                        xWorld.z
                            - originWorld.z);

                Vector2 yBasis =
                    new Vector2(
                        yWorld.x
                            - originWorld.x,
                        yWorld.z
                            - originWorld.z);

                float determinant =
                    xBasis.x * yBasis.y
                    - xBasis.y * yBasis.x;

                if (Mathf.Abs(
                        determinant)
                    > 0.000001f)
                {
                    float inverseDeterminant =
                        1f / determinant;

                    gridOrigin =
                        new Vector3(
                            originWorld.x,
                            originWorld.z,
                            ResolveTransparentFallbackPlaneY(
                                originWorld.y));

                    worldToGrid =
                        new Vector4(
                            yBasis.y
                                * inverseDeterminant,

                            -yBasis.x
                                * inverseDeterminant,

                            -xBasis.y
                                * inverseDeterminant,

                            xBasis.x
                                * inverseDeterminant);

                    return;
                }
            }

            if (_context.IsValid
                && _context.HasMapWorldBounds)
            {
                Bounds bounds =
                    _context.MapWorldBounds;

                float cellWidth =
                    Mathf.Max(
                        0.0001f,
                        bounds.size.x
                        / Mathf.Max(
                            1,
                            _width));

                float cellDepth =
                    Mathf.Max(
                        0.0001f,
                        bounds.size.z
                        / Mathf.Max(
                            1,
                            _height));

                gridOrigin =
                    new Vector3(
                        bounds.min.x
                            + cellWidth * 0.5f,
                        bounds.min.z
                            + cellDepth * 0.5f,
                        ResolveTransparentFallbackPlaneY(
                            0f));

                worldToGrid =
                    new Vector4(
                        1f / cellWidth,
                        0f,
                        0f,
                        1f / cellDepth);

                return;
            }

            float cellSize =
                _context.IsValid
                    ? Mathf.Max(
                        0.0001f,
                        _context.CellSize)
                    : 1f;

            gridOrigin =
                new Vector3(
                    0f,
                    0f,
                    ResolveTransparentFallbackPlaneY(
                        0f));

            worldToGrid =
                new Vector4(
                    1f / cellSize,
                    0f,
                    0f,
                    1f / cellSize);
        }

        private FogScreenSpaceSettings
            ResolveScreenSettings()
        {
            FogScreenSpaceSettings result =
                _settings != null
                    ? _settings.ScreenSpace
                    : null;

            if (result == null)
            {
                result =
                    new FogScreenSpaceSettings();
            }

            result.EnsureDefaults();

            return result;
        }

        private bool IsInBounds(
            Vector2Int tile)
        {
            return tile.x >= 0
                   && tile.x < _width
                   && tile.y >= 0
                   && tile.y < _height;
        }

        private static Color32 EncodeState(
            FogStateType state)
        {
            switch (state)
            {
                case FogStateType.Visible:
                    return VisibleValue;

                case FogStateType.Explored:
                    return ExploredValue;

                default:
                    return UnexploredValue;
            }
        }

        private static bool IsInsideShape(
            Vector2Int tile,
            Vector2Int center,
            int radius,
            FogRevealShape shape)
        {
            int dx =
                Mathf.Abs(
                    tile.x - center.x);

            int dy =
                Mathf.Abs(
                    tile.y - center.y);

            switch (shape)
            {
                case FogRevealShape.Diamond:
                    return dx + dy <= radius;

                case FogRevealShape.Square:
                    return Mathf.Max(
                               dx,
                               dy)
                           <= radius;

                default:
                    return dx * dx
                               + dy * dy
                           <= radius * radius;
            }
        }
    }
}
