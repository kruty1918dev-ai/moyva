using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public interface IConfiremationPanel
    {
        bool TryGetReqest(out ConfirmationRequest? request);

        void Show(ConfirmationRequest request);
        void ForeceHide();
        Action OnConfirme { get; set; }
        Action OnCancled { get; set; }
    }
}