using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
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
        private readonly IReadOnlyList<ITurnBlocker> _blockers;

        private TMP_Text _turnText;
        private TMP_Text _statusText;
        private TMP_Text _unitText;
        private TMP_Text _queueText;
        private Button _endTurnButton;
        private GameObject _recruitmentPanel;
        private RectTransform _recipeRoot;
        private readonly List<Button> _recipeButtons = new();
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
            [InjectOptional] List<ITurnBlocker> blockers = null)
        {
            _turns = turns;
            _units = units;
            _unitConfigs = unitConfigs;
            _recruitment = recruitment;
            _construction = construction;
            _buildings = buildings;
            _signals = signals;
            _blockers = blockers ?? (IReadOnlyList<ITurnBlocker>)Array.Empty<ITurnBlocker>();
        }

        public void Initialize()
        {
            BuildUi();
            _turns.StateChanged += OnTurnStateChanged;
            _signals.Subscribe<UnitInfoPanelRequestedSignal>(OnUnitSelected);
            _signals.Subscribe<BuildingInfoPanelRequestedSignal>(OnBuildingSelected);
            _signals.Subscribe<WorldInfoPanelClosedSignal>(OnSelectionClosed);
            RefreshTurnAuthority();
        }

        public void Dispose()
        {
            _turns.StateChanged -= OnTurnStateChanged;
            _signals.TryUnsubscribe<UnitInfoPanelRequestedSignal>(OnUnitSelected);
            _signals.TryUnsubscribe<BuildingInfoPanelRequestedSignal>(OnBuildingSelected);
            _signals.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnSelectionClosed);
            if (_turnText != null)
                UnityEngine.Object.Destroy(_turnText.transform.root.gameObject);
        }

        public void Tick()
        {
            // ITurnBlocker state may change asynchronously without StateChanged.
            RefreshTurnAuthority();
            RefreshUnit();
            RefreshQueue();
        }

        private void OnTurnStateChanged()
        {
            _statusOverride = null;
            RefreshTurnAuthority();
            RefreshQueue();
        }

        private void BuildUi()
        {
            GameObject canvasObject = new("Turn HUD (Runtime)", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 210;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform bar = CreatePanel(canvasObject.transform, "TurnBar", new Color32(22, 27, 32, 242));
            Anchor(bar, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-310f, -82f), new Vector2(620f, 64f));
            _turnText = CreateText(bar, "Turn", 22, TextAlignmentOptions.MidlineLeft);
            Anchor(_turnText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 0f), new Vector2(-190f, 0f));
            _endTurnButton = CreateButton(bar, "EndTurn", "Завершити хід", OnEndTurn);
            Anchor((RectTransform)_endTurnButton.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-85f, 0f), new Vector2(150f, 42f));

            _statusText = CreateText(canvasObject.transform, "TurnStatus", 16, TextAlignmentOptions.Center);
            Anchor(_statusText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(620f, 30f));

            _unitText = CreateText(canvasObject.transform, "UnitStamina", 18, TextAlignmentOptions.MidlineLeft);
            Anchor(_unitText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(190f, 48f), new Vector2(340f, 36f));

            _recruitmentPanel = CreatePanel(canvasObject.transform, "RecruitmentPanel", new Color32(25, 30, 35, 248)).gameObject;
            Anchor((RectTransform)_recruitmentPanel.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-190f, 0f), new Vector2(360f, 420f));
            TMP_Text title = CreateText(_recruitmentPanel.transform, "Title", 22, TextAlignmentOptions.Center);
            title.text = "Найм юнітів";
            Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -28f), new Vector2(0f, 42f));
            GameObject recipes = new("Recipes", typeof(RectTransform), typeof(VerticalLayoutGroup));
            _recipeRoot = (RectTransform)recipes.transform;
            _recipeRoot.SetParent(_recruitmentPanel.transform, false);
            Anchor(_recipeRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -150f), new Vector2(-24f, 170f));
            VerticalLayoutGroup layout = recipes.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            _queueText = CreateText(_recruitmentPanel.transform, "Queue", 16, TextAlignmentOptions.TopLeft);
            Anchor(_queueText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 78f), new Vector2(-30f, 120f));
            _recruitmentPanel.SetActive(false);
        }

        private void RefreshTurnAuthority()
        {
            _authority = GameplayTurnHudAuthorityPolicy.Evaluate(_turns, _blockers);
            string actor = _authority.IsActiveFactionBot ? "Бот" : "Фракція";
            string owner = string.IsNullOrWhiteSpace(_authority.ActiveOwnerId)
                ? "—"
                : _authority.ActiveOwnerId;
            _turnText.text =
                $"{actor}: {owner}    Раунд {_turns.Round}    Хід {_turns.GlobalTurn}    " +
                $"Фаза {GameplayTurnHudAuthorityPolicy.LocalizePhase(_authority.Phase)}    " +
                $"Дії {_turns.ActionsThisTurn}";
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
            _recipeButtons.Clear();
            for (int index = _recipeRoot.childCount - 1; index >= 0; index--)
                UnityEngine.Object.Destroy(_recipeRoot.GetChild(index).gameObject);

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
            if (module.Recipes != null)
            {
                foreach (UnitRecruitmentRecipeDefinition recipe in module.Recipes)
                {
                    if (recipe == null || string.IsNullOrWhiteSpace(recipe.UnitTypeId))
                        continue;

                    string unitTypeId = recipe.UnitTypeId.Trim();
                    string label = $"{ResolveUnitName(unitTypeId)}  ·  {Mathf.Max(1, recipe.TrainingTurns)} х.";
                    Button button = CreateButton(
                        _recipeRoot,
                        $"Recruit-{unitTypeId}",
                        label,
                        () => Enqueue(unitTypeId));
                    ((RectTransform)button.transform).sizeDelta = new Vector2(320f, 44f);
                    _recipeButtons.Add(button);
                }
            }

            RefreshRecruitmentAuthority();
            RefreshQueue();
        }

        private void Enqueue(string unitTypeId)
        {
            RefreshTurnAuthority();
            if (!_selectedBuilding.HasValue)
                return;

            if (!_authority.CanIssueLocalCommands)
            {
                _statusOverride = _authority.StatusText;
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
                    button.interactable = canRecruit;
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
                return;
            }

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
            => _unitConfigs.GetConfig(typeId)?.DisplayName ?? typeId;

        private static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return (RectTransform)go.transform;
        }

        private static TMP_Text CreateText(
            Transform parent,
            string name,
            int size,
            TextAlignmentOptions alignment)
        {
            GameObject go = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            TMP_Text text = go.GetComponent<TMP_Text>();
            text.fontSize = size;
            text.alignment = alignment;
            text.color = new Color32(238, 241, 236, 255);
            text.enableWordWrapping = false;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string label,
            UnityEngine.Events.UnityAction action)
        {
            RectTransform panel = CreatePanel(parent, name, new Color32(48, 96, 80, 255));
            Button button = panel.gameObject.AddComponent<Button>();
            button.targetGraphic = panel.GetComponent<Image>();
            button.onClick.AddListener(action);
            TMP_Text text = CreateText(panel, "Label", 16, TextAlignmentOptions.Center);
            text.text = label;
            Anchor(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void Anchor(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 position,
            Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
