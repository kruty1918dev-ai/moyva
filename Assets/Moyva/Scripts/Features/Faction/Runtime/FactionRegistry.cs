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
        public FactionDefinition LocalPlayerFaction { get; }

        public FactionRegistry(IEnumerable<FactionDefinition> definitions)
        {
            _all   = definitions?.ToList() ?? new List<FactionDefinition>();
            _byId  = _all.ToDictionary(d => d.FactionId.Value);
            LocalPlayerFaction = _all.FirstOrDefault(d => d.FactionType == FactionType.Human);
        }

        /// <summary>Повертає всі фракції поточної сесії.</summary>
        public IReadOnlyList<FactionDefinition> GetAll()          => _all;

        /// <summary>Шукає фракцію за стабільним ідентифікатором.</summary>
        public bool TryGet(FactionId id, out FactionDefinition definition)
            => _byId.TryGetValue(id.Value, out definition);
    }
}
