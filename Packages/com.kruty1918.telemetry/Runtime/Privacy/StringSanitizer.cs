using System;
using System.Text.RegularExpressions;

namespace Kruty1918.Telemetry.Privacy
{
    /// <summary>
    /// Defense-in-depth PII scrubbing for string field values. Telemetry producers
    /// should never emit PII in the first place; this catches accidents.
    /// Replaces e-mail-shaped and secret-shaped strings with stable redaction markers.
    /// </summary>
    public static class StringSanitizer
    {
        private static readonly Regex Email =
            new Regex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);
        private static readonly Regex Bearerish =
            new Regex(@"(?i)(bearer|token|secret|apikey|api_key|password|passwd|pwd)\s*[:=]\s*\S+",
                RegexOptions.Compiled);
        private static readonly Regex LongDigitRun =
            new Regex(@"\b\d{13,19}\b", RegexOptions.Compiled); // card-like

        public const int MaxStringLength = 512;

        /// <summary>Sanitize a value; returns redacted string or null if it should be dropped.</summary>
        public static string Sanitize(string value)
        {
            if (value == null) return null;
            if (value.Length > MaxStringLength) value = value.Substring(0, MaxStringLength);
            value = Email.Replace(value, "[email]");
            value = Bearerish.Replace(value, "[redacted]");
            value = LongDigitRun.Replace(value, "[digits]");
            return value;
        }

        /// <summary>True if the string looks like it carries sensitive content even after scrub.</summary>
        public static bool LooksSensitive(string key)
        {
            if (key == null) return false;
            var k = key.ToLowerInvariant();
            return k.Contains("password") || k.Contains("secret") || k.Contains("token")
                || k.Contains("email") || k.Contains("credential") || k.Contains("apikey");
        }
    }
}
