using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.SaveSystem
{
    internal sealed class SaveService : ISaveService, IInitializable, IDisposable
    {
        private const int MaxSlots     = 99;
        private const int MaxBlockBytes = 10 * 1024 * 1024; // 10 MB per block

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
            if (!ValidateSlot(slot))
            {
                FireCompleted(slot, false, $"Invalid slot {slot}");
                return;
            }

            if (!EnsureDirectoryExists(out string dirError))
            {
                Debug.LogError($"[SaveSystem] Directory error: {dirError}");
                FireCompleted(slot, false, dirError);
                return;
            }

            var    blocks = CollectBlocks();
            byte[] data   = SaveFileCodec.Encode(blocks);

            if (!VerifyAssembledBuffer(data))
            {
                FireCompleted(slot, false, "Buffer verification failed");
                return;
            }

            if (AtomicWrite(slot, data))
                FireCompleted(slot, true, null);
            else
                FireCompleted(slot, false, "Write failed");
        }

        public void Load(int slot = 0)
        {
            if (!ValidateSlot(slot))
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

            ExecuteLoad(bytes, slot);
        }

        public bool HasSave(int slot = 0)
            => ValidateSlot(slot) && File.Exists(GetPath(slot));

        public void Delete(int slot = 0)
        {
            if (!ValidateSlot(slot)) return;
            TryDelete(GetPath(slot));
            TryDelete(GetPath(slot) + ".bak");
            TryDelete(GetPath(slot) + ".tmp");
        }

        public SaveSlotInfo GetSlotInfo(int slot = 0)
        {
            if (!ValidateSlot(slot))
                return new SaveSlotInfo(slot, false, 0, DateTime.MinValue);

            string path = GetPath(slot);
            if (!File.Exists(path))
                return new SaveSlotInfo(slot, false, 0, DateTime.MinValue);

            var fi = new FileInfo(path);
            return new SaveSlotInfo(slot, true, fi.Length, fi.LastWriteTimeUtc);
        }

        // ─── Internal — write pipeline ────────────────────────────────────

        private List<(uint blockId, byte[] payload)> CollectBlocks()
        {
            var blocks = new List<(uint, byte[])>(_modules.Count);

            foreach (var module in _modules)
            {
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

        private bool AtomicWrite(int slot, byte[] data)
        {
            string final  = GetPath(slot);
            string tmp    = final + ".tmp";
            string backup = final + ".bak";

            // Write to .tmp
            try   { File.WriteAllBytes(tmp, data); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Write to .tmp failed: {e.Message}");
                return false;
            }

            // Verify .tmp
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

            // Backup existing save
            if (File.Exists(final))
            {
                try   { File.Copy(final, backup, overwrite: true); }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveSystem] Backup failed: {e.Message}");
                }
            }

            // Atomic move: .tmp → final
            try
            {
                if (File.Exists(final))
                    File.Delete(final);
                File.Move(tmp, final);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Atomic rename failed: {e.Message}");
                TryRestoreBackup(backup, final);
                return false;
            }
        }

        // ─── Internal — load pipeline ────────────────────────────────────

        private void ExecuteLoad(byte[] bytes, int slot)
        {
            var result = SaveFileCodec.TryDecode(
                bytes, out _, out var decodedBlocks, out string error);

            if (result != SaveFileCodec.DecodeError.None)
            {
                Debug.LogWarning($"[SaveSystem] Decode failed for slot {slot}: {error}");
                if (result == SaveFileCodec.DecodeError.CrcMismatch ||
                    result == SaveFileCodec.DecodeError.BadMagic)
                    TryLoadBackup(slot);
                return;
            }

            // Build blockId → module lookup
            var map = new Dictionary<uint, ISaveModule>();
            foreach (var module in _modules)
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
                        $"[SaveSystem] Unknown blockId={blockId:X8} ({payload.Length}b). " +
                        $"Module removed or newer save format. Skipped.");
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
        }

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

            ExecuteLoad(bytes, slot);
        }

        // ─── Helpers ──────────────────────────────────────────────────────

        private static bool ValidateSlot(int slot)
        {
            if (slot >= 0 && slot <= MaxSlots) return true;
            Debug.LogWarning($"[SaveSystem] Invalid slot={slot}. Must be 0\u2013{MaxSlots}.");
            return false;
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

        internal static string GetPath(int slot)
            => Path.Combine(
                Path.Combine(Application.persistentDataPath, "saves"),
                $"slot{slot:D2}.mvs");

        private void FireCompleted(int slot, bool success, string errorMessage)
        {
            _signalBus.Fire(new SaveCompletedSignal
            {
                Slot         = slot,
                Success      = success,
                ErrorMessage = errorMessage
            });
        }

        private static void TryDelete(string path)
        {
            try   { if (File.Exists(path)) File.Delete(path); }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Delete failed '{path}': {e.Message}");
            }
        }

        private static void TryRestoreBackup(string backup, string final)
        {
            if (!File.Exists(backup)) return;
            try   { File.Copy(backup, final, overwrite: true); }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Backup restore failed: {e.Message}");
            }
        }
    }
}
