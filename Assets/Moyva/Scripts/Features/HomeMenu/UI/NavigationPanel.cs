using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class NavigationPanel : MonoBehaviour, INavigationPanel, IInitializable
    {
        [SerializeField] private string _menuName;
        public string MenuName => _menuName;

        public void Open()
        {
            gameObject.SetActive(true);
            Debug.Log($"[NavigationPanel] Opened panel '{_menuName}'.");
        }

        public void Close()
        {
            gameObject.SetActive(false);
            Debug.Log($"[NavigationPanel] Closed panel '{_menuName}'.");
        }

        public void Initialize()
        {
            Close(); // Ensure all panels start closed
        }
    }
}