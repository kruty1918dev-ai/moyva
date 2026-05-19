using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт UI-панелі підтвердження дій.
    /// Залежності: використовується IConfirmationService.
    /// </summary>
    public interface IConfiremationPanel
    {
        /// <summary>Спробувати отримати поточний активний запит підтвердження.</summary>
        bool TryGetReqest(out ConfirmationRequest? request);

        /// <summary>Показати запит підтвердження у UI.</summary>
        void Show(ConfirmationRequest request);

        /// <summary>Примусово сховати панель підтвердження.</summary>
        void ForeceHide();

        /// <summary>Колбек підтвердження дії.</summary>
        Action OnConfirme { get; set; }

        /// <summary>Колбек скасування дії.</summary>
        Action OnCancled { get; set; }
    }
}