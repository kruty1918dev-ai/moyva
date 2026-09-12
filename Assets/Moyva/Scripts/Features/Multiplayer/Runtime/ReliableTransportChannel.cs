using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;
using TransportStatusCode = Unity.Networking.Transport.Error.StatusCode;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    internal sealed class TransportSendQueueFullException : InvalidOperationException
    {
        public TransportSendQueueFullException()
            : base("Multiplayer send queue is full; the remote peer is not keeping up.") { }
    }

    /// <summary>Ordered, bounded transport delivery, including recovery from send queue pressure.</summary>
    internal sealed class ReliableTransportChannel
    {
        internal const int MaximumFrameBytes = 60 * 1024 + 3;
        private const int SequenceBytes = 8;
        private const int MaximumQueuedFrames = 128;
        private const int MaximumQueuedBytes = 4 * 1024 * 1024;
        private const int MaximumBytesPerPeerPerTick = 64 * 1024;
        private const int MaximumFramesPerPeerPerTick = 16;
        private const double QueueTimeoutSeconds = 120;

        private readonly NetworkPipeline _pipeline;
        private readonly Dictionary<NetworkConnection, SendQueue> _sendQueues = new();
        private readonly Dictionary<NetworkConnection, ulong> _receivedSequences = new();
        private readonly List<NetworkConnection> _iteration = new();
        private int _queuedFrames;
        private int _queuedBytes;

        private sealed class SendQueue
        {
            public readonly Queue<PendingFrame> Frames = new();
            public ulong NextSequence = 1;
            public double RetryAt;
            public double RetryDelay = 0.05;
        }

        private readonly struct PendingFrame
        {
            public readonly byte[] Bytes;
            public readonly double EnqueuedAt;

            public PendingFrame(byte[] bytes, double enqueuedAt)
            {
                Bytes = bytes;
                EnqueuedAt = enqueuedAt;
            }
        }

        public static void Configure(ref NetworkSettings settings)
        {
            settings.WithNetworkConfigParameters(
                disconnectTimeoutMS: 120_000,
                heartbeatTimeoutMS: 1_000);
            settings.WithFragmentationStageParameters(MaximumFrameBytes + SequenceBytes);
            // A maximum-sized frame must fit in one reliable window (~46 MTU-sized fragments).
            settings.WithReliableStageParameters(windowSize: 64);
        }

        public ReliableTransportChannel(NetworkDriver driver)
        {
            _pipeline = driver.CreatePipeline(
                typeof(FragmentationPipelineStage),
                typeof(ReliableSequencedPipelineStage));
        }

        public void Send(NetworkDriver driver, NetworkConnection connection, byte[] frame)
        {
            if (frame == null || frame.Length < 3 || frame.Length > MaximumFrameBytes)
                throw new ArgumentOutOfRangeException(nameof(frame), "Transport frame exceeds the supported size.");
            if (!driver.IsCreated || !connection.IsCreated ||
                driver.GetConnectionState(connection) != NetworkConnection.State.Connected)
                throw new InvalidOperationException("Cannot send to a disconnected multiplayer peer.");

            EnsureCapacity(frame.Length);
            var length = frame.Length + SequenceBytes;

            if (!_sendQueues.TryGetValue(connection, out var queue))
                _sendQueues.Add(connection, queue = new SendQueue());

            var bytes = new byte[length];
            var sequence = queue.NextSequence++;
            for (var i = 0; i < SequenceBytes; i++)
                bytes[i] = (byte)(sequence >> (8 * i));
            Buffer.BlockCopy(frame, 0, bytes, SequenceBytes, frame.Length);
            queue.Frames.Enqueue(new PendingFrame(bytes, NowSeconds));
            _queuedFrames++;
            _queuedBytes += length;
        }

        public void EnsureCapacity(int frameBytes, int frameCount = 1)
        {
            if (frameBytes < 3 || frameBytes > MaximumFrameBytes || frameCount < 0)
                throw new ArgumentOutOfRangeException(nameof(frameBytes));
            if ((long)_queuedFrames + frameCount > MaximumQueuedFrames ||
                _queuedBytes + ((long)frameBytes + SequenceBytes) * frameCount > MaximumQueuedBytes)
                throw new TransportSendQueueFullException();
        }

        public void Flush(NetworkDriver driver, Action<NetworkConnection, string> failed)
        {
            _iteration.Clear();
            _iteration.AddRange(_sendQueues.Keys);
            foreach (var connection in _iteration)
            {
                if (!_sendQueues.TryGetValue(connection, out var queue))
                    continue;
                if (!driver.IsCreated || driver.GetConnectionState(connection) != NetworkConnection.State.Connected)
                {
                    var hadPendingFrames = queue.Frames.Count > 0;
                    Forget(connection);
                    if (hadPendingFrames)
                        failed?.Invoke(connection, "Peer disconnected with undelivered multiplayer messages.");
                    continue;
                }

                var now = NowSeconds;
                if (queue.Frames.Count == 0 || now < queue.RetryAt)
                    continue;
                if (now - queue.Frames.Peek().EnqueuedAt >= QueueTimeoutSeconds)
                {
                    Forget(connection);
                    failed?.Invoke(connection, "Multiplayer send timed out while waiting for the remote peer.");
                    continue;
                }

                var sentBytes = 0;
                var sentFrames = 0;
                while (queue.Frames.Count > 0 && sentFrames < MaximumFramesPerPeerPerTick)
                {
                    var pending = queue.Frames.Peek();
                    if (sentBytes > 0 && sentBytes + pending.Bytes.Length > MaximumBytesPerPeerPerTick)
                        break;

                    var result = TrySend(driver, connection, pending.Bytes);
                    if (result == (int)TransportStatusCode.NetworkSendQueueFull)
                    {
                        // Leave room for acknowledgements before retrying a whole fragmented frame.
                        queue.RetryAt = now + queue.RetryDelay;
                        queue.RetryDelay = Math.Min(0.5, queue.RetryDelay * 2);
                        break;
                    }
                    if (result < 0)
                    {
                        Forget(connection);
                        failed?.Invoke(connection, $"Multiplayer transport send failed ({result}).");
                        break;
                    }

                    queue.Frames.Dequeue();
                    _queuedFrames--;
                    _queuedBytes -= pending.Bytes.Length;
                    sentBytes += pending.Bytes.Length;
                    sentFrames++;
                    queue.RetryAt = 0;
                    queue.RetryDelay = 0.05;
                }
            }
        }

        public bool TryReadFrame(NetworkConnection connection, ref DataStreamReader stream, out string error)
        {
            error = null;
            var remaining = stream.Length - stream.GetBytesRead();
            if (remaining < SequenceBytes + 3 || remaining > MaximumFrameBytes + SequenceBytes)
            {
                error = "Invalid multiplayer transport frame size.";
                return false;
            }

            var sequence = (ulong)stream.ReadUInt() | ((ulong)stream.ReadUInt() << 32);
            _receivedSequences.TryGetValue(connection, out var previous);
            if (sequence == 0 || sequence > previous + 1)
            {
                error = "Multiplayer transport sequence is invalid or incomplete.";
                return false;
            }
            if (sequence <= previous)
                return false;

            // EndSend can fail after reliable fragments were buffered. A whole-frame retry then
            // arrives twice; suppress it before any gameplay or handshake callback can run.
            _receivedSequences[connection] = sequence;
            return true;
        }

        public void Forget(NetworkConnection connection)
        {
            if (_sendQueues.TryGetValue(connection, out var queue))
            {
                foreach (var pending in queue.Frames)
                {
                    _queuedFrames--;
                    _queuedBytes -= pending.Bytes.Length;
                }
                _sendQueues.Remove(connection);
            }
            _receivedSequences.Remove(connection);
        }

        public void Clear()
        {
            _sendQueues.Clear();
            _receivedSequences.Clear();
            _queuedFrames = 0;
            _queuedBytes = 0;
        }

        private int TrySend(NetworkDriver driver, NetworkConnection connection, byte[] bytes)
        {
            var result = driver.BeginSend(_pipeline, connection, out var writer, bytes.Length);
            if (result != 0)
                return result;

            using (var buffer = new NativeArray<byte>(bytes, Allocator.Temp))
                writer.WriteBytes(buffer);
            if (writer.HasFailedWrites)
            {
                driver.AbortSend(writer);
                return (int)TransportStatusCode.NetworkPacketOverflow;
            }
            return driver.EndSend(writer);
        }

        private static double NowSeconds => (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
    }
}
