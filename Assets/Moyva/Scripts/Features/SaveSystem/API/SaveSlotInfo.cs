using System;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Метадані слоту збереження (read-only snapshot).
    /// </summary>
    public sealed class SaveSlotInfo
    {
        public int      Slot             { get; }
        public bool     Exists           { get; }
        public long     FileSizeBytes    { get; }
        public DateTime LastWriteTimeUtc { get; }

        public SaveSlotInfo(int slot, bool exists, long fileSizeBytes, DateTime lastWriteTimeUtc)
        {
            Slot             = slot;
            Exists           = exists;
            FileSizeBytes    = fileSizeBytes;
            LastWriteTimeUtc = lastWriteTimeUtc;
        }
    }
}
