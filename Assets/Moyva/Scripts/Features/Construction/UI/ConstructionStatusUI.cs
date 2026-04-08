using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Відображає поточний стан будівництва: режим розміщення, вибрана будівля, стан preview.
    ///
    /// ЯК ПІДКЛЮЧИТИ В UNITY:
    /// 1. Додай компонент до панелі статусу.
    /// 2. Перетягни TextMeshProUGUI лейбли у поля в Inspector (всі необов'язкові).
    /// 3. Призначи панель у поле <see cref="ConstructionUIController.statusDisplay"/>.
    ///    Контролер викликає <see cref="UpdateState"/> автоматично.
    ///
    /// Значення лейблів:
    ///   placementStateLabel   → "Idle" | "Placing" | "Confirmed"
    ///   selectedBuildingLabel → назва вибраної будівлі, або "--"
    ///   previewStateLabel     → "OK Дійсно" | "X Заблоковано" | "--"
    /// </summary>
    public class ConstructionStatusUI : MonoBehaviour
    {
        [Header("Лейбли статусу (перетягни в Inspector)")]
        [Tooltip("Показує поточний стан розміщення (Idle / Placing / Confirmed).")]
        [SerializeField] private TextMeshProUGUI placementStateLabel;

        [Tooltip("Показує назву вибраної будівлі або '--' якщо нічого не вибрано.")]
        [SerializeField] private TextMeshProUGUI selectedBuildingLabel;

        [Tooltip("Показує стан preview (✓ Дійсно / ✗ Заблоковано / --).")]
        [SerializeField] private TextMeshProUGUI previewStateLabel;

        private void Awake()
        {
            if (placementStateLabel == null)
                Debug.LogWarning($"[ConstructionStatusUI] Поле 'placementStateLabel' не призначено на '{name}'.", this);
            if (selectedBuildingLabel == null)
                Debug.LogWarning($"[ConstructionStatusUI] Поле 'selectedBuildingLabel' не призначено на '{name}'.", this);
            if (previewStateLabel == null)
                Debug.LogWarning($"[ConstructionStatusUI] Поле 'previewStateLabel' не призначено на '{name}'.", this);
        }

        /// <summary>
        /// Оновлює всі лейбли на основі переданого знімку UI-стану.
        /// Викликається автоматично через <see cref="ConstructionUIController"/>.
        /// </summary>
        public void UpdateState(ConstructionUIState state)
        {
            if (placementStateLabel != null)
                placementStateLabel.text = state.PlacementState.ToString();

            if (selectedBuildingLabel != null)
                selectedBuildingLabel.text = state.HasSelection ? state.SelectedBuildingId : "--";

            if (previewStateLabel != null)
            {
                switch (state.LastPreviewState)
                {
                    case BuildingPreviewState.Valid:
                        previewStateLabel.text = "OK Дійсно";
                        break;
                    case BuildingPreviewState.Blocked:
                        previewStateLabel.text = "X Заблоковано";
                        break;
                    default:
                        previewStateLabel.text = "--";
                        break;
                }
            }
        }
    }
}
