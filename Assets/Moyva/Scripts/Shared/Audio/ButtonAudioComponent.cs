using Kruty1918.Moyva.Audio.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Audio.Runtime
{
    /// <summary>
    /// Компонент для відтворення звуку при натисканні на кнопку.
    ///
    /// Використання:
    /// 1. Додай компонент на об'єкт з <see cref="UnityEngine.UI.Button"/>.
    /// 2. В інспекторі обери потрібний звук із випадаючого списку.
    /// 3. Компонент автоматично підпишеться на onClick кнопки через Zenject IInitializable.
    ///
    /// Звук викликається через <see cref="IAudioService"/>, який Zenject інжектує автоматично.
    /// Якщо реєстр не знайдено — скористайся кнопкою "Знайти реєстр" в інспекторі.
    /// </summary>
    [AddComponentMenu("Moyva/Audio/Button Audio")]
    [RequireComponent(typeof(Button))]
    [DisallowMultipleComponent]
    public sealed class ButtonAudioComponent : MonoBehaviour
    {
        [Tooltip("Ключ звуку, що відтворюється при натисканні на кнопку.")]
        [SerializeField] private string _soundKey = string.Empty;

        [Tooltip("Кешований реєстр звуків. Заповнюється автоматично при старті редактора.")]
        [SerializeField] private AudioRegistrySO _cachedRegistry;

        [Inject] private IAudioService _audioService;

        private Button _button;

        /// <summary>Ключ звуку, що відтворюється при кліку.</summary>
        public string SoundKey => _soundKey;

        /// <summary>Кешований реєстр звуків (може бути null у runtime).</summary>
        public AudioRegistrySO CachedRegistry => _cachedRegistry;

        private void Awake()
        {
            _button = GetComponent<Button>();
            TryCacheRegistry();
             if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }



        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (string.IsNullOrWhiteSpace(_soundKey))
                return;

            _audioService?.Play(_soundKey);
        }

        /// <summary>
        /// Намагається автоматично знайти реєстр звуків через Resources.
        /// Викликається в Awake та з редактора.
        /// </summary>
        public void TryCacheRegistry()
        {
            if (_cachedRegistry != null)
                return;

            _cachedRegistry = Resources.Load<AudioRegistrySO>("MoyvaAudioRegistry");
        }
    }
}
