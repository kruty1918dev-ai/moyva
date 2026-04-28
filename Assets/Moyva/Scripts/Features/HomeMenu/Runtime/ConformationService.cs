using System;
using System.IO;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class ContinuePanelService : Kruty1918.Moyva.HomeMenu.API.IContinuePanelService, IInitializable, IDisposable
    {
        [Inject] private IContinueViewController _viewController;
        [Inject] private ISaveService _saveService;
        [Inject] private SignalBus _signalBus;

        public void Dispose()
        {
            _signalBus.Unsubscribe<SaveCompletedSignal>(OnSaveCompleted);
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

    internal struct BotDefaultSettings
    {
        public BotDifficulty Difficulty;
        public BotStrategy Strategy;
        public int BotCount;
        public bool AllowBotCheating;
    }

    internal sealed class BotPanelService : IBotPanelService, IInitializable
    {
        [Inject] private IBotViewController _botViewController;
        [InjectOptional] private BotDefaultSettings _defaultSettings;
        [InjectOptional] private Kruty1918.Moyva.HomeMenu.API.ISelectedGameModeService _selectedGameModeService;

        public void Initialize()
        {
            var ds = _defaultSettings;
            if (ds.BotCount <= 0) ds.BotCount = 1; // fallback

            _botViewController.AllowBotCheating = ds.AllowBotCheating;
            _botViewController.BotCount = ds.BotCount;
            _botViewController.Difficulty = ds.Difficulty;
            _botViewController.Strategy = ds.Strategy;
            _botViewController.OnButtonNextClicked += OnNext;
            _botViewController.Refresh();
        }

        private void OnNext()
        {
            _selectedGameModeService.SetSelectedGameMode(Kruty1918.Moyva.HomeMenu.API.GameMode.Bot);
        }
    }

    internal class SelectedGameModeService : Kruty1918.Moyva.HomeMenu.API.ISelectedGameModeService
    {
        public Kruty1918.Moyva.HomeMenu.API.GameMode SelectedGameMode { get; private set; }

        public event Action<Kruty1918.Moyva.HomeMenu.API.GameMode> OnSelectedGameModeChanged;

        public void SetSelectedGameMode(Kruty1918.Moyva.HomeMenu.API.GameMode gameMode)
        {
            SelectedGameMode = gameMode;
            OnSelectedGameModeChanged?.Invoke(gameMode);
        }
    }

    internal class CreateRoomPanelService : ICreateRoomPanelService, IInitializable, IDisposable
    {
        [Inject] private ICreateRoomViewController _viewController;

        public void Dispose()
        {
            _viewController.OnButtonNextClicked -= OnBtnNextClicked;
        }

        public void Initialize()
        {
            _viewController.OnButtonNextClicked += OnBtnNextClicked;
        }

        public void Refresh()
        {

        }

        private void OnBtnNextClicked()
        {

        }
    }

    internal class JoinRoomPanelService : IJoinRoomPanelService, IInitializable, IDisposable
    {
        [Inject] private IJoinRoomViewController _viewController;
        [InjectOptional] private ILobbyService _lobbyService;
        [InjectOptional] private IOverlayLoader _loader;

        private CancellationTokenSource _roomsCts;
        private Action _onListRefreshRequested;
        private Action _onJoinCodeChangedCallback;

        public void Dispose()
        {
            try { if (_onListRefreshRequested != null) _viewController.OnListRoomsRefresh -= _onListRefreshRequested; } catch { }
            try { if (_onJoinCodeChangedCallback != null) _viewController.OnJoinCodeChanged -= _onJoinCodeChangedCallback; } catch { }

            try
            {
                if (_viewController.JoinToRoomButton != null)
                    _viewController.JoinToRoomButton.onClick.RemoveListener(OnJoinClicked);
            }
            catch { }

            _roomsCts?.Cancel();
            _roomsCts?.Dispose();
        }

        public void Initialize()
        {
            if (_viewController.JoinToRoomButton != null)
                _viewController.JoinToRoomButton.onClick.AddListener(OnJoinClicked);

            _onJoinCodeChangedCallback = () => OnJoinCodeChanged(_viewController.JoinCode);
            _viewController.OnJoinCodeChanged += _onJoinCodeChangedCallback;

            _onListRefreshRequested = () => _ = RefreshRoomListAsync();
            _viewController.OnListRoomsRefresh += _onListRefreshRequested;

            Refresh();
        }

        public void Refresh()
        {
            OnJoinCodeChanged(_viewController.JoinCode);
            RefreshRoomList();
        }

        public void RefreshRoomList()
        {
            _ = RefreshRoomListAsync();
        }

        private async Task RefreshRoomListAsync()
        {
            _roomsCts?.Cancel();
            _roomsCts?.Dispose();
            _roomsCts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var ct = _roomsCts.Token;

            var overlay = _loader?.LoadOverlay(0f, 100f, "%");
            overlay?.SetLoading(true, 0f);
            try
            {
                if (_lobbyService == null)
                {
                    Debug.LogWarning("[JoinRoomPanelService] ILobbyService not available; clearing room list.");
                    MainThreadDispatcher.Enqueue(() => _viewController.ClearRoomList());
                    return;
                }

                var rooms = await _lobbyService.QueryRoomsAsync(ct);

                MainThreadDispatcher.Enqueue(() =>
                {
                    _viewController.ClearRoomList();
                    if (rooms != null)
                    {
                        foreach (var r in rooms)
                        {
                            var roomInfo = new Kruty1918.Moyva.HomeMenu.API.RoomInfo
                            {
                                RoomName = r.Name,
                                JoinCode = r.LobbyCode,
                                CurrentPlayers = r.Players?.Count ?? 0,
                                MaxPlayers = r.MaxPlayers
                            };
                            _viewController.AddRoomToList(roomInfo);
                        }
                    }
                });
            }
            catch (OperationCanceledException)
            {
                // request was canceled/timeout
            }
            catch (Exception e)
            {
                Debug.LogError($"[JoinRoomPanelService] RefreshRoomListAsync failed: {e}");
            }
            finally
            {
                float progress = overlay != null ? overlay.Progress : 100f;
                overlay?.SetLoading(false, progress);
                _loader?.StopOverlay(true);
            }
        }

        private void OnJoinClicked()
        {
            // join logic is handled elsewhere; keep placeholder
        }

        private void OnJoinCodeChanged(string code)
        {
            // Встановлюємо реверс значення для interactable, 
            // щоб кнопка була активною лише коли код не порожній
            bool interactable = !string.IsNullOrWhiteSpace(code);
            
            if (_viewController.JoinToRoomButton != null)
                _viewController.JoinToRoomButton.interactable = interactable;
        }
    }

    internal sealed class ConformationService : IConfirmationService, IInitializable
    {
        [Inject] private IConfiremationPanel _panel;
        [Inject] private IConfirmationButton[] _buttons;

        public void Initialize()
        {
            GetPanel().OnConfirme += ForeceHide;
            GetPanel().OnCancled += ForeceHide;

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttons[i].OnClicked += Show;
            }
        }


        public void ForeceHide() => GetPanel().ForeceHide();

        public void Show(ConfirmationRequest request) => GetPanel().Show(request);

        public bool TryGetReqest(out ConfirmationRequest? request) => GetPanel().TryGetReqest(out request);


        private IConfiremationPanel GetPanel()
        {
            if (_panel == null)
            {
                Debug.LogError("[ConformationService]: The confirmed panel was not injected.");
                return null;
            }

            return _panel;
        }

        private IConfirmationButton[] GetButtons()
        {
            if (_buttons == null)
            {
                Debug.LogError("[ConformationService]: The confirmed buttons was not injected");
                return null;
            }

            return _buttons;
        }
    }
}