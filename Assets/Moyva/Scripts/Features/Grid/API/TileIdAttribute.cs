using System;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Атрибут для полів-рядків, що мають містити валідний TileId із реєстру тайлів.
    /// Використовується editor-інструментами для спеціального дровера/валідації.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class TileIdAttribute : PropertyAttribute { }
}
