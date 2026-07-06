namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphLogicalTileMapText
    {
        public const string Tag = "[MoyvaGraphFinalMap]";

        public static string Normalize(string value)
        {
            return string.IsNullOrEmpty(value) ? "<empty>" : value;
        }
    }
}
