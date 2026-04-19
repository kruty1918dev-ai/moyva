using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    internal static class BoolMaskMathUtility
    {
        internal static bool ValidatePair(bool[,] a, bool[,] b, out int w, out int h, out string error)
        {
            w = 0;
            h = 0;
            error = null;

            if (a == null || b == null)
            {
                error = "Обидві маски (A і B) мають бути підключені.";
                return false;
            }

            int aw = a.GetLength(0);
            int ah = a.GetLength(1);
            int bw = b.GetLength(0);
            int bh = b.GetLength(1);

            if (aw != bw || ah != bh)
            {
                error = $"Розміри масок не збігаються: A={aw}x{ah}, B={bw}x{bh}.";
                return false;
            }

            w = aw;
            h = ah;
            return true;
        }

        internal static NodeOutput MissingSourceError(string sourceName = "Source") =>
            NodeOutput.Error($"{sourceName} input is required.");
    }
}
