namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Визначає, з якого джерела береться висота для fog volume build.
    /// </summary>
    public enum FogVolumeHeightSource
    {
        /// <summary>
        /// Спершу використовується terrain level map, а за її відсутності — точна height map.
        /// </summary>
        TerrainLevelMapThenHeightMap = 0,

        /// <summary>
        /// Спершу використовується точна height map, а за її відсутності — terrain level map.
        /// </summary>
        HeightMapThenTerrainLevelMap = 1,

        /// <summary>
        /// Уся fog volume геометрія будується на пласкій висоті.
        /// </summary>
        Flat = 2,
    }
}
