namespace Kruty1918.Moyva.HomeMenu.Runtime.Startup
{
    /// <summary>
    /// Етапи startup-пайплайна перед входом у gameplay-сцену.
    /// Залежності: використовується GameplayStartupPipeline та HomeMenuGameStarter для відстеження прогресу.
    /// </summary>
    internal enum GameplayStartupPhase
    {
        /// <summary>Пайплайн ще не стартував.</summary>
        None = 0,

        /// <summary>Попереднє завантаження ресурсів і сцени.</summary>
        Preload = 1,

        /// <summary>Фаза прив'язки runtime-контексту та fallback-налаштувань.</summary>
        Bind = 2,

        /// <summary>Прогрів допоміжних систем перед активацією сцени.</summary>
        Warmup = 3,

        /// <summary>Активація завантаженої сцени.</summary>
        SceneActivate = 4,

        /// <summary>Пайплайн повністю завершений.</summary>
        Completed = 5
    }
}
