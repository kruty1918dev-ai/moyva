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
    internal sealed class GameplayWorldFocusPingPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly SignalBus _signals;
        private readonly IGridProjection _grid;
        private GameObject _root;
        private LineRenderer _line;
        private float _startedAt = -1f;
        private float _duration = 1.2f;
        private Vector3 _center;

        public GameplayWorldFocusPingPresenter(SignalBus signals, IGridProjection grid)
        {
            _signals = signals;
            _grid = grid;
        }
        public void Initialize() => _signals.Subscribe<WorldFocusPingRequestedSignal>(OnPing);

        public void Tick()
        {
            if (_line == null || _startedAt < 0f)
                return;
            float progress = Mathf.Clamp01((Time.unscaledTime - _startedAt) / _duration);
            if (progress >= 1f)
            {
                _line.enabled = false;
                _startedAt = -1f;
                return;
            }

            float radius = Mathf.Lerp(0.35f, 1.65f, progress);
            float alpha = 1f - progress;
            Color color = new Color(0.94f, 0.73f, 0.25f, alpha);
            _line.startColor = color;
            _line.endColor = color;
            SetCircle(radius);
        }

        public void Dispose()
        {
            _signals.TryUnsubscribe<WorldFocusPingRequestedSignal>(OnPing);
            if (_root != null)
                UnityEngine.Object.Destroy(_root);
        }

        private void OnPing(WorldFocusPingRequestedSignal signal)
        {
            EnsureView();
            _center = _grid.GridToWorld(signal.Position);
            _duration = Mathf.Max(0.2f, signal.DurationSeconds);
            _startedAt = Time.unscaledTime;
            _line.enabled = true;
            SetCircle(0.35f);
        }

        private void EnsureView()
        {
            if (_line != null)
                return;
            _root = new GameObject("Presentation/WorldFocusPing");
            _line = _root.AddComponent<LineRenderer>();
            _line.loop = true;
            _line.useWorldSpace = true;
            _line.positionCount = 49;
            _line.widthMultiplier = 0.08f;
            _line.numCornerVertices = 2;
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.sortingOrder = 32000;
        }

        private void SetCircle(float radius)
        {
            for (int index = 0; index < _line.positionCount; index++)
            {
                float angle = index / (float)(_line.positionCount - 1) * Mathf.PI * 2f;
                Vector3 offset = _grid.WorldPlane == GridWorldPlane.XZ
                    ? new Vector3(Mathf.Cos(angle) * radius, 0.08f, Mathf.Sin(angle) * radius)
                    : new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, -0.08f);
                _line.SetPosition(index, _center + offset);
            }
        }
    }
}
