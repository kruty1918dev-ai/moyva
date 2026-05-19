using Kruty1918.Moyva.HomeMenu.UI;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт панелі налаштування параметрів світу перед стартом гри.
    /// Залежності: використовує UI-тип WolrdCreationMode і реалізується WorldCreationPanelService.
    /// </summary>
    public interface IWorldCreationPanelService
    {
        /// <summary>Налаштувати режим роботи панелі створення світу.</summary>
        void SetupMode(WolrdCreationMode mode);
    }
}