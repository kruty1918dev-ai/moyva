namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт модального сервісу підтвердження дій користувача.
    /// Залежності: працює поверх ConfirmationPanel та використовується навігацією й налаштуваннями.
    /// </summary>
    public interface IConfirmationService
    {
        /// <summary>Показати запит на підтвердження.</summary>
        /// <param name="request">Дані модального вікна та колбеки відповіді.</param>
        void Show(ConfirmationRequest request);

        /// <summary>Примусово приховати вікно підтвердження.</summary>
        void ForeceHide();

        /// <summary>Спробувати отримати поточний активний запит підтвердження.</summary>
        /// <param name="request">Поточний запит, якщо він існує.</param>
        bool TryGetReqest(out ConfirmationRequest? request);
    }
}