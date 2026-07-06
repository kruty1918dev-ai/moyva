namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualWorldState : IMapVisualWorldState
    {
        private GeneratedWorldData _pendingWorldData;
        private GeneratedWorldData _currentWorldData;

        public bool HasPendingWorldData => _pendingWorldData != null;

        public void SetPendingWorldData(GeneratedWorldData data)
        {
            _pendingWorldData = data?.Clone();
        }

        public bool TryConsumePendingWorldData(out GeneratedWorldData data)
        {
            data = _pendingWorldData;
            _pendingWorldData = null;
            return data != null;
        }

        public void SetCurrentWorldData(GeneratedWorldData data)
        {
            _currentWorldData = data?.Clone();
        }

        public bool TryGetCurrentWorldData(out GeneratedWorldData data)
        {
            data = _currentWorldData?.Clone();
            return data != null;
        }

        public void ApplySpawnPositions(SpawnPositionAssignment[] assignments)
        {
            if (_currentWorldData == null || assignments == null || assignments.Length == 0)
                return;

            _currentWorldData.SpawnPositions = (SpawnPositionAssignment[])assignments.Clone();
        }
    }
}
