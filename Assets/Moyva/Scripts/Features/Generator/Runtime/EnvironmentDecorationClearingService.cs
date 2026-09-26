using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Clears environment decorations and spawned props under a committed
    /// building footprint. Runs only after a successful placement signal —
    /// preview and cancelled placement never touch decorations. The cleared
    /// cells stay excluded from respawns for the rest of the session, and a
    /// save-load replays placements through the same signal, so the cleared
    /// footprint survives rebuilds without extra persistence.
    /// </summary>
    internal sealed class EnvironmentDecorationClearingService
        : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly EnvironmentDecorationSpawner _decorationSpawner;
        private readonly IChunkFirstObjectSpawner _objectSpawner;

        [Inject]
        public EnvironmentDecorationClearingService(
            SignalBus signalBus,
            [InjectOptional] IBuildingRegistry buildingRegistry = null,
            [InjectOptional] EnvironmentDecorationSpawner decorationSpawner = null,
            [InjectOptional] IChunkFirstObjectSpawner objectSpawner = null)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _decorationSpawner = decorationSpawner;
            _objectSpawner = objectSpawner;
        }

        public void Initialize()
        {
            _signalBus?.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
        }

        public void Dispose()
        {
            _signalBus?.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            IReadOnlyList<Vector2Int> cells = ResolveFootprintCells(signal);
            _decorationSpawner?.ClearDecorationsInCells(cells);
            _objectSpawner?.ClearPropsInCells(cells);
        }

        private IReadOnlyList<Vector2Int> ResolveFootprintCells(
            BuildingPlacedSignal signal)
        {
            BuildingDefinition definition =
                _buildingRegistry?.GetById(signal.BuildingId);
            if (definition == null)
                return new[] { signal.Position };

            ConstructionRotation rotation =
                ConstructionRotationUtility.Normalize(
                    signal.RotationQuarterTurns);
            return BuildingFootprintUtility.GetOccupiedCells(
                definition,
                signal.Position,
                rotation);
        }
    }
}
