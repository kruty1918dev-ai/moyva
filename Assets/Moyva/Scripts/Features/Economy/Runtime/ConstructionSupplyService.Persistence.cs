using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    [SaveModuleId("Kruty1918.Moyva.Economy.ConstructionSupply")]
    internal sealed partial class ConstructionSupplyService : IStagedSaveModule
    {
        private const int SaveVersion = 2;

        public void OnSave(ISaveContext context)
        {
            var writer = context.Writer;
            writer.Write(SaveVersion);
            writer.Write(_ordersByPosition.Count);
            writer.Write(_orderSequence);
            foreach (var pair in _ordersByPosition)
            {
                var order = pair.Value;
                writer.Write(order.OrderId ?? string.Empty);
                writer.Write(order.OwnerId ?? string.Empty);
                writer.Write(order.BuildingId ?? string.Empty);
                writer.Write(order.Position.x);
                writer.Write(order.Position.y);
                writer.Write(order.SettlementId ?? string.Empty);
                writer.Write(order.WarehouseKey ?? string.Empty);
                writer.Write((int)order.Status);
                WriteMap(writer, order.Required);
                WriteMap(writer, order.Delivered);
                WriteMap(writer, order.Remaining);
                writer.Write(order.WagonIds.Count);
                foreach (var wagonId in order.WagonIds)
                    writer.Write(wagonId ?? string.Empty);
                writer.Write(order.SourceReservations.Count);
                foreach (var reservation in order.SourceReservations)
                {
                    writer.Write(reservation.SettlementId ?? string.Empty);
                    writer.Write(reservation.WarehouseKey ?? string.Empty);
                    writer.Write(reservation.ResourceId ?? string.Empty);
                    writer.Write(reservation.Amount);
                }
            }
        }

        public void OnLoad(ISaveContext context)
            => PrepareLoad(context)();

        public Action PrepareLoad(ISaveContext context)
        {
            var reader = context.Reader;
            int version = reader.ReadInt32();
            if (version < 1 || version > SaveVersion)
                throw new InvalidDataException("Unsupported construction supply save version.");

            var orders = new List<SupplyOrder>();
            int orderCount = ReadCount(reader, 4096);
            int orderSequence = reader.ReadInt32();
            if (orderSequence < 0) orderSequence = 0;
            for (int index = 0; index < orderCount; index++)
            {
                var order = new SupplyOrder
                {
                    OrderId = reader.ReadString(),
                    OwnerId = reader.ReadString(),
                    BuildingId = reader.ReadString(),
                    Position = new Vector2Int(reader.ReadInt32(), reader.ReadInt32()),
                    SettlementId = reader.ReadString(),
                    WarehouseKey = reader.ReadString(),
                };
                int status = reader.ReadInt32();
                if (status < (int)ConstructionSupplyOrderStatus.Active
                    || status > (int)ConstructionSupplyOrderStatus.Cancelled)
                    throw new InvalidDataException("Invalid construction supply order status.");
                order.Status = (ConstructionSupplyOrderStatus)status;
                ReadMap(reader, order.Required);
                ReadMap(reader, order.Delivered);
                ReadMap(reader, order.Remaining);

                if (string.IsNullOrWhiteSpace(order.OrderId)
                    || string.IsNullOrWhiteSpace(order.OwnerId)
                    || string.IsNullOrWhiteSpace(order.SettlementId)
                    || string.IsNullOrWhiteSpace(order.WarehouseKey))
                    throw new InvalidDataException("Invalid construction supply order identity.");
                if (order.Status == ConstructionSupplyOrderStatus.Active && order.Remaining.Count == 0)
                    order.Status = ConstructionSupplyOrderStatus.Ready;

                int wagonCount = ReadCount(reader, 1024);
                for (int wagonIndex = 0; wagonIndex < wagonCount; wagonIndex++)
                {
                    string wagonId = reader.ReadString();
                    if (!string.IsNullOrWhiteSpace(wagonId) && !order.WagonIds.Contains(wagonId))
                        order.WagonIds.Add(wagonId);
                }
                if (version >= 2)
                {
                    int reservationCount = ReadCount(reader, 1024);
                    for (int index2 = 0; index2 < reservationCount; index2++)
                    {
                        var reservation = new SourceReservation
                        {
                            SettlementId = reader.ReadString(),
                            WarehouseKey = reader.ReadString(),
                            ResourceId = reader.ReadString(),
                            Amount = reader.ReadSingle(),
                        };
                        if (string.IsNullOrWhiteSpace(reservation.SettlementId)
                            || string.IsNullOrWhiteSpace(reservation.WarehouseKey)
                            || string.IsNullOrWhiteSpace(reservation.ResourceId)
                            || float.IsNaN(reservation.Amount) || float.IsInfinity(reservation.Amount)
                            || reservation.Amount <= 0f)
                            throw new InvalidDataException("Invalid saved supply source reservation.");
                        order.SourceReservations.Add(reservation);
                    }
                }
                orders.Add(order);
            }

            if (reader.BaseStream.Position != reader.BaseStream.Length)
                throw new InvalidDataException("Unexpected trailing construction supply save data.");

            return () =>
            {
                _ordersByPosition.Clear();
                _orderSequence = orderSequence;
                foreach (var order in orders)
                    _ordersByPosition[order.Position] = order;
                Changed?.Invoke();
            };
        }

        public Action PrepareMissingData() => () =>
        {
            _ordersByPosition.Clear();
            Changed?.Invoke();
        };

        private static int ReadCount(BinaryReader reader, int maximum)
        {
            int count = reader.ReadInt32();
            if (count < 0 || count > maximum)
                throw new InvalidDataException("Invalid construction supply collection size.");
            return count;
        }

        private static void WriteMap(BinaryWriter writer, Dictionary<string, float> map)
        {
            writer.Write(map?.Count ?? 0);
            if (map == null) return;
            foreach (var pair in map)
            {
                writer.Write(pair.Key ?? string.Empty);
                writer.Write(pair.Value);
            }
        }

        private static void ReadMap(BinaryReader reader, Dictionary<string, float> map)
        {
            int count = ReadCount(reader, 256);
            for (int i = 0; i < count; i++)
            {
                string id = reader.ReadString();
                float amount = reader.ReadSingle();
                if (string.IsNullOrWhiteSpace(id) || map.ContainsKey(id)
                    || float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f)
                    throw new InvalidDataException("Invalid saved construction supply resource.");
                map.Add(id, amount);
            }
        }
    }
}
