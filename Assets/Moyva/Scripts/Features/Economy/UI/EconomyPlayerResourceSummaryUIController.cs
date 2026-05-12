using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// UI-контролер, що показує сумарні ресурси по всіх поселеннях конкретного owner.
    /// Виводить агрегати: Food, Materials і Money.
    /// Owner визначається автоматично системою (без поля в Inspector).
    /// </summary>
    public sealed class EconomyPlayerResourceSummaryUIController : MonoBehaviour
    {
        [Header("UI тексти")]
        [SerializeField] private GameObject _textsRoot;
        [SerializeField] private TextMeshProUGUI _totalMaterialsText;
        [SerializeField] private TextMeshProUGUI _totalFoodText;
        [SerializeField] private TextMeshProUGUI _totalMoneyText;

        private IEconomyRuntimeApi _economyApi;
        private SignalBus _signalBus;
        private IConstructionService _constructionService;
        private bool _subscribed;
        private string _ownerId = EconomyManager.DefaultOwnerId;

        [Inject]
        public void Construct(
            IEconomyRuntimeApi economyApi,
            SignalBus signalBus,
            [InjectOptional] IConstructionService constructionService)
        {
            _economyApi = economyApi;
            _signalBus = signalBus;
            _constructionService = constructionService;

            ResolveOwnerFromSystems();
        }

        private void OnEnable()
        {
            ResolveOwnerFromSystems();
            SetTextsVisible(true);
            TrySubscribe();
            RefreshTotals();
        }

        private void Start()
        {
            ResolveOwnerFromSystems();
            SetTextsVisible(true);
            TrySubscribe();
            RefreshTotals();
        }

        private void OnDisable()
        {
            if (!_subscribed || _signalBus == null)
                return;

            _signalBus.TryUnsubscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
            _signalBus.TryUnsubscribe<SettlementCreatedSignal>(OnSettlementCreated);
            _signalBus.TryUnsubscribe<SettlementDeactivatedSignal>(OnSettlementDeactivated);
            _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _subscribed = false;
        }

        public void SetOwner(string ownerId)
        {
            _ownerId = string.IsNullOrWhiteSpace(ownerId)
                ? EconomyManager.DefaultOwnerId
                : ownerId.Trim();

            RefreshTotals();
        }

        private void TrySubscribe()
        {
            if (_subscribed || _signalBus == null)
                return;

            _signalBus.Subscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
            _signalBus.Subscribe<SettlementCreatedSignal>(OnSettlementCreated);
            _signalBus.Subscribe<SettlementDeactivatedSignal>(OnSettlementDeactivated);
            _signalBus.Subscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _subscribed = true;
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            SetTextsVisible(signal.NewMode != GameModeType.Construction);
        }

        private void OnEconomyTickCompleted(EconomyTickCompletedSignal signal)
        {
            if (signal.OwnerId == _ownerId)
                RefreshTotals();
        }

        private void OnSettlementCreated(SettlementCreatedSignal signal)
        {
            if (signal.OwnerId == _ownerId)
                RefreshTotals();
        }

        private void OnSettlementDeactivated(SettlementDeactivatedSignal signal)
        {
            if (signal.OwnerId == _ownerId)
                RefreshTotals();
        }

        private void OnSettlementResourceChanged(SettlementResourceChangedSignal signal)
        {
            if (signal.OwnerId == _ownerId)
                RefreshTotals();
        }

        private void RefreshTotals()
        {
            if (_economyApi == null)
                return;

            var totals = _economyApi.GetFormattedOwnerCategoryTotals(_ownerId);

            if (_totalMaterialsText != null)
                _totalMaterialsText.text = totals.MaterialsText;

            if (_totalFoodText != null)
                _totalFoodText.text = totals.FoodText;

            if (_totalMoneyText != null)
                _totalMoneyText.text = totals.MoneyText;
        }

        private void ResolveOwnerFromSystems()
        {
            var owner = _constructionService?.GetActiveOwner();
            if (string.IsNullOrWhiteSpace(owner))
            {
                _ownerId = EconomyManager.DefaultOwnerId;
                return;
            }

            _ownerId = owner.Trim();
        }

        private void SetTextsVisible(bool isVisible)
        {
            if (_textsRoot == null)
                return;

            if (_textsRoot.activeSelf != isVisible)
                _textsRoot.SetActive(isVisible);
        }
    }
}
