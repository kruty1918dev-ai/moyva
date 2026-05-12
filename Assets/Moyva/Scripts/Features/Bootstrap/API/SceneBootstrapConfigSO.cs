using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.API
{
    /// <summary>
    /// Конфіг для інсталяції всіх UI панелей сцени.
    /// Містить посилання на всі необхідні конфіги (WorldUIConfig, тощо).
    /// </summary>
    [CreateAssetMenu(menuName = "Moyva/Scenes/Scene Bootstrap Config", fileName = "SceneBootstrapConfig")]
    public sealed class SceneBootstrapConfigSO : ScriptableObject
    {
        [Header("UI Configs")]
        [SerializeField] private GameObject _worldUIConfigPrefab;
        
        [Header("Game Session")]
        [SerializeField] private GameObject _gameSessionConfigPrefab;

        public GameObject WorldUIConfigPrefab => _worldUIConfigPrefab;
        public GameObject GameSessionConfigPrefab => _gameSessionConfigPrefab;

        private void OnValidate()
        {
            if (_worldUIConfigPrefab != null && _worldUIConfigPrefab.scene.rootCount > 0)
            {
                Debug.LogError("[SceneBootstrapConfigSO] Посилання повинні бути префабами!");
                _worldUIConfigPrefab = null;
            }
        }
    }
}
