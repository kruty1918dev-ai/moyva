using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    internal sealed class FakeUnitService : IUnitService
    {
        private readonly Dictionary<string, Vector2Int> _positions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _typeIds = new(StringComparer.Ordinal);

        public void Add(string unitId, Vector2Int position, string typeId)
        {
            _positions[unitId] = position;
            _typeIds[unitId] = typeId;
        }

        public float GetStamina(string unitId) => 0f;
        public void SetStamina(string unitId, float stamina) { }
        public bool TryGetUnitPosition(string unitId, out Vector2Int position)
            => _positions.TryGetValue(unitId ?? string.Empty, out position);
        public GameObject GetUnitObject(string unitId) => null;
        public IReadOnlyCollection<string> GetAllUnitIds() => _positions.Keys;
        public string GetUnitTypeId(string unitId)
            => _typeIds.TryGetValue(unitId ?? string.Empty, out var typeId) ? typeId : null;
    }

    internal sealed class FakeUnitClasses : IUnitClassConfig
    {
        private readonly Dictionary<string, UnitClassConfig> _configs = new(StringComparer.Ordinal);

        public void Add(string typeId, UnitClassConfig config) => _configs[typeId] = config;
        public UnitClassConfig GetConfig(string typeId)
            => typeId != null && _configs.TryGetValue(typeId, out var config) ? config : null;
    }

    internal sealed class FakeHealth : IHealth
    {
        public FakeHealth(string entityId, int maxHp)
        {
            EntityId = entityId;
            MaxHp = maxHp;
            CurrentHp = maxHp;
        }

        public string EntityId { get; }
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }
        public bool IsDestroyed => CurrentHp <= 0;
        public event Action<string, int, int, int> OnHealthChanged;
        public event Action<string> OnDestroyed;

        public void TakeDamage(int amount)
        {
            int old = CurrentHp;
            CurrentHp = Math.Max(0, CurrentHp - Math.Max(0, amount));
            OnHealthChanged?.Invoke(EntityId, old, CurrentHp, MaxHp);
            if (CurrentHp == 0) OnDestroyed?.Invoke(EntityId);
        }

        public void Heal(int amount) => CurrentHp = Math.Min(MaxHp, CurrentHp + Math.Max(0, amount));
        public void Kill() => TakeDamage(CurrentHp);
        public void Initialize(string entityId, int maxHp) { MaxHp = maxHp; CurrentHp = maxHp; }
    }

    internal sealed class FakeHealthRegistry : IHealthRegistry
    {
        private readonly Dictionary<string, IHealth> _health = new(StringComparer.Ordinal);

        public void Add(IHealth health) => _health[health.EntityId] = health;
        public void Register(IHealth health) => Add(health);
        public void Unregister(string entityId) => _health.Remove(entityId);
        public IHealth Get(string entityId)
            => _health.TryGetValue(entityId ?? string.Empty, out var health) ? health : null;
        public bool TryGet(string entityId, out IHealth health)
            => _health.TryGetValue(entityId ?? string.Empty, out health) && health != null && !health.IsDestroyed;
        public IReadOnlyCollection<IHealth> GetAll() => _health.Values;
        public IReadOnlyCollection<IHealth> GetMany(IEnumerable<string> entityIds)
        {
            var result = new List<IHealth>();
            if (entityIds != null)
                foreach (var id in entityIds)
                    if (_health.TryGetValue(id, out var health))
                        result.Add(health);
            return result;
        }
        public int Count => _health.Count;
    }

    internal sealed class FakeOwnership : IUnitOwnershipQuery
    {
        private readonly Dictionary<string, string> _owners = new(StringComparer.Ordinal);
        public void SetOwner(string unitId, string ownerId) => _owners[unitId] = ownerId;
        public string GetUnitOwnerId(string unitId)
            => _owners.TryGetValue(unitId ?? string.Empty, out var owner) ? owner : string.Empty;
    }

    internal sealed class FakeTurns : ITurnService
    {
        public event Action StateChanged;
        public TurnPhase Phase => TurnPhase.AwaitingInput;
        public int Round => 1;
        public long GlobalTurn => 1;
        public int ActionsThisTurn => 0;
        public string ActiveOwnerId => "player_0";
        public string LocalOwnerId => "player_0";
        public IReadOnlyList<TurnFaction> Factions => Array.Empty<TurnFaction>();
        public bool IsOwnerActive(string ownerId) => true;
        public bool CanOwnerAct(string ownerId, out string reason) { reason = null; return true; }
        public bool TryRecordAction(string ownerId, string actionId) => true;
        public bool TryEndTurn(string requesterOwnerId, out string reason) { reason = null; return true; }
    }

    internal sealed class FakeAuthority : ITurnAuthorityPolicy
    {
        public bool IsAuthoritative => true;
    }

    internal sealed class FakeFog : IFogOwnerStateReader
    {
        public FogStateType GetFogState(string ownerId, Vector2Int position) => FogStateType.Visible;
        public bool IsVisible(string ownerId, Vector2Int position) => true;
        public bool IsExplored(string ownerId, Vector2Int position) => true;
    }

    internal sealed class FakeCombatTargets : IConstructionBuildingCombatTargetQuery
    {
        public bool TryGetCombatTarget(string entityId, out ConstructionBuildingCombatTarget target)
        {
            target = default;
            return false;
        }
    }

    internal sealed class FakeBuildingRegistry : IBuildingRegistry
    {
        public BuildingDefinition[] GetAll() => Array.Empty<BuildingDefinition>();
        public BuildingDefinition GetById(string id) => null;
        public BuildingDefinition[] GetByCategory(BuildingCategory category) => Array.Empty<BuildingDefinition>();
        public WallCollectionDefinition[] GetWallCollections() => Array.Empty<WallCollectionDefinition>();
        public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId) => null;
    }

    internal sealed class FakeOwnershipTransfer : IConstructionOwnershipTransfer
    {
        public bool TryTransferPlacedBuildingOwner(Vector2Int position, string previousOwnerId,
            string nextOwnerId, out string reason)
        {
            reason = null;
            return true;
        }
    }

    internal sealed class FakeCaravanGameplayAccess : ICaravanGameplayAccess
    {
        public readonly Dictionary<string, CaravanUnitSnapshot> Wagons =
            new(StringComparer.Ordinal);

        public event Action ProgressAvailable;
        public bool IsAuthoritative => true;

        public bool TryGetWagon(string unitId, out CaravanUnitSnapshot unit)
            => Wagons.TryGetValue(unitId ?? string.Empty, out unit);

        public bool CanCommand(string ownerId, string unitId, out string reason)
        {
            reason = "Select a wagon belonging to your kingdom.";
            if (!Wagons.TryGetValue(unitId ?? string.Empty, out var wagon)) return false;
            if (!string.Equals(wagon.OwnerId, ownerId, StringComparison.Ordinal)) return false;
            reason = null;
            return true;
        }

        public bool CanAccessWarehouse(string unitId, Vector2Int origin, out string reason)
        {
            reason = null;
            return true;
        }

        public Task<CaravanTransferResult> MoveToWarehouseAsync(string unitId, Vector2Int origin,
            CancellationToken token)
            => Task.FromResult(CaravanTransferResult.Success());
    }
}
