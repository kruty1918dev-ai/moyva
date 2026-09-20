using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.Runtime.Motion
{
    /// <summary>Безперервний рух юніта шляхом: інтерполює позицію/yaw між тайлами з прискоренням, поворотами і bob.</summary>
    public sealed class PathTraversalMotion
    {
        private const float ArriveEpsilon = 0.005f;
        private const float MinCreepSpeed = 0.05f;

        private readonly IReadOnlyList<Vector3> _points;
        private readonly float[] _cumLengths;
        private readonly float _cruiseSpeed;
        private readonly float _acceleration;
        private readonly float _deceleration;
        private readonly float _turnSpeedDegPerSec;
        private readonly bool _faceDirection;
        private readonly float _bobAmplitude;
        private readonly float _bobFrequency;
        private readonly float _cornerAnticipation;
        private readonly Func<int, bool> _canAdvance;
        private readonly Action<int> _onVertexReached;

        private readonly float _totalLength;
        private float _travelled;
        private float _speed;
        private float _yawDeg;
        private float _bobPhase;
        private int _cursor;
        private Vector3 _pathPosition;

        /// <summary>Індекс останньої досягнутої точки шляху.</summary>
        public int ReachedIndex { get; private set; }

        /// <summary>Чи завершено проходження шляху.</summary>
        public bool IsComplete { get; private set; }

        /// <summary>Чи було проходження зупинено достроково.</summary>
        public bool WasHalted { get; private set; }

        /// <summary>Поточна світова позиція руху.</summary>
        public Vector3 Position { get; private set; }

        /// <summary>Поточний кут yaw у градусах.</summary>
        public float YawDegrees => _yawDeg;

        /// <summary>Поточна швидкість руху.</summary>
        public float Speed => _speed;

        /// <summary>Створює рух по шляху з налаштуваннями та списком тайлів.</summary>
        public PathTraversalMotion(
            IReadOnlyList<Vector3> points,
            float cruiseSpeed,
            float acceleration,
            float deceleration,
            float turnSpeedDegPerSec,
            bool faceDirection,
            float bobAmplitude,
            float bobFrequency,
            float cornerAnticipation,
            float initialYawDeg,
            Func<int, bool> canAdvance = null,
            Action<int> onVertexReached = null)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("PathTraversalMotion requires at least one point.", nameof(points));

            _points = points;
            _cruiseSpeed = Mathf.Max(0.01f, cruiseSpeed);
            _acceleration = Mathf.Max(0.01f, acceleration);
            _deceleration = Mathf.Max(0.01f, deceleration);
            _turnSpeedDegPerSec = Mathf.Max(0f, turnSpeedDegPerSec);
            _faceDirection = faceDirection;
            _bobAmplitude = Mathf.Max(0f, bobAmplitude);
            _bobFrequency = Mathf.Max(0f, bobFrequency);
            _cornerAnticipation = Mathf.Clamp01(cornerAnticipation);
            _canAdvance = canAdvance;
            _onVertexReached = onVertexReached;
            _yawDeg = initialYawDeg;

            _cumLengths = new float[points.Count];
            for (int i = 1; i < points.Count; i++)
                _cumLengths[i] = _cumLengths[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            _totalLength = _cumLengths[points.Count - 1];

            _pathPosition = points[0];
            Position = _pathPosition;

            if (_totalLength <= ArriveEpsilon)
            {
                // Degenerate path — snap and complete.
                _pathPosition = points[points.Count - 1];
                Position = _pathPosition;
                IsComplete = true;
                return;
            }

            // Same authority ordering as the legacy loop: standing at vertex 0,
            // the gate for vertex 1 is evaluated before any motion happens.
            if (_canAdvance != null && !_canAdvance(1))
            {
                WasHalted = true;
                IsComplete = true;
            }
        }

        /// <summary>Прокачує рух на deltaTime: позиція, поворот, bob, колбеки кроків.</summary>
        public void Tick(float deltaTime)
        {
            if (IsComplete || deltaTime <= 0f)
                return;

            float remaining = _totalLength - _travelled;

            // Velocity profile: accelerate to cruise, brake into the terminal vertex.
            float brakeDist = _speed * _speed / (2f * _deceleration);
            float targetSpeed = remaining <= brakeDist + ArriveEpsilon
                ? Mathf.Max(MinCreepSpeed, Mathf.Sqrt(2f * _deceleration * Mathf.Max(remaining, 0f)))
                : _cruiseSpeed;

            // Facing: large heading error bleeds speed — the unit turns, then accelerates.
            if (_faceDirection)
            {
                float yawError = Mathf.Abs(Mathf.DeltaAngle(_yawDeg, DesiredYaw()));
                if (yawError > 45f)
                    targetSpeed = Mathf.Min(targetSpeed, _cruiseSpeed * 0.35f);
            }

            _speed = targetSpeed < _speed
                ? Mathf.Max(targetSpeed, _speed - _deceleration * deltaTime)
                : Mathf.Min(targetSpeed, _speed + _acceleration * deltaTime);

            _travelled = Mathf.Min(_travelled + _speed * deltaTime, _totalLength);

            // Vertex crossings — authority events and gates.
            while (!IsComplete
                   && ReachedIndex + 1 < _points.Count
                   && _travelled >= _cumLengths[ReachedIndex + 1] - ArriveEpsilon)
            {
                int reached = ReachedIndex + 1;
                ReachedIndex = reached;
                _onVertexReached?.Invoke(reached);

                if (reached == _points.Count - 1)
                {
                    CompleteAt(reached);
                    return;
                }

                if (_canAdvance != null && !_canAdvance(reached + 1))
                {
                    // Gate refused the next tile — pin exactly on the last valid vertex.
                    WasHalted = true;
                    CompleteAt(reached);
                    return;
                }
            }

            // Resolve position along the polyline.
            while (_cursor + 1 < _points.Count - 1 && _travelled > _cumLengths[_cursor + 1])
                _cursor++;
            float segStart = _cumLengths[_cursor];
            float segLen = Mathf.Max(_cumLengths[_cursor + 1] - segStart, 0.0001f);
            float segT = Mathf.Clamp01((_travelled - segStart) / segLen);
            _pathPosition = Vector3.LerpUnclamped(_points[_cursor], _points[_cursor + 1], segT);

            // Bob: speed-scaled subtle vertical rhythm (secondary motion).
            if (_bobAmplitude > 0f && _bobFrequency > 0f)
            {
                _bobPhase += _speed * _bobFrequency * deltaTime;
                _pathPosition.y += _bobAmplitude * Mathf.Sin(_bobPhase * Mathf.PI * 2f);
            }
            Position = _pathPosition;

            // Facing follows the (anticipation-blended) heading.
            if (_faceDirection && _turnSpeedDegPerSec > 0f)
            {
                float desired = DesiredYaw();
                _yawDeg = Mathf.MoveTowardsAngle(_yawDeg, desired, _turnSpeedDegPerSec * deltaTime);
            }
        }

        private void CompleteAt(int vertexIndex)
        {
            _travelled = _cumLengths[vertexIndex];
            _pathPosition = _points[vertexIndex];
            Position = _pathPosition;
            _speed = 0f;
            IsComplete = true;
        }

        /// <summary>
        /// Desired facing yaw: current segment direction, blended toward the next
        /// segment's direction near the vertex so corners turn progressively
        /// instead of snapping 90° in one frame.
        /// </summary>
        private float DesiredYaw()
        {
            int seg = Mathf.Min(_cursor, _points.Count - 2);
            Vector3 dir = _points[seg + 1] - _points[seg];
            dir.y = 0f;

            if (_cornerAnticipation > 0f && seg + 2 <= _points.Count - 1)
            {
                float segLen = Mathf.Max(_cumLengths[seg + 1] - _cumLengths[seg], 0.0001f);
                float distToVertex = _cumLengths[seg + 1] - _travelled;
                float blend = Mathf.Clamp01(1f - distToVertex / (segLen * _cornerAnticipation));
                if (blend > 0f)
                {
                    Vector3 next = _points[seg + 2] - _points[seg + 1];
                    next.y = 0f;
                    dir = Vector3.Lerp(dir.normalized, next.normalized, blend);
                }
            }

            if (dir.sqrMagnitude < 0.000001f)
                return _yawDeg;
            return Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        }
    }
}
