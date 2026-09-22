using Kruty1918.Moyva.FogOfWar.Runtime.SettingsValidation;
using UnityEngine;

using Kruty1918.JsonConfig;
namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Root JsonConfigObject config for FogOfWar. Serialized fields live in partial files
    /// so Unity keeps existing field names while feature sections stay isolated.
    /// </summary>
public partial class FogOfWarSettings : JsonConfigObject
    {
        /// <summary>
        /// Tint used for remembered entities rendered by the ghost presenter.
        /// </summary>
        public Color ExploredColor = new Color(0f, 0f, 0f, 0.5f);

        /// <summary>
        /// Optional icon sprite set; the first entry is used for remembered ghost entities.
        /// </summary>
        public Sprite[] FogIconSprites;

        private void OnValidate()
        {
            FogOfWarSettingsValidator.Normalize(this);
        }
    }
}
