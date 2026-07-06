using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionPlacedVisualSignalHandler : IConstructionPlacedVisualSignalHandler
    {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IConstructionPreviewVisualService _previewVisuals;
        private readonly IConstructionPlacedVisualService _placedVisuals;
        private readonly IConstructionWallVisualRefreshService _wallVisuals;
        private readonly IConstructionInfluenceRadiusVisualService _radiusVisuals;
        private readonly IConstructionBuildGridOverlayService _buildGridOverlay;
        private readonly int _townHallBuildRadius;

        [Inject]
        public ConstructionPlacedVisualSignalHandler(
            IBuildingRegistry buildingRegistry,
            IWallTopologyService wallTopologyService,
            IConstructionPreviewVisualService previewVisuals,
            IConstructionPlacedVisualService placedVisuals,
            IConstructionWallVisualRefreshService wallVisuals,
            IConstructionInfluenceRadiusVisualService radiusVisuals,
            IConstructionBuildGridOverlayService buildGridOverlay,
            [Inject(Id = "townHallBuildRadius")] int townHallBuildRadius)
        {
            _buildingRegistry = buildingRegistry;
            _wallTopologyService = wallTopologyService;
            _previewVisuals = previewVisuals;
            _placedVisuals = placedVisuals;
            _wallVisuals = wallVisuals;
            _radiusVisuals = radiusVisuals;
            _buildGridOverlay = buildGridOverlay;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
        }

        public void Handle(BuildingPlacedSignal signal)
        {
            _previewVisuals.Remove(signal.Position);
            _radiusVisuals.HidePreview();
            _buildGridOverlay.MarkDirty();

            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
            {
                _wallVisuals.RefreshPlacedNeighborhood(signal.Position);
                return;
            }

            BuildingDefinition def = _buildingRegistry.GetById(signal.BuildingId);
            if (def?.Prefab != null)
                _placedVisuals.Replace(signal.Position, signal.BuildingId, def.Prefab, Quaternion.identity, def.VisualYOffset);
        }

        public void Handle(BuildingDemolishedSignal signal)
        {
            _buildGridOverlay.MarkDirty();
            _placedVisuals.Remove(signal.Position);

            if (_placedVisuals.ClearSelectionIfMatches(signal.Position))
                _radiusVisuals.HideInspection();

            if (_wallTopologyService.IsWallOrGate(signal.BuildingId))
                _wallVisuals.RefreshPlacedNeighborhood(signal.Position);
        }

        public void Handle(WorldInfoSelectionChangedSignal signal)
        {
            if (signal.Kind != WorldInfoSelectionKind.Building || string.IsNullOrWhiteSpace(signal.ObjectId))
            {
                _placedVisuals.ClearSelection();
                _radiusVisuals.HideInspection();
                return;
            }

            _placedVisuals.Select(signal.Position);
            BuildingDefinition def = _buildingRegistry.GetById(signal.ObjectId);
            if (HasInfluenceRadius(def))
                _radiusVisuals.ShowInspection(signal.Position, ResolveInfluenceRadius(def));
            else
                _radiusVisuals.HideInspection();
        }

        public void Handle(GameModeChangedSignal signal)
        {
            _buildGridOverlay.SetConstructionModeActive(signal.NewMode == GameModeType.Construction);
        }

        private bool HasInfluenceRadius(BuildingDefinition def)
            => def != null && (BuildingDefinitionCapabilities.IsTownHall(def) || BuildingDefinitionCapabilities.IsCastle(def));

        private int ResolveInfluenceRadius(BuildingDefinition def)
            => BuildingDefinitionCapabilities.GetInfluenceRadius(def, _townHallBuildRadius);
    }
}
