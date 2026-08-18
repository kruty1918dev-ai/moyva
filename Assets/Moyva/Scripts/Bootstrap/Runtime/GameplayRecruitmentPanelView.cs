using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Scene-authored P24B recruitment presentation references.
    /// GameplayTurnHudPresenter owns behavior; this component only exposes UI objects.
    /// </summary>
    public sealed class GameplayRecruitmentPanelView : MonoBehaviour
    {
        [Header("Recipe presentation")]
        [SerializeField] private Image[] _recipeIcons = Array.Empty<Image>();

        [Header("Selected unit")]
        [SerializeField] private GameObject _selectionPanel;
        [SerializeField] private Image _selectionIcon;
        [SerializeField] private TMP_Text _selectionName;
        [SerializeField] private TMP_Text _selectionStats;
        [SerializeField] private Button _hireButton;
        [SerializeField] private TMP_Text _hireButtonLabel;

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
        [SerializeField] private Button[] _queueButtons = Array.Empty<Button>();
        [SerializeField] private Image[] _queueIcons = Array.Empty<Image>();
        [SerializeField] private TMP_Text[] _queueLabels = Array.Empty<TMP_Text>();
        [SerializeField] private Image[] _queueProgressFills = Array.Empty<Image>();

        public int RecipeIconCount => _recipeIcons?.Length ?? 0;
        public int CostRowCount => Math.Min(
            _costRows?.Length ?? 0,
            Math.Min(_costIcons?.Length ?? 0, _costLabels?.Length ?? 0));
        public int QueueRowCount => Math.Min(
            Math.Min(_queueButtons?.Length ?? 0, _queueIcons?.Length ?? 0),
            Math.Min(_queueLabels?.Length ?? 0, _queueProgressFills?.Length ?? 0));

        public GameObject SelectionPanel => _selectionPanel;
        public Image SelectionIcon => _selectionIcon;
        public TMP_Text SelectionName => _selectionName;
        public TMP_Text SelectionStats => _selectionStats;
        public Button HireButton => _hireButton;
        public TMP_Text HireButtonLabel => _hireButtonLabel;
        public GameObject BuildingStatePanel => _buildingStatePanel;
        public TMP_Text BuildingStateTitle => _buildingStateTitle;
        public TMP_Text BuildingStateDetail => _buildingStateDetail;
        public Image BuildingStateProgressFill => _buildingStateProgressFill;

        public Image GetRecipeIcon(int index) => _recipeIcons[index];
        public GameObject GetCostRow(int index) => _costRows[index];
        public Image GetCostIcon(int index) => _costIcons[index];
        public TMP_Text GetCostLabel(int index) => _costLabels[index];
        public Button GetQueueButton(int index) => _queueButtons[index];
        public Image GetQueueIcon(int index) => _queueIcons[index];
        public TMP_Text GetQueueLabel(int index) => _queueLabels[index];
        public Image GetQueueProgressFill(int index) => _queueProgressFills[index];

        public void ValidateConfiguration(int recipeSlotCount)
        {
            if (_recipeIcons == null || _recipeIcons.Length != recipeSlotCount)
            {
                throw new InvalidOperationException(
                    $"GameplayRecruitmentPanelView recipe icon mismatch: expected={recipeSlotCount}, actual={_recipeIcons?.Length ?? 0}.");
            }

            for (int index = 0; index < _recipeIcons.Length; index++)
            {
                if (_recipeIcons[index] == null)
                    throw Missing($"{nameof(_recipeIcons)}[{index}]");
            }

            if (_selectionPanel == null)
                throw Missing(nameof(_selectionPanel));
            if (_selectionIcon == null)
                throw Missing(nameof(_selectionIcon));
            if (_selectionName == null)
                throw Missing(nameof(_selectionName));
            if (_selectionStats == null)
                throw Missing(nameof(_selectionStats));
            if (_hireButton == null)
                throw Missing(nameof(_hireButton));
            if (_hireButtonLabel == null)
                throw Missing(nameof(_hireButtonLabel));
            if (_buildingStatePanel == null)
                throw Missing(nameof(_buildingStatePanel));
            if (_buildingStateTitle == null)
                throw Missing(nameof(_buildingStateTitle));
            if (_buildingStateDetail == null)
                throw Missing(nameof(_buildingStateDetail));
            if (_buildingStateProgressFill == null)
                throw Missing(nameof(_buildingStateProgressFill));

            if (CostRowCount <= 0)
                throw new InvalidOperationException("GameplayRecruitmentPanelView requires at least one authored cost row.");
            if ((_costRows?.Length ?? 0) != CostRowCount
                || (_costIcons?.Length ?? 0) != CostRowCount
                || (_costLabels?.Length ?? 0) != CostRowCount)
            {
                throw new InvalidOperationException("GameplayRecruitmentPanelView cost row arrays must have equal lengths.");
            }

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
            if ((_queueButtons?.Length ?? 0) != QueueRowCount
                || (_queueIcons?.Length ?? 0) != QueueRowCount
                || (_queueLabels?.Length ?? 0) != QueueRowCount
                || (_queueProgressFills?.Length ?? 0) != QueueRowCount)
            {
                throw new InvalidOperationException("GameplayRecruitmentPanelView queue row arrays must have equal lengths.");
            }

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

        private InvalidOperationException Missing(string field)
            => new($"GameplayRecruitmentPanelView '{name}' has an unassigned scene reference: {field}.");
    }
}
