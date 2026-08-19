using System.Collections.Generic;

namespace Kruty1918.Moyva.UIActions.API
{
    public interface IUiActionJournal
    {
        int Capacity { get; }
        int Count { get; }
        void Record(UiActionRequest request, string activeContext, UiActionResult result);
        IReadOnlyList<UiActionJournalEntry> GetRecent(int count);
    }
}
