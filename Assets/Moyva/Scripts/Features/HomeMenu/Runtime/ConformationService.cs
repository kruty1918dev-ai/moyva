using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class ConformationService : IConfirmationService, IInitializable
    {
        [Inject] private IConfiremationPanel _panel;
        [Inject] private IConfirmationButton[] _buttons;

        public void Initialize()
        {
            GetPanel().OnConfirme += ForeceHide;
            GetPanel().OnCancled += ForeceHide;

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].OnClicked += Show;
            }
        }


        public void ForeceHide() => GetPanel().ForeceHide();

        public void Show(ConfirmationRequest request) => GetPanel().Show(request);

        public bool TryGetReqest(out ConfirmationRequest? request) => GetPanel().TryGetReqest(out request);


        private IConfiremationPanel GetPanel()
        {
            if (_panel == null)
            {
                Debug.LogError("[ConformationService]: The confirmed panel was not injected.");
                return null;
            }

            return _panel;
        }

        private IConfirmationButton[] GetButtons()
        {
            if (_buttons == null)
            {
                Debug.LogError("[ConformationService]: The confirmed buttons was not injected");
                return null;
            }

            return _buttons;
        }
    }
}