using System;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotInfluenceMapService : IBotInfluenceMapService
    {
        private readonly IUnitClassConfig _configs;
        private readonly IBotUnitRoleResolver _roles;

        [Inject]
        public BotInfluenceMapService(
            [InjectOptional] IUnitClassConfig configs = null,
            [InjectOptional] IBotUnitRoleResolver roles = null)
        {
            _configs = configs;
            _roles = roles;
        }

        public BotInfluenceScore Evaluate(BotWorldSnapshot snapshot, Vector2Int position)
        {
            if (snapshot == null)
                return default;

            long threat = 0;
            long opportunity = 0;
            long support = 0;
            long formation = 0;

            for (int index = 0; index < snapshot.VisibleEnemyUnits.Count; index++)
            {
                BotUnitSnapshot enemy = snapshot.VisibleEnemyUnits[index];
                UnitClassConfig config = BotTacticalAnalysis.SafeGetConfig(_configs, enemy.TypeId);
                int damage = Math.Max(1, BotAdvancedHeuristics.RawDamage(config));
                int range = Math.Max(1, config?.AttackRange ?? 1);
                int distance = BotAdvancedHeuristics.Chebyshev(position, enemy.Position);
                if (distance <= range)
                    threat += damage * 40L + (range - distance + 1) * 30L;
                else if (distance <= range + 2)
                    threat += damage * 12L;

                opportunity += Math.Max(0, 8 - distance) * 10L;
            }

            for (int index = 0; index < snapshot.OwnUnits.Count; index++)
            {
                BotUnitSnapshot ally = snapshot.OwnUnits[index];
                int distance = BotAdvancedHeuristics.Chebyshev(position, ally.Position);
                if (distance > 3)
                    continue;

                UnitClassConfig config = BotTacticalAnalysis.SafeGetConfig(_configs, ally.TypeId);
                int durability = Math.Max(1, config?.HitPoints ?? 1) + BotAdvancedHeuristics.RawDefense(config);
                support += Math.Max(0, 4 - distance) * Math.Max(4, durability / 4);

                BotUnitTacticalRole role = _roles?.Resolve(config) ?? BotAdvancedHeuristics.InferRole(config);
                if (role == BotUnitTacticalRole.Frontline && distance <= 2)
                    formation += 80;
                else if (role == BotUnitTacticalRole.Ranged && distance == 1)
                    formation += 30;
            }

            return new BotInfluenceScore(
                Clamp(threat),
                Clamp(opportunity),
                Clamp(support),
                formation > int.MaxValue ? int.MaxValue : formation < int.MinValue ? int.MinValue : (int)formation);
        }

        private static int Clamp(long value)
            => value <= 0 ? 0 : value >= int.MaxValue ? int.MaxValue : (int)value;
    }
}
