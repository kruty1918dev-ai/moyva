using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Shared.Notifications
{
    [RequireComponent(typeof(Button))]
    public class ButtonInteractableOnNetwork : MonoBehaviour
    {
        [Tooltip("Button to enable/disable. If empty, will get a Button on the same GameObject.")]
        public Button targetButton;

        [Tooltip("If true -> button is interactable when online. If false -> interactable when offline.")]
        public bool interactableWhenOnline = true;

        [Tooltip("If true the button will be disabled until the first connectivity check completes.")]
        public bool startDisabledUntilChecked = false;

        [Tooltip("Polling interval in seconds. Set to 0 to check only once on start.")]
        public float pollingInterval = 10f;

        public UnityEvent OnOnline;
        public UnityEvent OnOffline;

        private IMultiplayerState _multiplayerState;
        private CancellationTokenSource _cts;
        private bool _lastIsOnline;

        private void Awake()
        {
            if (targetButton == null)
                targetButton = GetComponent<Button>();
        }

        public void Construct([InjectOptional] IMultiplayerState multiplayerState = null)
        {
            _multiplayerState = multiplayerState;
        }

        private void OnEnable()
        {
            if (startDisabledUntilChecked && targetButton != null)
                targetButton.interactable = false;

            // Try to resolve via ProjectContext if injection didn't provide the service
            if (_multiplayerState == null)
            {
                try
                {
                    if (Zenject.ProjectContext.Instance != null)
                    {
                        var container = Zenject.ProjectContext.Instance.Container;
                        if (container != null && container.HasBinding(typeof(IMultiplayerState)))
                            _multiplayerState = container.Resolve<IMultiplayerState>();
                    }
                }
                catch { }
            }

            _cts = new CancellationTokenSource();
            _ = MonitorLoopAsync(_cts.Token);
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts = null;
        }

        private async Task MonitorLoopAsync(CancellationToken ct)
        {
            await CheckAndApplyAsync();

            if (pollingInterval <= 0f)
                return;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(0.1f, pollingInterval)), ct);
                }
                catch (TaskCanceledException) { break; }

                await CheckAndApplyAsync();
            }
        }

        private async Task CheckAndApplyAsync()
        {
            bool online = false;

            if (_multiplayerState != null)
            {
                var state = _multiplayerState.ConnectionState;
                online = state.IsInitialized || state.IsConnected || state.IsAuthenticationReady;

                if (!online)
                {
                    try { online = await InternetChecker.HasInternetAsync(); } catch { online = false; }
                }
            }
            else
            {
                try { online = await InternetChecker.HasInternetAsync(); } catch { online = false; }
            }

            if (targetButton != null)
                targetButton.interactable = (online == interactableWhenOnline);

            if (online != _lastIsOnline)
            {
                _lastIsOnline = online;
                if (online) OnOnline?.Invoke(); else OnOffline?.Invoke();
            }
        }
    }
}
