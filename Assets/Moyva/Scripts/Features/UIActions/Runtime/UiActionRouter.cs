using System.Collections.Generic;
using Kruty1918.Moyva.UIActions.API;

namespace Kruty1918.Moyva.UIActions.Runtime
{
    internal sealed class UiActionRouter : IUiActionRouter
    {
        private readonly List<IUiActionHandler> _handlers;
        private readonly IUiContextStack _contexts;
        private readonly IUiActionJournal _journal;

        public UiActionRouter(
            List<IUiActionHandler> handlers,
            IUiContextStack contexts,
            IUiActionJournal journal)
        {
            _handlers = handlers ?? new List<IUiActionHandler>();
            _contexts = contexts;
            _journal = journal;
        }

        public UiActionResult Execute(
            string actionId,
            UiActionSource source = UiActionSource.Programmatic,
            string context = null,
            string targetId = null)
        {
            return Execute(new UiActionRequest(
                actionId,
                source,
                context,
                targetId));
        }

        public UiActionResult Execute(UiActionRequest request)
        {
            UiActionResult result = ExecuteCore(request);
            _journal?.Record(
                request,
                request.Context ?? _contexts?.ActiveContextId,
                result);
            return result;
        }

        private UiActionResult ExecuteCore(UiActionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ActionId))
                return UiActionResult.Ignored(UiActionReasonCode.ActionUnavailable);

            if (!_contexts.IsActionAllowedByContext(request.ActionId))
                return UiActionResult.Rejected(UiActionReasonCode.WrongContext);

            for (int i = 0; i < _handlers.Count; i++)
            {
                IUiActionHandler handler = _handlers[i];
                if (handler != null && handler.CanHandle(request.ActionId))
                    return handler.Handle(request);
            }

            return UiActionResult.Rejected(UiActionReasonCode.ActionUnavailable);
        }
    }
}
