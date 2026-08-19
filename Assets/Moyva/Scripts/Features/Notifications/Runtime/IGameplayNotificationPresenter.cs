using System;
using Kruty1918.Moyva.Notifications.API;

namespace Kruty1918.Moyva.Notifications.Runtime
{
    internal interface IGameplayNotificationPresenter
    {
        void Present(
            GameplayNotificationRequest request,
            float holdDuration,
            Action completed);

        void ResetPresentation();
    }
}
