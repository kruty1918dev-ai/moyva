using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Kruty1918.Telemetry.Serialization
{
    /// <summary>
    /// Binary container for a sealed batch on disk:
    ///   [4B magic 'TMB1'][4B LE headerLen][header JSON UTF8][payload bytes]
    /// The header carries payloadSha256 + payloadBytes so integrity is verified
    /// without trusting file length. Writes go through *.tmp + atomic rename.
    /// </summary>
    public static class BatchFileCodec
    {
        private static readonly byte[] MagicBytes = { (byte)'T', (byte)'M', (byte)'B', (byte)'1' };
        public const int MaxHeaderBytes = 64 * 1024;

        public static string Sha256Hex(byte[] data)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(data);
                var sb = new StringBuilder(64);
                foreach (var b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static byte[] Encode(TelemetryBatch batch)
        {
            byte[] header = Encoding.UTF8.GetBytes(batch.HeaderJson());
            if (header.Length > MaxHeaderBytes) throw new InvalidDataException("header too large");
            using (var ms = new MemoryStream(header.Length + (batch.Payload?.Length ?? 0) + 8))
            {
                ms.Write(MagicBytes, 0, 4);
                ms.Write(BitConverter.GetBytes(header.Length), 0, 4);
                ms.Write(header, 0, header.Length);
                if (batch.Payload != null) ms.Write(batch.Payload, 0, batch.Payload.Length);
                return ms.ToArray();
            }
        }

        /// <summary>Decode and verify. Throws InvalidDataException on any integrity failure.</summary>
        public static TelemetryBatch Decode(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 8) throw new InvalidDataException("too small");
            if (fileBytes[0] != 'T' || fileBytes[1] != 'M' || fileBytes[2] != 'B' || fileBytes[3] != '1')
                throw new InvalidDataException("bad magic");
            int headerLen = BitConverter.ToInt32(fileBytes, 4);
            if (headerLen <= 0 || headerLen > MaxHeaderBytes || 8 + headerLen > fileBytes.Length)
                throw new InvalidDataException("bad header length");
            var header = TelemetryJsonReader.Parse(Encoding.UTF8.GetString(fileBytes, 8, headerLen));
            if (header.GetString("fmt") != TelemetryBatch.Magic) throw new InvalidDataException("bad fmt");
            if (header.GetInt("fmtVer") > TelemetryBatch.FormatVersion) throw new InvalidDataException("future format");
            var batch = TelemetryBatch.FromHeader(header);
            int payloadLen = fileBytes.Length - 8 - headerLen;
            int declared = (int)header.GetInt("payloadBytes");
            if (payloadLen != declared) throw new InvalidDataException("payload length mismatch");
            var payload = new byte[payloadLen];
            Buffer.BlockCopy(fileBytes, 8 + headerLen, payload, 0, payloadLen);
            batch.Payload = payload;
            if (!string.IsNullOrEmpty(batch.PayloadSha256))
            {
                // Checksum covers the *uncompressed* payload; decode before verifying.
                byte[] raw = payload;
                if (batch.CompressionId != "none")
                    raw = new Compression.GZipCompressionProvider().Decompress(payload, 64 * 1024 * 1024);
                if (Sha256Hex(raw) != batch.PayloadSha256)
                    throw new InvalidDataException("payload checksum mismatch");
                if (raw.Length != batch.UncompressedBytes)
                    throw new InvalidDataException("uncompressed length mismatch");
            }
            return batch;
        }

        /// <summary>Atomic write: tmp file, flush, rename. Returns final path.</summary>
        public static string WriteAtomic(string finalPath, byte[] bytes)
        {
            var dir = Path.GetDirectoryName(finalPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            string tmp = finalPath + ".tmp";
            File.WriteAllBytes(tmp, bytes);
            if (File.Exists(finalPath)) File.Delete(finalPath);
            File.Move(tmp, finalPath);
            return finalPath;
        }
    }
}
