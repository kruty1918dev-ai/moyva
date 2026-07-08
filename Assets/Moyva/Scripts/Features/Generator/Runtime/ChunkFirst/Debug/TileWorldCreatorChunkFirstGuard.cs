using System;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal static class TileWorldCreatorChunkFirstGuard
    {
        [ThreadStatic] private static int _depth;

        public static bool IsActive => _depth > 0;

        public static IDisposable Enter()
        {
            _depth++;
            return new Scope();
        }

        private sealed class Scope : IDisposable
        {
            public void Dispose()
            {
                if (_depth > 0)
                    _depth--;
            }
        }
    }
}
