using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Коротка інформація про слот збереження для відображення у меню Continue/Load.
    /// Залежності: формується save layer і читається панелями HomeMenu.
    /// </summary>
    public struct GameSlotInfo
    {
        /// <summary>Людинозрозуміла назва слоту.</summary>
        public string SlotName;

        /// <summary>Порядковий індекс слоту.</summary>
        public int SlotIndex;

        /// <summary>Час останньої модифікації слоту.</summary>
        public DateTime LastModified;
    }
}
