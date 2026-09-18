using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    [SaveModuleId("Kruty1918.Moyva.Economy.Caravans")]
    internal sealed partial class CaravanService : IStagedSaveModule, IInitializable, IDisposable
    {
        public void Initialize()
        {
            _signals.Subscribe<UnitMovedSignal>(OnMoved);
            _signals.Subscribe<UnitDestroyedSignal>(OnDestroyed);
            _signals.Subscribe<SettlementResourceChangedSignal>(OnSuppliesChanged);
            _signals.Subscribe<MoveUnitRequestSignal>(OnManualMoveRequested);
            _signals.Subscribe<SettlementCreatedSignal>(OnSettlementCreated);
            _gameplay.Value.ProgressAvailable += RequestRouteTick;
        }

        public void Dispose()
        {
            _disposed = true;
            ClearRoutes();
            _gameplay.Value.ProgressAvailable -= RequestRouteTick;
            _signals.TryUnsubscribe<UnitMovedSignal>(OnMoved);
            _signals.TryUnsubscribe<UnitDestroyedSignal>(OnDestroyed);
            _signals.TryUnsubscribe<SettlementResourceChangedSignal>(OnSuppliesChanged);
            _signals.TryUnsubscribe<MoveUnitRequestSignal>(OnManualMoveRequested);
            _signals.TryUnsubscribe<SettlementCreatedSignal>(OnSettlementCreated);
        }

        private void OnMoved(UnitMovedSignal signal)
        {
            if (_cargo.TryGetValue(signal.UnitId, out var cargo))
                cargo.Position = signal.NewPosition;
        }

        private void OnDestroyed(UnitDestroyedSignal signal)
        {
            bool routeRemoved = RemoveRoute(signal.UnitId);
            if (!_cargo.TryGetValue(signal.UnitId, out var cargo))
            { if (routeRemoved) Changed?.Invoke(); return; }
            _cargo.Remove(signal.UnitId);
            if (_gameplay.Value.IsAuthoritative && cargo.Resources.Count > 0)
            {
                if (!_loot.TryGetValue(cargo.Position, out var loot))
                    _loot[cargo.Position] = loot = new Dictionary<string, float>(StringComparer.Ordinal);
                foreach (var pair in cargo.Resources) Add(loot, pair.Key, pair.Value);
            }
            Changed?.Invoke();
        }

        public void OnSave(ISaveContext context)
        {
            if (_executing) throw new InvalidOperationException("Cannot save during a cargo transaction.");
            var writer = context.Writer;
            writer.Write(3);
            writer.Write(_cargo.Count);
            foreach (var pair in _cargo)
            {
                writer.Write(pair.Key);
                writer.Write(pair.Value.OwnerId);
                writer.Write(pair.Value.Position.x);
                writer.Write(pair.Value.Position.y);
                WriteResources(writer, pair.Value.Resources);
            }
            writer.Write(_loot.Count);
            foreach (var pair in _loot)
            {
                writer.Write(pair.Key.x);
                writer.Write(pair.Key.y);
                WriteResources(writer, pair.Value);
            }
            WriteRoutes(writer);
            writer.Write(_pendingFoundingCargoDeposits.Count);
            foreach (var pair in _pendingFoundingCargoDeposits)
            {
                writer.Write(pair.Key.x);
                writer.Write(pair.Key.y);
                writer.Write(pair.Value ?? string.Empty);
            }
        }

        public void OnLoad(ISaveContext context)
            => PrepareLoad(context)();

        public Action PrepareLoad(ISaveContext context)
        {
            if (_executing) throw new InvalidOperationException("Cannot load during a cargo transaction.");
            var reader = context.Reader;
            int version = reader.ReadInt32();
            if (version < 1 || version > 3) throw new InvalidDataException("Unsupported caravan save version.");
            var cargo = new Dictionary<string, CargoState>(StringComparer.Ordinal);
            int cargoCount = ReadCount(reader, 100000);
            for (int i = 0; i < cargoCount; i++)
            {
                string unitId = reader.ReadString();
                string ownerId = reader.ReadString();
                if (string.IsNullOrWhiteSpace(unitId) || string.IsNullOrWhiteSpace(ownerId)
                    || cargo.ContainsKey(unitId)) throw new InvalidDataException("Invalid caravan identity.");
                var state = new CargoState
                {
                    OwnerId = ownerId,
                    Position = new Vector2Int(reader.ReadInt32(), reader.ReadInt32()),
                };
                ReadResources(reader, state.Resources);
                cargo.Add(unitId, state);
            }
            var loot = new Dictionary<Vector2Int, Dictionary<string, float>>();
            int lootCount = ReadCount(reader, 100000);
            for (int i = 0; i < lootCount; i++)
            {
                var position = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
                if (loot.ContainsKey(position)) throw new InvalidDataException("Duplicate loot tile.");
                var resources = new Dictionary<string, float>(StringComparer.Ordinal);
                ReadResources(reader, resources);
                loot.Add(position, resources);
            }
            var routes = version >= 2 ? ReadRoutes(reader) : new Dictionary<string, RouteState>(StringComparer.Ordinal);
            var pendingDeposits = version >= 3 ? ReadPendingFoundingDeposits(reader)
                : new Dictionary<Vector2Int, string>();
            ValidateLoadedRoutes(cargo, routes);
            ValidateLoadedFoundingDeposits(cargo, pendingDeposits);
            if (reader.BaseStream.Position != reader.BaseStream.Length)
                throw new InvalidDataException("Unexpected trailing caravan save data.");
            return () =>
            {
                ClearRoutes();
                _cargo.Clear();
                _loot.Clear();
                _pendingFoundingCargoDeposits.Clear();
                foreach (var pair in cargo) _cargo.Add(pair.Key, pair.Value);
                foreach (var pair in loot) _loot.Add(pair.Key, pair.Value);
                foreach (var pair in routes) _routes.Add(pair.Key, pair.Value);
                foreach (var pair in pendingDeposits) _pendingFoundingCargoDeposits.Add(pair.Key, pair.Value);
                RequestRouteTick();
                Changed?.Invoke();
            };
        }

        public Action PrepareMissingData() => () =>
        {
            ClearRoutes();
            _cargo.Clear();
            _loot.Clear();
            _pendingFoundingCargoDeposits.Clear();
            Changed?.Invoke();
        };

        private static Dictionary<Vector2Int, string> ReadPendingFoundingDeposits(BinaryReader reader)
        {
            var deposits = new Dictionary<Vector2Int, string>();
            int count = ReadCount(reader, 100000);
            for (int index = 0; index < count; index++)
            {
                var position = new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
                string unitId = reader.ReadString();
                if (string.IsNullOrWhiteSpace(unitId) || deposits.ContainsKey(position))
                    throw new InvalidDataException("Invalid pending founding cargo deposit.");
                deposits.Add(position, unitId);
            }
            return deposits;
        }

        private static int ReadCount(BinaryReader reader, int maximum)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > maximum) throw new InvalidDataException("Invalid caravan collection size.");
            return count;
        }

        private static void WriteResources(BinaryWriter writer, Dictionary<string, float> resources)
        {
            writer.Write(resources.Count);
            foreach (var pair in resources) { writer.Write(pair.Key); writer.Write(pair.Value); }
        }

        private static void ReadResources(BinaryReader reader, Dictionary<string, float> resources)
        {
            int count = ReadCount(reader, 256);
            for (int i = 0; i < count; i++)
            {
                string id = reader.ReadString();
                float amount = reader.ReadSingle();
                if (string.IsNullOrWhiteSpace(id) || resources.ContainsKey(id)
                    || float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                    throw new InvalidDataException("Invalid saved cargo resource.");
                resources.Add(id, amount);
            }
            if (Sum(resources) > float.MaxValue) throw new InvalidDataException("Saved cargo exceeds numeric limits.");
        }

        private static void ValidateLoadedRoutes(
            Dictionary<string, CargoState> cargo,
            Dictionary<string, RouteState> routes)
        {
            foreach (var pair in routes)
            {
                var route = pair.Value;
                bool hasCargo = cargo.TryGetValue(pair.Key, out var loaded) && loaded.Resources.Count > 0;
                if (route.Phase == CaravanRoutePhase.ToDestination)
                {
                    if (!hasCargo || loaded.OwnerId != route.Request.OwnerId
                        || !ContainsAll(loaded.Resources, route.Request.Resources))
                        throw new InvalidDataException("Saved caravan delivery route is missing its paid cargo.");
                    continue;
                }

                if (route.Phase == CaravanRoutePhase.ToSource && hasCargo)
                    throw new InvalidDataException("Saved caravan pickup route already has cargo.");
            }
        }

        private static void ValidateLoadedFoundingDeposits(
            Dictionary<string, CargoState> cargo,
            Dictionary<Vector2Int, string> deposits)
        {
            foreach (var pair in deposits)
            {
                if (!cargo.TryGetValue(pair.Value, out var state) || state.Resources.Count == 0)
                    throw new InvalidDataException("Saved pending founding deposit is missing wagon cargo.");
            }
        }
    }
}
