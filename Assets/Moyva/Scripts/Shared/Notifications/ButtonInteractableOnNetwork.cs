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
        private const string Prefix = "[ButtonInteractableOnNetwork]";
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
            Debug.Log($"{Prefix} Awake start. targetButtonAssigned={targetButton != null}");
            if (targetButton == null)
                targetButton = GetComponent<Button>();
            Debug.Log($"{Prefix} Awake end. targetButtonSet={targetButton != null}");
        }

        public void Construct([InjectOptional] IMultiplayerState multiplayerState = null)
        {
            _multiplayerState = multiplayerState;
            Debug.Log($"{Prefix} Construct called. multiplayerStateInjected={_multiplayerState != null}");
        }

        private void OnEnable()
        {
            Debug.Log($"{Prefix} OnEnable start. startDisabledUntilChecked={startDisabledUntilChecked}, pollingInterval={pollingInterval}");
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
            Debug.Log($"{Prefix} Monitor loop started.");
        }

        private void OnDisable()
        {
            Debug.Log($"{Prefix} OnDisable called. Cancelling monitor if exists.");
            _cts?.Cancel();
            _cts = null;
        }

        private async Task MonitorLoopAsync(CancellationToken ct)
        {
            Debug.Log($"{Prefix} MonitorLoopAsync start.");
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
            Debug.Log($"{Prefix} MonitorLoopAsync exiting.");
        }

        private async Task CheckAndApplyAsync()
        {
            Debug.Log($"{Prefix} CheckAndApplyAsync start.");
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

            Debug.Log($"{Prefix} Check result: online={online}, interactableSet={(targetButton!=null?targetButton.interactable.ToString():"no-button")}");

            if (online != _lastIsOnline)
            {
                _lastIsOnline = online;
                if (online)
                {
                    Debug.Log($"{Prefix} Status changed to ONLINE. Invoking OnOnline.");
                    OnOnline?.Invoke();
                }
                else
                {
                    Debug.Log($"{Prefix} Status changed to OFFLINE. Invoking OnOffline.");
                    OnOffline?.Invoke();
                }
            }
            Debug.Log($"{Prefix} CheckAndApplyAsync end.");
        }
    }
}
