using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class SettlementLabelService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly ConstructionVisualService _visualService;
        private readonly IEconomyInfoMediator _economyInfo;
        private readonly SettlementLabelSettings _settings;
        private readonly ILocalPlayerIdentityProvider _localPlayerIdentityProvider;

        private readonly Dictionary<Vector2Int, GameObject> _labels = new();

        private enum LabelKind { TownHall, Castle }

        [Inject]
        public SettlementLabelService(
            SignalBus signalBus,
            IBuildingRegistry buildingRegistry,
            ConstructionVisualService visualService,
            IEconomyInfoMediator economyInfo,
            SettlementLabelSettings settings,
            [InjectOptional] ILocalPlayerIdentityProvider localPlayerIdentityProvider)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _visualService = visualService;
            _economyInfo = economyInfo;
            _settings = settings;
            _localPlayerIdentityProvider = localPlayerIdentityProvider;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<SettlementCreatedSignal>(OnSettlementCreated);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<SettlementCreatedSignal>(OnSettlementCreated);

            foreach (var kvp in _labels)
            {
                if (kvp.Value != null)
                    UnityEngine.Object.Destroy(kvp.Value);
            }
            _labels.Clear();
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            if (!CanLocalPlayerSeeBuilding(signal.OwnerId))
                return;

            var def = _buildingRegistry.GetById(signal.BuildingId);
            if (def == null) return;

            LabelKind? kind = null;
            if (BuildingDefinitionCapabilities.IsTownHall(def))
                kind = LabelKind.TownHall;
            else if (BuildingDefinitionCapabilities.IsCastle(def))
                kind = LabelKind.Castle;

            if (!kind.HasValue) return;

            if (!_visualService.TryGetPlacedVisual(signal.Position, out var buildingGo))
            {
                Debug.LogWarning($"[SettlementLabel] Візуал будівлі '{signal.BuildingId}' не знайдено на {signal.Position}.");
                return;
            }

            var textSettings = kind.Value == LabelKind.TownHall ? _settings.TownHall : _settings.Castle;
            var label = CreateLabel(buildingGo.transform, signal.Position, textSettings);
            _labels[signal.Position] = label;

            // Назва поселення може ще не існувати (EconomyManager отримає сигнал пізніше).
            // Тому ставимо тимчасовий текст — SettlementCreatedSignal оновить.
            UpdateLabelText(signal.Position);
        }

        private void OnSettlementCreated(SettlementCreatedSignal signal)
        {
            if (_labels.ContainsKey(signal.TownHallPosition))
                UpdateLabelText(signal.TownHallPosition);
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            if (_labels.TryGetValue(signal.Position, out var label))
            {
                if (label != null)
                    UnityEngine.Object.Destroy(label);
                _labels.Remove(signal.Position);
            }
        }

        private void UpdateLabelText(Vector2Int position)
        {
            if (!_labels.TryGetValue(position, out var labelGo) || labelGo == null)
                return;

            var tm = labelGo.GetComponent<TextMesh>();
            if (tm == null) return;

            if (_economyInfo.TryGetSettlementContext(position, out var ctx))
                tm.text = ctx.SettlementName;
            else
                tm.text = string.Empty;
        }

        private bool CanLocalPlayerSeeBuilding(string ownerId)
        {
            if (_localPlayerIdentityProvider == null)
                return true;

            string localPlayerId = _localPlayerIdentityProvider.LocalPlayerId;
            if (string.IsNullOrWhiteSpace(localPlayerId))
                return true;

            if (string.IsNullOrWhiteSpace(ownerId))
                return true;

            return string.Equals(ownerId, localPlayerId, StringComparison.Ordinal);
        }

        private static GameObject CreateLabel(Transform parent, Vector2Int position, SettlementLabelTextSettings s)
        {
            var go = new GameObject($"SettlementLabel_{position.x}_{position.y}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = s.Offset;

            var tm = go.AddComponent<TextMesh>();
            tm.fontSize = s.FontSize;
            tm.color = s.Color;
            tm.anchor = s.Anchor;
            tm.alignment = s.Alignment;
            tm.characterSize = 0.1f;

            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingLayerName = s.SortingLayerName;
                mr.sortingOrder = s.SortingOrder;
            }

            return go;
        }
    }
}
