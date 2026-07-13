using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionPreviewVisualSignalHandler
    {
        void Handle(BuildingPreviewChangedSignal signal);
        void Handle(BuildingPreviewMovedSignal signal);
        void Handle(BuildingPreviewDragVisualSignal signal);
        void Handle(BuildGridHoverChangedSignal signal);
        void Handle(BuildingCancelledSignal signal);
    }
}
