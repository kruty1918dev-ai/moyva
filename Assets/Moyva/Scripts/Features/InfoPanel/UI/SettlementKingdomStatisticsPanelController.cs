using System;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    public sealed class SettlementKingdomStatisticsPanelController : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IEconomyStatisticsMediator _statisticsMediator;

        private readonly GameObject _panelRoot;
        private readonly TMP_Text _titleText;
        private readonly TMP_Text _bodyText;
        private readonly Button _settlementTabButton;
        private readonly Button _kingdomTabButton;
        private readonly Button _closeButton;

        private string _currentSettlementId;
        private string _currentOwnerId;

        public SettlementKingdomStatisticsPanelController(
            SignalBus signalBus,
            [InjectOptional] IEconomyStatisticsMediator statisticsMediator,
            [Inject(Id = "StatisticsMenuRoot")] GameObject panelRoot,
            [Inject(Id = "StatisticsMenuTitle")] TMP_Text titleText,
            [Inject(Id = "StatisticsMenuBody")] TMP_Text bodyText,
            [Inject(Id = "StatisticsMenuSettlementButton")] Button settlementTabButton,
            [Inject(Id = "StatisticsMenuKingdomButton")] Button kingdomTabButton,
            [Inject(Id = "StatisticsMenuCloseButton")] Button closeButton)
        {
            _signalBus = signalBus;
            _statisticsMediator = statisticsMediator;
            _panelRoot = panelRoot;
            _titleText = titleText;
            _bodyText = bodyText;
            _settlementTabButton = settlementTabButton;
            _kingdomTabButton = kingdomTabButton;
            _closeButton = closeButton;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<SettlementStatisticsMenuRequestedSignal>(OnSettlementRequested);
            _signalBus.Subscribe<KingdomStatisticsMenuRequestedSignal>(OnKingdomRequested);
            _signalBus.Subscribe<StatisticsMenuClosedSignal>(OnMenuClosed);

            _settlementTabButton?.onClick.AddListener(ShowSettlementStatistics);
            _kingdomTabButton?.onClick.AddListener(ShowKingdomStatistics);
            _closeButton?.onClick.AddListener(CloseMenu);

            SetVisible(false);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<SettlementStatisticsMenuRequestedSignal>(OnSettlementRequested);
            _signalBus.TryUnsubscribe<KingdomStatisticsMenuRequestedSignal>(OnKingdomRequested);
            _signalBus.TryUnsubscribe<StatisticsMenuClosedSignal>(OnMenuClosed);

            _settlementTabButton?.onClick.RemoveListener(ShowSettlementStatistics);
            _kingdomTabButton?.onClick.RemoveListener(ShowKingdomStatistics);
            _closeButton?.onClick.RemoveListener(CloseMenu);
        }

        private void OnSettlementRequested(SettlementStatisticsMenuRequestedSignal signal)
        {
            _currentSettlementId = signal.SettlementId;
            _currentOwnerId = signal.OwnerId;

            ShowSettlementStatistics();
            SetVisible(true);
        }

        private void OnKingdomRequested(KingdomStatisticsMenuRequestedSignal signal)
        {
            _currentOwnerId = signal.OwnerId;
            if (!string.IsNullOrWhiteSpace(signal.PreferredSettlementId))
                _currentSettlementId = signal.PreferredSettlementId;

            ShowKingdomStatistics();
            SetVisible(true);
        }

        private void OnMenuClosed(StatisticsMenuClosedSignal _)
        {
            SetVisible(false);
        }

        private void ShowSettlementStatistics()
        {
            if (_statisticsMediator == null)
            {
                _titleText.text = "Статистика селища";
                _bodyText.text = "Статистика недоступна: mediator не прив'язаний.";
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentSettlementId)
                || !_statisticsMediator.TryGetSettlementStatistics(_currentSettlementId, out var snapshot))
            {
                _titleText.text = "Статистика селища";
                _bodyText.text = "Немає даних для обраного селища.";
                return;
            }

            _titleText.text = $"Селище: {snapshot.SettlementName}";
            _bodyText.text =
                $"Населення: {snapshot.Population}\n" +
                $"Останній хід: прибуло {snapshot.LastArrivals}, померло {snapshot.LastDeaths}\n\n" +
                $"Середні метрики за {snapshot.HistoryTurns} ходів:\n" +
                $"- Середнє прибуття: {snapshot.AvgArrivalsPerTurn:0.##} / хід\n" +
                $"- Середня народжуваність: {snapshot.AvgBirthRatePerTurn * 100f:0.##}% / хід\n" +
                $"- Середня смертність: {snapshot.AvgMortalityPerTurn * 100f:0.##}% / хід\n" +
                $"- Настрій населення: {snapshot.AvgMood:0.#}/100";
        }

        private void ShowKingdomStatistics()
        {
            if (_statisticsMediator == null)
            {
                _titleText.text = "Статистика королівства";
                _bodyText.text = "Статистика недоступна: mediator не прив'язаний.";
                return;
            }

            var snapshot = _statisticsMediator.GetKingdomStatistics(_currentOwnerId);

            _titleText.text = $"Королівство: {snapshot.OwnerId}";
            _bodyText.text =
                $"Активних селищ: {snapshot.ActiveSettlements}\n" +
                $"Загальне населення: {snapshot.TotalPopulation}\n" +
                $"Останній хід: прибуло {snapshot.LastArrivals}, померло {snapshot.LastDeaths}\n\n" +
                $"Середні метрики за {snapshot.HistoryTurns} ходів:\n" +
                $"- Середнє прибуття: {snapshot.AvgArrivalsPerTurn:0.##} / хід\n" +
                $"- Середня народжуваність: {snapshot.AvgBirthRatePerTurn * 100f:0.##}% / хід\n" +
                $"- Середня смертність: {snapshot.AvgMortalityPerTurn * 100f:0.##}% / хід\n" +
                $"- Середній настрій населення: {snapshot.AvgMood:0.#}/100";
        }

        private void CloseMenu()
        {
            _signalBus.Fire(new StatisticsMenuClosedSignal());
        }

        private void SetVisible(bool visible)
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(visible);
        }
    }
}
