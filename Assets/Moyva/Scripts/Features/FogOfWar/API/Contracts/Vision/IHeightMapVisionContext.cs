namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Тримає лише відповідальність за подачу height map у terrain-aware vision pipeline.
    /// </summary>
    public interface IHeightMapVisionContext
    {
        /// <summary>
        /// Передає height map generated світу для подальших LOS-обчислень.
        /// </summary>
        /// <param name="heightMap">Мапа висот у координатах клітинок.</param>
        void SetHeightMap(float[,] heightMap);
    }
}
