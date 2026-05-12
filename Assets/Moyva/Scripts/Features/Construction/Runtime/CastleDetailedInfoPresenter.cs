using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Відповідає за відображення детальної інформації про замок.
    /// Реагує на запити про відкриття панелі замку та обробляє взаємодію з кнопками переміщення.
    /// </summary>
    internal sealed class CastleDetailedInfoPresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IEconomyInfoMediator _economyInfoMediator;
        private readonly EconomyManager _economyManager;

        private string _currentCastleSettlementId;
        private Vector2Int _currentCastlePosition;

        [Inject]
        public CastleDetailedInfoPresenter(
            SignalBus signalBus,
            IBuildingRegistry buildingRegistry,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator,
            [InjectOptional] EconomyManager economyManager)
        {
            _signalBus = signalBus;
            _buildingRegistry = buildingRegistry;
            _economyInfoMediator = economyInfoMediator;
            _economyManager = economyManager;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(OnWorldInfoPanelRequested);
            _signalBus.Subscribe<WorldInfoPanelClosedSignal>(OnWorldInfoPanelClosed);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldInfoPanelRequestedSignal>(OnWorldInfoPanelRequested);
            _signalBus.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnWorldInfoPanelClosed);
        }

        private void OnWorldInfoPanelRequested(WorldInfoPanelRequestedSignal signal)
        {
            // Перевірити чи це замок (замок матиме певну структуру в контенту)
            if (signal.Subtitle != null && signal.Subtitle.Contains("Капітал"))
            {
                // Зберегти контекст для подальших дій
                _currentCastleSettlementId = ExtractSettlementIdFromSubtitle(signal.Subtitle);
            }
        }

        private void OnWorldInfoPanelClosed(WorldInfoPanelClosedSignal signal)
        {
            _currentCastleSettlementId = null;
        }

        /// <summary>
        /// Обробити клік на кнопку переміщення до будівлі.
        /// Називається з UI.
        /// </summary>
        public void NavigateToBuildingAt(Vector2Int position, string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return;

            _signalBus.Fire(new CameraFocusBuildingSignal
            {
                Position = position,
                BuildingId = buildingId,
            });
        }

        private string ExtractSettlementIdFromSubtitle(string subtitle)
        {
            // Очікуємо формат: "Капітал • SettlementName"
            // Нам потрібен settlement ID, но це важко отримати з сабтайтлу
            // Натомість будемо шукати по текущому контексту
            return null;
        }
    }
}
