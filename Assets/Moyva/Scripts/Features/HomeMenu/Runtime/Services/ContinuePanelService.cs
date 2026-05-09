using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class ContinuePanelService : Kruty1918.Moyva.HomeMenu.API.IContinuePanelService, IInitializable, IDisposable
    {
        [Inject] private IContinueViewController _viewController;
        [Inject] private ISaveService _saveService;
        [Inject] private SignalBus _signalBus;
        [Inject] private IGameplaySession _gameplaySession;
        [Inject] private IHomeMenuGameStarter _gameStarter;
        [InjectOptional] private ILocalGameSettingsService _localSettings;
        [InjectOptional] private IInfoPanelService _infoPanelService;

        private bool _isStarting;

        public void Dispose()
        {
            if (_viewController != null)
                _viewController.OnSlotSelected -= OnSlotSelected;

            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<SaveCompletedSignal>(OnSaveCompleted);
            }
        }

        public void Initialize()
        {
            if (_viewController != null)
            {
                _viewController.OnSlotSelected -= OnSlotSelected;
                _viewController.OnSlotSelected += OnSlotSelected;
            }

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
                    SlotName = string.IsNullOrWhiteSpace(info.WorldName) ? slotFileName : info.WorldName,
                    SlotIndex = i,
                    LastModified = chosenUtc.ToLocalTime()
                };

                _viewController.AddSlot(gs);
            }
        }

        private async void OnSlotSelected(GameSlotInfo slot)
        {
            if (_isStarting)
                return;

            int slotIndex = Mathf.Clamp(slot.SlotIndex, 0, 99);
            if (!_saveService.HasSave(slotIndex))
            {
                _infoPanelService?.Show(new InfoMessage("Збереження не знайдено", $"Слот {slotIndex:D2} більше не існує."));
                RefreshSlotsList();
                return;
            }

            _isStarting = true;
            try
            {
                GameLaunchContext.ConfigureMenuLoadGame(slotIndex);
                ApplyOfflineSession(slot.SlotName);
                await _gameStarter.StartGameAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ContinuePanelService] Failed to load slot {slotIndex:D2}: {e}");
                _infoPanelService?.Show(new InfoMessage("Помилка завантаження", e.Message));
            }
            finally
            {
                _isStarting = false;
            }
        }

        private void ApplyOfflineSession(string worldName)
        {
            string localId = "local-player";
            string playerName = string.IsNullOrWhiteSpace(_localSettings?.PlayerName)
                ? "Player"
                : _localSettings.PlayerName;

            var players = new List<GameplayPlayer>
            {
                new GameplayPlayer(localId, playerName, isHost: true, isLocal: true)
            };

            var displayName = string.IsNullOrWhiteSpace(worldName) ? "Збережений світ" : worldName.Trim();
            var worldSettings = new WorldSettingsDto(displayName, 0, (int)WorldSize.Medium, MapType.Continents, Difficulty.Normal, 1, true);
            _gameplaySession.Apply(NetworkProviderType.Offline, worldSettings, players, localId);
        }
    }
}