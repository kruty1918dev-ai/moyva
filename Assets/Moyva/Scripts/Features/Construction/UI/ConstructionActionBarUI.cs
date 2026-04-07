using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// UI scaffold for the construction action bar (Confirm / Cancel / Undo / Redo).
    /// Drives button interactability based on the current <see cref="ConstructionUIState"/>
    /// and exposes events that <see cref="ConstructionUIController"/> subscribes to.
    ///
    /// HOW TO WIRE IN UNITY:
    /// 1. Add this component to your action bar panel.
    /// 2. Drag the four <c>Button</c> references into the fields.
    /// 3. Add the panel to the scene and assign it to <see cref="ConstructionUIController.actionBar"/>.
    ///    The controller wires the events automatically.
    ///
    /// ALTERNATIVE (no sub-panel):
    /// Wire Button.OnClick directly to the public methods on <see cref="ConstructionUIController"/>:
    ///   Confirm → ConstructionUIController.OnConfirmClicked()
    ///   Cancel  → ConstructionUIController.OnCancelClicked()
    ///   Undo    → ConstructionUIController.OnUndoClicked()
    ///   Redo    → ConstructionUIController.OnRedoClicked()
    /// </summary>
    public class ConstructionActionBarUI : MonoBehaviour
    {
        [Header("Action Buttons (drag in Inspector)")]
        [Tooltip("Calls IConstructionService.Confirm().")]
        [SerializeField] private Button confirmButton;

        [Tooltip("Calls IConstructionService.Cancel().")]
        [SerializeField] private Button cancelButton;

        [Tooltip("Calls IConstructionService.UndoLast().")]
        [SerializeField] private Button undoButton;

        [Tooltip("Calls IConstructionService.RedoLast().")]
        [SerializeField] private Button redoButton;

        /// <summary>Fired when the Confirm button is clicked.</summary>
        public Action OnConfirmClicked;

        /// <summary>Fired when the Cancel button is clicked.</summary>
        public Action OnCancelClicked;

        /// <summary>Fired when the Undo button is clicked.</summary>
        public Action OnUndoClicked;

        /// <summary>Fired when the Redo button is clicked.</summary>
        public Action OnRedoClicked;

        private void Awake()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
            if (cancelButton  != null) cancelButton.onClick.AddListener(HandleCancel);
            if (undoButton    != null) undoButton.onClick.AddListener(HandleUndo);
            if (redoButton    != null) redoButton.onClick.AddListener(HandleRedo);
        }

        private void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
            if (cancelButton  != null) cancelButton.onClick.RemoveListener(HandleCancel);
            if (undoButton    != null) undoButton.onClick.RemoveListener(HandleUndo);
            if (redoButton    != null) redoButton.onClick.RemoveListener(HandleRedo);
        }

        private void HandleConfirm() => OnConfirmClicked?.Invoke();
        private void HandleCancel()  => OnCancelClicked?.Invoke();
        private void HandleUndo()    => OnUndoClicked?.Invoke();
        private void HandleRedo()    => OnRedoClicked?.Invoke();

        /// <summary>
        /// Update button interactability based on the current construction state.
        /// Called automatically by <see cref="ConstructionUIController"/>.
        /// </summary>
        public void SetState(ConstructionUIState state)
        {
            bool isPlacing = state.IsPlacing;

            if (confirmButton != null) confirmButton.interactable = isPlacing;
            if (cancelButton  != null) cancelButton.interactable  = isPlacing;
            if (undoButton    != null) undoButton.interactable    = isPlacing;
            if (redoButton    != null) redoButton.interactable    = isPlacing;
        }
    }
}
