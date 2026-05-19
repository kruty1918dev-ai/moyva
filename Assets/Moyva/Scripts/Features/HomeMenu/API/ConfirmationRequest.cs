namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// DTO запиту на показ модального вікна підтвердження.
    /// Залежності: передається в IConfirmationService та UI-панель підтвердження.
    /// </summary>
    public struct ConfirmationRequest
    {
        /// <summary>Текст заголовка або назви дії.</summary>
        public string LabelText;

        /// <summary>Основний текст повідомлення для користувача.</summary>
        public string MessageText;

        /// <summary>Колбек, що викликається після підтвердження дії.</summary>
        public System.Action OnConfirm;

        /// <summary>Колбек, що викликається після скасування дії.</summary>
        public System.Action OnCancel;
    }
}