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
        public const int CurrentVersion = 3;

        public string WorldName { get; }
        public int Seed { get; }
        public int Size { get; }
        public int Width { get; }
        public int Height { get; }
        public MapType MapType { get; }
        public Difficulty Difficulty { get; }
        public int MaxPlayers { get; }
        public bool IsPrivate { get; }

        public WorldSettingsDto(int seed, int size, MapType mapType, Difficulty difficulty, int maxPlayers, bool isPrivate)
            : this("Новий світ", seed, size, mapType, difficulty, maxPlayers, isPrivate)
        {
        }

        public WorldSettingsDto(string worldName, int seed, int size, MapType mapType, Difficulty difficulty, int maxPlayers, bool isPrivate)
            : this(worldName, seed, size, ResolveDefaultSide(size), ResolveDefaultSide(size), mapType, difficulty, maxPlayers, isPrivate)
        {
        }

        public WorldSettingsDto(string worldName, int seed, int size, int width, int height, MapType mapType, Difficulty difficulty, int maxPlayers, bool isPrivate)
        {
            WorldName = string.IsNullOrWhiteSpace(worldName) ? "Новий світ" : worldName.Trim();
            Seed = seed;
            Size = size;
            Width = width > 0 ? width : ResolveDefaultSide(size);
            Height = height > 0 ? height : ResolveDefaultSide(size);
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
            w.Write(WorldName ?? string.Empty);
            w.Write(Width);
            w.Write(Height);
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
                if (version != 1 && version != 2 && version != CurrentVersion) return false;
                var seed = r.ReadInt32();
                var size = r.ReadInt32();
                var mapType = (MapType)r.ReadInt32();
                var diff = (Difficulty)r.ReadInt32();
                var max = r.ReadInt32();
                var priv = r.ReadBoolean();
                var worldName = version >= 2 && ms.Position < ms.Length
                    ? r.ReadString()
                    : "Новий світ";
                int width = ResolveDefaultSide(size);
                int height = width;
                if (version >= 3 && ms.Position < ms.Length)
                {
                    width = r.ReadInt32();
                    height = r.ReadInt32();
                }

                dto = new WorldSettingsDto(worldName, seed, size, width, height, mapType, diff, max, priv);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static int ResolveDefaultSide(int size)
        {
            return size switch
            {
                0 => 32,
                1 => 64,
                2 => 128,
                _ => 64,
            };
        }
    }
}
