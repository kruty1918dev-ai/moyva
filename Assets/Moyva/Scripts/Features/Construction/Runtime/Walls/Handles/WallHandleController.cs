using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallHandleController : IWallHandleController
    {
        private readonly SignalBus _signalBus;
        private readonly IConstructionWallSettingsProvider _wallSettingsProvider;
        private Vector2Int? _dragStartPosition;

        [Inject]
        public WallHandleController(SignalBus signalBus, [InjectOptional] IConstructionWallSettingsProvider wallSettingsProvider = null)
        {
            _signalBus = signalBus;
            _wallSettingsProvider = wallSettingsProvider;
        }

        public void Show(Vector2Int wallPosition)
        {
            if (_wallSettingsProvider != null && !_wallSettingsProvider.ShowWallHandles)
                return;

            _dragStartPosition = wallPosition;
            _signalBus.Fire(new ShowWallHandlesSignal { Center = wallPosition, Hide = false });
        }

        public void TrackDragStart(Vector2Int startPosition)
        {
            _dragStartPosition = startPosition;
        }

        public void EndDrag()
        {
            if (!_dragStartPosition.HasValue)
                return;

            _signalBus.Fire(new ShowWallHandlesSignal { Center = _dragStartPosition.Value, Hide = true });
            _dragStartPosition = null;
        }
    }
}
