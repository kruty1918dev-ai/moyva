using Kruty1918.Moyva.UIActions.API;
using TMPro;
using UnityEngine.EventSystems;

namespace Kruty1918.Moyva.UIActions.Runtime
{
    internal sealed class UiEscapeRouter : IUiEscapeRouter
    {
        private readonly IUiContextStack _contexts;
        private readonly IUiActionRouter _actions;
        private readonly IUiActionJournal _journal;

        public UiEscapeRouter(
            IUiContextStack contexts,
            IUiActionRouter actions,
            IUiActionJournal journal)
        {
            _contexts = contexts;
            _actions = actions;
            _journal = journal;
        }

        public bool TryHandleEscape()
        {
            _journal?.Record(
                new UiActionRequest(UiActionIds.Diagnostics.InputEscape, UiActionSource.Escape, _contexts?.ActiveContextId),
                UiActionResult.Performed());

            if (TryUnfocusTextInput())
                return true;

            var active = _contexts.ActiveContexts;
            for (int i = 0; i < active.Count; i++)
            {
                UiContextRegistration context = active[i];
                if (!UiActionId.IsValid(context.EscapeActionId.Value))
                    continue;

                UiActionResult result = _actions.Execute(new UiActionRequest(
                    context.EscapeActionId,
                    UiActionSource.Escape,
                    context.ContextId));
                if (result.Consumed)
                    return true;

                if (result.Status == UiActionStatus.Rejected
                    && result.Reason != UiActionReason.WrongContext)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryUnfocusTextInput()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || eventSystem.currentSelectedGameObject == null)
                return false;

            var input = eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>();
            if (input == null || !input.isFocused)
                return false;

            eventSystem.SetSelectedGameObject(null);
            _journal?.Record(
                new UiActionRequest(UiActionIds.Diagnostics.TextUnfocus, UiActionSource.Escape, "TextEditing"),
                UiActionResult.Performed());
            return true;
        }
    }
}
