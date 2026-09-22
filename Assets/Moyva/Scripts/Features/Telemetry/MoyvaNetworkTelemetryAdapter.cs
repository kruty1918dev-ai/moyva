using System;
using System.Security.Cryptography;
using System.Text;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Telemetry.Core;
using Zenject;

namespace Kruty1918.Moyva.Telemetry
{
    /// <summary>
    /// Multiplayer session boundary: peer connect/disconnect transitions.
    /// Peer ids are hashed before recording — raw network ids never leave the device.
    /// Optional dependency: inert when multiplayer is not bound.
    /// </summary>
    public sealed class MoyvaNetworkTelemetryAdapter : IInitializable, IDisposable
    {
        private readonly ITelemetrySink _sink;
        private readonly INetworkProvider _provider;

        public MoyvaNetworkTelemetryAdapter(ITelemetrySink sink,
            [InjectOptional] INetworkProvider provider = null)
        {
            _sink = sink;
            _provider = provider;
        }

        public void Initialize()
        {
            if (_provider == null) return;
            _provider.PeerConnected += OnPeerConnected;
            _provider.PeerDisconnected += OnPeerDisconnected;
        }

        public void Dispose()
        {
            if (_provider == null) return;
            _provider.PeerConnected -= OnPeerConnected;
            _provider.PeerDisconnected -= OnPeerDisconnected;
        }

        private void OnPeerConnected(string peerId) => Track("connected", peerId);
        private void OnPeerDisconnected(string peerId) => Track("disconnected", peerId);

        private void Track(string kind, string peerId)
            => _sink.Track(new MoyvaNetPeerEvent
            { Kind = kind, PeerIdHash = HashPeerId(peerId) });

        /// <summary>Stable non-reversible peer id hash — raw ids never leave the device.</summary>
        internal static string HashPeerId(string peerId)
        {
            if (string.IsNullOrEmpty(peerId)) return "";
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(peerId)))
                    .Replace("-", "").ToLowerInvariant().Substring(0, 16);
        }
    }
}
