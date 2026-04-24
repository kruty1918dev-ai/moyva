using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class NavigationButton : MonoBehaviour
    {
        [SerializeField] private string _menuToOpen;
        [SerializeField] private string _menuToClose;

        private INavigation _navigation;

        [Inject]
        public void Construct(INavigation navigation)
        {
            _navigation = navigation;
        }

        public void OnButtonClicked()
        {
            if (!string.IsNullOrWhiteSpace(_menuToClose))
                _navigation.Close(_menuToClose);

            if (!string.IsNullOrWhiteSpace(_menuToOpen))
                _navigation.Open(_menuToOpen);
        }
    }
}