using System;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [Flags]
    public enum BuildingRuntimeFlags
    {
        [InspectorName("Немає")]
        None = 0,
        [InspectorName("Блокує шлях")]
        BlocksPath = 1 << 0,
        [InspectorName("Можна вибирати")]
        Selectable = 1 << 1,
        [InspectorName("Можна пошкодити")]
        Damageable = 1 << 2,
        [InspectorName("Потребує завершення")]
        RequiresCompletion = 1 << 3,
    }
}
