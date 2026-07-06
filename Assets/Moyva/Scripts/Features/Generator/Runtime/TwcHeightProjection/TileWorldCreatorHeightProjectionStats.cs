namespace Kruty1918.Moyva.Generator.Runtime
{
    internal struct TileWorldCreatorHeightProjectionStats
    {
        public int Changed;
        public int Unchanged;
        public int Clamped;
        public int MinLevel;
        public int MaxLevel;
        private bool _hasLevel;

        public void RegisterLevel(int level)
        {
            if (!_hasLevel)
            {
                MinLevel = level;
                MaxLevel = level;
                _hasLevel = true;
                return;
            }

            if (level < MinLevel)
                MinLevel = level;
            if (level > MaxLevel)
                MaxLevel = level;
        }
    }
}
