using Sirenix.OdinInspector;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public partial class FogOfWarSettings
    {
        /// <summary>
        /// Базовий vision range, який можна використовувати як стандартне значення для юнітів.
        /// </summary>
        [TitleGroup("Vision")]
        [MinValue(1)]
        public int DefaultVisionRange = 5;

        /// <summary>
        /// Мінімальна допустима дальність огляду в системі FogOfWar.
        /// </summary>
        [TitleGroup("Vision")]
        [MinValue(1)]
        public int MinVisionRange = 1;

        /// <summary>
        /// Максимальна допустима дальність огляду в системі FogOfWar.
        /// </summary>
        [TitleGroup("Vision")]
        [MinValue(1)]
        public int MaxVisionRange = 12;

        /// <summary>
        /// Клітини в межах цієї відстані від краю карти ніколи не розкриваються:
        /// ні visible, ні explored. Туман завжди накриває межу світу, тому
        /// гравець не бачить, де закінчується карта.
        /// </summary>
        [TitleGroup("Vision")]
        [MinValue(0)]
        public int BoundaryFogMarginCells = 2;
    }
}
