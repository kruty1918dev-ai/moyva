using System.Collections.Generic;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Mutually exclusive world-grid action overlay. One owner may render at a time.
    /// </summary>
    public interface IGridActionOverlayService
    {
        GridActionOverlayOwner ActiveOwner { get; }
        bool HasActiveOwner { get; }
        bool Acquire(GridActionOverlayOwner owner);
        void Show(GridActionOverlayOwner owner, IReadOnlyList<GridActionOverlayCell> cells);
        void Update(GridActionOverlayOwner owner, IReadOnlyList<GridActionOverlayCell> cells);
        void Hide(GridActionOverlayOwner owner);
        void Release(GridActionOverlayOwner owner);
    }
}
