using System;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    /// <summary>
    /// Marks a string field as a semantic terrain tag selected from terrain profiles.
    /// The value remains a plain string so existing Unity serialization stays intact.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class TerrainTagAttribute : PropertyAttribute
    {
    }
}
