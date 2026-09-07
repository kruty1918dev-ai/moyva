using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed partial class CaravanService
    {
        private void WriteRoutes(BinaryWriter writer)
        {
            writer.Write(_routes.Count);
            foreach (var route in _routes.Values)
            {
                var request = route.Request;
                writer.Write(request.OwnerId); writer.Write(request.UnitId);
                writer.Write(request.SourceSettlementId); writer.Write(request.SourceWarehouseKey);
                writer.Write(request.TargetSettlementId); writer.Write(request.TargetWarehouseKey);
                writer.Write(request.Repeat); writer.Write((int)route.Phase);
                var resources = new Dictionary<string, float>(StringComparer.Ordinal);
                foreach (var pair in request.Resources) resources.Add(pair.Key, pair.Value);
                WriteResources(writer, resources);
            }
        }

        private static Dictionary<string, RouteState> ReadRoutes(BinaryReader reader)
        {
            var routes = new Dictionary<string, RouteState>(StringComparer.Ordinal);
            int count = ReadCount(reader, 100000);
            for (int i = 0; i < count; i++)
            {
                string owner = reader.ReadString(), unit = reader.ReadString();
                string sourceSettlement = reader.ReadString(), sourceKey = reader.ReadString();
                string targetSettlement = reader.ReadString(), targetKey = reader.ReadString();
                bool repeat = reader.ReadBoolean();
                var phase = (CaravanRoutePhase)reader.ReadInt32();
                var resources = new Dictionary<string, float>(StringComparer.Ordinal);
                ReadResources(reader, resources);
                if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(unit)
                    || string.IsNullOrWhiteSpace(sourceSettlement) || string.IsNullOrWhiteSpace(targetSettlement)
                    || routes.ContainsKey(unit) || !TryWarehousePosition(sourceKey, out _)
                    || !TryWarehousePosition(targetKey, out _) || sourceKey == targetKey
                    || phase < CaravanRoutePhase.ToSource || phase > CaravanRoutePhase.Completed
                    || !TryGetTotal(resources, out _))
                    throw new InvalidDataException("Invalid saved caravan route.");
                routes.Add(unit, new RouteState
                {
                    Request = new CaravanRouteRequest(owner, unit, sourceSettlement, sourceKey,
                        targetSettlement, targetKey, Freeze(resources), repeat),
                    Phase = phase,
                    Status = phase == CaravanRoutePhase.Completed ? "Delivery complete." : "Route restored.",
                });
            }
            return routes;
        }
    }
}
