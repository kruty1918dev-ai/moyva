using System;

namespace Kruty1918.Moyva.Construction.API
{
    [Flags]
    public enum BuildingRuntimeFlags
    {
        None = 0,
        BlocksPath = 1 << 0,
        Selectable = 1 << 1,
        Damageable = 1 << 2,
        RequiresCompletion = 1 << 3,
    }
}
