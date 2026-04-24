using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class NavigationButton : MonoBehaviour
    {
        [SerializeField] private string _menuToOpen;
        [SerializeField] private string _menuToClose;
        [SerializeField] private bool _openLast;

        private INavigation _navigation;

        [Inject]
        public void Construct(INavigation navigation)
        {
            _navigation = navigation;
        }

        private void Awake()
        {
            var button = GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
            else
            {
                Debug.LogError($"[NavigationButton] No Button component found on '{gameObject.name}'. NavigationButton will not work.");
            }
        }

        private void OnButtonClicked()
        {
            if (_openLast)
            {
                _navigation.OpenLast();
                return;
            }

            if (!string.IsNullOrWhiteSpace(_menuToClose))
                _navigation.Close(_menuToClose);

            if (!string.IsNullOrWhiteSpace(_menuToOpen))
                _navigation.Open(_menuToOpen);
        }
    }
}