using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap
{
    internal sealed class DirectGameplayLaunchModeInitializer : IInitializable
    {
        private const string StartupDiagTag = "[MOYVA_DIAG][STARTUP]";

        private readonly IWorldGenerationDiagnostics _worldDiagnostics;
        private readonly string _sceneName;

        public DirectGameplayLaunchModeInitializer(
            string sceneName,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null)
        {
            _sceneName = string.IsNullOrWhiteSpace(sceneName) ? "<unknown>" : sceneName;
            _worldDiagnostics = worldDiagnostics;
        }

        public void Initialize()
        {
            GameLaunchMode requestedMode = GameLaunchContext.Mode;
            GameLaunchSource requestedSource = GameLaunchContext.Source;
            bool expiredBeforeInitialize = GameLaunchContext.IsExpired;
            GameLaunchContext.EnsureNotExpired();

            string reason = "existing-launch-context";
            if (GameLaunchContext.Mode == GameLaunchMode.Unknown)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLaunchContext.ConfigureDirectGameplayTest();
                reason = "debug-direct-gameplay-fallback";
#else
                reason = "missing-launch-context";
#endif
            }

            _worldDiagnostics?.DirectLaunchConfigured(
                $"mode={GameLaunchContext.Mode}, source={GameLaunchContext.Source}, " +
                $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, reason={reason}");
            LogStartupDecision(requestedMode, requestedSource, expiredBeforeInitialize, reason);
        }

        private void LogStartupDecision(
            GameLaunchMode requestedMode,
            GameLaunchSource requestedSource,
            bool expiredBeforeInitialize,
            string reason)
        {
            GameLaunchMode resolvedMode = GameLaunchContext.Mode;
            string details =
                $"scene={_sceneName} requestedMode={requestedMode} resolvedMode={resolvedMode} " +
                $"requestedSource={requestedSource} source={GameLaunchContext.Source} expiredBefore={expiredBeforeInitialize} " +
                $"worldSettings={GameLaunchContext.HasWorldSettings} maxPlayers={GameLaunchContext.MaxPlayers} " +
                $"autoLoad={GameLaunchContext.IsAutoLoadEnabled()} saveSlot={GameLaunchContext.SaveSlot} reason={reason}";

            if (resolvedMode == GameLaunchMode.Unknown)
            {
                Debug.LogError($"{StartupDiagTag}[CRITICAL] gameplay-start-invalid {details}");
                return;
            }

            Debug.Log($"{StartupDiagTag}[INFO] launch-resolved {details}");
        }
    }
}
