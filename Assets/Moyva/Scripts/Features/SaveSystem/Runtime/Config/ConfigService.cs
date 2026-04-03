using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Сервіс для збереження глобального конфігу (config.mvs).
    /// Використовує той самий формат .mvs та модульну архітектуру як SaveService,
    /// але працює з одним файлом замість слотів.
    /// </summary>
    internal sealed class ConfigService : IConfigService
    {
        private const int MaxBlockBytes = 10 * 1024 * 1024; // 10 MB per block

        // ─── IInitializable / IDisposable ──────────────────────────────────

        public void Initialize()
        {
            // Конфіг не потребує signal subscriptions на відміну від SaveService
        }

        public void Dispose()
        {
            // Нічого з очищення
        }

        // ─── IConfigService ────────────────────────────────────────────────

        public void SaveConfig(List<ISaveModule> modules)
        {
            if (modules == null || modules.Count == 0)
            {
                Debug.LogWarning("[SaveSystem] SaveConfig: no modules provided.");
                return;
            }

            if (!EnsureDirectoryExists(out string dirError))
            {
                Debug.LogError($"[SaveSystem] Directory error: {dirError}");
                return;
            }

            var blocks = CollectBlocks(modules);
            byte[] data = SaveFileCodec.Encode(blocks);

            if (!VerifyAssembledBuffer(data))
            {
                Debug.LogError("[SaveSystem] Config buffer verification failed");
                return;
            }

            AtomicWrite(data);
        }

        public void LoadConfig(List<ISaveModule> modules)
        {
            if (modules == null || modules.Count == 0)
            {
                Debug.LogWarning("[SaveSystem] LoadConfig: no modules provided.");
                return;
            }

            if (!HasConfig())
            {
                Debug.LogWarning("[SaveSystem] Config file not found.");
                return;
            }

            string path = GetConfigPath();
            byte[] bytes;

            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Cannot read config file: {e.Message}");
                return;
            }

            ExecuteLoad(bytes, modules);
        }

        public bool HasConfig()
            => File.Exists(GetConfigPath());

        public void DeleteConfig()
            => TryDelete(GetConfigPath());

        public SaveSlotInfo GetConfigInfo()
        {
            string path = GetConfigPath();
            if (!File.Exists(path))
                return new SaveSlotInfo(0, false, 0, DateTime.MinValue);

            var fi = new FileInfo(path);
            return new SaveSlotInfo(0, true, fi.Length, fi.LastWriteTimeUtc);
        }

        // ─── Internal ──────────────────────────────────────────────────────

        private List<(uint blockId, byte[] payload)> CollectBlocks(List<ISaveModule> modules)
        {
            var blocks = new List<(uint, byte[])>(modules.Count);

            foreach (var module in modules)
            {
                if (module == null)
                {
                    Debug.LogWarning("[SaveSystem] Null ISaveModule instance — skipped.");
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

        private static bool VerifyAssembledBuffer(byte[] data)
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

        private static bool AtomicWrite(byte[] data)
        {
            string final = GetConfigPath();
            string tmp = final + ".tmp";
            string backup = final + ".bak";

            try { File.WriteAllBytes(tmp, data); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Write to .tmp failed: {e.Message}");
                return false;
            }

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

            if (File.Exists(final))
            {
                try { File.Copy(final, backup, overwrite: true); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveSystem] Config backup failed: {e.Message}");
                }
            }

            try
            {
                if (File.Exists(final))
                    File.Delete(final);
                File.Move(tmp, final);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Config rename failed: {e.Message}");
                return false;
            }
        }

        private static void ExecuteLoad(byte[] bytes, List<ISaveModule> modules)
        {
            var result = SaveFileCodec.TryDecode(
                bytes, out _, out var decodedBlocks, out string error);

            if (result != SaveFileCodec.DecodeError.None)
            {
                Debug.LogWarning($"[SaveSystem] Config decode failed: {error}");
                return;
            }

            var map = new Dictionary<uint, ISaveModule>();
            foreach (var module in modules)
            {
                if (module == null) continue;
                uint id = SaveFileCodec.ComputeBlockId(module.GetType());
                map[id] = module;
            }

            foreach (var (blockId, payload) in decodedBlocks)
            {
                if (!map.TryGetValue(blockId, out var module))
                {
                    Debug.LogWarning(
                        $"[SaveSystem] Unknown blockId={blockId:X8} ({payload.Length}b). Skipped.");
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
                            $"[SaveSystem] '{module.GetType().FullName}' left {unread}b unread.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"[SaveSystem] '{module.GetType().FullName}' OnLoad threw: " +
                        $"{e.GetType().Name} — {e.Message}");
                }
            }
        }

        private static bool EnsureDirectoryExists(out string error)
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

        private static string GetDirectory()
            => Path.Combine(Application.persistentDataPath, "saves");

        private static string GetConfigPath()
            => Path.Combine(
                Path.Combine(Application.persistentDataPath, "saves"),
                "config.mvs");

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Delete failed '{path}': {e.Message}");
            }
        }
    }
}
