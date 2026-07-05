namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionInputSettingsProvider
    {
        float TouchTapMaxMovePixels { get; }
        float TouchTapMaxDurationSeconds { get; }
        bool EnableMousePendingPreviewDrag { get; }
        bool EnableTouchPendingPreviewDrag { get; }
        bool EnableMultiTouchCancel { get; }
        bool BlockInteractiveUI { get; }
        bool AllowClicksThroughNonInteractiveUI { get; }
    }
}
