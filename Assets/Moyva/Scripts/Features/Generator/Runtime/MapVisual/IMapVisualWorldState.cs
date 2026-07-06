using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMapVisualWorldState
    {
        bool HasPendingWorldData { get; }
        void SetPendingWorldData(GeneratedWorldData data);
        bool TryConsumePendingWorldData(out GeneratedWorldData data);
        void SetCurrentWorldData(GeneratedWorldData data);
        bool TryGetCurrentWorldData(out GeneratedWorldData data);
        void ApplySpawnPositions(SpawnPositionAssignment[] assignments);
    }
}
