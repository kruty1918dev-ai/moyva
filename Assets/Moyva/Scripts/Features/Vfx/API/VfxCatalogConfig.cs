using System;
using Kruty1918.JsonConfig;
using Kruty1918.Vfx;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.API
{
    /// <summary>
    /// JSON-мапа gameplay-подій на VFX-префаби.
    /// Модель пресету: "vfx-catalog" (схема moyva.vfx-catalog).
    /// Префаби посилаються через {"$asset": key, "editorPath": path} і
    /// резолвляться у GameObject через runtime asset catalog.
    /// </summary>
    [Serializable]
    public sealed class VfxCatalogConfig : JsonConfigObject
    {
        /// <summary>Події → ефекти. Перше співпадіння (eventName + context) виграє.</summary>
        public VfxEffectRule[] effects = Array.Empty<VfxEffectRule>();

        /// <summary>Глобальні бюджети за профілем якості.</summary>
        public VfxBudgetSettings budget = new VfxBudgetSettings();
    }


}
