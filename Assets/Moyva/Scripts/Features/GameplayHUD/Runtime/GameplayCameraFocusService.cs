using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Notifications.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using UnityHTML.Runtime;
using Zenject;
using System.Text;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplayCameraFocusService : IGameplayCameraFocusService
    {
        private readonly ICameraMovement _camera;
        private readonly IGridProjection _grid;
        private readonly SignalBus _signals;

        public GameplayCameraFocusService(ICameraMovement camera, IGridProjection grid, SignalBus signals)
        {
            _camera = camera;
            _grid = grid;
            _signals = signals;
        }
        public void FocusGridPosition(Vector2Int gridPosition, string targetId = null)
        {
            _camera.MoveCameraFocusToWorldPoint(_grid.GridToWorld(gridPosition), false);
            _signals.Fire(new WorldFocusPingRequestedSignal
            {
                TargetId = targetId ?? string.Empty,
                Position = gridPosition,
                DurationSeconds = 1.2f,
            });
        }
    }
}
