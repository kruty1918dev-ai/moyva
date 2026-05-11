using System;

namespace Kruty1918.Moyva.Shared.Common
{
    public static class MoyvaId
    {
        public static string NewGuidN()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string NewTraceId(int length = 8)
        {
            if (length <= 0)
                length = 8;

            var raw = Guid.NewGuid().ToString("N").ToUpperInvariant();
            return length >= raw.Length ? raw : raw.Substring(0, length);
        }
    }
}
