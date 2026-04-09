using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.SaveSystem
{
    /// <summary>
    /// Сервіс збереження та завантаження ігрових даних у слотах (slot00–slot99).
    /// Використовує формат .mvs з CRC-верифікацією та атомарним записом.
    /// Делегує спільну логіку конвеєра до SavePipelineHelper.
    /// </summary>
    internal sealed class SaveService : ISaveService, IInitializable, IDisposable
    {
        private readonly List<ISaveModule> _modules;
        private readonly SignalBus         _signalBus;

        public SaveService(List<ISaveModule> modules, SignalBus signalBus)
        {
            _modules   = modules ?? new List<ISaveModule>();
            _signalBus = signalBus;
        }

        // ─── IInitializable / IDisposable ──────────────────────────────────

        public void Initialize()
        {
            _signalBus.Subscribe<SaveRequestedSignal>(OnSaveRequested);
            _signalBus.Subscribe<LoadRequestedSignal>(OnLoadRequested);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<SaveRequestedSignal>(OnSaveRequested);
            _signalBus.Unsubscribe<LoadRequestedSignal>(OnLoadRequested);
        }

        private void OnSaveRequested(SaveRequestedSignal signal) => Save(signal.Slot);
        private void OnLoadRequested(LoadRequestedSignal signal) => Load(signal.Slot);

        // ─── ISaveService ──────────────────────────────────────────────────

        public void Save(int slot = 0)
        {
            if (!SavePipelineHelper.ValidateSlot(slot))
            {
                FireCompleted(slot, false, $"Invalid slot {slot}");
                return;
            }

            if (!SavePipelineHelper.EnsureDirectoryExists(out string dirError))
            {
                Debug.LogError($"[SaveSystem] Directory error: {dirError}");
                FireCompleted(slot, false, dirError);
                return;
            }

            var    blocks = SavePipelineHelper.CollectBlocks(_modules);
            byte[] data   = SaveFileCodec.Encode(blocks);

            if (!SavePipelineHelper.VerifyAssembledBuffer(data))
            {
                FireCompleted(slot, false, "Buffer verification failed");
                return;
            }

            if (SavePipelineHelper.AtomicWrite(GetPath(slot), data))
                FireCompleted(slot, true, null);
            else
                FireCompleted(slot, false, "Write failed");
        }

        public void Load(int slot = 0)
        {
            if (!SavePipelineHelper.ValidateSlot(slot))
                return;

            string path = GetPath(slot);

            if (!File.Exists(path))
            {
                Debug.LogError($"[SaveSystem] Save file not found: '{path}'");
                TryLoadBackup(slot);
                return;
            }

            byte[] bytes;
            try   { bytes = File.ReadAllBytes(path); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Cannot read file: {e.Message}");
                TryLoadBackup(slot);
                return;
            }

            if (!SavePipelineHelper.ExecuteLoad(bytes, _modules, $"slot {slot}"))
            {
                TryLoadBackup(slot);
            }
        }

        public bool HasSave(int slot = 0)
            => SavePipelineHelper.ValidateSlot(slot) && File.Exists(GetPath(slot));

        public void Delete(int slot = 0)
        {
            if (!SavePipelineHelper.ValidateSlot(slot)) return;
            SavePipelineHelper.TryDelete(GetPath(slot));
            SavePipelineHelper.TryDelete(GetPath(slot) + ".bak");
            SavePipelineHelper.TryDelete(GetPath(slot) + ".tmp");
        }

        public SaveSlotInfo GetSlotInfo(int slot = 0)
        {
            if (!SavePipelineHelper.ValidateSlot(slot))
                return new SaveSlotInfo(slot, false, 0, DateTime.MinValue);

            string path = GetPath(slot);
            if (!File.Exists(path))
                return new SaveSlotInfo(slot, false, 0, DateTime.MinValue);

            var fi = new FileInfo(path);
            return new SaveSlotInfo(slot, true, fi.Length, fi.LastWriteTimeUtc);
        }

        // ─── Fallback ─────────────────────────────────────────────────────

        private void TryLoadBackup(int slot)
        {
            string backup = GetPath(slot) + ".bak";
            if (!File.Exists(backup))
            {
                Debug.LogWarning($"[SaveSystem] No .bak available for slot {slot}. Load failed.");
                return;
            }

            Debug.LogWarning($"[SaveSystem] Falling back to .bak for slot {slot}.");
            byte[] bytes;
            try   { bytes = File.ReadAllBytes(backup); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Backup unreadable: {e.Message}");
                return;
            }

            SavePipelineHelper.ExecuteLoad(bytes, _modules, $"slot {slot} backup");
        }

        // ─── Helpers ──────────────────────────────────────────────────────

        internal static string GetPath(int slot)
            => Path.Combine(SavePipelineHelper.GetDirectory(), $"slot{slot:D2}.mvs");

        private void FireCompleted(int slot, bool success, string errorMessage)
        {
            _signalBus.Fire(new SaveCompletedSignal
            {
                Slot         = slot,
                Success      = success,
                ErrorMessage = errorMessage
            });
        }
    }
}
