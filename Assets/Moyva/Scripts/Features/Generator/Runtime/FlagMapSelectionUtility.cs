using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class FlagMapSelectionUtility
    {
        internal static HashSet<string> BuildFilterSet(string[] flagIds)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (flagIds == null)
                return result;

            for (int i = 0; i < flagIds.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(flagIds[i]))
                    result.Add(flagIds[i].Trim());
            }

            return result;
        }

        internal static bool IsSelected(string[,] flagMap, int x, int y, HashSet<string> filterSet)
        {
            if (flagMap == null)
                return false;

            string value = flagMap[x, y];
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return filterSet == null || filterSet.Count == 0 || filterSet.Contains(value.Trim());
        }

        internal static bool IsConnected(
            string[,] flagMap,
            int ax,
            int ay,
            int bx,
            int by,
            HashSet<string> filterSet,
            bool requireSameFlag)
        {
            if (!IsSelected(flagMap, ax, ay, filterSet) || !IsSelected(flagMap, bx, by, filterSet))
                return false;

            if (!requireSameFlag)
                return true;

            return string.Equals(flagMap[ax, ay], flagMap[bx, by], StringComparison.OrdinalIgnoreCase);
        }
    }
}