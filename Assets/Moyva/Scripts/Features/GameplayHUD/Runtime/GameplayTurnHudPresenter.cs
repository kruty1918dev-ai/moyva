using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class GameplayTurnHudPresenter : IInitializable, IDisposable, ITickable, IUiActionHandler
    {
        private readonly ITurnService _turns;
        private readonly IUnitService _units;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IConstructionSessionCommands _construction;
        private readonly IBuildingRegistry _buildings;
        private readonly SignalBus _signals;
        private readonly GameplayTurnHudView _view;
        private readonly IUiContextStack _uiContexts;
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

        private UnityAction _endTurnButtonHandler;
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
        private IDisposable _recruitmentPanelContext;

        public GameplayTurnHudPresenter(
            ITurnService turns,
            IUnitService units,
            IUnitClassConfig unitConfigs,
            IUnitRecruitmentService recruitment,
            IConstructionSessionCommands construction,
            IBuildingRegistry buildings,
            SignalBus signals,
            GameplayTurnHudView view,
            [InjectOptional] IUiContextStack uiContexts = null,
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
            _uiContexts = uiContexts;
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
            _recruitmentPanelContext = _uiContexts?.Push(new UiContextRegistration(
                "RecruitmentPanel",
                UiContextLayer.Panel,
                20,
                () => _recruitmentPanel != null && _recruitmentPanel.activeSelf,
                UiActionIds.Recruitment.Close));
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
            _recruitmentPanelContext?.Dispose();

            if (_endTurnButton != null && _endTurnButtonHandler != null)
                _endTurnButton.onClick.RemoveListener(_endTurnButtonHandler);

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
            _endTurnButtonHandler = () => ExecuteActionOrFallback(UiActionIds.EndTurn, UiActionSource.Button);
            _endTurnButton.onClick.AddListener(_endTurnButtonHandler);

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

            _hireButtonHandler = () => ExecuteActionOrFallback(UiActionIds.Recruitment.Enqueue, UiActionSource.Button);
            _recruitmentView.HireButton.onClick.AddListener(_hireButtonHandler);
            _closeButtonHandler = () => ExecuteActionOrFallback(UiActionIds.Recruitment.Close, UiActionSource.Button);
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

        public IReadOnlyCollection<UiActionId> ActionIds { get; } =
            new[]
            {
                UiActionIds.EndTurn,
                UiActionIds.Recruitment.Close,
                UiActionIds.Recruitment.Enqueue,
            };

        public UiActionResult Execute(in UiActionRequest request)
        {
            if (request.ActionId == UiActionIds.EndTurn)
                return HandleEndTurnAction();

            if (request.ActionId == UiActionIds.Recruitment.Close)
            {
                if (_recruitmentPanel == null || !_recruitmentPanel.activeSelf)
                    return UiActionResult.Rejected(UiActionReason.WrongContext);
                CloseRecruitmentPanel();
                return UiActionResult.Performed();
            }

            if (request.ActionId == UiActionIds.Recruitment.Enqueue)
                return HandleRecruitmentEnqueueAction();

            return UiActionResult.Ignored(UiActionReason.ActionUnavailable);
        }

        private UiActionResult HandleEndTurnAction()
        {
            GameplayTurnHudAuthoritySnapshot current =
                GameplayTurnHudAuthorityPolicy.Evaluate(_turns, _blockers);
            if (!current.CanEndTurn)
            {
                OnEndTurn();
                return UiActionResult.Rejected(UiActionReason.ActionUnavailable, current.StatusText);
            }

            OnEndTurn();
            return UiActionResult.Performed();
        }


        private void ExecuteActionOrFallback(UiActionId actionId, UiActionSource source)
        {
            Execute(new UiActionRequest(actionId, source, ActiveHudContext()));
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
