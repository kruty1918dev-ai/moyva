using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotMemoryStore : IBotMemoryStore
    {
        private readonly Dictionary<string, Dictionary<string, BotKnownEntityMemory>> _byOwner = new(StringComparer.Ordinal);

        [Inject]
        public BotMemoryStore()
        {
        }

        public IReadOnlyList<BotKnownEntityMemory> GetMemory(string ownerId, long globalTurn)
        {
            string owner = Normalize(ownerId);
            if (owner == null || !_byOwner.TryGetValue(owner, out Dictionary<string, BotKnownEntityMemory> records))
                return Array.Empty<BotKnownEntityMemory>();

            var result = new List<BotKnownEntityMemory>(records.Values);
            result.RemoveAll(record => record.WasConfirmedDestroyed || GetConfidence(record, globalTurn) <= 0);
            result.Sort(CompareMemory);
            return result;
        }

        public void UpdateFromObservation(BotWorldSnapshot snapshot, BotPlanningProfile profile)
        {
            if (snapshot == null)
                return;

            string owner = Normalize(snapshot.OwnerId);
            if (owner == null)
                return;

            if (!_byOwner.TryGetValue(owner, out Dictionary<string, BotKnownEntityMemory> records))
            {
                records = new Dictionary<string, BotKnownEntityMemory>(StringComparer.Ordinal);
                _byOwner.Add(owner, records);
            }

            for (int index = 0; index < snapshot.VisibleEnemyUnits.Count; index++)
            {
                BotUnitSnapshot unit = snapshot.VisibleEnemyUnits[index];
                if (string.IsNullOrWhiteSpace(unit.UnitId))
                    continue;

                records[unit.UnitId] = new BotKnownEntityMemory(
                    unit.UnitId,
                    BotKnownEntityKind.Unit,
                    unit.OwnerId,
                    unit.TypeId,
                    unit.Position,
                    snapshot.GlobalTurn);
            }

            for (int index = 0; index < snapshot.VisibleEnemyBuildings.Count; index++)
            {
                BotBuildingSnapshot building = snapshot.VisibleEnemyBuildings[index];
                if (string.IsNullOrWhiteSpace(building.BuildingId))
                    continue;

                string entityId = $"building:{building.OwnerId}:{building.BuildingId}:{building.Position.x},{building.Position.y}";
                records[entityId] = new BotKnownEntityMemory(
                    entityId,
                    IsLikelyCastle(building.BuildingId) ? BotKnownEntityKind.Objective : BotKnownEntityKind.Building,
                    building.OwnerId,
                    building.BuildingId,
                    building.Position,
                    snapshot.GlobalTurn);
            }

            Prune(owner, snapshot.GlobalTurn, profile);
        }

        public int GetConfidence(BotKnownEntityMemory memory, long globalTurn)
        {
            if (memory.WasConfirmedDestroyed)
                return 0;

            long age = Math.Max(0L, globalTurn - memory.LastSeenGlobalTurn);
            if (memory.Kind != BotKnownEntityKind.Unit)
            {
                if (age == 0) return 1000;
                if (age <= 2) return 900;
                if (age <= 6) return 750;
                if (age <= 12) return 500;
                return 250;
            }

            if (age == 0) return 1000;
            if (age == 1) return 850;
            if (age == 2) return 650;
            if (age == 3) return 450;
            if (age == 4) return 300;
            if (age < 6) return 150;
            return 0;
        }

        public IReadOnlyList<BotMemoryOwnerSnapshot> CaptureMemory()
        {
            var owners = new List<string>(_byOwner.Keys);
            owners.Sort(StringComparer.Ordinal);

            var result = new List<BotMemoryOwnerSnapshot>(owners.Count);
            for (int ownerIndex = 0; ownerIndex < owners.Count; ownerIndex++)
            {
                string owner = owners[ownerIndex];
                if (!_byOwner.TryGetValue(owner, out Dictionary<string, BotKnownEntityMemory> records))
                    continue;

                var values = new List<BotKnownEntityMemory>(records.Values);
                values.Sort(CompareMemoryForSave);
                result.Add(new BotMemoryOwnerSnapshot(owner, values));
            }

            return result;
        }

        public void RestoreMemory(IReadOnlyList<BotMemoryOwnerSnapshot> snapshots)
        {
            _byOwner.Clear();
            if (snapshots == null)
                return;

            for (int index = 0; index < snapshots.Count; index++)
            {
                BotMemoryOwnerSnapshot snapshot = snapshots[index];
                string owner = Normalize(snapshot.OwnerId);
                if (owner == null)
                    continue;

                var records = new Dictionary<string, BotKnownEntityMemory>(StringComparer.Ordinal);
                IReadOnlyList<BotKnownEntityMemory> source = snapshot.Records;
                if (source != null)
                {
                    for (int recordIndex = 0; recordIndex < source.Count; recordIndex++)
                    {
                        BotKnownEntityMemory memory = source[recordIndex];
                        if (string.IsNullOrWhiteSpace(memory.EntityId) || memory.WasConfirmedDestroyed)
                            continue;

                        records[memory.EntityId] = memory;
                    }
                }

                _byOwner[owner] = records;
            }
        }

        private void Prune(string ownerId, long globalTurn, BotPlanningProfile profile)
        {
            if (!_byOwner.TryGetValue(ownerId, out Dictionary<string, BotKnownEntityMemory> records))
                return;

            var expired = new List<string>();
            foreach (KeyValuePair<string, BotKnownEntityMemory> pair in records)
            {
                if (GetConfidence(pair.Value, globalTurn) <= 0)
                    expired.Add(pair.Key);
            }

            for (int index = 0; index < expired.Count; index++)
                records.Remove(expired[index]);
        }

        private static int CompareMemory(BotKnownEntityMemory left, BotKnownEntityMemory right)
        {
            int turn = right.LastSeenGlobalTurn.CompareTo(left.LastSeenGlobalTurn);
            return turn != 0 ? turn : string.CompareOrdinal(left.EntityId, right.EntityId);
        }

        private static int CompareMemoryForSave(BotKnownEntityMemory left, BotKnownEntityMemory right)
            => string.CompareOrdinal(left.EntityId, right.EntityId);

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool IsLikelyCastle(string buildingId)
            => !string.IsNullOrWhiteSpace(buildingId)
                && buildingId.IndexOf("castle", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
