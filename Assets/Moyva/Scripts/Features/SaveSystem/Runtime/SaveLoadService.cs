using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Kruty1918.Moyva.SaveSystem
{
    internal sealed class SaveLoadService : ISaveLoadService
    {
        public bool TryLoad(int slot, IReadOnlyList<ISaveModule> modules, string requiredBlockModuleFullName, out string errorMessage)
        {
            errorMessage = null;

            if (!SavePipelineHelper.ValidateSlot(slot))
            {
                errorMessage = $"Invalid slot {slot}";
                return false;
            }

            string path = SaveService.GetPath(slot);
            if (!File.Exists(path))
            {
                errorMessage = $"Save file not found: '{path}'";
                return TryLoadBackup(slot, modules, requiredBlockModuleFullName, out _);
            }

            byte[] bytes;
            try
            {
                bytes = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                errorMessage = $"Cannot read file: {e.Message}";
                return TryLoadBackup(slot, modules, requiredBlockModuleFullName, out _);
            }

            if (HasRegisteredModule(modules, requiredBlockModuleFullName) && !ContainsBlock(bytes, requiredBlockModuleFullName))
            {
                errorMessage = $"Slot {slot} has no generated-world block.";
                return TryLoadBackup(slot, modules, requiredBlockModuleFullName, out _);
            }

            if (SavePipelineHelper.ExecuteLoad(bytes, modules, $"slot {slot}"))
                return true;

            errorMessage = $"Decode/execute failed for slot {slot}";
            return TryLoadBackup(slot, modules, requiredBlockModuleFullName, out _);
        }

        private bool TryLoadBackup(int slot, IReadOnlyList<ISaveModule> modules, string requiredBlockModuleFullName, out string errorMessage)
        {
            string backup = SaveService.GetPath(slot) + ".bak";
            if (!File.Exists(backup))
            {
                errorMessage = $"No .bak available for slot {slot}.";
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
                return false;
            }

            if (HasRegisteredModule(modules, requiredBlockModuleFullName) && !ContainsBlock(bytes, requiredBlockModuleFullName))
            {
                errorMessage = $"Backup for slot {slot} has no generated-world block.";
                return false;
            }

            if (!SavePipelineHelper.ExecuteLoad(bytes, modules, $"slot {slot} backup"))
            {
                errorMessage = $"Decode/execute failed for backup of slot {slot}";
                return false;
            }

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
