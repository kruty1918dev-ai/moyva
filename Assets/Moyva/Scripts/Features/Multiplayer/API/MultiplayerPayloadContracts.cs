using System;
using System.IO;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public readonly struct VersionedPayloadEnvelope
    {
        public const uint Magic = 0x4D50564C; // MPVL
        public const ushort FormatVersion = 1;

        public readonly ushort SchemaId;
        public readonly ushort SchemaVersion;
        public readonly ushort MinSupportedReaderVersion;
        public readonly string TraceId;
        public readonly byte[] Payload;

        public VersionedPayloadEnvelope(
            ushort schemaId,
            ushort schemaVersion,
            ushort minSupportedReaderVersion,
            string traceId,
            byte[] payload)
        {
            SchemaId = schemaId;
            SchemaVersion = schemaVersion;
            MinSupportedReaderVersion = minSupportedReaderVersion;
            TraceId = traceId ?? string.Empty;
            Payload = payload ?? Array.Empty<byte>();
        }
    }

    public readonly struct PayloadCompatibilityResult
    {
        public readonly bool IsCompatible;
        public readonly string Reason;

        public PayloadCompatibilityResult(bool isCompatible, string reason)
        {
            IsCompatible = isCompatible;
            Reason = reason ?? string.Empty;
        }
    }

    public static class MultiplayerPayloadContracts
    {
        public static byte[] Encode(VersionedPayloadEnvelope envelope)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(VersionedPayloadEnvelope.Magic);
            writer.Write(VersionedPayloadEnvelope.FormatVersion);
            writer.Write(envelope.SchemaId);
            writer.Write(envelope.SchemaVersion);
            writer.Write(envelope.MinSupportedReaderVersion);
            writer.Write(envelope.TraceId ?? string.Empty);
            writer.Write(envelope.Payload.Length);
            if (envelope.Payload.Length > 0)
                writer.Write(envelope.Payload);
            return ms.ToArray();
        }

        public static bool TryDecode(byte[] bytes, out VersionedPayloadEnvelope envelope)
        {
            envelope = default;
            if (bytes == null || bytes.Length < 16)
                return false;

            try
            {
                using var ms = new MemoryStream(bytes);
                using var reader = new BinaryReader(ms);
                var magic = reader.ReadUInt32();
                if (magic != VersionedPayloadEnvelope.Magic)
                    return false;

                var formatVersion = reader.ReadUInt16();
                if (formatVersion != VersionedPayloadEnvelope.FormatVersion)
                    return false;

                var schemaId = reader.ReadUInt16();
                var schemaVersion = reader.ReadUInt16();
                var minSupportedReaderVersion = reader.ReadUInt16();
                var traceId = reader.ReadString();
                var payloadLength = reader.ReadInt32();
                if (payloadLength < 0 || payloadLength > 256 * 1024)
                    return false;

                var payload = payloadLength == 0 ? Array.Empty<byte>() : reader.ReadBytes(payloadLength);
                if (payload.Length != payloadLength)
                    return false;

                envelope = new VersionedPayloadEnvelope(schemaId, schemaVersion, minSupportedReaderVersion, traceId, payload);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static PayloadCompatibilityResult CheckCompatibility(
            VersionedPayloadEnvelope envelope,
            ushort expectedSchemaId,
            ushort readerSchemaVersion,
            ushort minSupportedWriterVersion)
        {
            if (envelope.SchemaId != expectedSchemaId)
                return new PayloadCompatibilityResult(false, $"Schema mismatch: expected={expectedSchemaId}, actual={envelope.SchemaId}.");

            if (readerSchemaVersion < envelope.MinSupportedReaderVersion)
                return new PayloadCompatibilityResult(false, $"Reader too old: reader={readerSchemaVersion}, minReader={envelope.MinSupportedReaderVersion}.");

            if (envelope.SchemaVersion < minSupportedWriterVersion)
                return new PayloadCompatibilityResult(false, $"Writer too old: writer={envelope.SchemaVersion}, minWriter={minSupportedWriterVersion}.");

            return new PayloadCompatibilityResult(true, string.Empty);
        }
    }
}