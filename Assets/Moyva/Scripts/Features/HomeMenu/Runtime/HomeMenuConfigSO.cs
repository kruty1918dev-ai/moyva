using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    [CreateAssetMenu(fileName = "HomeMenuConfig", menuName = "Moyva/Home Menu/Home Menu Config")]
    public sealed class HomeMenuConfigSO : ScriptableObject
    {
        public string gameplaySceneName = "Gamplay_Scene";
        public string homeMenuSceneName = "HomeMenu";
        public float minPreloadSeconds = 0.8f;
        public float sceneActivationDelay = 0.2f;
    }
}