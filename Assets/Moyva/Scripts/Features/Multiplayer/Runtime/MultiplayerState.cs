using System;
using Kruty1918.Moyva.Multiplayer.Networking;
#if MOYVA_UGS_RELAY || MOYVA_UGS_LOBBY
using Unity.Services.Authentication;
using Unity.Services.Core;
#endif

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal class MultiplayerState : IMultiplayerState
    {
        public MultiplayerConnectionState ConnectionState => BuildCurrentState();

        public void UpdateConnectionState(bool isUnityServicesInitialized, bool isAuthenticated, bool isConnecting, bool isConnected, float connectionProgress)
        {
            _manualState = new MultiplayerConnectionState(
                isUnityServicesInitialized,
                isAuthenticated,
                isConnecting,
                isConnected,
                connectionProgress);
        }

        private MultiplayerConnectionState BuildCurrentState()
        {
#if MOYVA_UGS_RELAY || MOYVA_UGS_LOBBY
            var unityInitialized = UnityServices.State == ServicesInitializationState.Initialized;
            var unityInitializing = UnityServices.State == ServicesInitializationState.Initializing;
            var authenticated = AuthenticationService.Instance.IsSignedIn;

            var isConnecting = unityInitializing || (unityInitialized && !authenticated);
            var isConnected = unityInitialized && authenticated;
            var progress = unityInitializing
                ? 0.25f
                : unityInitialized
                    ? (authenticated ? 1f : 0.75f)
                    : 0f;

            return new MultiplayerConnectionState(
                unityInitialized,
                authenticated,
                isConnecting,
                isConnected,
                progress);
#else
            return new MultiplayerConnectionState(true, true, false, true, 1f);
#endif
        }

        private MultiplayerConnectionState _manualState;
    }
}
