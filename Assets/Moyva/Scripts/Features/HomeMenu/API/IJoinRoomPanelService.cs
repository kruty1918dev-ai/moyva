using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт сервісу панелі приєднання до кімнати.
    /// Залежності: спирається на multiplayer provider type, room list та UI-панель JoinRoom.
    /// </summary>
    public interface IJoinRoomPanelService
    {
        /// <summary>Назва панелі, з якої востаннє відкривали join flow.</summary>
        string LastJoinPanelName { get; }

        /// <summary>Тип мережевого провайдера, з яким пов'язаний останній join flow.</summary>
        NetworkProviderType LastJoinProviderType { get; }

        /// <summary>Підготувати панель до відкриття, наприклад синхронізувати режим і попередні залежності.</summary>
        Task<bool> PrepareForOpenAsync(CancellationToken ct = default);

        /// <summary>Оновити список доступних кімнат.</summary>
        Task<bool> RefreshRoomListAsync(CancellationToken ct = default);
    }
}