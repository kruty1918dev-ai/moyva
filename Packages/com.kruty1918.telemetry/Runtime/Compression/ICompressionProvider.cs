using System.IO;
using System.IO.Compression;

namespace Kruty1918.Telemetry.Compression
{
    /// <summary>Pluggable payload compression. "id" goes on the wire so backends can decode.</summary>
    public interface ICompressionProvider
    {
        /// <summary>Stable wire id: "none", "gzip".</summary>
        string Id { get; }
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data, int maxOutputBytes);
    }

    public sealed class NoCompressionProvider : ICompressionProvider
    {
        public string Id => "none";
        public byte[] Compress(byte[] data) => data;
        public byte[] Decompress(byte[] data, int maxOutputBytes)
        {
            if (data.Length > maxOutputBytes) throw new InvalidDataException("size-limit");
            return data;
        }
    }

    /// <summary>
    /// GZip via System.IO.Compression — available on every Unity target incl. IL2CPP/Android,
    /// no native plugins. Decompression is size-capped (decompression bomb protection).
    /// </summary>
    public sealed class GZipCompressionProvider : ICompressionProvider
    {
        public string Id => "gzip";

        public byte[] Compress(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gz = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
                    gz.Write(data, 0, data.Length);
                return ms.ToArray();
            }
        }

        public byte[] Decompress(byte[] data, int maxOutputBytes)
        {
            using (var input = new MemoryStream(data))
            using (var gz = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                var buf = new byte[8192];
                int total = 0;
                while (true)
                {
                    int read = gz.Read(buf, 0, buf.Length);
                    if (read == 0) break;
                    total += read;
                    if (total > maxOutputBytes) throw new InvalidDataException("decompression-size-limit");
                    output.Write(buf, 0, read);
                }
                return output.ToArray();
            }
        }
    }
}
