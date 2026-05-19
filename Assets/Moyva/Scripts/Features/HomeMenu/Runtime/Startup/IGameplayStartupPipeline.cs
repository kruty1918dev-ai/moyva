using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Startup
{
    /// <summary>
    /// Контракт пайплайна запуску gameplay-сцени з HomeMenu.
    /// Залежності: реалізується GameplayStartupPipeline і викликається HomeMenuGameStarter.
    /// </summary>
    internal interface IGameplayStartupPipeline
    {
        /// <summary>Поточна фаза виконання пайплайна.</summary>
        GameplayStartupPhase CurrentPhase { get; }

        /// <summary>Запустити повний пайплайн переходу до gameplay-сцени.</summary>
        Task RunAsync(CancellationToken ct = default);
    }
}
