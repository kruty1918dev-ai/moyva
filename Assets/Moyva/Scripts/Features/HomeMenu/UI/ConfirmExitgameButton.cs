using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class ConfirmExitgameButton : ConfirmButton
    {
        protected override void OnButtonClicked()
        {
            RaiseOnClicked(new ConfirmationRequest
            {
                LabelText = _label,
                MessageText = _message,
                OnConfirm = () =>
                {
                    Debug.Log("[ConfirmExitgameButton] Confirm action executed. Exiting game...");
                    Application.Quit();
                },
                OnCancel = () => Debug.Log("[ConfirmExitgameButton] Cancel action executed. Exit game cancelled.")
            });
        }
    }
}