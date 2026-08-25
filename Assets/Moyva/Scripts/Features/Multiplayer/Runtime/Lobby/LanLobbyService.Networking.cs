using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    public sealed partial class LanLobbyService
    {
        private static UdpClient CreateListeningClient(int port)
        {
            var client = new UdpClient(AddressFamily.InterNetwork);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            return client;
        }

        private static UdpClient CreateQueryClient()
        {
            var client = new UdpClient(AddressFamily.InterNetwork);
            client.EnableBroadcast = true;
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            return client;
        }

        private static async Task<byte[]> ReceiveWithTimeoutAsync(UdpClient client, TimeSpan timeout, CancellationToken ct)
        {
            var result = await ReceiveResultWithTimeoutAsync(client, timeout, ct).ConfigureAwait(false);
            return result.HasValue ? result.Value.Buffer : null;
        }

        private static async Task<UdpReceiveResult?> ReceiveResultWithTimeoutAsync(UdpClient client, TimeSpan timeout, CancellationToken ct)
        {
            var receiveTask = client.ReceiveAsync();
            var delayTask = Task.Delay(timeout, ct);
            var completed = await Task.WhenAny(receiveTask, delayTask).ConfigureAwait(false);
            if (completed != receiveTask)
                return null;

            return receiveTask.Result;
        }

        private static string ResolveHostDisplayName(LobbyRoom room)
        {
            if (room?.Players != null)
            {
                foreach (var player in room.Players)
                {
                    if (player != null && player.IsHost && !string.IsNullOrWhiteSpace(player.DisplayName))
                        return player.DisplayName.Trim();
                }
            }

            return Environment.MachineName ?? "Player";
        }

        private static string BuildLocalHostId()
        {
            var machineName = string.IsNullOrWhiteSpace(Environment.MachineName) ? "local" : Environment.MachineName;
            if (!MultiplayerClientScope.IsDefault)
                return $"{machineName}-{MultiplayerClientScope.ScopeId}";

            return $"{machineName}-{System.Diagnostics.Process.GetCurrentProcess().Id}";
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up)
                        continue;

                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    {
                        continue;
                    }

                    var props = ni.GetIPProperties();
                    foreach (var uni in props.UnicastAddresses)
                    {
                        if (uni.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;

                        var ip = uni.Address.ToString();
                        if (ip.StartsWith("169.254.", StringComparison.Ordinal))
                            continue;

                        return ip;
                    }
                }
            }

            catch { }

            try
            {
                foreach (var ni in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ni.AddressFamily == AddressFamily.InterNetwork && !ni.ToString().StartsWith("127.", StringComparison.Ordinal))
                        return ni.ToString();
                }
            }
            catch { }

            return null;
        }

        public void Dispose()
        {
            StopBroadcastLoop();
            try { _udp.Close(); _udp.Dispose(); } catch { }
        }
    }
}
