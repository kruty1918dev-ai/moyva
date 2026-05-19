using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{

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
