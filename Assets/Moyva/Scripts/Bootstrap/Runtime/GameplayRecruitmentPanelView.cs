using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Scene-authored recruitment presentation references.
    /// GameplayTurnHudPresenter owns behavior; this component only exposes UI objects.
    /// </summary>
    public sealed class GameplayRecruitmentPanelView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text _buildingName;
        [SerializeField] private TMP_Text _subtitle;
        [SerializeField] private GameObject _stateChip;
        [SerializeField] private TMP_Text _stateChipLabel;
        [SerializeField] private Button _closeButton;

        [Header("Recipe presentation")]
        [SerializeField] private Image[] _recipeIcons = Array.Empty<Image>();
        [SerializeField] private TMP_Text[] _recipeNameTexts = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] _recipeTrainingTexts = Array.Empty<TMP_Text>();
        [SerializeField] private GameObject[] _recipeSelectedAccents = Array.Empty<GameObject>();

        [Header("Selected unit")]
        [SerializeField] private GameObject _selectionPanel;
        [SerializeField] private Image _selectionIcon;
        [SerializeField] private TMP_Text _selectionName;
        [SerializeField] private TMP_Text _selectionClass;
        [SerializeField] private TMP_Text _selectionStats;
        [SerializeField] private GameObject[] _statRows = Array.Empty<GameObject>();
        [SerializeField] private TMP_Text[] _statLabels = Array.Empty<TMP_Text>();
        [SerializeField] private TMP_Text[] _statValues = Array.Empty<TMP_Text>();
        [SerializeField] private Button _hireButton;
        [SerializeField] private TMP_Text _hireButtonLabel;
        [SerializeField] private TMP_Text _actionHint;

        [Header("Selected unit costs")]
        [SerializeField] private GameObject[] _costRows = Array.Empty<GameObject>();
        [SerializeField] private Image[] _costIcons = Array.Empty<Image>();
        [SerializeField] private TMP_Text[] _costLabels = Array.Empty<TMP_Text>();

        [Header("Building recruitment state")]
        [SerializeField] private GameObject _buildingStatePanel;
        [SerializeField] private TMP_Text _buildingStateTitle;
        [SerializeField] private TMP_Text _buildingStateDetail;
        [SerializeField] private Image _buildingStateProgressFill;

        [Header("Queue")]
        [SerializeField] private GameObject _queueSection;
        [SerializeField] private TMP_Text _queueTitle;
        [SerializeField] private TMP_Text _queueCapacity;
        [SerializeField] private TMP_Text _queueEmptyText;
        [SerializeField] private Button[] _queueButtons = Array.Empty<Button>();
        [SerializeField] private Image[] _queueIcons = Array.Empty<Image>();
        [SerializeField] private TMP_Text[] _queueLabels = Array.Empty<TMP_Text>();
        [SerializeField] private Image[] _queueProgressFills = Array.Empty<Image>();

        public int RecipeIconCount => RecipePresentationCount;
        public int RecipePresentationCount => Math.Min(
            Math.Min(_recipeIcons?.Length ?? 0, _recipeNameTexts?.Length ?? 0),
            Math.Min(_recipeTrainingTexts?.Length ?? 0, _recipeSelectedAccents?.Length ?? 0));
        public int CostRowCount => Math.Min(
            _costRows?.Length ?? 0,
            Math.Min(_costIcons?.Length ?? 0, _costLabels?.Length ?? 0));
        public int StatRowCount => Math.Min(
            _statRows?.Length ?? 0,
            Math.Min(_statLabels?.Length ?? 0, _statValues?.Length ?? 0));
        public int QueueRowCount => Math.Min(
            Math.Min(_queueButtons?.Length ?? 0, _queueIcons?.Length ?? 0),
            Math.Min(_queueLabels?.Length ?? 0, _queueProgressFills?.Length ?? 0));

        public TMP_Text BuildingName => _buildingName;
        public TMP_Text Subtitle => _subtitle;
        public Button CloseButton => _closeButton;
        public GameObject SelectionPanel => _selectionPanel;
        public Image SelectionIcon => _selectionIcon;
        public TMP_Text SelectionName => _selectionName;
        public TMP_Text SelectionClass => _selectionClass;
        public TMP_Text SelectionStats => _selectionStats;
        public Button HireButton => _hireButton;
        public TMP_Text HireButtonLabel => _hireButtonLabel;
        public TMP_Text ActionHint => _actionHint;
        public GameObject BuildingStatePanel => _buildingStatePanel;
        public TMP_Text BuildingStateTitle => _buildingStateTitle;
        public TMP_Text BuildingStateDetail => _buildingStateDetail;
        public Image BuildingStateProgressFill => _buildingStateProgressFill;
        public GameObject QueueSection => _queueSection;
        public TMP_Text QueueTitle => _queueTitle;
        public TMP_Text QueueCapacity => _queueCapacity;
        public TMP_Text QueueEmptyText => _queueEmptyText;

        public Image GetRecipeIcon(int index) => _recipeIcons[index];
        public TMP_Text GetRecipeNameText(int index) => _recipeNameTexts[index];
        public TMP_Text GetRecipeTrainingText(int index) => _recipeTrainingTexts[index];
        public GameObject GetRecipeSelectedAccent(int index) => _recipeSelectedAccents[index];
        public GameObject GetStatRow(int index) => _statRows[index];
        public TMP_Text GetStatLabel(int index) => _statLabels[index];
        public TMP_Text GetStatValue(int index) => _statValues[index];
        public GameObject GetCostRow(int index) => _costRows[index];
        public Image GetCostIcon(int index) => _costIcons[index];
        public TMP_Text GetCostLabel(int index) => _costLabels[index];
        public Button GetQueueButton(int index) => _queueButtons[index];
        public Image GetQueueIcon(int index) => _queueIcons[index];
        public TMP_Text GetQueueLabel(int index) => _queueLabels[index];
        public Image GetQueueProgressFill(int index) => _queueProgressFills[index];

        public void SetHeader(string buildingName, string subtitle)
        {
            if (_buildingName != null)
                _buildingName.text = buildingName ?? string.Empty;
            if (_subtitle != null)
                _subtitle.text = subtitle ?? string.Empty;
        }

        public void SetStateChip(string label, Color color)
        {
            if (_stateChip != null)
            {
                _stateChip.SetActive(!string.IsNullOrWhiteSpace(label));
                if (_stateChip.TryGetComponent(out Image image))
                    image.color = color;
            }

            if (_stateChipLabel != null)
                _stateChipLabel.text = label ?? string.Empty;
        }

        public void SetSelectedRecipeIndex(int index)
        {
            for (int i = 0; i < RecipePresentationCount; i++)
            {
                GameObject accent = _recipeSelectedAccents[i];
                if (accent != null)
                    accent.SetActive(i == index);
            }
        }

        public void SetRecipeTrainingText(int index, string value)
        {
            if (index >= 0 && index < (_recipeTrainingTexts?.Length ?? 0))
                _recipeTrainingTexts[index].text = value ?? string.Empty;
        }

        public void SetStatRow(int index, string label, string value, bool active = true)
        {
            if (index < 0 || index >= StatRowCount)
                return;

            _statRows[index].SetActive(active);
            _statLabels[index].text = active ? label ?? string.Empty : string.Empty;
            _statValues[index].text = active ? value ?? string.Empty : string.Empty;
        }

        public void ClearStatRows()
        {
            for (int index = 0; index < StatRowCount; index++)
                SetStatRow(index, string.Empty, string.Empty, active: false);
        }

        public void SetActionHint(string value)
        {
            if (_actionHint != null)
                _actionHint.text = value ?? string.Empty;
        }

        public void SetQueueSummary(string title, string capacity, bool empty)
        {
            if (_queueTitle != null)
                _queueTitle.text = title ?? string.Empty;
            if (_queueCapacity != null)
                _queueCapacity.text = capacity ?? string.Empty;
            if (_queueEmptyText != null)
            {
                _queueEmptyText.text = empty ? "Порожньо" : string.Empty;
                _queueEmptyText.gameObject.SetActive(empty);
            }
        }

        public void ValidateConfiguration(int recipeSlotCount)
        {
            ValidateRecipePresentation(recipeSlotCount);

            if (_buildingName == null)
                throw Missing(nameof(_buildingName));
            if (_subtitle == null)
                throw Missing(nameof(_subtitle));
            if (_stateChip == null)
                throw Missing(nameof(_stateChip));
            if (_stateChipLabel == null)
                throw Missing(nameof(_stateChipLabel));
            if (_closeButton == null)
                throw Missing(nameof(_closeButton));
            if (_selectionPanel == null)
                throw Missing(nameof(_selectionPanel));
            if (_selectionIcon == null)
                throw Missing(nameof(_selectionIcon));
            if (_selectionName == null)
                throw Missing(nameof(_selectionName));
            if (_selectionClass == null)
                throw Missing(nameof(_selectionClass));
            if (_hireButton == null)
                throw Missing(nameof(_hireButton));
            if (_hireButtonLabel == null)
                throw Missing(nameof(_hireButtonLabel));
            if (_actionHint == null)
                throw Missing(nameof(_actionHint));
            if (_buildingStatePanel == null)
                throw Missing(nameof(_buildingStatePanel));
            if (_buildingStateTitle == null)
                throw Missing(nameof(_buildingStateTitle));
            if (_buildingStateDetail == null)
                throw Missing(nameof(_buildingStateDetail));
            if (_buildingStateProgressFill == null)
                throw Missing(nameof(_buildingStateProgressFill));
            if (_queueSection == null)
                throw Missing(nameof(_queueSection));
            if (_queueTitle == null)
                throw Missing(nameof(_queueTitle));
            if (_queueCapacity == null)
                throw Missing(nameof(_queueCapacity));
            if (_queueEmptyText == null)
                throw Missing(nameof(_queueEmptyText));

            ValidateParallelArrays(
                "stat rows",
                StatRowCount,
                _statRows?.Length ?? 0,
                _statLabels?.Length ?? 0,
                _statValues?.Length ?? 0);

            for (int index = 0; index < StatRowCount; index++)
            {
                if (_statRows[index] == null)
                    throw Missing($"{nameof(_statRows)}[{index}]");
                if (_statLabels[index] == null)
                    throw Missing($"{nameof(_statLabels)}[{index}]");
                if (_statValues[index] == null)
                    throw Missing($"{nameof(_statValues)}[{index}]");
            }

            if (CostRowCount <= 0)
                throw new InvalidOperationException("GameplayRecruitmentPanelView requires at least one authored cost row.");
            ValidateParallelArrays(
                "cost rows",
                CostRowCount,
                _costRows?.Length ?? 0,
                _costIcons?.Length ?? 0,
                _costLabels?.Length ?? 0);

            for (int index = 0; index < CostRowCount; index++)
            {
                if (_costRows[index] == null)
                    throw Missing($"{nameof(_costRows)}[{index}]");
                if (_costIcons[index] == null)
                    throw Missing($"{nameof(_costIcons)}[{index}]");
                if (_costLabels[index] == null)
                    throw Missing($"{nameof(_costLabels)}[{index}]");
            }

            if (QueueRowCount <= 0)
                throw new InvalidOperationException("GameplayRecruitmentPanelView requires at least one authored queue row.");
            ValidateParallelArrays(
                "queue rows",
                QueueRowCount,
                _queueButtons?.Length ?? 0,
                _queueIcons?.Length ?? 0,
                _queueLabels?.Length ?? 0,
                _queueProgressFills?.Length ?? 0);

            for (int index = 0; index < QueueRowCount; index++)
            {
                if (_queueButtons[index] == null)
                    throw Missing($"{nameof(_queueButtons)}[{index}]");
                if (_queueIcons[index] == null)
                    throw Missing($"{nameof(_queueIcons)}[{index}]");
                if (_queueLabels[index] == null)
                    throw Missing($"{nameof(_queueLabels)}[{index}]");
                if (_queueProgressFills[index] == null)
                    throw Missing($"{nameof(_queueProgressFills)}[{index}]");
            }
        }

        private void ValidateRecipePresentation(int recipeSlotCount)
        {
            if (RecipePresentationCount != recipeSlotCount
                || (_recipeIcons?.Length ?? 0) != recipeSlotCount
                || (_recipeNameTexts?.Length ?? 0) != recipeSlotCount
                || (_recipeTrainingTexts?.Length ?? 0) != recipeSlotCount
                || (_recipeSelectedAccents?.Length ?? 0) != recipeSlotCount)
            {
                throw new InvalidOperationException(
                    $"GameplayRecruitmentPanelView recipe presentation mismatch: expected={recipeSlotCount}, actual={RecipePresentationCount}.");
            }

            for (int index = 0; index < recipeSlotCount; index++)
            {
                if (_recipeIcons[index] == null)
                    throw Missing($"{nameof(_recipeIcons)}[{index}]");
                if (_recipeNameTexts[index] == null)
                    throw Missing($"{nameof(_recipeNameTexts)}[{index}]");
                if (_recipeTrainingTexts[index] == null)
                    throw Missing($"{nameof(_recipeTrainingTexts)}[{index}]");
                if (_recipeSelectedAccents[index] == null)
                    throw Missing($"{nameof(_recipeSelectedAccents)}[{index}]");
            }
        }

        private static void ValidateParallelArrays(
            string label,
            int effectiveCount,
            params int[] lengths)
        {
            for (int index = 0; index < lengths.Length; index++)
            {
                if (lengths[index] != effectiveCount)
                    throw new InvalidOperationException(
                        $"GameplayRecruitmentPanelView {label} arrays must have equal lengths.");
            }
        }

        private InvalidOperationException Missing(string field)
            => new($"GameplayRecruitmentPanelView '{name}' has an unassigned scene reference: {field}.");
    }
}
