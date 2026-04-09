using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Спільна логіка для конвеєрів збереження/завантаження даних.
    /// Використовується SaveService та ConfigService для уникнення дублювання коду.
    /// Містить: збирання блоків, верифікацію буфера, атомарний запис,
    /// завантаження з декодуванням, утиліти файлової системи.
    /// </summary>
    internal static class SavePipelineHelper
    {
        /// <summary>Максимальний розмір одного блоку даних (10 МБ).</summary>
        internal const int MaxBlockBytes = 10 * 1024 * 1024;

        /// <summary>Максимальна кількість слотів збереження.</summary>
        internal const int MaxSlots = 99;

        /// <summary>
        /// Збирає бінарні блоки з кожного ISaveModule.
        /// Пропускає null-модулі, порожні та завеликі блоки з логуванням.
        /// </summary>
        internal static List<(uint blockId, byte[] payload)> CollectBlocks(
            IReadOnlyList<ISaveModule> modules)
        {
            var blocks = new List<(uint, byte[])>(modules.Count);

            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module == null)
                {
                    Debug.LogError("[SaveSystem] Null ISaveModule instance — skipped.");
                    continue;
                }

                uint blockId = SaveFileCodec.ComputeBlockId(module.GetType());

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);

                try
                {
                    module.OnSave(new SaveContext(bw, null));
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"[SaveSystem] '{module.GetType().FullName}' OnSave threw: {e.Message}. Block skipped.");
                    continue;
                }

                bw.Flush();
                byte[] payload = ms.ToArray();

                if (payload.Length == 0)
                {
                    Debug.LogWarning(
                        $"[SaveSystem] '{module.GetType().FullName}' wrote 0 bytes. Block skipped.");
                    continue;
                }

                if (payload.Length > MaxBlockBytes)
                {
                    Debug.LogError(
                        $"[SaveSystem] '{module.GetType().FullName}' payload {payload.Length}b " +
                        $"> {MaxBlockBytes}b limit. Block skipped.");
                    continue;
                }

                blocks.Add((blockId, payload));
            }

            return blocks;
        }

        /// <summary>
        /// Перевіряє мінімальний розмір та magic-байти зібраного буфера.
        /// </summary>
        internal static bool VerifyAssembledBuffer(byte[] data)
        {
            if (data.Length < SaveFileCodec.FileLayout.MinFileSize)
            {
                Debug.LogError(
                    $"[SaveSystem] Assembled buffer {data.Length}b < min {SaveFileCodec.FileLayout.MinFileSize}b.");
                return false;
            }

            var magic = SaveFileCodec.FileLayout.Magic;
            if (data[0] != magic[0] || data[1] != magic[1] ||
                data[2] != magic[2] || data[3] != magic[3])
            {
                Debug.LogError("[SaveSystem] Header magic corrupted in assembled buffer.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Атомарний запис даних у файл: tmp → verify → backup → rename.
        /// Повертає true при успішному записі.
        /// </summary>
        internal static bool AtomicWrite(string finalPath, byte[] data)
        {
            string tmp    = finalPath + ".tmp";
            string backup = finalPath + ".bak";

            // Запис у .tmp
            try { File.WriteAllBytes(tmp, data); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Write to .tmp failed: {e.Message}");
                return false;
            }

            // Верифікація .tmp
            try
            {
                using var fs = File.OpenRead(tmp);
                if (fs.Length != data.Length)
                    throw new IOException($"Size mismatch: expected {data.Length}, got {fs.Length}");

                var magicBuf = new byte[4];
                _ = fs.Read(magicBuf, 0, 4);
                var magic = SaveFileCodec.FileLayout.Magic;
                if (magicBuf[0] != magic[0] || magicBuf[1] != magic[1] ||
                    magicBuf[2] != magic[2] || magicBuf[3] != magic[3])
                    throw new IOException("Magic bytes invalid in .tmp");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] .tmp verification failed: {e.Message}");
                TryDelete(tmp);
                return false;
            }

            // Бекап існуючого файлу
            if (File.Exists(finalPath))
            {
                try { File.Copy(finalPath, backup, overwrite: true); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveSystem] Backup failed: {e.Message}");
                }
            }

            // Атомарне переміщення: .tmp → final
            try
            {
                if (File.Exists(finalPath))
                    File.Delete(finalPath);
                File.Move(tmp, finalPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Atomic rename failed: {e.Message}");
                TryRestoreBackup(backup, finalPath);
                return false;
            }
        }

        /// <summary>
        /// Декодує байти та розподіляє блоки по відповідних ISaveModule.
        /// Повертає true при успішному завантаженні.
        /// </summary>
        internal static bool ExecuteLoad(byte[] bytes, IReadOnlyList<ISaveModule> modules,
            string contextLabel)
        {
            var result = SaveFileCodec.TryDecode(
                bytes, out _, out var decodedBlocks, out string error);

            if (result != SaveFileCodec.DecodeError.None)
            {
                Debug.LogWarning($"[SaveSystem] Decode failed ({contextLabel}): {error}");
                return false;
            }

            var map = new Dictionary<uint, ISaveModule>();
            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module == null) continue;
                uint id = SaveFileCodec.ComputeBlockId(module.GetType());
                map[id] = module;
            }

            foreach (var (blockId, payload) in decodedBlocks)
            {
                if (!map.TryGetValue(blockId, out var module))
                {
                    Debug.LogWarning(
                        $"[SaveSystem] Unknown blockId={blockId:X8} ({payload.Length}b). " +
                        $"Module removed or newer format. Skipped.");
                    continue;
                }

                try
                {
                    using var ms = new MemoryStream(payload);
                    using var br = new BinaryReader(ms);
                    module.OnLoad(new SaveContext(null, br));

                    long unread = ms.Length - ms.Position;
                    if (unread > 0)
                        Debug.LogWarning(
                            $"[SaveSystem] '{module.GetType().FullName}' left {unread}b unread. " +
                            $"Data version mismatch?");
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"[SaveSystem] '{module.GetType().FullName}' OnLoad threw: " +
                        $"{e.GetType().Name} — {e.Message}");
                }
            }

            return true;
        }

        /// <summary>
        /// Перевіряє валідність номера слоту (0–99).
        /// </summary>
        internal static bool ValidateSlot(int slot)
        {
            if (slot >= 0 && slot <= MaxSlots) return true;
            Debug.LogWarning($"[SaveSystem] Invalid slot={slot}. Must be 0–{MaxSlots}.");
            return false;
        }

        /// <summary>
        /// Забезпечує існування директорії збережень.
        /// </summary>
        internal static bool EnsureDirectoryExists(out string error)
        {
            try
            {
                string dir = GetDirectory();
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                error = null;
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;
                return false;
            }
        }

        /// <summary>Шлях до директорії збережень.</summary>
        internal static string GetDirectory()
            => Path.Combine(Application.persistentDataPath, "saves");

        /// <summary>Безпечне видалення файлу з логуванням.</summary>
        internal static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Delete failed '{path}': {e.Message}");
            }
        }

        /// <summary>Відновлення з бекапу після невдалого запису.</summary>
        internal static void TryRestoreBackup(string backup, string final)
        {
            if (!File.Exists(backup)) return;
            try { File.Copy(backup, final, overwrite: true); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Backup restore failed: {e.Message}");
            }
        }
    }
}
