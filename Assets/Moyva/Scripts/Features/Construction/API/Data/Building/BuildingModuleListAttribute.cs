using System;

namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>Позначає список модулів, який у редакторі малюється спеціальним Odin drawer.</summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class BuildingModuleListAttribute : Attribute
    {
    }
}
