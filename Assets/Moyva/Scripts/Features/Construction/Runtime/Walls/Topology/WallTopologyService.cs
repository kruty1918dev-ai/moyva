using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallTopologyService :
        IWallTopologyService,
        IConstructionGateStateService,
        IConstructionModuleStatePersistence,
        IInitializable,
        IDisposable
    {
        private readonly LazyInject<IConstructionSessionCommands> _constructionService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectsMapService _objectsMapService;
        private readonly ConstructionPlacedVisualService _placedVisuals;
        private readonly SignalBus _signalBus;
        private readonly Dictionary<Vector2Int, bool>
            _gateOpenState = new();

        public string StateKey => "gate-state.v1";

        [Inject]
        public WallTopologyService(
            LazyInject<IConstructionSessionCommands> constructionService,
            IBuildingRegistry buildingRegistry,
            IObjectsMapService objectsMapService,
            SignalBus signalBus,
            [InjectOptional]
            ConstructionPlacedVisualService placedVisuals = null)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _objectsMapService = objectsMapService;
            _signalBus = signalBus;
            _placedVisuals = placedVisuals;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BuildingPlacedSignal>(
                OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(
                OnBuildingDemolished);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(
                OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(
                OnBuildingDemolished);
            _gateOpenState.Clear();
        }

        private void OnBuildingPlaced(
            BuildingPlacedSignal signal)
        {
            if (signal.HasRelocationSource
                && signal.RelocationSourcePosition
                    != signal.Position)
            {
                _gateOpenState.Remove(
                    signal.RelocationSourcePosition);
            }

            // Newly placed/restored gates are closed by default.
            // A save-state payload is applied after placement restoration.
            if (TryGetPlacedGate(
                    signal.Position,
                    out _,
                    out _)
                && !_gateOpenState.ContainsKey(
                    signal.Position))
            {
                _gateOpenState[signal.Position] = false;
            }
        }

        private void OnBuildingDemolished(
            BuildingDemolishedSignal signal)
        {
            _gateOpenState.Remove(signal.Position);
        }

        public byte[] CaptureState()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(1);

            var openGates = new List<Vector2Int>();
            foreach (var pair in _gateOpenState)
            {
                if (pair.Value
                    && TryGetPlacedGate(
                        pair.Key,
                        out _,
                        out _))
                {
                    openGates.Add(pair.Key);
                }
            }

            openGates.Sort(
                (left, right) =>
                {
                    int byX =
                        left.x.CompareTo(right.x);
                    return byX != 0
                        ? byX
                        : left.y.CompareTo(right.y);
                });

            writer.Write(openGates.Count);
            for (int index = 0;
                 index < openGates.Count;
                 index++)
            {
                writer.Write(openGates[index].x);
                writer.Write(openGates[index].y);
            }

            writer.Flush();

            Debug.Log(
                $"[MoyvaConstructionModules] gate-save " +
                $"open={openGates.Count}");

            return stream.ToArray();
        }

        public void RestoreState(byte[] payload)
        {
            _gateOpenState.Clear();

            if (payload == null || payload.Length == 0)
                return;

            using var stream =
                new MemoryStream(payload, writable: false);
            using var reader = new BinaryReader(stream);

            int version = reader.ReadInt32();
            if (version != 1)
            {
                Debug.LogWarning(
                    $"[MoyvaConstructionModules] gate-load " +
                    $"unsupported-version={version}");
                return;
            }

            int count = Math.Max(0, reader.ReadInt32());
            int restored = 0;
            int skipped = 0;

            for (int index = 0;
                 index < count;
                 index++)
            {
                var position =
                    new Vector2Int(
                        reader.ReadInt32(),
                        reader.ReadInt32());

                if (!TryGetPlacedGate(
                        position,
                        out _,
                        out _))
                {
                    skipped++;
                    continue;
                }

                if (TrySetGateOpen(
                        position,
                        true,
                        out _,
                        out _))
                {
                    restored++;
                }
            }

            Debug.Log(
                $"[MoyvaConstructionModules] gate-load " +
                $"open={restored} skipped={skipped}");
        }

        public bool IsWallOrGate(string buildingId)
        {
            return _buildingRegistry.GetWallCollectionByBuildingId(buildingId) != null;
        }

        public bool IsWall(string buildingId)
        {
            var collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            return collection != null && collection.IsWall(buildingId);
        }

        public bool IsGate(string buildingId)
        {
            var collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            return collection != null && collection.IsGate(buildingId);
        }

        public bool IsGateOpen(Vector2Int position)
        {
            if (!TryGetPlacedGate(
                    position,
                    out _,
                    out _))
            {
                _gateOpenState.Remove(position);
                return false;
            }

            return _gateOpenState.TryGetValue(
                    position,
                    out bool isOpen)
                && isOpen;
        }

        public bool TrySetGateOpen(
            Vector2Int position,
            bool isOpen,
            out float transitionSeconds,
            out string reason)
        {
            transitionSeconds = 0f;
            reason = null;
            if (!TryGetPlacedGate(
                    position,
                    out string buildingId,
                    out GateBuildingModule gateModule))
            {
                reason = "На цій клітинці немає GateBuildingModule.";
                return false;
            }

            float speed = Mathf.Max(0f, gateModule.OpenSpeed);
            transitionSeconds = speed > 0f
                ? 1f / speed
                : 0f;
            _gateOpenState[position] = isOpen;
            ApplyGateVisualState(
                position,
                isOpen,
                speed);
            Debug.Log(
                $"[MoyvaConstructionModules] gate-state " +
                $"building={buildingId}@{position} open={isOpen} " +
                $"speed={speed:0.###} transition={transitionSeconds:0.###}s");
            return true;
        }

        public bool CanUnitPassGate(
            Vector2Int position,
            string unitOwnerId,
            out string reason)
        {
            reason = null;
            if (!TryGetPlacedGate(
                    position,
                    out _,
                    out _))
            {
                reason = "Ціль не є воротами.";
                return false;
            }

            if (_constructionService.Value
                    is IConstructionBuildingOwnershipQuery ownership
                && ownership.TryGetPlacedBuildingOwner(
                    position,
                    out string gateOwner)
                && !string.IsNullOrWhiteSpace(gateOwner)
                && !string.Equals(
                    gateOwner,
                    string.IsNullOrWhiteSpace(unitOwnerId)
                        ? "player_0"
                        : unitOwnerId.Trim(),
                    System.StringComparison.Ordinal))
            {
                reason = "Ворота належать іншому власнику.";
                return false;
            }

            return true;
        }

        public bool TryEnsureOpenForUnit(
            Vector2Int position,
            string unitOwnerId,
            out string reason)
        {
            if (!CanUnitPassGate(
                    position,
                    unitOwnerId,
                    out reason))
            {
                return false;
            }

            if (IsGateOpen(position))
                return true;

            return TrySetGateOpen(
                position,
                true,
                out _,
                out reason);
        }

        private void ApplyGateVisualState(
            Vector2Int position,
            bool isOpen,
            float openSpeed)
        {
            if (_placedVisuals == null
                || !_placedVisuals.TryGetPlacedVisual(
                    position,
                    out GameObject visual)
                || visual == null)
            {
                return;
            }

            Animator[] animators =
                visual.GetComponentsInChildren<Animator>(true);
            for (int animatorIndex = 0;
                 animatorIndex < animators.Length;
                 animatorIndex++)
            {
                Animator animator = animators[animatorIndex];
                if (animator == null)
                    continue;

                if (openSpeed > 0f)
                    animator.speed = openSpeed;

                bool hasIsOpen = false;
                AnimatorControllerParameter[] parameters =
                    animator.parameters;
                for (int parameterIndex = 0;
                     parameterIndex < parameters.Length;
                     parameterIndex++)
                {
                    AnimatorControllerParameter parameter =
                        parameters[parameterIndex];
                    if (parameter.type == AnimatorControllerParameterType.Bool
                        && string.Equals(
                            parameter.name,
                            "IsOpen",
                            System.StringComparison.Ordinal))
                    {
                        hasIsOpen = true;
                        break;
                    }
                }

                if (hasIsOpen)
                    animator.SetBool("IsOpen", isOpen);
            }
        }

        private bool TryGetPlacedGate(
            Vector2Int position,
            out string buildingId,
            out GateBuildingModule gateModule)
        {
            buildingId = null;
            gateModule = null;
            if (!_objectsMapService.TryGetOccupant(
                    position,
                    out buildingId)
                || string.IsNullOrWhiteSpace(buildingId))
            {
                return false;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(buildingId);
            return BuildingDefinitionCapabilities.TryGetEnabledModule(
                definition,
                out gateModule);
        }

        public bool TryBuildPlacedMask(Vector2Int position, string buildingId, out WallCollectionDefinition collection, out TopologyNeighborMask mask)
        {
            if (!TryGetCollection(buildingId, out collection))
            {
                mask = default;
                return false;
            }

            mask = BuildMask(position, collection, includePendingNeighbors: false);
            return true;
        }

        public bool TryBuildPreviewMask(Vector2Int position, string buildingId, out WallCollectionDefinition collection, out TopologyNeighborMask mask)
        {
            if (!TryGetCollection(buildingId, out collection))
            {
                mask = default;
                return false;
            }

            mask = BuildMask(position, collection, includePendingNeighbors: true);
            return true;
        }

        private bool TryGetCollection(string buildingId, out WallCollectionDefinition collection)
        {
            collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            return collection != null;
        }

        private TopologyNeighborMask BuildMask(Vector2Int position, WallCollectionDefinition collection, bool includePendingNeighbors)
        {
            bool n = IsConnected(position + Vector2Int.up, collection, includePendingNeighbors);
            bool e = IsConnected(position + Vector2Int.right, collection, includePendingNeighbors);
            bool s = IsConnected(position + Vector2Int.down, collection, includePendingNeighbors);
            bool w = IsConnected(position + Vector2Int.left, collection, includePendingNeighbors);

            return new TopologyNeighborMask(
                north: n,
                northEast: false,
                east: e,
                southEast: false,
                south: s,
                southWest: false,
                west: w,
                northWest: false);
        }

        public bool IsHorizontalWallSegment(Vector2Int position, WallCollectionDefinition collection, bool includePendingNeighbors)
        {
            bool n = IsConnected(position + Vector2Int.up, collection, includePendingNeighbors);
            bool e = IsConnected(position + Vector2Int.right, collection, includePendingNeighbors);
            bool s = IsConnected(position + Vector2Int.down, collection, includePendingNeighbors);
            bool w = IsConnected(position + Vector2Int.left, collection, includePendingNeighbors);

            bool hasHorizontalConnection = e || w;
            bool hasVerticalConnection = n || s;
            return hasHorizontalConnection && !hasVerticalConnection;
        }

        private bool IsConnected(Vector2Int position, WallCollectionDefinition collection, bool includePendingNeighbors)
        {
            if (_objectsMapService.TryGetOccupant(position, out var neighborId)
                && collection.ContainsBuilding(neighborId))
            {
                return true;
            }

            if (includePendingNeighbors
                && _constructionService.Value.TryGetPendingBuildingIdAt(position, out var pendingBuildingId)
                && collection.ContainsBuilding(pendingBuildingId))
            {
                return true;
            }

            return false;
        }
    }
}
