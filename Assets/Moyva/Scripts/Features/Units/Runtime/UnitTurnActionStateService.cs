using System.Collections.Generic;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitTurnActionStateService : IUnitTurnActionStateService, IInitializable, System.IDisposable
    {
        private readonly ITurnService _turns;
        private readonly HashSet<string> _attacked = new(System.StringComparer.Ordinal);
        private long _observedGlobalTurn;

        [Inject]
        public UnitTurnActionStateService([InjectOptional] ITurnService turns = null)
        {
            _turns = turns;
        }

        public void Initialize()
        {
            _observedGlobalTurn = _turns?.GlobalTurn ?? 0L;
            if (_turns != null)
                _turns.StateChanged += OnTurnStateChanged;
        }

        public void Dispose()
        {
            if (_turns != null)
                _turns.StateChanged -= OnTurnStateChanged;
        }

        public UnitTurnActionState Get(string unitId)
            => new(_attacked.Contains(Normalize(unitId)));

        public bool CanAttack(string attackerUnitId, out string reason)
        {
            if (_attacked.Contains(Normalize(attackerUnitId)))
            {
                reason = "Unit has already attacked this turn.";
                return false;
            }

            reason = null;
            return true;
        }

        public void RecordAttack(string unitId)
        {
            string normalized = Normalize(unitId);
            if (!string.IsNullOrEmpty(normalized))
                _attacked.Add(normalized);
        }

        public void ClearAll()
            => _attacked.Clear();

        private void OnTurnStateChanged()
        {
            long current = _turns?.GlobalTurn ?? 0L;
            if (current == _observedGlobalTurn)
                return;

            _observedGlobalTurn = current;
            ClearAll();
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
