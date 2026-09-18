using System.Collections.Generic;

namespace Kruty1918.Moyva.UIActions.API
{
    public interface IUiActionHandler
    {
        IReadOnlyCollection<UiActionId> ActionIds { get; }
        UiActionResult Execute(in UiActionRequest request);
    }
}
