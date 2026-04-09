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
    /// Делегує спільну логіку конвеєра до SavePipelineHelper.
    /// </summary>
    internal sealed class ConfigService : IConfigService
    {
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

            if (!SavePipelineHelper.EnsureDirectoryExists(out string dirError))
            {
                Debug.LogError($"[SaveSystem] Directory error: {dirError}");
                return;
            }

            var blocks = SavePipelineHelper.CollectBlocks(modules);
            byte[] data = SaveFileCodec.Encode(blocks);

            if (!SavePipelineHelper.VerifyAssembledBuffer(data))
            {
                Debug.LogError("[SaveSystem] Config buffer verification failed");
                return;
            }

            SavePipelineHelper.AtomicWrite(GetConfigPath(), data);
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

            SavePipelineHelper.ExecuteLoad(bytes, modules, "config");
        }

        public bool HasConfig()
            => File.Exists(GetConfigPath());

        public void DeleteConfig()
            => SavePipelineHelper.TryDelete(GetConfigPath());

        public SaveSlotInfo GetConfigInfo()
        {
            string path = GetConfigPath();
            if (!File.Exists(path))
                return new SaveSlotInfo(0, false, 0, DateTime.MinValue);

            var fi = new FileInfo(path);
            return new SaveSlotInfo(0, true, fi.Length, fi.LastWriteTimeUtc);
        }

        // ─── Helpers ───────────────────────────────────────────────────────

        private static string GetConfigPath()
            => Path.Combine(SavePipelineHelper.GetDirectory(), "config.mvs");
    }
}
