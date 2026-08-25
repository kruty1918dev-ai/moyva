using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Спеціалізована кнопка підтвердження виходу з гри.
    /// На підтвердження викликає <see cref="Application.Quit"/>.
    /// </summary>
    public class ConfirmExitgameButton : ConfirmButton
    {
        /// <summary>Перевизначити стандартний confirmation request для сценарію виходу.</summary>
        protected override void OnButtonClicked()
        {
            RaiseOnClicked(new ConfirmationRequest
            {
                LabelText = _label,
                MessageText = _message,
                OnConfirm = Application.Quit,
            });
        }
    }
}
