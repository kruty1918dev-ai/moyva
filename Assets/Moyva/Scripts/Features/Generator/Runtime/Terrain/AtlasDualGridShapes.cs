using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Canonical atlas dual-grid forms. The pack authors each form against a
    /// fixed canonical corner mask; runtime code only rotates them around Y.
    /// </summary>
    internal enum AtlasTileForm
    {
        Corner = 0,
        Edge = 1,
        Interior = 2,
        Merged = 3,
        Fill = 4,
    }

    /// <summary>
    /// Pack-space dual-grid corner masks and form/rotation resolution.
    ///
    /// Corner bits (per ASSET_CONTRACT): SW = 1, SE = 2, NE = 4, NW = 8.
    /// Canonical masks: corner = 1, edge = 3, interior = 14, merged = 5, fill = 15.
    ///
    /// Prefab rotation uses Unity yaw: +90 degrees about Y maps +Z to +X, i.e.
    /// SW -> NW -> NE -> SE -> SW for the corner positions.
    /// </summary>
    internal static class AtlasDualGridShapes
    {
        public const int CornerSw = 1;
        public const int CornerSe = 2;
        public const int CornerNe = 4;
        public const int CornerNw = 8;
        public const int MaskFill = 15;

        private static readonly int[] FormByMask = new int[16];
        private static readonly int[] RotationByMask = new int[16];
        private static readonly bool[] Resolved = new bool[16];

        static AtlasDualGridShapes()
        {
            Register(AtlasTileForm.Corner, CornerSw);
            Register(AtlasTileForm.Edge, CornerSw | CornerSe);
            Register(AtlasTileForm.Interior, MaskFill & ~CornerSw);
            Register(AtlasTileForm.Merged, CornerSw | CornerNe);
            Register(AtlasTileForm.Fill, MaskFill);
        }

        /// <summary>
        /// Builds a pack-space mask from four corner-match flags.
        /// </summary>
        public static int BuildMask(bool northWest, bool northEast, bool southWest, bool southEast)
        {
            return (southWest ? CornerSw : 0)
                   | (southEast ? CornerSe : 0)
                   | (northEast ? CornerNe : 0)
                   | (northWest ? CornerNw : 0);
        }

        /// <summary>
        /// Resolves a pack-space corner mask to a form plus Y rotation.
        /// Mask 0 (isolated fragment) and any unmapped value resolve to the flat
        /// fill form — a lone or fully covered fragment needs no authored border.
        /// </summary>
        public static bool TryResolve(
            int mask,
            out AtlasTileForm form,
            out int yRotation)
        {
            mask &= MaskFill;
            if (Resolved[mask])
            {
                form = (AtlasTileForm)FormByMask[mask];
                yRotation = RotationByMask[mask];
                return true;
            }

            form = AtlasTileForm.Fill;
            yRotation = 0;
            return false;
        }

        /// <summary>Rotates a pack-space mask by a 90-degree yaw step count.</summary>
        public static int RotateMask(int mask, int quarterTurns)
        {
            int result = mask & MaskFill;
            int turns = ((quarterTurns % 4) + 4) % 4;
            for (int i = 0; i < turns; i++)
            {
                int rotated = 0;
                // +90 yaw: SW -> NW, NW -> NE, NE -> SE, SE -> SW.
                if ((result & CornerSw) != 0) rotated |= CornerNw;
                if ((result & CornerNw) != 0) rotated |= CornerNe;
                if ((result & CornerNe) != 0) rotated |= CornerSe;
                if ((result & CornerSe) != 0) rotated |= CornerSw;
                result = rotated;
            }
            return result;
        }

        /// <summary>
        /// True when every unmatched side of the fragment is covered by equal or
        /// higher neighbour surfaces, so the flat fill form renders identically.
        /// </summary>
        public static bool ShouldDemoteToFill(int mask, TileMeshOccludedSides occludedSides)
        {
            if (mask == MaskFill)
                return false;

            bool openNorth = (mask & (CornerNw | CornerNe)) != (CornerNw | CornerNe);
            bool openEast = (mask & (CornerNe | CornerSe)) != (CornerNe | CornerSe);
            bool openSouth = (mask & (CornerSw | CornerSe)) != (CornerSw | CornerSe);
            bool openWest = (mask & (CornerNw | CornerSw)) != (CornerNw | CornerSw);

            bool uncoveredOpenSide =
                (openNorth && (occludedSides & TileMeshOccludedSides.North) == 0)
                || (openEast && (occludedSides & TileMeshOccludedSides.East) == 0)
                || (openSouth && (occludedSides & TileMeshOccludedSides.South) == 0)
                || (openWest && (occludedSides & TileMeshOccludedSides.West) == 0);

            return !uncoveredOpenSide;
        }

        /// <summary>
        /// Deepest exposed drop across open, non-occluded sides. Used to choose
        /// between the high (0.5 m) and low (0.25 m) border variants.
        /// </summary>
        public static float ResolveMaxOpenDrop(
            int mask,
            TileMeshOccludedSides occludedSides,
            float surfaceHeight,
            float northBottom,
            float eastBottom,
            float southBottom,
            float westBottom)
        {
            float maxDrop = 0f;
            AccumulateOpenSideDrop(mask, occludedSides, CornerNw | CornerNe,
                TileMeshOccludedSides.North, surfaceHeight, northBottom, ref maxDrop);
            AccumulateOpenSideDrop(mask, occludedSides, CornerNe | CornerSe,
                TileMeshOccludedSides.East, surfaceHeight, eastBottom, ref maxDrop);
            AccumulateOpenSideDrop(mask, occludedSides, CornerSw | CornerSe,
                TileMeshOccludedSides.South, surfaceHeight, southBottom, ref maxDrop);
            AccumulateOpenSideDrop(mask, occludedSides, CornerNw | CornerSw,
                TileMeshOccludedSides.West, surfaceHeight, westBottom, ref maxDrop);
            return maxDrop;
        }

        private static void AccumulateOpenSideDrop(
            int mask,
            TileMeshOccludedSides occludedSides,
            int sideCorners,
            TileMeshOccludedSides occlusionFlag,
            float surfaceHeight,
            float sideBottom,
            ref float maxDrop)
        {
            bool sideOpen = (mask & sideCorners) != sideCorners;
            if (!sideOpen || (occludedSides & occlusionFlag) != 0)
                return;

            if (float.IsNaN(sideBottom) || float.IsInfinity(sideBottom))
                return;

            float drop = surfaceHeight - sideBottom;
            if (drop > maxDrop)
                maxDrop = drop;
        }

        private static void Register(AtlasTileForm form, int canonicalMask)
        {
            for (int rotation = 0; rotation < 4; rotation++)
            {
                int mask = RotateMask(canonicalMask, rotation);
                if (Resolved[mask])
                    continue;

                Resolved[mask] = true;
                FormByMask[mask] = (int)form;
                RotationByMask[mask] = rotation * 90;
            }
        }
    }
}
