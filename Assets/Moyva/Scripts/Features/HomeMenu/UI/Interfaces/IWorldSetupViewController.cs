using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт UI-контролера панелі налаштування світу перед стартом сесії.
    /// Залежності: використовується WorldCreationPanelService.
    /// </summary>
    public interface IWorldSetupViewController
    {
        /// <summary>Назва світу.</summary>
        string WorldName { get; set; }

        /// <summary>Seed генерації світу.</summary>
        int Seed { get; set; }

        /// <summary>Розмір світу.</summary>
        WorldSize Size { get; set; }

        /// <summary>Тип мапи.</summary>
        MapType MapType { get; set; }

        /// <summary>Рівень складності.</summary>
        Difficulty Difficulty { get; set; }

        /// <summary>Подія натискання кнопки створення світу.</summary>
        event Action OnButtonNextClicked;

        /// <summary>Подія генерації випадкового seed.</summary>
        event Action OnRandomSeedClicked;

        /// <summary>Подія зміни будь-якого налаштування світу.</summary>
        event Action OnSettingsChanged;

        /// <summary>Кнопка створення світу.</summary>
        Button CreateWorldButton { get; }
    }
}
