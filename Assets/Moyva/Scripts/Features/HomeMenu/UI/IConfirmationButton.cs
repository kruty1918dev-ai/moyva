using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт кнопки підтвердження дії в HomeMenu.
    /// Залежності: використовується confirmation UI і <see cref="IConfirmationService"/>.
    /// </summary>
    public interface IConfirmationButton
    {
        /// <summary>
        /// Увімкнути або вимкнути взаємодію з кнопкою.
        /// </summary>
        /// <param name="interactable">True, якщо кнопку можна натискати.</param>
        void SetInteractable(bool interactable);

        /// <summary>
        /// Подія натискання кнопки з передачею контексту запиту підтвердження.
        /// </summary>
        event Action<ConfirmationRequest> OnClicked;
    }
}
