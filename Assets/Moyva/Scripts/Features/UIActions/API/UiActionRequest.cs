using System.Collections.Generic;

namespace Kruty1918.Moyva.UIActions.API
{
    public readonly struct UiActionRequest
    {
        public UiActionRequest(
            string actionId,
            UiActionSource source = UiActionSource.Programmatic,
            string context = null,
            string targetId = null,
            IReadOnlyDictionary<string, string> payload = null)
        {
            ActionId = actionId;
            Source = source;
            Context = context;
            TargetId = targetId;
            Payload = payload;
        }

        public string ActionId { get; }
        public UiActionSource Source { get; }
        public string Context { get; }
        public string TargetId { get; }
        public IReadOnlyDictionary<string, string> Payload { get; }
    }
}
