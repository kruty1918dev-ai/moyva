using System;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public readonly struct MultiplayerQosSnapshot
    {
        public readonly double AverageLatencyMs;
        public readonly double PacketLossPercent;
        public readonly int ReconnectCount;
        public readonly long PingSent;
        public readonly long PingAcked;

        public MultiplayerQosSnapshot(double averageLatencyMs, double packetLossPercent, int reconnectCount, long pingSent, long pingAcked)
        {
            AverageLatencyMs = averageLatencyMs;
            PacketLossPercent = packetLossPercent;
            ReconnectCount = reconnectCount;
            PingSent = pingSent;
            PingAcked = pingAcked;
        }
    }

    public interface IMultiplayerQosMonitorService
    {
        MultiplayerQosSnapshot Snapshot { get; }
        void RecordPingSent(string correlationId, long sentUtcTicks);
        void RecordPingAck(string correlationId, long sentUtcTicks, long ackUtcTicks);
        void RecordReconnect(string provider, int attempt);
        void RecordPacketDropped(string reason);
    }

    public sealed class MultiplayerQosMonitorService : IMultiplayerQosMonitorService
    {
        private readonly object _sync = new object();
        private double _avgLatencyMs;
        private long _pingSent;
        private long _pingAcked;
        private int _reconnectCount;
        private long _packetDrops;

        public MultiplayerQosSnapshot Snapshot
        {
            get
            {
                lock (_sync)
                {
                    double packetLossPercent = _pingSent <= 0
                        ? 0d
                        : (100d * Math.Max(0d, _pingSent - _pingAcked + _packetDrops) / _pingSent);
                    return new MultiplayerQosSnapshot(_avgLatencyMs, packetLossPercent, _reconnectCount, _pingSent, _pingAcked);
                }
            }
        }

        public void RecordPingSent(string correlationId, long sentUtcTicks)
        {
            lock (_sync)
                _pingSent++;
        }

        public void RecordPingAck(string correlationId, long sentUtcTicks, long ackUtcTicks)
        {
            var latencyMs = TimeSpan.FromTicks(Math.Max(0L, ackUtcTicks - sentUtcTicks)).TotalMilliseconds;
            lock (_sync)
            {
                _pingAcked++;
                _avgLatencyMs = _avgLatencyMs <= 0d
                    ? latencyMs
                    : (_avgLatencyMs * 0.85d + latencyMs * 0.15d);
            }
        }

        public void RecordReconnect(string provider, int attempt)
        {
            lock (_sync)
                _reconnectCount++;
        }

        public void RecordPacketDropped(string reason)
        {
            lock (_sync)
                _packetDrops++;
        }
    }
}