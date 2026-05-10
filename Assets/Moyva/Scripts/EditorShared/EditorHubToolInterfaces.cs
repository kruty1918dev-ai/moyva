using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public interface IMoyvaHubPreviewProvider
    {
        string HubToolMenuPath { get; }
        string GetHubPreviewSummary();
        void DrawHubPreview(Rect rect);
    }

    public interface IMoyvaHubSettingsOpener
    {
        bool OpenHubSettingsFromPreview();
    }
}
