using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Units.API;
using Newtonsoft.Json;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>JSON-backed unit registry. New Presets/Units/*.json are discovered automatically.</summary>
    [Serializable]
    public sealed class UnitRegistrySO : MoyvaJsonConfigObject
    {
        [JsonIgnore]
        public List<UnitClassConfig> Configs =>
            MoyvaJsonRuntime.GetAll<UnitClassConfig>().ToList();
    }
}
