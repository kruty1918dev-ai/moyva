using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Компонент, що відображає ім'я локального гравця у TMP-тексті.
    /// Залежності: <see cref="ILocalGameSettingsService"/> як джерело актуальних налаштувань.
    /// </summary>
    public sealed class PlayerNameTextComponent : MonoBehaviour, IInitializable
    {
        /// <summary>Цільовий текстовий компонент для відображення імені.</summary>
        [SerializeField] private TMP_Text _label;

        private ILocalGameSettingsService _settings;
        private bool _subscribed;

        [Inject]
        public void Construct(ILocalGameSettingsService settings)
        {
            _settings = settings;
        }

        private void Awake()
        {
            if (_label == null)
                _label = GetComponentInChildren<TMP_Text>(true);
        }

        /// <summary>Початкова ініціалізація компонента через Zenject lifecycle.</summary>
        public void Initialize()
        {
            if (_label == null)
                _label = GetComponentInChildren<TMP_Text>(true);

            // 1: Підписуємось на зміни налаштувань, щоб підтримувати UI актуальним.
            Subscribe();

            // 2: Одразу рендеримо поточне значення при ініціалізації.
            if (_settings != null)
                Refresh(_settings.Settings);
        }

        private void OnDestroy()
        {
            if (_settings != null && _subscribed)
                _settings.OnSettingsChanged -= Refresh;
            _subscribed = false;
        }

        private void Subscribe()
        {
            if (_settings == null || _subscribed)
                return;

            _settings.OnSettingsChanged += Refresh;
            _subscribed = true;
        }

        /// <summary>Оновити текст імені відповідно до нових налаштувань.</summary>
        private void Refresh(LocalGameSettings settings)
        {
            if (_label != null)
                _label.text = settings.PlayerName;
        }
    }
}