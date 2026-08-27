using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Універсальна кнопка навігації між панелями HomeMenu.
    /// Залежності: <see cref="INavigation"/>.
    /// </summary>
    public class NavigationButton : MonoBehaviour
    {
        /// <summary>Назва панелі, яку потрібно відкрити.</summary>
        [SerializeField] private string _menuToOpen;

        /// <summary>Назва панелі, яку потрібно закрити.</summary>
        [SerializeField] private string _menuToClose;

        /// <summary>True, якщо натискання має відкрити останню панель зі стеку.</summary>
        [SerializeField] private bool _openLast;

        /// <summary>Сервіс навігації меню.</summary>
        private INavigation _navigation;

        /// <summary>Кеш компонента Button.</summary>
        private Button _button;

        [Inject]
        public void Construct([InjectOptional] INavigation navigation = null)
        {
            _navigation = navigation;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
                _button.onClick.AddListener(OnButtonClicked);
            }
            else
            {
                Debug.LogError($"[NavigationButton] No Button component found on '{gameObject.name}'. NavigationButton will not work.");
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (_navigation == null)
                return;

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
