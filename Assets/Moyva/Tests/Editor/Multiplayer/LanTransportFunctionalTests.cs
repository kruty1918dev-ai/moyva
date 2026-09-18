using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    /// <summary>
    /// Functional loopback test: two real LanNetworkProvider instances talk
    /// over UDP on this machine. Covers handshake, reliable delivery of
    /// frames that require fragmentation, broadcast and disconnect events.
    /// </summary>
    public sealed class LanTransportFunctionalTests
    {
        private const string HostId = "test-host-peer";
        private const string ClientId = "test-client-peer";
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

        private sealed class Collector : IObserver<NetworkMessage>
        {
            public readonly ConcurrentQueue<NetworkMessage> Messages = new();
            public void OnNext(NetworkMessage value) => Messages.Enqueue(value);
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }

        [Test]
        public async Task Lan_Handshake_Message_50KB_And_Disconnect_Work()
        {
            var host = new LanNetworkProvider(MultiplayerConfig.Default());
            var client = new LanNetworkProvider(MultiplayerConfig.Default());
            host.SetLocalPeerId(HostId);
            client.SetLocalPeerId(ClientId);

            var hostConnected = new ConcurrentQueue<string>();
            var hostDisconnected = new ConcurrentQueue<string>();
            var clientConnected = new ConcurrentQueue<string>();
            host.PeerConnected += id => hostConnected.Enqueue(id);
            host.PeerDisconnected += id => hostDisconnected.Enqueue(id);
            client.PeerConnected += id => clientConnected.Enqueue(id);

            var hostMessages = new Collector();
            var clientMessages = new Collector();
            host.Messages.Subscribe(hostMessages);
            client.Messages.Subscribe(clientMessages);

            try
            {
                var hostResult = await host.HostSessionAsync("session");
                Assert.IsTrue(hostResult.Success, hostResult.ErrorMessage);
                Assert.IsTrue(
                    hostConnected.TryDequeue(out var selfId) && selfId == HostId,
                    "Host should announce itself as connected.");

                var joinCode = RewriteToLoopback(hostResult.SessionId);
                var clientResult = await client.JoinSessionAsync(joinCode);
                Assert.IsTrue(clientResult.Success, clientResult.ErrorMessage);

                Assert.IsTrue(
                    await WaitForAsync(() => clientConnected.TryDequeue(out var id) && id == HostId),
                    "Client never saw the host peer.");
                Assert.IsTrue(
                    await WaitForAsync(() => hostConnected.TryDequeue(out var id) && id == ClientId),
                    "Host never saw the client peer.");

                // Small broadcast host -> client.
                var small = new byte[] { 9, 8, 7, 6 };
                await host.SendMessageAsync("*", small);
                Assert.IsTrue(
                    await WaitForAsync(() =>
                        clientMessages.Messages.TryDequeue(out var m)
                        && m.SenderId == HostId
                        && m.Payload != null
                        && m.Payload.Length == small.Length),
                    "Client never received the small host broadcast.");

                // 50 KiB client -> host forces fragmentation + reassembly.
                // The host loopback-echoes its own broadcast, so drain the
                // queue until the client's large message shows up.
                var large = new byte[50 * 1024];
                for (var i = 0; i < large.Length; i++)
                    large[i] = (byte)(i % 251);
                await client.SendMessageAsync("*", large);
                Assert.IsTrue(
                    await WaitForAsync(() =>
                    {
                        while (hostMessages.Messages.TryDequeue(out var m))
                        {
                            if (m.SenderId == ClientId
                                && m.Payload != null
                                && m.Payload.Length == large.Length
                                && m.Payload[0] == large[0]
                                && m.Payload[large.Length - 1] == large[large.Length - 1])
                            {
                                return true;
                            }
                        }
                        return false;
                    }),
                    "Host never received the fragmented 50 KiB client message.");

                await client.LeaveSessionAsync();
                Assert.IsTrue(
                    await WaitForAsync(() => hostDisconnected.TryDequeue(out var id) && id == ClientId),
                    "Host never observed the client disconnect.");
            }
            finally
            {
                try { await client.LeaveSessionAsync(); } catch { }
                try { await host.LeaveSessionAsync(); } catch { }
                try { client.Dispose(); } catch { }
                try { host.Dispose(); } catch { }
            }
        }

        private static string RewriteToLoopback(string joinCode)
        {
            var parts = (joinCode ?? string.Empty).Split(':');
            Assert.GreaterOrEqual(parts.Length, 3, $"Join code '{joinCode}' is malformed.");
            return $"lan:127.0.0.1:{parts[2]}";
        }

        private static async Task<bool> WaitForAsync(Func<bool> condition)
        {
            var deadline = DateTime.UtcNow + Timeout;
            while (DateTime.UtcNow < deadline)
            {
                if (condition())
                    return true;
                await Task.Delay(25);
            }
            return false;
        }
    }
}
