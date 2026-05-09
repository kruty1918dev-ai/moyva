using System.IO;

namespace Kruty1918.Moyva.WorldCreation.API
{
    /// <summary>
    /// DTO налаштувань світу, що передається від хоста клієнтам разом з командою StartGame.
    /// Бінарно серіалізується для мережі: сумісний формат із <see cref="ToBytes"/>/<see cref="TryFromBytes"/>.
    /// Поле Size зберігається як int, щоб не утворювати циклічну залежність із HomeMenu.API.
    /// </summary>
    public readonly struct WorldSettingsDto
    {
        public const int CurrentVersion = 1;

        public int Seed { get; }
        public int Size { get; }
        public MapType MapType { get; }
        public Difficulty Difficulty { get; }
        public int MaxPlayers { get; }
        public bool IsPrivate { get; }

        public WorldSettingsDto(int seed, int size, MapType mapType, Difficulty difficulty, int maxPlayers, bool isPrivate)
        {
            Seed = seed;
            Size = size;
            MapType = mapType;
            Difficulty = difficulty;
            MaxPlayers = maxPlayers < 1 ? 1 : maxPlayers;
            IsPrivate = isPrivate;
        }

        public byte[] ToBytes()
        {
            using var ms = new MemoryStream();
            using var w = new BinaryWriter(ms);
            w.Write(CurrentVersion);
            w.Write(Seed);
            w.Write(Size);
            w.Write((int)MapType);
            w.Write((int)Difficulty);
            w.Write(MaxPlayers);
            w.Write(IsPrivate);
            return ms.ToArray();
        }

        public static bool TryFromBytes(byte[] bytes, out WorldSettingsDto dto)
        {
            dto = default;
            if (bytes == null || bytes.Length < 4) return false;
            try
            {
                using var ms = new MemoryStream(bytes);
                using var r = new BinaryReader(ms);
                var version = r.ReadInt32();
                if (version != CurrentVersion) return false;
                var seed = r.ReadInt32();
                var size = r.ReadInt32();
                var mapType = (MapType)r.ReadInt32();
                var diff = (Difficulty)r.ReadInt32();
                var max = r.ReadInt32();
                var priv = r.ReadBoolean();
                dto = new WorldSettingsDto(seed, size, mapType, diff, max, priv);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
