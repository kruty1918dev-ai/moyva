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
            try
            {
                client.ExclusiveAddressUse = false;
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                return client;
            }
            catch (SocketException ex)
            {
                client.Dispose();
                throw new NetworkTransportException($"LAN discovery cannot listen on UDP {port} ({ex.SocketErrorCode}).", ex);
            }
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
            if (client == null)
                return null;

            return await Task.Run(
                () =>
                {
                    var deadline = DateTime.UtcNow.Add(timeout);
                    while (DateTime.UtcNow < deadline)
                    {
                        ct.ThrowIfCancellationRequested();

                        var remaining = deadline - DateTime.UtcNow;
                        var sliceMs = Math.Min(100, Math.Max(1, (int)remaining.TotalMilliseconds));
                        if (!client.Client.Poll(sliceMs * 1000, SelectMode.SelectRead))
                            continue;

                        var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        var buffer = client.Receive(ref remoteEndPoint);
                        return (UdpReceiveResult?)new UdpReceiveResult(buffer, remoteEndPoint);
                    }

                    return null;
                },
                ct).ConfigureAwait(false);
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

        internal static string BuildLocalHostId()
        {
            var machineName = string.IsNullOrWhiteSpace(Environment.MachineName) ? "local" : Environment.MachineName;
            if (!MultiplayerClientScope.IsDefault)
                return $"{machineName}-{MultiplayerClientScope.ScopeId}";

            return $"{machineName}-{System.Diagnostics.Process.GetCurrentProcess().Id}";
        }

        internal static string GetLocalIPAddress()
            => GetPreferredLocalIPAddress();

        internal static string GetPreferredLocalIPAddress()
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

        private static IReadOnlyList<IPEndPoint> GetDirectedBroadcastEndPoints(int port)
        {
            var result = new List<IPEndPoint>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                        ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    {
                        continue;
                    }

                    foreach (var uni in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (uni.Address.AddressFamily != AddressFamily.InterNetwork)
                            continue;

                        var ip = uni.Address.ToString();
                        if (ip.StartsWith("169.254.", StringComparison.Ordinal))
                            continue;

                        if (!TryBuildDirectedBroadcastAddress(uni, out var broadcast))
                            continue;

                        var key = broadcast.ToString();
                        if (!seen.Add(key))
                            continue;

                        result.Add(new IPEndPoint(broadcast, port));
                    }
                }
            }
            catch { }

            return result;
        }

        private static bool TryBuildDirectedBroadcastAddress(UnicastIPAddressInformation address, out IPAddress broadcast)
        {
            broadcast = null;
            try
            {
                var mask = address.IPv4Mask;
                if (mask == null)
                    return false;

                var ipBytes = address.Address.GetAddressBytes();
                var maskBytes = mask.GetAddressBytes();
                if (ipBytes.Length != 4 || maskBytes.Length != 4)
                    return false;

                var broadcastBytes = new byte[4];
                for (var i = 0; i < 4; i++)
                    broadcastBytes[i] = (byte)(ipBytes[i] | ~maskBytes[i]);

                broadcast = new IPAddress(broadcastBytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            lock (_discoveryLock)
            {
                if (_disposed) return;
                _disposed = true;
                StopBroadcastLoop();
                try { _udp.Close(); _udp.Dispose(); } catch { }
            }
        }
    }
}
