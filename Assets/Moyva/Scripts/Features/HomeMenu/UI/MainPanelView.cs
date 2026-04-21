using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Головна панель з трьома кнопками: Start Game / Settings / Quit.
    /// </summary>
    public sealed class MainPanelView : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private IHomeMenuFlow _flow;
        private SignalBus _signalBus;

        [Inject]
        internal void Construct(IHomeMenuFlow flow, SignalBus signalBus)
        {
            _flow      = flow ?? throw new ArgumentNullException(nameof(flow));
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
        }

        private void OnEnable()
        {
            if (startButton    != null) startButton.onClick.AddListener(HandleStart);
            if (settingsButton != null) settingsButton.onClick.AddListener(HandleSettings);
            if (quitButton     != null) quitButton.onClick.AddListener(HandleQuit);
        }

        private void OnDisable()
        {
            if (startButton    != null) startButton.onClick.RemoveListener(HandleStart);
            if (settingsButton != null) settingsButton.onClick.RemoveListener(HandleSettings);
            if (quitButton     != null) quitButton.onClick.RemoveListener(HandleQuit);
        }

        private void HandleStart()
        {
            _signalBus.TryFire(new HomeMenuStartRequestedSignal());
            _flow.ShowWorldCreation();
        }

        private void HandleSettings() => _flow.ShowSettings();
        private void HandleQuit()     => _flow.RequestQuit();
    }
}
