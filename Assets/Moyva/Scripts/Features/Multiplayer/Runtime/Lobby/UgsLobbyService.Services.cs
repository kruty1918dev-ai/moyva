using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    public sealed partial class UgsLobbyService
    {
        private static async Task EnsureServicesReadyAsync()
        {
            if (_servicesReadyTask == null || !_servicesReadyTask.IsCompletedSuccessfully)
            {
                lock (ServicesReadyLock)
                {
                    if (_servicesReadyTask == null || !_servicesReadyTask.IsCompletedSuccessfully)
                    {
                        _servicesReadyTask = InitializeServicesAsync();
                    }
                }
            }

            await _servicesReadyTask;

            if (AuthenticationService.Instance == null)
                throw new InvalidOperationException("[UgsLobby] AuthenticationService instance is unavailable after Unity Services initialization.");

            await MultiplayerAuthenticationGate.EnsureReadyAsync();

            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId))
            {
                throw new InvalidOperationException("[UgsLobby] Authentication failed after sign-in; PlayerId is unavailable.");
            }
        }

        private static async Task InitializeServicesAsync()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return;

            await UnityServices.InitializeAsync();
        }

        private static Player BuildLocalPlayer(string displayName)
        {
            if (AuthenticationService.Instance == null)
                throw new InvalidOperationException("[UgsLobby] AuthenticationService instance is unavailable.");

            if (!AuthenticationService.Instance.IsSignedIn || string.IsNullOrEmpty(AuthenticationService.Instance.PlayerId))
                throw new InvalidOperationException("[UgsLobby] Cannot build local player: authentication is not completed or PlayerId is missing.");

            return new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    { "name", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, displayName ?? "Player") },
                    { LocalTimeTicksDataKey, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, DateTime.Now.Ticks.ToString()) },
                });
        }

    }
}
