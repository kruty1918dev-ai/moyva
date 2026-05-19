using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Signals;
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
        private const string GeneratedWorldSaveModuleFullName = "Kruty1918.Moyva.Generator.Runtime.GeneratedWorldSaveModule";

        private readonly List<ISaveModule> _modules;
        private readonly SignalBus _signalBus;
        private readonly ISaveWriteService _writeService;
        private readonly ISaveLoadService _loadService;
        private readonly ISaveSlotPolicyService _slotPolicyService;
        private readonly ISaveModuleRegistry _moduleRegistry;

        public SaveService(
            List<ISaveModule> modules,
            SignalBus signalBus,
            [InjectOptional] ISaveWriteService writeService = null,
            [InjectOptional] ISaveLoadService loadService = null,
            [InjectOptional] ISaveSlotPolicyService slotPolicyService = null,
            [InjectOptional] ISaveModuleRegistry moduleRegistry = null)
        {
            _modules = modules ?? new List<ISaveModule>();
            _signalBus = signalBus;
            _writeService = writeService ?? new SaveWriteService();
            _loadService = loadService ?? new SaveLoadService();
            _slotPolicyService = slotPolicyService ?? new SaveSlotPolicyService();
            _moduleRegistry = moduleRegistry;
        }

        // ─── IInitializable / IDisposable ──────────────────────────────────

        public void Initialize()
        {
            _signalBus.Subscribe<SaveRequestedSignal>(OnSaveRequested);
            _signalBus.Subscribe<LoadRequestedSignal>(OnLoadRequested);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<SaveRequestedSignal>(OnSaveRequested);
            _signalBus.TryUnsubscribe<LoadRequestedSignal>(OnLoadRequested);
        }

        private void OnSaveRequested(SaveRequestedSignal signal) => Save(signal.Slot);
        private void OnLoadRequested(LoadRequestedSignal signal) => Load(signal.Slot);

        // ─── ISaveService ──────────────────────────────────────────────────

        public void Save(int slot = 0)
        {
            var modules = GetCurrentModulesSnapshot();
            if (_writeService.TrySave(slot, modules, GeneratedWorldSaveModuleFullName, out var errorMessage))
                FireCompleted(slot, true, null);
            else
                FireCompleted(slot, false, errorMessage);
        }

        public void Load(int slot = 0)
        {
            var modules = GetCurrentModulesSnapshot();
            _loadService.TryLoad(slot, modules, GeneratedWorldSaveModuleFullName, out _);
        }

        public bool HasSave(int slot = 0)
            => _slotPolicyService.HasSave(slot);

        public void Delete(int slot = 0)
        {
            _slotPolicyService.Delete(slot);
        }

        public SaveSlotInfo GetSlotInfo(int slot = 0)
        {
            return _slotPolicyService.GetSlotInfo(slot);
        }

        // ─── Helpers ──────────────────────────────────────────────────────

        internal static string GetPath(int slot)
            => Path.Combine(SavePipelineHelper.GetDirectory(), $"slot{slot:D2}.mvs");

        private List<ISaveModule> GetCurrentModulesSnapshot()
        {
            var modules = new List<ISaveModule>(_modules.Count);
            for (int index = 0; index < _modules.Count; index++)
            {
                var module = _modules[index];
                if (module == null || modules.Contains(module))
                    continue;

                modules.Add(module);
            }

            _moduleRegistry?.AppendRegisteredModules(modules);
            return modules;
        }

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
