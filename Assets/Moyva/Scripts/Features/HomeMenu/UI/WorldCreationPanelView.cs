using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Мінімальна панель створення світу: підтвердити (з поточним CurrentConfig)
    /// або скасувати. Повноцінний UI з усіма параметрами реалізований у
    /// <c>Kruty1918.Moyva.WorldCreation.UI.WorldCreationUIController</c> і може
    /// бути доданий у цю ж панель пізніше — ця view служить як fallback-вхід.
    /// </summary>
    public sealed class WorldCreationPanelView : MonoBehaviour
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private SignalBus _signalBus;
        private IWorldCreationService _service;

        [Inject]
        internal void Construct(SignalBus signalBus, IWorldCreationService service)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
            _service   = service   ?? throw new ArgumentNullException(nameof(service));
        }

        private void OnEnable()
        {
            if (confirmButton != null) confirmButton.onClick.AddListener(HandleConfirm);
            if (cancelButton  != null) cancelButton.onClick.AddListener(HandleCancel);
        }

        private void OnDisable()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
            if (cancelButton  != null) cancelButton.onClick.RemoveListener(HandleCancel);
        }

        private void HandleConfirm()
        {
            var config = _service.CurrentConfig;
            if (!_service.ValidateConfig(config, out string error))
            {
                Debug.LogError($"[WorldCreationPanelView] Invalid default config: {error}");
                return;
            }

            var data = _service.ToSignalData(config);
            _signalBus.Fire(new WorldCreationConfirmedSignal { Config = data });
        }

        private void HandleCancel()
        {
            _signalBus.Fire(new WorldCreationCancelledSignal());
        }
    }
}
