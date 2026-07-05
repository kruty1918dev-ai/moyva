using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{
    public sealed class BuildingValidationContext
    {
        public IBuildingRegistry Registry;
        public ISet<string> ResourceIds;
        public bool RequireRegistryInclusion;
    }
}
