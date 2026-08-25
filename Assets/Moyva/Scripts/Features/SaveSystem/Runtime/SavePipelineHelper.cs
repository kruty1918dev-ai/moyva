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
        /// Збирає бінарні блоки з кожного ISaveModule у детермінованому
        /// dependency-aware порядку. Дубльована реєстрація одного типу модуля
        /// не створює дубльований blockId у файлі.
        /// </summary>
        internal static List<(uint blockId, byte[] payload)> CollectBlocks(
            IReadOnlyList<ISaveModule> modules)
        {
            List<ISaveModule> orderedModules = SaveModuleExecutionPlan.Build(modules);
            var blocks = new List<(uint, byte[])>(orderedModules.Count);

            for (int i = 0; i < orderedModules.Count; i++)
            {
                var module = orderedModules[i];
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
                catch (Exception)
                {
                    continue;
                }

                bw.Flush();
                byte[] payload = ms.ToArray();

                if (payload.Length == 0)
                {
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
                catch (Exception)
                {
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
        /// Декодує байти та відновлює блоки в поточному dependency-aware порядку
        /// модулів, а не у фізичному порядку блоків у .mvs. Це робить legacy saves
        /// незалежними від історичного порядку Zenject/registrar реєстрації.
        /// </summary>
        internal static bool ExecuteLoad(byte[] bytes, IReadOnlyList<ISaveModule> modules,
            string contextLabel)
        {
            var result = SaveFileCodec.TryDecode(
                bytes, out _, out var decodedBlocks, out string error);

            if (result != SaveFileCodec.DecodeError.None)
            {
                return false;
            }

            var payloadByBlockId = new Dictionary<uint, byte[]>();
            for (int index = 0; index < decodedBlocks.Count; index++)
            {
                var block = decodedBlocks[index];
                if (payloadByBlockId.ContainsKey(block.blockId))
                {
                    return false;
                }

                payloadByBlockId.Add(block.blockId, block.payload);
            }

            List<ISaveModule> orderedModules = SaveModuleExecutionPlan.Build(modules);
            var moduleNamesByBlockId = new Dictionary<uint, string>();

            for (int index = 0; index < orderedModules.Count; index++)
            {
                ISaveModule module = orderedModules[index];
                if (module == null)
                    continue;

                Type moduleType = module.GetType();
                string moduleName = moduleType.FullName ?? moduleType.Name;
                uint blockId = SaveFileCodec.ComputeBlockId(moduleType);

                if (moduleNamesByBlockId.TryGetValue(blockId, out string existingModuleName)
                    && !string.Equals(existingModuleName, moduleName, StringComparison.Ordinal))
                {
                    Debug.LogError(
                        $"[SaveSystem] Save block-id collision {blockId:X8}: " +
                        $"'{existingModuleName}' vs '{moduleName}'. Load rejected.");
                    return false;
                }

                moduleNamesByBlockId[blockId] = moduleName;

                if (!payloadByBlockId.TryGetValue(blockId, out byte[] payload))
                    continue;

                try
                {
                    using var ms = new MemoryStream(payload);
                    using var br = new BinaryReader(ms);
                    module.OnLoad(new SaveContext(null, br));

                    long unread = ms.Length - ms.Position;
                }
                catch (Exception)
                {
                }

                payloadByBlockId.Remove(blockId);
            }

            if (payloadByBlockId.Count > 0)
            {
                var unknownIds = new List<uint>(payloadByBlockId.Keys);
                unknownIds.Sort();
                for (int index = 0; index < unknownIds.Count; index++)
                {
                    uint blockId = unknownIds[index];
                    byte[] payload = payloadByBlockId[blockId];
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
