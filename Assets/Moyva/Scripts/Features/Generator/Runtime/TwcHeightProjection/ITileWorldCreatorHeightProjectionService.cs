using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public interface ITileWorldCreatorHeightProjectionService
    {
        void Configure(
            TileWorldCreatorHeightProjectionState state,
            MonoBehaviour owner,
            Transform targetRoot,
            int[,] terrainLevelMap,
            float cellSize,
            int heightStep,
            float trackingSeconds);

        void Tick(TileWorldCreatorHeightProjectionState state, MonoBehaviour owner);
    }
}
