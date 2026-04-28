// Compatibility shim: maps Unity Transport 2.x 'NetworkEndpoint' API to the local 1.x
// 'NetworkEndPoint' struct so that com.unity.services.multiplayer (which targets 2.x)
// can compile against the local transport package without modifications.
#if !UNITY_WEBGL
using System.Runtime.InteropServices;
using Unity.Baselib.LowLevel;
using Unity.Collections;
using Unity.Networking.Transport.Utilities;

namespace Unity.Networking.Transport
{
    /// <summary>
    /// Compatibility shim for Unity Transport 2.x <c>NetworkEndpoint</c>.
    /// Same memory layout as the local 1.x <see cref="NetworkEndPoint"/> struct.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct NetworkEndpoint
    {
        // Must match the internal layout of NetworkEndPoint (single Baselib_NetworkAddress field).
        internal Binding.Baselib_NetworkAddress rawNetworkAddress;

        // ── Properties ────────────────────────────────────────────────────────────

        public ushort Port
        {
            get => ((NetworkEndPoint)this).Port;
            set
            {
                var ep = (NetworkEndPoint)this;
                ep.Port = value;
                rawNetworkAddress = ep.rawNetworkAddress;
            }
        }

        /// <summary>IP address and port as a string, e.g. "127.0.0.1:7777".</summary>
        public string Address => ((NetworkEndPoint)this).Address;

        public NetworkFamily Family
        {
            get => ((NetworkEndPoint)this).Family;
            set
            {
                var ep = (NetworkEndPoint)this;
                ep.Family = value;
                rawNetworkAddress = ep.rawNetworkAddress;
            }
        }

        public bool IsValid => ((NetworkEndPoint)this).IsValid;

        // ── Methods ───────────────────────────────────────────────────────────────

        /// <summary>Returns a copy of this endpoint with the specified port.</summary>
        public NetworkEndpoint WithPort(ushort port)
        {
            NetworkEndpoint result = this;
            result.Port = port;
            return result;
        }

        /// <summary>Returns the IP address (without port) as a fixed-length string.</summary>
        public FixedString128Bytes ToFixedStringNoPort()
        {
            FixedString128Bytes str = default;
            switch (Family)
            {
                case NetworkFamily.Ipv4:
                    str.Append(rawNetworkAddress.data0);
                    str.Append((FixedString32Bytes)".");
                    str.Append(rawNetworkAddress.data1);
                    str.Append((FixedString32Bytes)".");
                    str.Append(rawNetworkAddress.data2);
                    str.Append((FixedString32Bytes)".");
                    str.Append(rawNetworkAddress.data3);
                    break;
                case NetworkFamily.Ipv6:
                    str.Append((FixedString32Bytes)"[");
                    str.AppendHex((ushort)(rawNetworkAddress.data1  | (rawNetworkAddress.data0  << 8)));
                    str.Append((FixedString32Bytes)":");
                    str.AppendHex((ushort)(rawNetworkAddress.data3  | (rawNetworkAddress.data2  << 8)));
                    str.Append((FixedString32Bytes)":");
                    str.AppendHex((ushort)(rawNetworkAddress.data5  | (rawNetworkAddress.data4  << 8)));
                    str.Append((FixedString32Bytes)":");
                    str.AppendHex((ushort)(rawNetworkAddress.data7  | (rawNetworkAddress.data6  << 8)));
                    str.Append((FixedString32Bytes)":");
                    str.AppendHex((ushort)(rawNetworkAddress.data9  | (rawNetworkAddress.data8  << 8)));
                    str.Append((FixedString32Bytes)":");
                    str.AppendHex((ushort)(rawNetworkAddress.data11 | (rawNetworkAddress.data10 << 8)));
                    str.Append((FixedString32Bytes)":");
                    str.AppendHex((ushort)(rawNetworkAddress.data13 | (rawNetworkAddress.data12 << 8)));
                    str.Append((FixedString32Bytes)":");
                    str.AppendHex((ushort)(rawNetworkAddress.data15 | (rawNetworkAddress.data14 << 8)));
                    str.Append((FixedString32Bytes)"]");
                    break;
            }
            return str;
        }

        // ── Static factories ─────────────────────────────────────────────────────

        public static NetworkEndpoint AnyIpv4      => NetworkEndPoint.AnyIpv4;
        public static NetworkEndpoint LoopbackIpv4 => NetworkEndPoint.LoopbackIpv4;
        public static NetworkEndpoint AnyIpv6      => NetworkEndPoint.AnyIpv6;
        public static NetworkEndpoint LoopbackIpv6 => NetworkEndPoint.LoopbackIpv6;

        public static bool TryParse(string address, ushort port,
            out NetworkEndpoint endpoint, NetworkFamily family = NetworkFamily.Ipv4)
        {
            bool ok = NetworkEndPoint.TryParse(address, port, out NetworkEndPoint ep, family);
            endpoint = ep;
            return ok;
        }

        // ── Conversions ───────────────────────────────────────────────────────────

        public static implicit operator NetworkEndpoint(NetworkEndPoint ep)
        {
            NetworkEndpoint result = default;
            result.rawNetworkAddress = ep.rawNetworkAddress;
            return result;
        }

        public static implicit operator NetworkEndPoint(NetworkEndpoint ep)
        {
            NetworkEndPoint result = default;
            result.rawNetworkAddress = ep.rawNetworkAddress;
            return result;
        }
    }
}
#endif
