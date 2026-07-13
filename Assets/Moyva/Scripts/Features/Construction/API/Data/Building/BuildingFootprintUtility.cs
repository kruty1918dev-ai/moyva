using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Resolves authored footprint cells without allocating temporary collections.
    /// The placement origin is the cell selected by input; authored cells are offset
    /// from the configured anchor.
    /// </summary>
    public static class BuildingFootprintUtility
    {
        public static int GetOccupiedCellCount(BuildingDefinition definition)
        {
            BuildingFootprint footprint = definition?.Footprint;
            if (footprint?.OccupiedCells != null && footprint.OccupiedCells.Length > 0)
                return footprint.OccupiedCells.Length;

            Vector2Int size = ResolveSize(footprint);
            return size.x * size.y;
        }

        public static Vector2Int GetOccupiedCellOffset(BuildingDefinition definition, int index)
        {
            BuildingFootprint footprint = definition?.Footprint;
            Vector2Int anchor = ResolveAnchor(footprint);
            if (footprint?.OccupiedCells != null && footprint.OccupiedCells.Length > 0)
            {
                int safeIndex = Mathf.Clamp(index, 0, footprint.OccupiedCells.Length - 1);
                return footprint.OccupiedCells[safeIndex] - anchor;
            }

            Vector2Int size = ResolveSize(footprint);
            int safeCellIndex = Mathf.Clamp(index, 0, size.x * size.y - 1);
            return new Vector2Int(safeCellIndex % size.x, safeCellIndex / size.x) - anchor;
        }

        public static Vector2Int GetOccupiedCell(BuildingDefinition definition, Vector2Int origin, int index)
            => origin + GetOccupiedCellOffset(definition, index);

        public static bool Contains(BuildingDefinition definition, Vector2Int origin, Vector2Int position)
        {
            int count = GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                if (GetOccupiedCell(definition, origin, index) == position)
                    return true;
            }

            return false;
        }

        public static bool TryValidate(BuildingDefinition definition, out string reason)
        {
            if (definition == null)
            {
                reason = "Building definition is missing.";
                return false;
            }

            BuildingFootprint footprint = definition.Footprint;
            if (footprint == null)
            {
                reason = null;
                return true;
            }

            if (footprint.Size.x < 1 || footprint.Size.y < 1)
            {
                reason = "Footprint size must be at least 1x1.";
                return false;
            }

            Vector2Int[] occupiedCells = footprint.OccupiedCells;
            if (occupiedCells == null || occupiedCells.Length == 0)
            {
                reason = null;
                return true;
            }

            for (int index = 0; index < occupiedCells.Length; index++)
            {
                Vector2Int cell = occupiedCells[index];
                if (cell.x < 0 || cell.y < 0 || cell.x >= footprint.Size.x || cell.y >= footprint.Size.y)
                {
                    reason = $"Footprint cell {cell} is outside authored size {footprint.Size}.";
                    return false;
                }

                for (int previous = 0; previous < index; previous++)
                {
                    if (occupiedCells[previous] != cell)
                        continue;

                    reason = $"Footprint cell {cell} is duplicated.";
                    return false;
                }
            }

            reason = null;
            return true;
        }

        private static Vector2Int ResolveSize(BuildingFootprint footprint)
        {
            Vector2Int size = footprint?.Size ?? Vector2Int.one;
            return new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
        }

        private static Vector2Int ResolveAnchor(BuildingFootprint footprint)
        {
            if (footprint == null)
                return Vector2Int.zero;

            Vector2Int size = ResolveSize(footprint);
            return footprint.Anchor switch
            {
                BuildingFootprintAnchor.SouthWest => Vector2Int.zero,
                BuildingFootprintAnchor.Custom => new Vector2Int(
                    Mathf.Clamp(footprint.CustomAnchor.x, 0, size.x - 1),
                    Mathf.Clamp(footprint.CustomAnchor.y, 0, size.y - 1)),
                _ => new Vector2Int((size.x - 1) / 2, (size.y - 1) / 2),
            };
        }
    }
}
