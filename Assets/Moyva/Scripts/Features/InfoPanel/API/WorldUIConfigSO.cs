using UnityEngine;

namespace Kruty1918.Moyva.InfoPanel.API
{
    /// <summary>
    /// Конфіг для World Info Panel та інших UI компонентів.
    /// Містить посилання на префаби і налаштування панелей.
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/UI/World UI Config", fileName = "WorldUIConfig")]
    public sealed class WorldUIConfigSO : ScriptableObject
    {
        [Header("World Info Panel")]
        [SerializeField] private GameObject _worldInfoPanelPrefab;

        [Header("Castle Detailed Info Panel")]
        [SerializeField] private GameObject _castleDetailedPanelPrefab;

        [Header("Parent Transform")]
        [SerializeField] private string _panelParentName = "Canvas";

        public GameObject WorldInfoPanelPrefab => _worldInfoPanelPrefab;
        public GameObject CastleDetailedPanelPrefab => _castleDetailedPanelPrefab;
        public string PanelParentName => _panelParentName;

        private void OnValidate()
        {
            if (_worldInfoPanelPrefab != null && _worldInfoPanelPrefab.scene.rootCount > 0)
            {
                Debug.LogError("[WorldUIConfigSO] WorldInfoPanelPrefab повинен бути префабом, не об'єктом зі сцени!");
                _worldInfoPanelPrefab = null;
            }
        }
    }
}
