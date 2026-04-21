using System;
using System.Collections;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Production-реалізація <see cref="ISceneLoadService"/>.
    /// Запускає <see cref="SceneManager.LoadSceneAsync"/> з позначенням прогресу
    /// та штучною затримкою активації для плавного оновлення UI.
    ///
    /// Є MonoBehaviour, щоб мати змогу запускати корутини завантаження.
    /// Додається до сцени автоматично через <see cref="HomeMenuInstaller"/>.
    /// </summary>
    public sealed class SceneLoadService : MonoBehaviour, ISceneLoadService
    {
        private float _activationDelay = 0.2f;
        private bool  _isLoading;

        /// <inheritdoc/>
        public bool IsLoading => _isLoading;

        [Inject]
        internal void Construct(HomeMenuConfigSO config)
        {
            if (config != null)
                _activationDelay = Mathf.Max(0f, config.SceneActivationDelay);
        }

        /// <inheritdoc/>
        public void LoadSceneAsync(string sceneName, Action<float> progress = null, Action onCompleted = null)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("Назва сцени не може бути порожньою.", nameof(sceneName));

            if (_isLoading)
            {
                Debug.LogWarning("[SceneLoadService] Спроба почати завантаження поки триває попереднє. Ігнорується.");
                return;
            }

            StartCoroutine(LoadRoutine(sceneName, progress, onCompleted));
        }

        /// <inheritdoc/>
        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator LoadRoutine(string sceneName, Action<float> progress, Action onCompleted)
        {
            _isLoading = true;
            progress?.Invoke(0f);

            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError($"[SceneLoadService] SceneManager.LoadSceneAsync повернув null для '{sceneName}'. " +
                               "Перевір, що сцена додана у Build Settings.");
                _isLoading = false;
                yield break;
            }

            op.allowSceneActivation = false;

            // Завантажуємо до 90%. Далі — штучна затримка, потім активація.
            while (op.progress < 0.9f)
            {
                progress?.Invoke(op.progress);
                yield return null;
            }

            progress?.Invoke(0.9f);

            if (_activationDelay > 0f)
                yield return new WaitForSecondsRealtime(_activationDelay);

            op.allowSceneActivation = true;

            while (!op.isDone)
            {
                progress?.Invoke(Mathf.Max(0.9f, op.progress));
                yield return null;
            }

            progress?.Invoke(1f);
            onCompleted?.Invoke();
            _isLoading = false;
        }
    }
}
