using Kruty1918.Moyva.HomeMenu.API;
using TMPro;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed class PlayerNameTextComponent : MonoBehaviour, IInitializable
    {
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

        public void Initialize()
        {
            if (_label == null)
                _label = GetComponentInChildren<TMP_Text>(true);

            Subscribe();

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

        private void Refresh(LocalGameSettings settings)
        {
            if (_label != null)
                _label.text = settings.PlayerName;
        }
    }
}