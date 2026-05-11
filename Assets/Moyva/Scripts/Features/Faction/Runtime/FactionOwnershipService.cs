using System.Collections.Generic;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Faction.Runtime
{
    /// <summary>
    /// Слухає UnitCreatedSignal і UnitDestroyedSignal,
    /// щоб автоматично реєструвати/знімати юніти з реєстру власності фракцій.
    /// </summary>
    internal sealed class FactionOwnershipService : IFactionOwnershipService, IInitializable, System.IDisposable
    {
        private readonly SignalBus _signalBus;

        private readonly Dictionary<string, FactionId> _ownerByUnit  = new();
        private readonly Dictionary<string, List<string>> _unitsByFaction = new();

        public FactionOwnershipService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
        }

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            if (string.IsNullOrEmpty(signal.OwnerId))
                return;

            Register(signal.UnitId, new FactionId(signal.OwnerId));
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            Unregister(signal.UnitId);
        }

        public FactionId GetOwner(string unitId)
            => _ownerByUnit.TryGetValue(unitId, out var id) ? id : FactionId.Empty;

        public IReadOnlyList<string> GetUnitIds(FactionId factionId)
        {
            if (_unitsByFaction.TryGetValue(factionId.Value, out var list))
                return list;
            return System.Array.Empty<string>();
        }

        public void Register(string unitId, FactionId factionId)
        {
            if (string.IsNullOrEmpty(unitId) || factionId.IsEmpty)
                return;

            // Якщо юніт вже зареєстрований під іншою фракцією — спочатку знімаємо
            Unregister(unitId);

            _ownerByUnit[unitId] = factionId;

            if (!_unitsByFaction.TryGetValue(factionId.Value, out var list))
            {
                list = new List<string>();
                _unitsByFaction[factionId.Value] = list;
            }

            if (!list.Contains(unitId))
                list.Add(unitId);
        }

        public void Unregister(string unitId)
        {
            if (!_ownerByUnit.TryGetValue(unitId, out var prevFaction))
                return;

            _ownerByUnit.Remove(unitId);

            if (_unitsByFaction.TryGetValue(prevFaction.Value, out var list))
                list.Remove(unitId);
        }
    }
}
