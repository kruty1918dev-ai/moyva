using System;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal static class BotAdvancedHeuristics
    {
        internal static int Chebyshev(Vector2Int a, Vector2Int b)
            => Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y));

        internal static int RawDamage(UnitClassConfig config)
            => config == null
                ? 0
                : Math.Max(0, config.CuttingDamage)
                  + Math.Max(0, config.PenetratingDamage)
                  + Math.Max(0, config.CrushingDamage);

        internal static int RawDefense(UnitClassConfig config)
            => config == null
                ? 0
                : Math.Max(0, config.CuttingDefense)
                  + Math.Max(0, config.PenetratingDefense)
                  + Math.Max(0, config.CrushingDefense);

        internal static BotUnitTacticalRole InferRole(UnitClassConfig config)
        {
            if (config == null)
                return BotUnitTacticalRole.Unknown;

            int damage = RawDamage(config);
            int defense = RawDefense(config);
            int range = Math.Max(1, config.AttackRange);
            float movement = Math.Max(0f, config.MovementPointsPerTurn);

            if (damage <= 0 && defense <= 0)
                return BotUnitTacticalRole.Worker;
            if (Math.Max(0, config.CrushingDamage) >= Math.Max(8, damage / 2) && movement <= 3f)
                return BotUnitTacticalRole.Siege;
            if (range >= 2)
                return BotUnitTacticalRole.Ranged;
            if (movement >= 5f)
                return BotUnitTacticalRole.FastScout;
            if (defense > damage * 2 && damage > 0)
                return BotUnitTacticalRole.Support;
            return BotUnitTacticalRole.Frontline;
        }

        internal static int DesiredRolePercent(BotUnitTacticalRole role, BotStrategicPosture posture)
        {
            return role switch
            {
                BotUnitTacticalRole.Frontline => posture == BotStrategicPosture.EmergencyDefense ? 45 : 40,
                BotUnitTacticalRole.Ranged => posture == BotStrategicPosture.Pressure ? 35 : 30,
                BotUnitTacticalRole.FastScout => posture == BotStrategicPosture.Search ? 25 : 10,
                BotUnitTacticalRole.Siege => posture == BotStrategicPosture.Siege ? 25 : 10,
                BotUnitTacticalRole.Support => 10,
                _ => 0,
            };
        }

        internal static int StableSectorKey(Vector2Int position, int sectorSize = 6)
        {
            int size = Math.Max(1, sectorSize);
            int sx = FloorDiv(position.x, size);
            int sy = FloorDiv(position.y, size);
            unchecked { return (sx * 397) ^ sy; }
        }

        private static int FloorDiv(int value, int divisor)
        {
            int q = value / divisor;
            int r = value % divisor;
            return r != 0 && ((r < 0) != (divisor < 0)) ? q - 1 : q;
        }
    }
}
