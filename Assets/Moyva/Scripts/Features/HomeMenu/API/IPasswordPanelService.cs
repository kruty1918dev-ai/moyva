using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Результат запиту пароля у користувача.
    /// </summary>
    public readonly struct PasswordPromptResult
    {
        /// <summary>True, якщо користувач натиснув OK; false — якщо скасував.</summary>
        public bool Confirmed { get; }
        /// <summary>Введений пароль (порожній, якщо <see cref="Confirmed"/> = false).</summary>
        public string Password { get; }

        public PasswordPromptResult(bool confirmed, string password)
        {
            Confirmed = confirmed;
            Password = password ?? string.Empty;
        }

        public static PasswordPromptResult Cancelled => new PasswordPromptResult(false, string.Empty);
        public static PasswordPromptResult Confirm(string password) => new PasswordPromptResult(true, password);
    }

    /// <summary>
    /// Сервіс модального запиту пароля для приєднання до приватної кімнати.
    /// </summary>
    public interface IPasswordPanelService
    {
        /// <summary>
        /// Показати панель і чекати на введення. Повертає введений пароль або <see cref="PasswordPromptResult.Cancelled"/>.
        /// </summary>
        /// <param name="roomDisplayName">Назва кімнати для підказки.</param>
        /// <param name="errorText">Якщо не порожнє — показати під полем як попереднє повідомлення про помилку.</param>
        Task<PasswordPromptResult> RequestPasswordAsync(string roomDisplayName, string errorText = null, CancellationToken ct = default);

        /// <summary>Примусово сховати панель і скасувати поточний запит.</summary>
        void Cancel();
    }
}
