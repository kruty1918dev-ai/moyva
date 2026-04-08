using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Панель кнопок дій будівництва: Підтвердити / Скасувати / Відмінити / Повторити / Знести.
    /// Керує станом interactable кнопок на основі <see cref="ConstructionUIState"/>
    /// та надає події, на які підписується <see cref="ConstructionUIController"/>.
    ///
    /// ЯК ПІДКЛЮЧИТИ В UNITY:
    /// 1. Додай компонент до панелі дій.
    /// 2. Перетягни кнопки у відповідні поля в Inspector.
    /// 3. Призначи панель у поле <see cref="ConstructionUIController.actionBar"/>.
    ///    Контролер підключає події автоматично.
    ///
    /// АЛЬТЕРНАТИВА: підключи кнопки напряму до методів <see cref="ConstructionUIController"/> через Button → OnClick.
    /// </summary>
    public class ConstructionActionBarUI : MonoBehaviour
    {
        [Header("Кнопки дій (перетягни в Inspector)")]
        [Tooltip("Виклик IConstructionService.Confirm().")]
        [SerializeField] private Button confirmButton;

        [Tooltip("Виклик IConstructionService.Cancel().")]
        [SerializeField] private Button cancelButton;

        [Tooltip("Виклик IConstructionService.UndoLast().")]
        [SerializeField] private Button undoButton;

        [Tooltip("Виклик IConstructionService.RedoLast().")]
        [SerializeField] private Button redoButton;

        [Tooltip("Перемикач режиму знесення (IConstructionService.ToggleDemolishMode()). Необов'язковий.")]
        [SerializeField] private Button demolishButton;

        [Header("Анімація натиску (DOTween)")]
        [Tooltip("Множник масштабу для анімації натиску кнопок дій.")]
        [SerializeField] private float pressScaleMultiplier = 1.07f;

        [Tooltip("Тривалість анімації натиску у секундах.")]
        [SerializeField] private float pressDuration = 0.12f;

        [Tooltip("Інтенсивність поштовху (кількість коливань).")]
        [SerializeField] private int pressVibrato = 7;

        [Tooltip("Пружність анімації натиску (0..1).")]
        [SerializeField] private float pressElasticity = 0.75f;

        /// <summary>Спрацьовує при натисканні кнопки «Підтвердити».</summary>
        public Action OnConfirmClicked;

        /// <summary>Спрацьовує при натисканні кнопки «Скасувати».</summary>
        public Action OnCancelClicked;

        /// <summary>Спрацьовує при натисканні кнопки «Відмінити».</summary>
        public Action OnUndoClicked;

        /// <summary>Спрацьовує при натисканні кнопки «Повторити».</summary>
        public Action OnRedoClicked;

        /// <summary>Спрацьовує при натисканні кнопки «Знести» (перемикає IsDemolishMode).</summary>
        public Action OnDemolishToggled;

        private void Awake()
        {
            if (confirmButton == null)
                Debug.LogWarning($"[ConstructionActionBarUI] Поле 'confirmButton' не призначено на '{name}'.", this);
            if (cancelButton == null)
                Debug.LogWarning($"[ConstructionActionBarUI] Поле 'cancelButton' не призначено на '{name}'.", this);
            if (undoButton == null)
                Debug.LogWarning($"[ConstructionActionBarUI] Поле 'undoButton' не призначено на '{name}'.", this);
            if (redoButton == null)
                Debug.LogWarning($"[ConstructionActionBarUI] Поле 'redoButton' не призначено на '{name}'.", this);

            if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
            if (cancelButton  != null) cancelButton.onClick.AddListener(HandleCancel);
            if (undoButton    != null) undoButton.onClick.AddListener(HandleUndo);
            if (redoButton    != null) redoButton.onClick.AddListener(HandleRedo);
            if (demolishButton != null) demolishButton.onClick.AddListener(HandleDemolish);
        }

        private void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
            if (cancelButton  != null) cancelButton.onClick.RemoveListener(HandleCancel);
            if (undoButton    != null) undoButton.onClick.RemoveListener(HandleUndo);
            if (redoButton    != null) redoButton.onClick.RemoveListener(HandleRedo);
            if (demolishButton != null) demolishButton.onClick.RemoveListener(HandleDemolish);
        }

        private void HandleConfirm()
        {
            AnimateButton(confirmButton);
            OnConfirmClicked?.Invoke();
        }

        private void HandleCancel()
        {
            AnimateButton(cancelButton);
            OnCancelClicked?.Invoke();
        }

        private void HandleUndo()
        {
            AnimateButton(undoButton);
            OnUndoClicked?.Invoke();
        }

        private void HandleRedo()
        {
            AnimateButton(redoButton);
            OnRedoClicked?.Invoke();
        }

        private void HandleDemolish()
        {
            AnimateButton(demolishButton);
            OnDemolishToggled?.Invoke();
        }

        private void AnimateButton(Button button)
        {
            if (button == null)
                return;

            ConstructionButtonPressAnimator.AnimatePress(button.transform, pressScaleMultiplier, pressDuration, pressVibrato, pressElasticity);
        }

        /// <summary>
        /// Оновлює стан interactable кнопок відповідно до поточного UI-стану.
        /// Викликається автоматично через <see cref="ConstructionUIController"/>.
        /// </summary>
        public void SetState(ConstructionUIState state)
        {
            bool isPlacing    = state.IsPlacing;
            bool isDemolish   = state.IsDemolishMode;

            if (confirmButton  != null) confirmButton.interactable  = isPlacing && !isDemolish;
            if (cancelButton   != null) cancelButton.interactable   = isPlacing || isDemolish;
            if (undoButton     != null) undoButton.interactable     = isPlacing && !isDemolish;
            if (redoButton     != null) redoButton.interactable     = isPlacing && !isDemolish;

            // Кнопка знесення активна завжди в режимі будівництва (для перемикання)
            if (demolishButton != null)
                demolishButton.interactable = state.IsConstructionModeActive;
        }
    }
}
