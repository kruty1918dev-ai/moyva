using System;
using System.IO;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class ContinuePanelService : Kruty1918.Moyva.HomeMenu.API.IContinuePanelService, IInitializable, IDisposable
    {
        [Inject] private IContinueViewController _viewController;
        [Inject] private ISaveService _saveService;
        [Inject] private SignalBus _signalBus;

        public void Dispose()
        {
            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<SaveCompletedSignal>(OnSaveCompleted);
            }
        }

        public void Initialize()
        {
            _signalBus.Subscribe<SaveCompletedSignal>(OnSaveCompleted);
            RefreshSlotsList();
        }

        private void OnSaveCompleted(SaveCompletedSignal _)
        {
            RefreshSlotsList();
        }

        private void RefreshSlotsList()
        {
            _viewController.ClearSlots();

            for (int i = 0; i <= 99; i++)
            {
                var info = _saveService.GetSlotInfo(i);
                if (!info.Exists) continue;

                string slotFileName = $"slot{i:D2}.mvs";
                string slotPath = Path.Combine(Application.persistentDataPath, "saves", slotFileName);

                DateTime lastWriteUtc = DateTime.MinValue;
                DateTime creationUtc = DateTime.MinValue;
                try
                {
                    if (File.Exists(slotPath))
                    {
                        lastWriteUtc = File.GetLastWriteTimeUtc(slotPath);
                        creationUtc = File.GetCreationTimeUtc(slotPath);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ContinuePanelService] Failed reading file times for {slotPath}: {e.Message}");
                }

                // Choose last modification time; if missing, fallback to creation time; if both missing, use info.LastWriteTimeUtc or now.
                DateTime chosenUtc = lastWriteUtc != DateTime.MinValue
                    ? lastWriteUtc
                    : (creationUtc != DateTime.MinValue ? creationUtc : info.LastWriteTimeUtc);

                if (chosenUtc == DateTime.MinValue)
                    chosenUtc = DateTime.UtcNow;

                var gs = new GameSlotInfo
                {
                    SlotName = slotFileName,
                    SlotIndex = i,
                    LastModified = chosenUtc.ToLocalTime()
                };

                _viewController.AddSlot(gs);
            }
        }
    }
}