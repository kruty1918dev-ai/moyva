namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphGenerationLayerLogText
    {
        public static string ValueOrFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        public static string FormatGeneratedCells(int count)
        {
            return count < 0 ? "<unknown>" : count.ToString();
        }
    }
}
