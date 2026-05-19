using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Базова реалізація однієї панелі навігації HomeMenu.
    /// Залежності: керується через <see cref="INavigation"/>.
    /// </summary>
    public class NavigationPanel : MonoBehaviour, INavigationPanel, IInitializable
    {
        /// <summary>Унікальна назва панелі для маршрутизації.</summary>
        [SerializeField] private string _menuName;

        /// <summary>Публічний ключ панелі.</summary>
        public string MenuName => _menuName;

        /// <summary>Відкрити панель.</summary>
        public void Open()
        {
            gameObject.SetActive(true);
            Debug.Log($"[NavigationPanel] Opened panel '{_menuName}'.");
        }

        /// <summary>Закрити панель.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
            Debug.Log($"[NavigationPanel] Closed panel '{_menuName}'.");
        }

        /// <summary>Початкова ініціалізація стану панелі.</summary>
        public void Initialize()
        {
            Close(); // Ensure all panels start closed
        }
    }
}