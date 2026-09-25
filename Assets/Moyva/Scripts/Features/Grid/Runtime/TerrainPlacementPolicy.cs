using Kruty1918.Moyva.Grid.API;
using Zenject;

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Tile-tag driven placement policy. Tile JSON marks blocked terrain via
    /// <c>"no-spawn"</c> (decorations, props, units, starting positions) and
    /// <c>"no-build"</c> (construction). Unknown/unresolved tile ids stay
    /// placeable so a missing repository never blocks unrelated systems;
    /// a conservative name fallback still covers shore tiles in bootstrap
    /// contexts where the repository is absent.
    /// </summary>
    internal sealed class TerrainPlacementPolicy : ITerrainPlacementPolicy
    {
        public const string NoSpawnTag = "no-spawn";
        public const string NoBuildTag = "no-build";

        private readonly ITileTypeRepository _tileTypes;

        public TerrainPlacementPolicy(
            [InjectOptional] ITileTypeRepository tileTypes = null)
        {
            _tileTypes = tileTypes;
        }

        public bool AllowsPlacement(string tileTypeId, TerrainPlacementOperation operation)
        {
            if (string.IsNullOrWhiteSpace(tileTypeId))
                return true;

            string blockingTag = operation == TerrainPlacementOperation.Building
                ? NoBuildTag
                : NoSpawnTag;

            if (_tileTypes != null
                && _tileTypes.TryGet(tileTypeId, out TileTypeSnapshot tileType))
            {
                return !tileType.HasTag(blockingTag);
            }

            return !LooksLikeBlockedShoreTile(tileTypeId);
        }

        /// <summary>
        /// Repository-free fallback: shore-band ids predate canonical tile
        /// resolution in a few generation-time paths, so recognise them by
        /// name instead of duplicating Contains("sand") checks in callers.
        /// </summary>
        internal static bool LooksLikeBlockedShoreTile(string tileTypeId)
        {
            string id = tileTypeId.Trim().ToLowerInvariant();
            return id.Contains("sand")
                   || id.Contains("beach")
                   || id.Contains("shore-band")
                   || id.Contains("shoreband")
                   || id.Contains("coast");
        }
    }
}
