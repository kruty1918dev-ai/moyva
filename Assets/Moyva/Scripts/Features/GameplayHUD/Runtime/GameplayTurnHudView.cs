using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Scene-authored references for the turn gameplay HUD.
    /// Runtime code only binds to these objects; it never creates or destroys HUD GameObjects.
    /// </summary>
    public sealed class GameplayTurnHudView : MonoBehaviour
    {
        [Header("Turn")]
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button _endTurnButton;

        [Header("Selection")]
        [SerializeField] private TMP_Text _unitText;

        [Header("Recruitment")]
        [SerializeField] private GameObject _recruitmentPanel;
        [SerializeField] private TMP_Text _queueText;
        [SerializeField] private Button[] _recipeButtons = Array.Empty<Button>();
        [SerializeField] private TMP_Text[] _recipeLabels = Array.Empty<TMP_Text>();

        public TMP_Text TurnText => _turnText;
        public TMP_Text StatusText => _statusText;
        public Button EndTurnButton => _endTurnButton;
        public TMP_Text UnitText => _unitText;
        public GameObject RecruitmentPanel => _recruitmentPanel;
        public TMP_Text QueueText => _queueText;

        public int RecipeSlotCount => Math.Min(
            _recipeButtons?.Length ?? 0,
            _recipeLabels?.Length ?? 0);

        public Button GetRecipeButton(int index) => _recipeButtons[index];
        public TMP_Text GetRecipeLabel(int index) => _recipeLabels[index];

        public void ValidateConfiguration()
        {
            if (_turnText == null)
                throw Missing(nameof(_turnText));
            if (_statusText == null)
                throw Missing(nameof(_statusText));
            if (_endTurnButton == null)
                throw Missing(nameof(_endTurnButton));
            if (_unitText == null)
                throw Missing(nameof(_unitText));
            if (_recruitmentPanel == null)
                throw Missing(nameof(_recruitmentPanel));
            if (_queueText == null)
                throw Missing(nameof(_queueText));
            if (_recipeButtons == null || _recipeLabels == null)
                throw new InvalidOperationException("GameplayTurnHudView recipe slot arrays are not assigned.");
            if (_recipeButtons.Length != _recipeLabels.Length)
                throw new InvalidOperationException(
                    $"GameplayTurnHudView recipe slot mismatch: buttons={_recipeButtons.Length}, labels={_recipeLabels.Length}.");
            if (_recipeButtons.Length == 0)
                throw new InvalidOperationException("GameplayTurnHudView must contain at least one authored recipe slot.");

            for (int index = 0; index < _recipeButtons.Length; index++)
            {
                if (_recipeButtons[index] == null)
                    throw Missing($"{nameof(_recipeButtons)}[{index}]");
                if (_recipeLabels[index] == null)
                    throw Missing($"{nameof(_recipeLabels)}[{index}]");
            }
        }

        private InvalidOperationException Missing(string field)
            => new($"GameplayTurnHudView '{name}' has an unassigned scene reference: {field}.");
    }
}
