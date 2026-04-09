using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    [Serializable]
    public sealed class TopologyCaseBinding
    {
        public TopologyCaseType CaseType = TopologyCaseType.Isolated;
        public List<string> VariantBuildingIds = new();
    }
}
