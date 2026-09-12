using System;

namespace Kruty1918.Moyva.AI.Training
{
    // The composition owner must create an initialized, exclusively owned gameplay
    // world. A production SceneContext with live player/bot controllers is not a scope.
    public interface ITrainingGameplayScopeFactory
    {
        TrainingGameplayScope Create(int environmentId);
    }

    public sealed class GameplayTrainingSimulationFactory : ITrainingSimulationFactory
    {
        public const string MissingScope = "GAMEPLAY_SCOPE_BLOCKED: MoyvaTraining has no isolated gameplay composition/reset binding. "
            + "GeneratorInstaller requires scene graph/TWC assets and startup; BootstrapInstaller also installs HUD, input, saves and multiplayer. "
            + "Bind ITrainingGameplayScopeFactory with world, participants, units, movement, fog, opponent progression and a verified episode reset. "
            + "Scaffold fallback is disabled.";
        private readonly ITrainingGameplayScopeFactory _scopes;
        public bool SupportsIndependentEnvironments => false;
        public GameplayTrainingSimulationFactory(ITrainingGameplayScopeFactory scopes = null) { _scopes = scopes; }
        public ITrainingSimulation Create(int environmentId)
        {
            if (environmentId != 0) throw new InvalidOperationException("Only environment 0 is supported; use separate player processes.");
            if (_scopes == null) throw new InvalidOperationException(MissingScope);
            var scope = _scopes.Create(environmentId);
            if (scope == null) throw new InvalidOperationException(MissingScope);
            try { return new GameplayTrainingSimulation(scope); }
            catch { scope.Dispose(); throw; }
        }
    }
}
