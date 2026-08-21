using System;
using Sirenix.OdinInspector;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    [Serializable]
    public sealed class BotAnalyzerViewState
    {
        [ToggleLeft] public bool FollowActiveBot = true;
        [ToggleLeft] public bool CapturePaused;
        [ToggleLeft] public bool AutoScrollTimeline = true;
        [ToggleLeft] public bool FollowLatest = true;
        public BotAnalyzerTimelineFilter TimelineFilter = BotAnalyzerTimelineFilter.All;
        public string SearchText = string.Empty;
        public string SelectedOwnerId = string.Empty;
    }
}
