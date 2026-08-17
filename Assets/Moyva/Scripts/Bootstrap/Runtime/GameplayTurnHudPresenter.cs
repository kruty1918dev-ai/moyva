using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
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
        private readonly IReadOnlyList<ITurnBlocker> _blockers;

        private TMP_Text _turnText;
        private TMP_Text _statusText;
        private TMP_Text _unitText;
        private TMP_Text _queueText;
        private Button _endTurnButton;
        private GameObject _recruitmentPanel;
        private readonly List<Button> _recipeButtons = new();
        private readonly List<string> _recipeUnitTypeIds = new();
        private readonly List<UnityAction> _recipeButtonHandlers = new();
        private string _selectedUnitId;
        private Vector2Int? _selectedBuilding;
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
            _blockers = blockers ?? (IReadOnlyList<ITurnBlocker>)Array.Empty<ITurnBlocker>();
        }

        public void Initialize()
        {
            BindSceneView();
            _turns.StateChanged += OnTurnStateChanged;
            _signals.Subscribe<UnitInfoPanelRequestedSignal>(OnUnitSelected);
            _signals.Subscribe<BuildingInfoPanelRequestedSignal>(OnBuildingSelected);
            _signals.Subscribe<WorldInfoPanelClosedSignal>(OnSelectionClosed);
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

            if (_endTurnButton != null)
                _endTurnButton.onClick.RemoveListener(OnEndTurn);

            for (int index = 0; index < _recipeButtons.Count && index < _recipeButtonHandlers.Count; index++)
            {
                Button button = _recipeButtons[index];
                UnityAction handler = _recipeButtonHandlers[index];
                if (button != null && handler != null)
                    button.onClick.RemoveListener(handler);
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

            _endTurnButton.onClick.AddListener(OnEndTurn);

            _recipeButtons.Clear();
            _recipeUnitTypeIds.Clear();
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
                _recipeButtonHandlers.Add(handler);
            }

            _recruitmentPanel.SetActive(false);
            _queueText.text = string.Empty;
            _unitText.text = string.Empty;
            _unitText.transform.parent.gameObject.SetActive(false);
        }

        private void OnTurnStateChanged()
        {
            _statusOverride = null;
            RefreshTurnAuthority();
            RefreshQueue();
        }

        private void RefreshTurnAuthority()
        {
            _authority = GameplayTurnHudAuthorityPolicy.Evaluate(_turns, _blockers);
            string actor = _authority.IsActiveFactionBot ? "Бот" : "Фракція";
            string owner = string.IsNullOrWhiteSpace(_authority.ActiveOwnerId)
                ? "—"
                : _authority.ActiveOwnerId;

            // Two compact authored rows keep the turn HUD clear of the scene-authored
            // resource summary at the top centre of the gameplay canvas.
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
            // Never trust a potentially stale Button.interactable value.
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
            _statusOverride = null;
            _recruitmentPanel.SetActive(false);
        }

        private void OnBuildingSelected(BuildingInfoPanelRequestedSignal signal)
        {
            _selectedUnitId = null;
            _selectedBuilding = signal.Position;
            _statusOverride = null;
            BuildRecipeButtons(signal.BuildingId);
        }

        private void OnSelectionClosed(WorldInfoPanelClosedSignal _)
        {
            _selectedUnitId = null;
            _selectedBuilding = null;
            _statusOverride = null;
            _recruitmentPanel.SetActive(false);
        }

        private void BuildRecipeButtons(string buildingId)
        {
            ClearRecipeSlots();

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

            _recruitmentPanel.SetActive(true);
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
                    _recipeUnitTypeIds[slotIndex] = unitTypeId;
                    label.text = $"{ResolveUnitName(unitTypeId)}  ·  {Mathf.Max(1, recipe.TrainingTurns)} х.";
                    button.gameObject.SetActive(true);
                    slotIndex++;
                }
            }

            if (skippedForCapacity > 0)
            {
                Debug.LogWarning(
                    $"[GameplayTurnHud] Recruitment recipes exceed authored scene slots. " +
                    $"Visible={_recipeButtons.Count}, skipped={skippedForCapacity}. " +
                    "Run the HUD authoring CLI with a larger slot count if needed.");
            }

            RefreshRecruitmentAuthority();
            RefreshQueue();
        }

        private void ClearRecipeSlots()
        {
            for (int index = 0; index < _recipeButtons.Count; index++)
            {
                _recipeUnitTypeIds[index] = null;
                Button button = _recipeButtons[index];
                if (button != null)
                    button.gameObject.SetActive(false);

                TMP_Text label = _view.GetRecipeLabel(index);
                if (label != null)
                    label.text = string.Empty;
            }
        }

        private void OnRecipeSlotClicked(int index)
        {
            if (index < 0 || index >= _recipeUnitTypeIds.Count)
                return;

            string unitTypeId = _recipeUnitTypeIds[index];
            if (!string.IsNullOrWhiteSpace(unitTypeId))
                Enqueue(unitTypeId);
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

            if (!_recruitment.TryEnqueue(
                    _authority.LocalOwnerId,
                    _selectedBuilding.Value,
                    unitTypeId,
                    out string reason))
            {
                _statusOverride = string.IsNullOrWhiteSpace(reason)
                    ? "Найм відхилено authoritative recruitment service."
                    : reason;
            }
            else
            {
                _statusOverride = null;
            }

            RefreshTurnAuthority();
            RefreshQueue();
        }

        private void RefreshRecruitmentAuthority()
        {
            if (_recruitmentPanel == null || !_recruitmentPanel.activeSelf)
                return;

            bool canRecruit = _authority.CanIssueLocalCommands
                && IsSelectedBuildingOwnedByLocalPlayer()
                && IsSelectedBuildingOperational();

            for (int index = 0; index < _recipeButtons.Count; index++)
            {
                Button button = _recipeButtons[index];
                if (button != null)
                {
                    button.interactable = canRecruit
                        && !string.IsNullOrWhiteSpace(_recipeUnitTypeIds[index]);
                }
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
            if (!_selectedBuilding.HasValue
                || _recruitmentPanel == null
                || !_recruitmentPanel.activeSelf
                || !IsSelectedBuildingOwnedByLocalPlayer())
            {
                return;
            }

            if (_construction is IConstructionLifecycle lifecycle
                && lifecycle.TryGetProgress(
                    _selectedBuilding.Value,
                    out int completed,
                    out int required)
                && completed < required)
            {
                _queueText.text = $"Будівництво: {completed}/{required}";
                return;
            }

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                _recruitment.GetQueue(
                    _turns.LocalOwnerId,
                    _selectedBuilding.Value);

            if (queue == null || queue.Count == 0)
            {
                _queueText.text = "Черга порожня";
                return;
            }

            var lines = new List<string>(queue.Count);
            for (int index = 0; index < queue.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot job = queue[index];
                lines.Add(job.IsReady
                    ? $"{ResolveUnitName(job.UnitTypeId)}: очікує місця"
                    : $"{ResolveUnitName(job.UnitTypeId)}: {job.RemainingTurns} х.");
            }
            _queueText.text = string.Join("\n", lines);
        }

        private string ResolveUnitName(string typeId)
        {
            UnitClassConfig config = _unitConfigs.GetConfig(typeId);
            return config != null && !string.IsNullOrWhiteSpace(config.DisplayName)
                ? config.DisplayName.Trim()
                : "Юніт";
        }
    }
}
