using System.Text.RegularExpressions;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    public static class RelayJoinCodeUtility
    {
        private static readonly Regex RelayJoinCodeRegex = new Regex("^[6789BCDFGHJKLMNPQRTWbcdfghjklmnpqrtw]{6,12}$", RegexOptions.Compiled);

        public static bool IsValid(string joinCode)
        {
            return !string.IsNullOrWhiteSpace(joinCode) && RelayJoinCodeRegex.IsMatch(joinCode.Trim());
        }
    }
}