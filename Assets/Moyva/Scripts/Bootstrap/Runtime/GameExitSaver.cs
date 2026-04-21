using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Автоматично зберігає гру (слот 0) при виході з додатку.
    /// Підписується на Application.quitting — не потребує MonoBehaviour.
    /// </summary>
    internal sealed class GameExitSaver : IInitializable, System.IDisposable
    {
        private readonly ISaveService _saveService;

        public GameExitSaver(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public void Initialize()
        {
            Application.quitting += OnApplicationQuitting;
        }

        public void Dispose()
        {
            Application.quitting -= OnApplicationQuitting;
        }

        private void OnApplicationQuitting()
        {
            if (!GameLaunchContext.IsAutoSaveEnabled())
            {
                Debug.Log("[GameExitSaver] Auto Save вимкнено — збереження при виході пропущено.");
                return;
            }

            int slot = GameLaunchContext.SaveSlot;
            Debug.Log("[GameExitSaver] Автозбереження при виході...");
            _saveService.Save(slot);
            Debug.Log("[GameExitSaver] Автозбереження завершено.");
        }
    }
}
