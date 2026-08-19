using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplayTurnHudPresenter : IInitializable, IDisposable, ITickable
    {
        private readonly ITurnService _turns;
        private readonly IUnitService _units;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IConstructionService _construction;
        private readonly IBuildingRegistry _buildings;
        private readonly SignalBus _signals;
        private readonly GameplayTurnHudView _view;
        private readonly EconomyDatabaseSO _economyDatabase;
        private readonly IReadOnlyList<ITurnBlocker> _blockers;

        private TMP_Text _turnText;
        private TMP_Text _statusText;
        private TMP_Text _unitText;
        private TMP_Text _queueText;
        private Button _endTurnButton;
        private GameObject _recruitmentPanel;
        private GameplayRecruitmentPanelView _recruitmentView;

        private readonly List<Button> _recipeButtons = new();
        private readonly List<string> _recipeUnitTypeIds = new();
        private readonly List<UnitRecruitmentRecipeDefinition> _recipeDefinitions = new();
        private readonly List<UnityAction> _recipeButtonHandlers = new();
        private readonly List<UnityAction> _queueButtonHandlers = new();
        private readonly List<UnitRecruitmentQueueItemSnapshot> _visibleQueueItems = new();

        private UnityAction _hireButtonHandler;
        private UnityAction _closeButtonHandler;
        private string _selectedUnitId;
        private Vector2Int? _selectedBuilding;
        private string _selectedBuildingId;
        private UnitRecruitmentBuildingModule _selectedRecruitmentModule;
        private UnitRecruitmentRecipeDefinition _selectedRecruitmentRecipe;
        private int _selectedRecipeIndex = -1;
        private bool _selectionFromQueue;
        private long _selectedQueueId;
        private string _statusOverride;
        private GameplayTurnHudAuthoritySnapshot _authority;

        public GameplayTurnHudPresenter(
            ITurnService turns,
            IUnitService units,
            IUnitClassConfig unitConfigs,
            IUnitRecruitmentService recruitment,
            IConstructionService construction,
            IBuildingRegistry buildings,
            SignalBus signals,
            GameplayTurnHudView view,
            [InjectOptional] EconomyDatabaseSO economyDatabase = null,
            [InjectOptional] List<ITurnBlocker> blockers = null)
        {
            _turns = turns;
            _units = units;
            _unitConfigs = unitConfigs;
            _recruitment = recruitment;
            _construction = construction;
            _buildings = buildings;
            _signals = signals;
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _economyDatabase = economyDatabase;
            _blockers = blockers ?? (IReadOnlyList<ITurnBlocker>)Array.Empty<ITurnBlocker>();
        }

        public void Initialize()
        {
            BindSceneView();
            _turns.StateChanged += OnTurnStateChanged;
            _signals.Subscribe<UnitInfoPanelRequestedSignal>(OnUnitSelected);
            _signals.Subscribe<BuildingInfoPanelRequestedSignal>(OnBuildingSelected);
            _signals.Subscribe<WorldInfoPanelClosedSignal>(OnSelectionClosed);
            _signals.Subscribe<UnitRecruitmentQueueChangedSignal>(OnRecruitmentQueueChanged);
            _signals.Subscribe<UnitRecruitmentReadySignal>(OnRecruitmentReady);
            _signals.Subscribe<UnitRecruitmentDeployedSignal>(OnRecruitmentDeployed);
            RefreshTurnAuthority();
            RefreshUnit();
            RefreshQueue();
        }

        public void Dispose()
        {
            _turns.StateChanged -= OnTurnStateChanged;
            _signals.TryUnsubscribe<UnitInfoPanelRequestedSignal>(OnUnitSelected);
            _signals.TryUnsubscribe<BuildingInfoPanelRequestedSignal>(OnBuildingSelected);
            _signals.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnSelectionClosed);
            _signals.TryUnsubscribe<UnitRecruitmentQueueChangedSignal>(OnRecruitmentQueueChanged);
            _signals.TryUnsubscribe<UnitRecruitmentReadySignal>(OnRecruitmentReady);
            _signals.TryUnsubscribe<UnitRecruitmentDeployedSignal>(OnRecruitmentDeployed);

            if (_endTurnButton != null)
                _endTurnButton.onClick.RemoveListener(OnEndTurn);

            if (_recruitmentView != null
                && _recruitmentView.HireButton != null
                && _hireButtonHandler != null)
            {
                _recruitmentView.HireButton.onClick.RemoveListener(_hireButtonHandler);
            }

            if (_recruitmentView != null
                && _recruitmentView.CloseButton != null
                && _closeButtonHandler != null)
            {
                _recruitmentView.CloseButton.onClick.RemoveListener(_closeButtonHandler);
            }

            for (int index = 0; index < _recipeButtons.Count && index < _recipeButtonHandlers.Count; index++)
            {
                Button button = _recipeButtons[index];
                UnityAction handler = _recipeButtonHandlers[index];
                if (button != null && handler != null)
                    button.onClick.RemoveListener(handler);
            }

            if (_recruitmentView != null)
            {
                for (int index = 0; index < _recruitmentView.QueueRowCount && index < _queueButtonHandlers.Count; index++)
                {
                    Button button = _recruitmentView.GetQueueButton(index);
                    UnityAction handler = _queueButtonHandlers[index];
                    if (button != null && handler != null)
                        button.onClick.RemoveListener(handler);
                }
            }
        }

        public void Tick()
        {
            // ITurnBlocker state may change asynchronously without StateChanged.
            RefreshTurnAuthority();
            RefreshUnit();
            RefreshQueue();
        }

        private void BindSceneView()
        {
            _view.ValidateConfiguration();

            _turnText = _view.TurnText;
            _statusText = _view.StatusText;
            _unitText = _view.UnitText;
            _queueText = _view.QueueText;
            _endTurnButton = _view.EndTurnButton;
            _recruitmentPanel = _view.RecruitmentPanel;
            _recruitmentView = _recruitmentPanel.GetComponent<GameplayRecruitmentPanelView>();

            if (_recruitmentView == null)
            {
                throw new InvalidOperationException(
                    "GameplayTurnHud recruitment panel is missing GameplayRecruitmentPanelView. Apply P24B scene authoring.");
            }

            _recruitmentView.ValidateConfiguration(_view.RecipeSlotCount);
            _endTurnButton.onClick.AddListener(OnEndTurn);

            _recipeButtons.Clear();
            _recipeUnitTypeIds.Clear();
            _recipeDefinitions.Clear();
            _recipeButtonHandlers.Clear();

            for (int index = 0; index < _view.RecipeSlotCount; index++)
            {
                Button button = _view.GetRecipeButton(index);
                int capturedIndex = index;
                UnityAction handler = () => OnRecipeSlotClicked(capturedIndex);
                button.onClick.AddListener(handler);
                button.gameObject.SetActive(false);

                _recipeButtons.Add(button);
                _recipeUnitTypeIds.Add(null);
                _recipeDefinitions.Add(null);
                _recipeButtonHandlers.Add(handler);

                Image icon = _recruitmentView.GetRecipeIcon(index);
                icon.sprite = null;
                icon.enabled = false;
                _recruitmentView.GetRecipeNameText(index).text = string.Empty;
                _recruitmentView.SetRecipeTrainingText(index, string.Empty);
            }

            _recruitmentView.SetSelectedRecipeIndex(-1);

            _queueButtonHandlers.Clear();
            for (int index = 0; index < _recruitmentView.QueueRowCount; index++)
            {
                int capturedIndex = index;
                Button button = _recruitmentView.GetQueueButton(index);
                UnityAction handler = () => OnQueueSlotClicked(capturedIndex);
                button.onClick.AddListener(handler);
                button.gameObject.SetActive(false);
                _queueButtonHandlers.Add(handler);
            }

            _hireButtonHandler = OnHireClicked;
            _recruitmentView.HireButton.onClick.AddListener(_hireButtonHandler);
            _closeButtonHandler = CloseRecruitmentPanel;
            _recruitmentView.CloseButton.onClick.AddListener(_closeButtonHandler);
            _recruitmentView.SelectionPanel.SetActive(false);
            _recruitmentPanel.SetActive(false);
            _recruitmentView.SetHeader(string.Empty, string.Empty);
            _recruitmentView.SetStateChip(string.Empty, Color.clear);
            _recruitmentView.SetActionHint(string.Empty);
            _recruitmentView.SetQueueSummary("Черга", "0/0", empty: true);
            _queueText.text = string.Empty;
            _unitText.text = string.Empty;
            _unitText.transform.parent.gameObject.SetActive(false);
            ClearCostRows();
            _recruitmentView.ClearStatRows();
            ClearBuildingRecruitmentState();
        }

        private void OnTurnStateChanged()
        {
            _statusOverride = null;
            RefreshTurnAuthority();
            RefreshQueue();
        }

        private void OnRecruitmentQueueChanged(UnitRecruitmentQueueChangedSignal signal)
        {
            if (IsSignalForSelectedBuilding(signal.OwnerId, signal.BuildingPosition))
            {
                RefreshQueue();
                RefreshTurnAuthority();
            }
        }

        private void OnRecruitmentReady(UnitRecruitmentReadySignal signal)
        {
            if (IsSignalForSelectedBuilding(signal.OwnerId, signal.BuildingPosition))
            {
                _statusOverride = $"{ResolveUnitName(signal.UnitTypeId)} готовий до розміщення.";
                RefreshQueue();
                RefreshTurnAuthority();
            }
        }

        private void OnRecruitmentDeployed(UnitRecruitmentDeployedSignal signal)
        {
            if (IsSignalForSelectedBuilding(signal.OwnerId, signal.BuildingPosition))
            {
                if (_selectionFromQueue && _selectedQueueId == signal.QueueId)
                    SelectFirstRecipeIfAvailable();

                _statusOverride = $"{ResolveUnitName(signal.UnitTypeId)} розміщено.";
                RefreshQueue();
                RefreshTurnAuthority();
            }
        }

        private bool IsSignalForSelectedBuilding(string ownerId, Vector2Int position)
        {
            return _selectedBuilding.HasValue
                && _selectedBuilding.Value == position
                && string.Equals(
                    ownerId?.Trim(),
                    _turns.LocalOwnerId?.Trim(),
                    StringComparison.Ordinal);
        }

        private void RefreshTurnAuthority()
        {
            _authority = GameplayTurnHudAuthorityPolicy.Evaluate(_turns, _blockers);
            string actor = _authority.IsActiveFactionBot ? "Бот" : "Фракція";
            string owner = string.IsNullOrWhiteSpace(_authority.ActiveOwnerId)
                ? "—"
                : _authority.ActiveOwnerId;

            _turnText.text =
                $"{actor}: {owner}    Раунд {_turns.Round}    Хід {_turns.GlobalTurn}\n" +
                $"Фаза {GameplayTurnHudAuthorityPolicy.LocalizePhase(_authority.Phase)}    Дії {_turns.ActionsThisTurn}";
            _endTurnButton.interactable = _authority.CanEndTurn;

            bool blockersTakePriority = _authority.IsLocalOwnerTurn
                && _authority.BlockingReasons.Count > 0;
            _statusText.text = blockersTakePriority || string.IsNullOrWhiteSpace(_statusOverride)
                ? _authority.StatusText
                : _statusOverride;

            RefreshRecruitmentAuthority();
        }

        private void OnEndTurn()
        {
            GameplayTurnHudAuthoritySnapshot current =
                GameplayTurnHudAuthorityPolicy.Evaluate(_turns, _blockers);
            if (!current.CanEndTurn)
            {
                _statusOverride = string.IsNullOrWhiteSpace(current.StatusText)
                    ? "Завершення ходу зараз недоступне."
                    : current.StatusText;
                RefreshTurnAuthority();
                return;
            }

            if (!_turns.TryEndTurn(current.LocalOwnerId, out string reason))
                _statusOverride = string.IsNullOrWhiteSpace(reason)
                    ? "Система ходів відхилила завершення ходу."
                    : reason;
            else
                _statusOverride = null;

            RefreshTurnAuthority();
        }

        private void OnUnitSelected(UnitInfoPanelRequestedSignal signal)
        {
            _selectedUnitId = signal.UnitId;
            _selectedBuilding = null;
            _selectedBuildingId = null;
            _selectedRecruitmentModule = null;
            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _statusOverride = null;
            _recruitmentPanel.SetActive(false);
            _recruitmentView.SetSelectedRecipeIndex(-1);
            _recruitmentView.SetHeader(string.Empty, string.Empty);
        }

        private void OnBuildingSelected(BuildingInfoPanelRequestedSignal signal)
        {
            _selectedUnitId = null;
            _selectedBuilding = signal.Position;
            _selectedBuildingId = signal.BuildingId;
            _statusOverride = null;
            BuildRecipeButtons(signal.BuildingId);
        }

        private void OnSelectionClosed(WorldInfoPanelClosedSignal _)
        {
            _selectedUnitId = null;
            _selectedBuilding = null;
            _selectedBuildingId = null;
            _selectedRecruitmentModule = null;
            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _statusOverride = null;
            _recruitmentPanel.SetActive(false);
            _recruitmentView.SetSelectedRecipeIndex(-1);
            _recruitmentView.SetHeader(string.Empty, string.Empty);
            ClearBuildingRecruitmentState();
        }

        private void CloseRecruitmentPanel()
        {
            _selectedBuilding = null;
            _selectedBuildingId = null;
            _selectedRecruitmentModule = null;
            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _statusOverride = null;
            _recruitmentPanel.SetActive(false);
            _recruitmentView.SetSelectedRecipeIndex(-1);
            _recruitmentView.SetHeader(string.Empty, string.Empty);
            ClearBuildingRecruitmentState();
            HideQueueRows();
            ClearCostRows();
            _recruitmentView.ClearStatRows();
        }

        private void BuildRecipeButtons(string buildingId)
        {
            ClearRecipeSlots();
            _selectedRecruitmentModule = null;

            if (!IsSelectedBuildingOwnedByLocalPlayer())
            {
                _recruitmentPanel.SetActive(false);
                return;
            }

            BuildingDefinition definition = _buildings.GetById(buildingId);
            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out UnitRecruitmentBuildingModule module))
            {
                _recruitmentPanel.SetActive(false);
                return;
            }

            _selectedRecruitmentModule = module;
            _recruitmentPanel.SetActive(true);
            _recruitmentView.SetHeader(ResolveBuildingName(definition), "Найм військ");
            int slotIndex = 0;
            int skippedForCapacity = 0;

            if (module.Recipes != null)
            {
                foreach (UnitRecruitmentRecipeDefinition recipe in module.Recipes)
                {
                    if (recipe == null || string.IsNullOrWhiteSpace(recipe.UnitTypeId))
                        continue;

                    if (slotIndex >= _recipeButtons.Count)
                    {
                        skippedForCapacity++;
                        continue;
                    }

                    string unitTypeId = recipe.UnitTypeId.Trim();
                    Button button = _recipeButtons[slotIndex];
                    TMP_Text label = _view.GetRecipeLabel(slotIndex);
                    UnitClassConfig config = _unitConfigs.GetConfig(unitTypeId);
                    Image icon = _recruitmentView.GetRecipeIcon(slotIndex);

                    _recipeUnitTypeIds[slotIndex] = unitTypeId;
                    _recipeDefinitions[slotIndex] = recipe;
                    label.text = ResolveUnitName(unitTypeId);
                    _recruitmentView.GetRecipeNameText(slotIndex).text =
                        ResolveUnitName(unitTypeId);
                    _recruitmentView.SetRecipeTrainingText(
                        slotIndex,
                        $"{Mathf.Max(1, recipe.TrainingTurns)} р.");

                    icon.sprite = config?.ResolveCustomSprite();
                    icon.enabled = icon.sprite != null;
                    button.gameObject.SetActive(true);
                    slotIndex++;
                }
            }

            if (skippedForCapacity > 0)
            {
                Debug.LogWarning(
                    $"[GameplayTurnHud] Recruitment recipes exceed authored scene slots. " +
                    $"Visible={_recipeButtons.Count}, skipped={skippedForCapacity}.");
            }

            SelectFirstRecipeIfAvailable();
            RefreshRecruitmentAuthority();
            RefreshQueue();
        }

        private void ClearRecipeSlots()
        {
            for (int index = 0; index < _recipeButtons.Count; index++)
            {
                _recipeUnitTypeIds[index] = null;
                _recipeDefinitions[index] = null;
                Button button = _recipeButtons[index];
                if (button != null)
                    button.gameObject.SetActive(false);

                TMP_Text label = _view.GetRecipeLabel(index);
                if (label != null)
                    label.text = string.Empty;

                if (_recruitmentView != null && index < _recruitmentView.RecipeIconCount)
                {
                    Image icon = _recruitmentView.GetRecipeIcon(index);
                    icon.sprite = null;
                    icon.enabled = false;
                    _recruitmentView.GetRecipeNameText(index).text = string.Empty;
                    _recruitmentView.SetRecipeTrainingText(index, string.Empty);
                }
            }

            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            if (_recruitmentView != null)
            {
                _recruitmentView.SelectionPanel.SetActive(false);
                _recruitmentView.SetSelectedRecipeIndex(-1);
                _recruitmentView.ClearStatRows();
                _recruitmentView.SetActionHint(string.Empty);
            }
            ClearCostRows();
        }

        private void OnRecipeSlotClicked(int index)
        {
            if (index < 0 || index >= _recipeDefinitions.Count)
                return;

            UnitRecruitmentRecipeDefinition recipe = _recipeDefinitions[index];
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.UnitTypeId))
                return;

            _selectedRecruitmentRecipe = recipe;
            _selectedRecipeIndex = index;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _recruitmentView.SetSelectedRecipeIndex(index);
            RefreshSelectionDetails(recipe.UnitTypeId, recipe, null);
            RefreshRecruitmentAuthority();
        }

        private void OnQueueSlotClicked(int index)
        {
            if (index < 0 || index >= _visibleQueueItems.Count)
                return;

            UnitRecruitmentQueueItemSnapshot job = _visibleQueueItems[index];
            UnitRecruitmentRecipeDefinition recipe = ResolveRecipe(job.UnitTypeId);
            _selectedRecruitmentRecipe = recipe;
            _selectionFromQueue = true;
            _selectedQueueId = job.QueueId;
            _selectedRecipeIndex = FindRecipeIndex(job.UnitTypeId);
            _recruitmentView.SetSelectedRecipeIndex(_selectedRecipeIndex);
            RefreshSelectionDetails(job.UnitTypeId, recipe, job);
            RefreshRecruitmentAuthority();
        }

        private void OnHireClicked()
        {
            if (_selectionFromQueue
                || _selectedRecruitmentRecipe == null
                || string.IsNullOrWhiteSpace(_selectedRecruitmentRecipe.UnitTypeId))
            {
                return;
            }

            Enqueue(_selectedRecruitmentRecipe.UnitTypeId);
        }

        private void Enqueue(string unitTypeId)
        {
            RefreshTurnAuthority();
            if (!_selectedBuilding.HasValue)
                return;

            if (!_authority.CanIssueLocalCommands)
            {
                _statusOverride = _authority.StatusText;
                RefreshTurnAuthority();
                return;
            }

            if (!IsSelectedBuildingOwnedByLocalPlayer())
            {
                _statusOverride = "Найм доступний лише у власній будівлі.";
                RefreshTurnAuthority();
                return;
            }

            if (!IsSelectedBuildingOperational())
            {
                _statusOverride =
                    "Військова будівля ще будується. Найм стане доступний після завершення будівництва.";
                RefreshTurnAuthority();
                RefreshQueue();
                return;
            }

            if (!_recruitment.TryEnqueue(
                    _authority.LocalOwnerId,
                    _selectedBuilding.Value,
                    unitTypeId,
                    out string reason))
            {
                _statusOverride = PresentRecruitmentFailure(reason);
            }
            else
            {
                _statusOverride =
                    $"{ResolveUnitName(unitTypeId)} додано до черги. " +
                    "Стан навчання показано у верхньому блоці казарми.";
            }

            RefreshTurnAuthority();
            RefreshQueue();
        }

        private static string PresentRecruitmentFailure(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return "Найм не вдалося розпочати.";

            string value = reason.Trim();
            return value switch
            {
                "Recruiting building is still under construction." =>
                    "Військова будівля ще будується. Найм стане доступний після завершення будівництва.",
                "Recruiting building belongs to another owner." =>
                    "Найм доступний лише у власній будівлі.",
                "Recruiting building is not a committed construction placement." =>
                    "Будівля ще не готова до використання.",
                "Building has no enabled unit recruitment module." =>
                    "Ця будівля не підтримує найм юнітів.",
                "Turn authority is unavailable for recruitment." =>
                    "Найм тимчасово недоступний: немає активного ходу.",
                "Construction recruitment context is unavailable." =>
                    "Найм тимчасово недоступний через стан будівництва.",
                "Economy is unavailable for recruitment costs." =>
                    "Неможливо перевірити ресурси для найму.",
                "No owned settlement is available to fund recruitment at this building." =>
                    "Будівля не підключена до власного поселення, яке може оплатити найм.",
                _ when value.StartsWith("Building cannot recruit unit type '", StringComparison.Ordinal) =>
                    "Ця будівля не може наймати вибраний тип юніта.",
                _ when value.StartsWith("Unit type '", StringComparison.Ordinal)
                    && value.EndsWith("' is not registered.", StringComparison.Ordinal) =>
                    "Вибраний тип юніта не зареєстрований у грі.",
                _ => value,
            };
        }

        private void RefreshRecruitmentAuthority()
        {
            if (_recruitmentPanel == null || !_recruitmentPanel.activeSelf)
                return;

            bool ownsBuilding = IsSelectedBuildingOwnedByLocalPlayer();
            bool operational = IsSelectedBuildingOperational();
            bool queueFull = false;
            if (_selectedBuilding.HasValue && _selectedRecruitmentModule != null)
            {
                IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                    _recruitment.GetQueue(_turns.LocalOwnerId, _selectedBuilding.Value);
                queueFull = (queue?.Count ?? 0)
                    >= Mathf.Max(1, _selectedRecruitmentModule.QueueCapacity);
            }

            bool canRecruit = _authority.CanIssueLocalCommands
                && ownsBuilding
                && operational
                && !_selectionFromQueue
                && _selectedRecruitmentRecipe != null
                && !queueFull;

            for (int index = 0; index < _recipeButtons.Count; index++)
            {
                Button button = _recipeButtons[index];
                if (button != null)
                {
                    // Recipe cards remain inspectable outside the local action phase.
                    button.interactable = ownsBuilding
                        && !string.IsNullOrWhiteSpace(_recipeUnitTypeIds[index]);
                }
            }

            if (_recruitmentView != null)
            {
                _recruitmentView.HireButton.interactable = canRecruit;
                _recruitmentView.HireButtonLabel.text = "Найняти";
                _recruitmentView.SetActionHint(ResolveRecruitmentActionHint(
                    ownsBuilding,
                    operational,
                    queueFull,
                    canRecruit));
            }
        }

        private bool IsSelectedBuildingOwnedByLocalPlayer()
        {
            if (!_selectedBuilding.HasValue
                || string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
                || _construction is not IConstructionBuildingOwnershipQuery ownership)
            {
                return false;
            }

            return ownership.TryGetPlacedBuildingOwner(
                       _selectedBuilding.Value,
                       out string ownerId)
                && string.Equals(
                    ownerId?.Trim(),
                    _turns.LocalOwnerId.Trim(),
                    StringComparison.Ordinal);
        }

        private bool IsSelectedBuildingOperational()
        {
            if (!_selectedBuilding.HasValue)
                return false;

            return _construction is not IConstructionLifecycle lifecycle
                || lifecycle.IsOperational(_selectedBuilding.Value);
        }

        private void RefreshUnit()
        {
            if (string.IsNullOrWhiteSpace(_selectedUnitId))
            {
                _unitText.text = string.Empty;
                _unitText.transform.parent.gameObject.SetActive(false);
                return;
            }

            _unitText.transform.parent.gameObject.SetActive(true);
            string typeId = _units.GetUnitTypeId(_selectedUnitId);
            UnitClassConfig config = _unitConfigs.GetConfig(typeId);
            _unitText.text =
                $"{ResolveUnitName(typeId)}    Stamina " +
                $"{_units.GetStamina(_selectedUnitId):0.#}/{config?.BaseStamina ?? 0f:0.#}";
        }

        private void RefreshQueue()
        {
            if (_recruitmentView == null)
                return;

            if (!_selectedBuilding.HasValue
                || _recruitmentPanel == null
                || !_recruitmentPanel.activeSelf
                || !IsSelectedBuildingOwnedByLocalPlayer())
            {
                HideQueueRows();
                _recruitmentView.SetQueueSummary("Черга", "0/0", empty: true);
                ClearBuildingRecruitmentState();
                return;
            }

            int selectedCapacity = Mathf.Max(1, _selectedRecruitmentModule?.QueueCapacity ?? 1);

            if (_construction is IConstructionLifecycle lifecycle
                && lifecycle.TryGetProgress(
                    _selectedBuilding.Value,
                    out int completed,
                    out int required)
                && completed < required)
            {
                _queueText.text = $"Черга 0/{selectedCapacity}";
                _recruitmentView.SetQueueSummary("Черга", $"0/{selectedCapacity}", empty: true);
                HideQueueRows();
                SetBuildingRecruitmentState(
                    "Будівля будується",
                    $"{completed}/{required}",
                    required > 0 ? Mathf.Clamp01((float)completed / required) : 0f,
                    "БУДУЄТЬСЯ",
                    new Color32(142, 158, 174, 255));
                return;
            }

            // Some lifecycle implementations may not expose progress during every
            // construction phase. Operational state is authoritative for recruitment,
            // so never present a non-operational building as "free".
            if (!IsSelectedBuildingOperational())
            {
                _queueText.text = $"Черга 0/{selectedCapacity}";
                _recruitmentView.SetQueueSummary("Черга", $"0/{selectedCapacity}", empty: true);
                HideQueueRows();
                SetBuildingRecruitmentState(
                    "Будівля будується",
                    "Найм стане доступний одразу після завершення будівництва.",
                    0f,
                    "БУДУЄТЬСЯ",
                    new Color32(142, 158, 174, 255));
                return;
            }

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                _recruitment.GetQueue(
                    _turns.LocalOwnerId,
                    _selectedBuilding.Value);

            _visibleQueueItems.Clear();
            int capacity = selectedCapacity;
            int queueCount = queue?.Count ?? 0;
            int readyCount = 0;

            if (queue != null)
            {
                for (int index = 0; index < queue.Count; index++)
                {
                    if (queue[index].IsReady)
                        readyCount++;
                }
            }

            _queueText.text = queueCount == 0
                ? $"Черга 0/{capacity}"
                : $"Черга {queueCount}/{capacity}" + (readyCount > 0 ? $" · готово {readyCount}" : string.Empty);
            _recruitmentView.SetQueueSummary("Черга", $"{queueCount}/{capacity}", queueCount == 0);

            RefreshBuildingRecruitmentState(queue, capacity);

            for (int index = 0; index < _recruitmentView.QueueRowCount; index++)
            {
                Button button = _recruitmentView.GetQueueButton(index);
                Image icon = _recruitmentView.GetQueueIcon(index);
                TMP_Text label = _recruitmentView.GetQueueLabel(index);
                Image progressFill = _recruitmentView.GetQueueProgressFill(index);

                if (queue == null || index >= queue.Count)
                {
                    button.gameObject.SetActive(false);
                    icon.sprite = null;
                    icon.enabled = false;
                    label.text = string.Empty;
                    if (progressFill != null)
                        progressFill.fillAmount = 0f;
                    continue;
                }

                UnitRecruitmentQueueItemSnapshot job = queue[index];
                _visibleQueueItems.Add(job);
                UnitClassConfig config = _unitConfigs.GetConfig(job.UnitTypeId);
                icon.sprite = config?.ResolveCustomSprite();
                icon.enabled = icon.sprite != null;

                if (job.IsReady)
                {
                    label.text = $"{ResolveUnitName(job.UnitTypeId)}   Готовий";
                    if (progressFill != null)
                        progressFill.fillAmount = 1f;
                }
                else if (index == 0)
                {
                    label.text =
                        $"{ResolveUnitName(job.UnitTypeId)}   Навчання {job.CompletedTurns}/{job.TrainingTurns}";
                    if (progressFill != null)
                    {
                        progressFill.fillAmount = job.TrainingTurns > 0
                            ? Mathf.Clamp01((float)job.CompletedTurns / job.TrainingTurns)
                            : 0f;
                    }
                }
                else
                {
                    label.text = $"{ResolveUnitName(job.UnitTypeId)}   Очікує";
                    if (progressFill != null)
                        progressFill.fillAmount = 0f;
                }

                button.gameObject.SetActive(true);
                button.interactable = true;
            }

            if (_selectionFromQueue)
            {
                UnitRecruitmentQueueItemSnapshot? selected = FindVisibleQueueItem(_selectedQueueId);
                if (selected.HasValue)
                {
                    UnitRecruitmentRecipeDefinition recipe = ResolveRecipe(selected.Value.UnitTypeId);
                    RefreshSelectionDetails(selected.Value.UnitTypeId, recipe, selected.Value);
                }
                else
                {
                    SelectFirstRecipeIfAvailable();
                }
            }
        }

        private void RefreshBuildingRecruitmentState(
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue,
            int capacity)
        {
            int count = queue?.Count ?? 0;
            if (count == 0)
            {
                SetBuildingRecruitmentState(
                    "Вільна",
                    $"Черга 0/{capacity}",
                    0f,
                    "ВІЛЬНА",
                    new Color32(92, 154, 98, 255));
                return;
            }

            UnitRecruitmentQueueItemSnapshot head = queue[0];
            int waiting = Mathf.Max(0, count - 1);

            if (head.IsReady)
            {
                SetBuildingRecruitmentState(
                    $"{ResolveUnitName(head.UnitTypeId)} готовий",
                    waiting > 0 ? $"Очікує розміщення · у черзі {waiting}" : "Очікує розміщення",
                    1f,
                    "ГОТОВО",
                    new Color32(91, 176, 96, 255));
                return;
            }

            float progress = head.TrainingTurns > 0
                ? Mathf.Clamp01((float)head.CompletedTurns / head.TrainingTurns)
                : 0f;
            string suffix = waiting > 0
                ? $" • після нього очікує: {waiting}"
                : string.Empty;

            SetBuildingRecruitmentState(
                $"Навчається: {ResolveUnitName(head.UnitTypeId)}",
                $"{head.CompletedTurns}/{head.TrainingTurns} раунди · " +
                $"залишилось {head.RemainingTurns}{suffix}",
                progress,
                "НАВЧАННЯ",
                new Color32(190, 132, 62, 255));
        }

        private void SetBuildingRecruitmentState(
            string title,
            string detail,
            float progress,
            string chipLabel,
            Color chipColor)
        {
            if (_recruitmentView == null
                || _recruitmentView.BuildingStatePanel == null)
            {
                return;
            }

            _recruitmentView.BuildingStatePanel.SetActive(true);
            _recruitmentView.BuildingStateTitle.text = title ?? string.Empty;
            _recruitmentView.BuildingStateDetail.text = detail ?? string.Empty;
            if (_recruitmentView.BuildingStateProgressFill != null)
                _recruitmentView.BuildingStateProgressFill.fillAmount =
                    Mathf.Clamp01(progress);
            _recruitmentView.SetStateChip(chipLabel, chipColor);
        }

        private void ClearBuildingRecruitmentState()
        {
            if (_recruitmentView == null
                || _recruitmentView.BuildingStatePanel == null)
            {
                return;
            }

            _recruitmentView.BuildingStatePanel.SetActive(false);
            if (_recruitmentView.BuildingStateTitle != null)
                _recruitmentView.BuildingStateTitle.text = string.Empty;
            if (_recruitmentView.BuildingStateDetail != null)
                _recruitmentView.BuildingStateDetail.text = string.Empty;
            if (_recruitmentView.BuildingStateProgressFill != null)
                _recruitmentView.BuildingStateProgressFill.fillAmount = 0f;
            _recruitmentView.SetStateChip(string.Empty, Color.clear);
        }

        private UnitRecruitmentQueueItemSnapshot? FindVisibleQueueItem(long queueId)
        {
            for (int index = 0; index < _visibleQueueItems.Count; index++)
            {
                if (_visibleQueueItems[index].QueueId == queueId)
                    return _visibleQueueItems[index];
            }

            return null;
        }

        private void HideQueueRows()
        {
            _visibleQueueItems.Clear();
            if (_recruitmentView == null)
                return;

            for (int index = 0; index < _recruitmentView.QueueRowCount; index++)
            {
                Button button = _recruitmentView.GetQueueButton(index);
                Image icon = _recruitmentView.GetQueueIcon(index);
                TMP_Text label = _recruitmentView.GetQueueLabel(index);
                Image progressFill = _recruitmentView.GetQueueProgressFill(index);
                button.gameObject.SetActive(false);
                icon.sprite = null;
                icon.enabled = false;
                label.text = string.Empty;
                if (progressFill != null)
                    progressFill.fillAmount = 0f;
            }
        }

        private void SelectFirstRecipeIfAvailable()
        {
            for (int index = 0; index < _recipeDefinitions.Count; index++)
            {
                if (_recipeDefinitions[index] == null)
                    continue;

                _selectedRecruitmentRecipe = _recipeDefinitions[index];
                _selectedRecipeIndex = index;
                _selectionFromQueue = false;
                _selectedQueueId = 0;
                _recruitmentView.SetSelectedRecipeIndex(index);
                RefreshSelectionDetails(
                    _selectedRecruitmentRecipe.UnitTypeId,
                    _selectedRecruitmentRecipe,
                    null);
                return;
            }

            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _recruitmentView.SetSelectedRecipeIndex(-1);
            _recruitmentView.SelectionPanel.SetActive(false);
            _recruitmentView.ClearStatRows();
            _recruitmentView.SetActionHint(string.Empty);
            ClearCostRows();
        }

        private UnitRecruitmentRecipeDefinition ResolveRecipe(string unitTypeId)
        {
            if (_selectedRecruitmentModule?.Recipes == null || string.IsNullOrWhiteSpace(unitTypeId))
                return null;

            for (int index = 0; index < _selectedRecruitmentModule.Recipes.Count; index++)
            {
                UnitRecruitmentRecipeDefinition recipe = _selectedRecruitmentModule.Recipes[index];
                if (recipe != null
                    && string.Equals(recipe.UnitTypeId?.Trim(), unitTypeId.Trim(), StringComparison.Ordinal))
                {
                    return recipe;
                }
            }

            return null;
        }

        private int FindRecipeIndex(string unitTypeId)
        {
            if (string.IsNullOrWhiteSpace(unitTypeId))
                return -1;

            string normalized = unitTypeId.Trim();
            for (int index = 0; index < _recipeUnitTypeIds.Count; index++)
            {
                if (string.Equals(
                        _recipeUnitTypeIds[index]?.Trim(),
                        normalized,
                        StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private string ResolveRecruitmentActionHint(
            bool ownsBuilding,
            bool operational,
            bool queueFull,
            bool canRecruit)
        {
            if (_selectionFromQueue)
            {
                UnitRecruitmentQueueItemSnapshot? selected =
                    FindVisibleQueueItem(_selectedQueueId);
                if (selected.HasValue && selected.Value.IsReady)
                    return "Готовий юніт розміщується через індикатор біля будівлі.";
                return _selectedQueueId > 0 ? "Цей запис уже в черзі." : string.Empty;
            }

            if (!ownsBuilding)
                return "Найм доступний лише у власній будівлі.";
            if (!operational)
                return "Будівля будується.";
            if (!_authority.CanIssueLocalCommands)
                return "Зараз не ваш хід.";
            if (_selectedRecruitmentRecipe == null)
                return "Оберіть юніта.";
            if (queueFull)
                return "Черга заповнена.";

            return canRecruit ? string.Empty : "Найм зараз недоступний.";
        }

        private void RefreshSelectionDetails(
            string unitTypeId,
            UnitRecruitmentRecipeDefinition recipe,
            UnitRecruitmentQueueItemSnapshot? queueItem)
        {
            if (_recruitmentView == null || string.IsNullOrWhiteSpace(unitTypeId))
                return;

            UnitClassConfig config = _unitConfigs.GetConfig(unitTypeId);
            _recruitmentView.SetSelectedRecipeIndex(
                queueItem.HasValue ? FindRecipeIndex(unitTypeId) : _selectedRecipeIndex);
            _recruitmentView.SelectionPanel.SetActive(true);
            _recruitmentView.SelectionName.text = ResolveUnitName(unitTypeId);
            _recruitmentView.SelectionClass.text =
                config != null ? LocalizeCombatType(config.CombatType) : string.Empty;
            _recruitmentView.SelectionIcon.sprite = config?.ResolveCustomSprite();
            _recruitmentView.SelectionIcon.enabled = _recruitmentView.SelectionIcon.sprite != null;
            if (_recruitmentView.SelectionStats != null)
                _recruitmentView.SelectionStats.text = string.Empty;

            int trainingTurns = Mathf.Max(1, recipe?.TrainingTurns ?? queueItem?.TrainingTurns ?? 1);
            string progress = queueItem.HasValue
                ? queueItem.Value.IsReady
                    ? "Готовий до розміщення"
                    : queueItem.Value.QueueId == (_visibleQueueItems.Count > 0 ? _visibleQueueItems[0].QueueId : -1)
                        ? $"Навчається {queueItem.Value.CompletedTurns}/{queueItem.Value.TrainingTurns}"
                        : $"Очікує {queueItem.Value.CompletedTurns}/{queueItem.Value.TrainingTurns}"
                : $"Навчання: {trainingTurns} {RoundWord(trainingTurns)}";

            PopulateStatRows(config, trainingTurns, progress);

            RefreshCostRows(recipe);
            _recruitmentView.HireButtonLabel.text = "Найняти";
            if (queueItem.HasValue)
                _recruitmentView.SetActionHint(queueItem.Value.IsReady
                    ? "Готовий юніт розміщується через індикатор біля будівлі."
                    : "Цей запис уже в черзі.");
        }

        private void PopulateStatRows(
            UnitClassConfig config,
            int trainingTurns,
            string progress)
        {
            _recruitmentView.ClearStatRows();

            int row = 0;
            if (config != null)
            {
                AddStatRow(ref row, "Здоров'я", config.HitPoints > 0 ? config.HitPoints.ToString() : null);
                AddStatRow(ref row, "Витривалість", config.BaseStamina > 0f ? $"{config.BaseStamina:0.#}" : null);
                AddStatRow(ref row, "Огляд", config.VisionRange > 0 ? config.VisionRange.ToString() : null);
                AddStatRow(ref row, "Шкода", FormatCombatTriple(
                    config.PenetratingDamage,
                    config.CuttingDamage,
                    config.CrushingDamage));
                AddStatRow(ref row, "Захист", FormatCombatTriple(
                    config.PenetratingDefense,
                    config.CuttingDefense,
                    config.CrushingDefense));
            }

            AddStatRow(
                ref row,
                "Навчання",
                string.IsNullOrWhiteSpace(progress)
                    ? $"{Mathf.Max(1, trainingTurns)} {RoundWord(trainingTurns)}"
                    : progress);
        }

        private void AddStatRow(
            ref int row,
            string label,
            string value)
        {
            if (row >= _recruitmentView.StatRowCount
                || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            _recruitmentView.SetStatRow(row, label, value);
            row++;
        }

        private static string FormatCombatTriple(
            int penetrating,
            int cutting,
            int crushing)
        {
            int total = Mathf.Max(0, penetrating)
                + Mathf.Max(0, cutting)
                + Mathf.Max(0, crushing);
            if (total <= 0)
                return null;

            var parts = new List<string>(3);
            if (penetrating > 0)
                parts.Add($"кол. {penetrating}");
            if (cutting > 0)
                parts.Add($"ріж. {cutting}");
            if (crushing > 0)
                parts.Add($"дроб. {crushing}");

            return parts.Count <= 1
                ? total.ToString()
                : string.Join(" / ", parts);
        }

        private void RefreshCostRows(UnitRecruitmentRecipeDefinition recipe)
        {
            ClearCostRows();
            if (_recruitmentView == null || recipe?.Costs == null)
                return;

            int visible = 0;
            for (int index = 0; index < recipe.Costs.Count && visible < _recruitmentView.CostRowCount; index++)
            {
                BuildingResourceAmount cost = recipe.Costs[index];
                if (cost == null || string.IsNullOrWhiteSpace(cost.ResourceId) || cost.Amount <= 0f)
                    continue;

                ResolveResourcePresentation(cost.ResourceId, out _, out Sprite icon);
                GameObject row = _recruitmentView.GetCostRow(visible);
                Image rowIcon = _recruitmentView.GetCostIcon(visible);
                TMP_Text rowLabel = _recruitmentView.GetCostLabel(visible);

                row.SetActive(true);
                rowIcon.sprite = icon;
                rowIcon.enabled = icon != null;
                rowLabel.text = $"{cost.Amount:0.#}";
                visible++;
            }
        }

        private void ClearCostRows()
        {
            if (_recruitmentView == null)
                return;

            for (int index = 0; index < _recruitmentView.CostRowCount; index++)
            {
                GameObject row = _recruitmentView.GetCostRow(index);
                Image icon = _recruitmentView.GetCostIcon(index);
                TMP_Text label = _recruitmentView.GetCostLabel(index);
                row.SetActive(false);
                icon.sprite = null;
                icon.enabled = false;
                label.text = string.Empty;
            }
        }

        private void ResolveResourcePresentation(string resourceId, out string displayName, out Sprite icon)
        {
            displayName = "Невідомий ресурс";
            icon = null;

            IReadOnlyList<EconomyResourceDefinition> resources = _economyDatabase?.Resources;
            if (resources == null || string.IsNullOrWhiteSpace(resourceId))
                return;

            string normalized = resourceId.Trim();
            for (int index = 0; index < resources.Count; index++)
            {
                EconomyResourceDefinition resource = resources[index];
                if (resource == null
                    || !string.Equals(resource.Id, normalized, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(resource.DisplayName))
                    displayName = resource.DisplayName.Trim();
                icon = resource.Icon;
                return;
            }
        }

        private string ResolveUnitName(string typeId)
        {
            UnitClassConfig config = _unitConfigs.GetConfig(typeId);
            return config != null && !string.IsNullOrWhiteSpace(config.DisplayName)
                ? config.DisplayName.Trim()
                : "Юніт";
        }

        private string ResolveBuildingName(BuildingDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition?.DisplayName))
                return definition.DisplayName.Trim();

            return string.IsNullOrWhiteSpace(_selectedBuildingId)
                ? "Будівля"
                : "Будівля";
        }

        private static string LocalizeCombatType(UnitCombatType combatType)
        {
            return combatType switch
            {
                UnitCombatType.Cavalry => "Кавалерія",
                UnitCombatType.SiegeMachine => "Облогова машина",
                _ => "Піхота",
            };
        }

        private static string RoundWord(int count)
        {
            int value = Mathf.Abs(count);
            int lastTwo = value % 100;
            int last = value % 10;
            if (lastTwo >= 11 && lastTwo <= 14)
                return "раундів";
            if (last == 1)
                return "раунд";
            if (last >= 2 && last <= 4)
                return "раунди";
            return "раундів";
        }
    }
}
