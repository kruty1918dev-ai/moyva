using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// FSM-реалізація AI-мозку для однієї Bot-фракції.
    ///
    /// Стани:
    ///   Idle      → початковий стан після ініціалізації.
    ///   Expanding → бот нарощує кількість юнітів.
    ///   Attacking → достатньо юнітів; рухає їх до найближчого ворога.
    ///   Defending → мало юнітів; тримає базові охоронці.
    /// </summary>
    internal sealed class BotBrain : IBotController
    {
        private const int AttackRange   = 8;
        private const int MinBaseGuards = 2;

        public FactionId FactionId    => _definition.FactionId;
        public BotState  CurrentState { get; private set; } = BotState.Idle;

        private readonly FactionDefinition        _definition;
        private readonly IFactionRegistry         _factionRegistry;
        private readonly IUnitFactory             _unitFactory;
        private readonly IFactionOwnershipService _ownership;
        private readonly IUnitService             _unitService;
        private readonly IUnitMovementService     _movementService;
        private readonly IBotDifficultySettings   _settings;

        [InjectOptional]
        private IFogOfWarServiceRegistry _fogRegistry;

        // CancellationTokenSource per unit to cancel previous orders
        private readonly Dictionary<string, CancellationTokenSource> _activeMoves = new();

        /// <summary>
        /// Zenject inject: _definition передається як extra arg через container.Instantiate,
        /// решта — стандартні DI-залежності.
        /// </summary>
        [Inject]
        public BotBrain(
            FactionDefinition        definition,
            IFactionRegistry         factionRegistry,
            IUnitFactory             unitFactory,
            IFactionOwnershipService ownership,
            IUnitService             unitService,
            IUnitMovementService     movementService,
            IBotDifficultySettings   settings)
        {
            _definition      = definition;
            _factionRegistry = factionRegistry;
            _unitFactory     = unitFactory;
            _ownership       = ownership;
            _unitService     = unitService;
            _movementService = movementService;
            _settings        = settings;
        }

        public void Tick()
        {
            var myUnits   = _ownership.GetUnitIds(_definition.FactionId);
            int unitCount = myUnits.Count;

            switch (CurrentState)
            {
                case BotState.Idle:
                    TransitionToExpanding();
                    break;

                case BotState.Expanding:
                    TickExpanding(unitCount, myUnits);
                    break;

                case BotState.Attacking:
                    TickAttacking(unitCount, myUnits);
                    break;

                case BotState.Defending:
                    TickDefending(unitCount, myUnits);
                    break;
            }
        }

        // ─── State Transitions ────────────────────────────────────────────────

        private void TransitionToExpanding()
        {
            CurrentState = BotState.Expanding;
            Debug.Log($"[BotBrain:{_definition.FactionId}] → Expanding");
        }

        private void TickExpanding(int unitCount, IReadOnlyList<string> myUnits)
        {
            if (unitCount >= _settings.AttackThreshold)
            {
                CurrentState = BotState.Attacking;
                Debug.Log($"[BotBrain:{_definition.FactionId}] → Attacking ({unitCount} юнітів)");
                return;
            }

            SpawnStartUnit();
        }

        private void TickAttacking(int unitCount, IReadOnlyList<string> myUnits)
        {
            if (unitCount <= _settings.DefendThreshold)
            {
                CurrentState = BotState.Defending;
                Debug.Log($"[BotBrain:{_definition.FactionId}] → Defending ({unitCount} юнітів)");
                return;
            }

            ExecuteAttack(myUnits);
        }

        private void TickDefending(int unitCount, IReadOnlyList<string> myUnits)
        {
            if (unitCount < _settings.DefendThreshold)
            {
                CurrentState = BotState.Expanding;
                Debug.Log($"[BotBrain:{_definition.FactionId}] → Expanding (мало юнітів: {unitCount})");
                return;
            }

            // In defending state, hold guard positions (same logic as attack but staying near base)
        }

        // ─── Combat Logic ─────────────────────────────────────────────────────

        private void ExecuteAttack(IReadOnlyList<string> myUnits)
        {
            var enemyPositions = CollectEnemyPositions();
            if (enemyPositions.Count == 0)
                return;

            // Determine which of our units are "base guards"
            var baseGuards = new HashSet<string>();
            if (myUnits.Count > MinBaseGuards)
            {
                int guardCount = 0;
                foreach (var uid in myUnits)
                {
                    if (_unitService.TryGetUnitPosition(uid, out var pos)
                        && ChebyshevDist(pos, _definition.StartPosition) <= 3)
                    {
                        baseGuards.Add(uid);
                        if (++guardCount >= MinBaseGuards) break;
                    }
                }
            }

            // Send non-guard units toward nearest visible enemy
            foreach (var uid in myUnits)
            {
                if (baseGuards.Contains(uid)) continue;
                if (!_unitService.TryGetUnitPosition(uid, out var myPos)) continue;

                var target = FindNearestVisibleEnemy(myPos, enemyPositions);
                if (target == null) continue;

                IssueMoveOrder(uid, target.Value);
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private List<Vector2Int> CollectEnemyPositions()
        {
            var result = new List<Vector2Int>();
            foreach (var faction in _factionRegistry.GetAll())
            {
                if (faction.FactionId == _definition.FactionId) continue;
                foreach (var uid in _ownership.GetUnitIds(faction.FactionId))
                {
                    if (_unitService.TryGetUnitPosition(uid, out var pos))
                        result.Add(pos);
                }
            }
            return result;
        }

        private Vector2Int? FindNearestVisibleEnemy(Vector2Int from, List<Vector2Int> enemies)
        {
            IFogOfWarService fog = null;
            _fogRegistry?.TryGetFor(_definition.FactionId.Value, out fog);

            Vector2Int? best     = null;
            int         bestDist = int.MaxValue;

            foreach (var ePos in enemies)
            {
                int d = ManhattanDist(from, ePos);
                if (d > AttackRange) continue;
                if (fog != null && !fog.IsVisible(ePos)) continue;
                if (d < bestDist) { bestDist = d; best = ePos; }
            }

            return best;
        }

        private void IssueMoveOrder(string unitId, Vector2Int target)
        {
            if (_activeMoves.TryGetValue(unitId, out var old))
            {
                old.Cancel();
                old.Dispose();
            }

            var cts = new CancellationTokenSource();
            _activeMoves[unitId] = cts;

            _ = _movementService.MoveUnitAsync(unitId, target, cts.Token);
        }

        private void SpawnStartUnit()
        {
            if (string.IsNullOrEmpty(_definition.DefaultUnitTypeId))
            {
                Debug.LogWarning($"[BotBrain:{_definition.FactionId}] DefaultUnitTypeId не вказано — спавн пропущено.");
                return;
            }

            string unitId = _unitFactory.CreateUnit(
                _definition.DefaultUnitTypeId,
                _definition.StartPosition,
                _definition.FactionId.Value);

            if (!string.IsNullOrEmpty(unitId))
                Debug.Log($"[BotBrain:{_definition.FactionId}] Spawned unit '{unitId}' at {_definition.StartPosition}.");
        }

        private static int ManhattanDist(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        private static int ChebyshevDist(Vector2Int a, Vector2Int b)
            => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }
}
