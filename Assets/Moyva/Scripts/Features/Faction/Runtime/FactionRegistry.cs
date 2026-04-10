using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Faction.API;

namespace Kruty1918.Moyva.Faction.Runtime
{
    internal sealed class FactionRegistry : IFactionRegistry
    {
        private readonly List<FactionDefinition> _all;
        private readonly Dictionary<string, FactionDefinition> _byId;
        private readonly List<FactionDefinition> _bots;

        public FactionDefinition LocalPlayerFaction { get; }

        public FactionRegistry(IEnumerable<FactionDefinition> definitions)
        {
            _all   = definitions?.ToList() ?? new List<FactionDefinition>();
            _byId  = _all.ToDictionary(d => d.FactionId.Value);
            _bots  = _all.Where(d => d.FactionType == FactionType.Bot).ToList();

            LocalPlayerFaction = _all.FirstOrDefault(d => d.FactionType == FactionType.Human);

            if (LocalPlayerFaction == null && _all.Count > 0)
            {
                UnityEngine.Debug.LogWarning("[FactionRegistry] Жодної Human-фракції у конфігу сесії не знайдено.");
            }
        }

        public IReadOnlyList<FactionDefinition> GetAll()          => _all;
        public IReadOnlyList<FactionDefinition> GetBotFactions()  => _bots;

        public bool TryGet(FactionId id, out FactionDefinition definition)
            => _byId.TryGetValue(id.Value, out definition);
    }
}
