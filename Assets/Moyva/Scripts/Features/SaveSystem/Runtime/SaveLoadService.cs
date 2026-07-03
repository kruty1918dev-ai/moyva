using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem
{
    internal sealed class SaveLoadService : ISaveLoadService
    {
        private readonly ISaveLoadDiagnostics _diagnostics;

        public SaveLoadService([Zenject.InjectOptional] ISaveLoadDiagnostics diagnostics = null)
        {
            _diagnostics = diagnostics;
        }

        public bool TryLoad(int slot, IReadOnlyList<ISaveModule> modules, string requiredBlockModuleFullName, IDiagnosticFlow flow, out string errorMessage)
        {
            errorMessage = null;

            if (!SavePipelineHelper.ValidateSlot(slot))
            {
                errorMessage = $"Invalid slot {slot}";
                _diagnostics?.FailStep(flow, SaveLoadDiagnosticSteps.SlotResolved, "invalid-slot", $"slot={slot}");
                return false;
            }

            string path = SaveService.GetPath(slot);
            _diagnostics?.CompleteStep(flow, SaveLoadDiagnosticSteps.FileReadStarted, $"slot={slot}, path={path}");
            if (!File.Exists(path))
            {
                errorMessage = $"Save file not found: '{path}'";
                return TryLoadBackup(slot, modules, requiredBlockModuleFullName, flow, out _);
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                errorMessage = $"Cannot read file: {e.Message}";
                _diagnostics?.FailStep(flow, SaveLoadDiagnosticSteps.FileReadCompleted, "file-read-exception", errorMessage);
                return TryLoadBackup(slot, modules, requiredBlockModuleFullName, flow, out _);
            }

            if (HasRegisteredModule(modules, requiredBlockModuleFullName) && !ContainsBlock(bytes, requiredBlockModuleFullName))
            {
                errorMessage = $"Slot {slot} has no generated-world block.";
                _diagnostics?.FailStep(flow, SaveLoadDiagnosticSteps.SaveDataDeserialized, "missing-generated-world-block", errorMessage);
                return TryLoadBackup(slot, modules, requiredBlockModuleFullName, flow, out _);
            }

            _diagnostics?.CompleteStep(flow, SaveLoadDiagnosticSteps.FileReadCompleted, $"slot={slot}, bytes={bytes.Length}");
            if (SavePipelineHelper.ExecuteLoad(bytes, modules, $"slot {slot}"))
            {
                _diagnostics?.CompleteStep(flow, SaveLoadDiagnosticSteps.SaveDataDeserialized, $"slot={slot}, source=primary");
                return true;
            }

            errorMessage = $"Decode/execute failed for slot {slot}";
            _diagnostics?.FailStep(flow, SaveLoadDiagnosticSteps.SaveDataDeserialized, "decode-execute-failed", errorMessage);
            return TryLoadBackup(slot, modules, requiredBlockModuleFullName, flow, out _);
        }

        private bool TryLoadBackup(int slot, IReadOnlyList<ISaveModule> modules, string requiredBlockModuleFullName, IDiagnosticFlow flow, out string errorMessage)
        {
            string backup = SaveService.GetPath(slot) + ".bak";
            if (!File.Exists(backup))
            {
                errorMessage = $"No .bak available for slot {slot}.";
                _diagnostics?.FailStep(flow, SaveLoadDiagnosticSteps.FileReadCompleted, "backup-missing", errorMessage);
                return false;
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(backup);
            }
            catch (Exception e)
            {
                errorMessage = $"Backup unreadable: {e.Message}";
                _diagnostics?.FailStep(flow, SaveLoadDiagnosticSteps.FileReadCompleted, "backup-read-failed", errorMessage);
                return false;
            }

            if (HasRegisteredModule(modules, requiredBlockModuleFullName) && !ContainsBlock(bytes, requiredBlockModuleFullName))
            {
                errorMessage = $"Backup for slot {slot} has no generated-world block.";
                _diagnostics?.FailStep(flow, SaveLoadDiagnosticSteps.SaveDataDeserialized, "backup-missing-generated-world-block", errorMessage);
                return false;
            }

            if (!SavePipelineHelper.ExecuteLoad(bytes, modules, $"slot {slot} backup"))
            {
                errorMessage = $"Decode/execute failed for backup of slot {slot}";
                _diagnostics?.FailStep(flow, SaveLoadDiagnosticSteps.SaveDataDeserialized, "backup-decode-failed", errorMessage);
                return false;
            }

            _diagnostics?.CompleteStep(flow, SaveLoadDiagnosticSteps.FileReadCompleted, $"slot={slot}, source=backup, bytes={bytes.Length}");
            _diagnostics?.CompleteStep(flow, SaveLoadDiagnosticSteps.SaveDataDeserialized, $"slot={slot}, source=backup");
            errorMessage = null;
            return true;
        }

        private static bool HasRegisteredModule(IReadOnlyList<ISaveModule> modules, string moduleTypeFullName)
        {
            if (modules == null || string.IsNullOrWhiteSpace(moduleTypeFullName))
                return false;

            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i];
                if (module == null)
                    continue;

                if (string.Equals(module.GetType().FullName, moduleTypeFullName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool ContainsBlock(byte[] bytes, string moduleTypeFullName)
        {
            if (bytes == null || string.IsNullOrWhiteSpace(moduleTypeFullName))
                return false;

            var result = SaveFileCodec.TryDecode(bytes, out _, out var blocks, out _);
            if (result != SaveFileCodec.DecodeError.None || blocks == null)
                return false;

            uint blockId = SaveFileCodec.ComputeBlockId(moduleTypeFullName);
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].blockId == blockId)
                    return true;
            }

            return false;
        }
    }
}
