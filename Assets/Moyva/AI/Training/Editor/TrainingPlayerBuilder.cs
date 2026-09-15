using System;
using System.IO;
using System.Linq;
using System.Threading;
using Kruty1918.Moyva.AI.Bot;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training.Editor
{
    public static class TrainingPlayerBuilder
    {
        public const string Scene = "Assets/Moyva/AI/Training/Scenes/MoyvaTraining.unity";
        public static void Build()
        {
            var args = Environment.GetCommandLineArgs();
            string targetName = Value(args, "-moyvaBuildTarget", "StandaloneLinux64");
            var target = (BuildTarget)Enum.Parse(typeof(BuildTarget), targetName);
            string output = Value(args, "-moyvaBuildOutput", "Build/Training/MoyvaTraining.x86_64");
            BuildPlayer(target, output);
        }
        public static void ValidateScope()
        {
            var config = TrainingConfig.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"));
            var container = new Zenject.DiContainer();
            new TrainingInstaller().Install(container, config);
            var factory = container.Resolve<ITrainingSimulationFactory>();
            using var simulation = factory.Create(0);
            using var environment = new TrainingEnvironment(0, config, simulation);
            for (int episode = 0; episode < 2; episode++)
            {
                environment.BeginEpisode();
                var report = environment.CheckReadiness(factory);
                if (!report.IsReady) throw new InvalidOperationException(report.ToString());
                long turn = simulation.Turns.GlobalTurn;
                var frame = environment.Bridge.Frame;
                int slot = -1;
                for (int i = 0; i < frame.Candidates.Count; i++)
                    if (frame.Candidates[i].Intent == BotIntentType.EndTurn) { slot = i; break; }
                if (slot < 0 || !environment.Step(slot)) throw new InvalidOperationException("No legal EndTurn.");
                for (int i = 0; i < 20; i++) environment.Tick(0.02f);
                if (simulation.Turns.GlobalTurn <= turn + 1) throw new InvalidOperationException("Learner/opponent turns did not both progress.");
                environment.EndEpisode(TrainingEpisodeResult.Timeout);
            }
            Debug.Log("MOYVA_SCOPE_SANITY_OK: two worlds, EndTurn, opponent, reset, real observations/candidates.");
        }
        public static void ValidateFullGameScope()
        {
            var config = TrainingConfig.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Moyva/Presets/AI/MoyvaTrainingConfig.json"));
            config.curriculum.stage = TrainingCurriculumStage.FullGame;
            var container = new Zenject.DiContainer();
            new TrainingInstaller().Install(container, config);
            var factory = container.Resolve<ITrainingSimulationFactory>();
            using var simulation = factory.Create(0);
            using var environment = new TrainingEnvironment(0, config, simulation);
            for (int episode = 0; episode < 2; episode++)
            {
                environment.BeginEpisode();
                if (!environment.IsReady) throw new InvalidOperationException(environment.Diagnostics.LastError);
                AdvanceLegalSetupIfNeeded(environment, simulation);
                var report = TrainingReadinessValidator.Validate(factory, simulation, environment);
                Debug.Log(report.ToString());
                if (!report.IsReady) throw new InvalidOperationException(report.ToString());
                var source = (ITrainingBotRuntimeSource)simulation;
                var frame = environment.Bridge.Frame;
                if (frame == null || frame.Candidates.Count < 2 || frame.Observations[BotObservationSchema.EconomyAvailable] != 1)
                    throw new InvalidOperationException("FullGame is missing real actions or economy.");
                foreach (var id in new[] { BotCapabilityId.Turn, BotCapabilityId.Movement, BotCapabilityId.Combat,
                    BotCapabilityId.Recruitment, BotCapabilityId.Construction, BotCapabilityId.Capture })
                {
                    var capability = source.Capabilities.Get(id);
                    if (capability == null || capability.UnavailableReason(simulation.PlayerId) != null)
                        throw new InvalidOperationException("FullGame capability unavailable: " + id);
                    int count = 0;
                    foreach (var action in capability.Enumerate(simulation.PlayerId))
                    {
                        if (!capability.Validate(simulation.PlayerId, action, out string reason))
                            throw new InvalidOperationException("Invalid enumerated candidate: " + reason);
                        count++;
                    }
                    Debug.Log($"MOYVA_FULLGAME_CANDIDATES episode={episode + 1} capability={id} legal={count}");
                }
                var recruitment = source.Capabilities.Get(BotCapabilityId.Recruitment);
                BotCandidateAction recruit = null;
                for (int round = 0; round < 12 && recruit == null; round++)
                {
                    recruit = recruitment.Enumerate(simulation.PlayerId).FirstOrDefault(c => c.ActorKey == "enqueue");
                    if (recruit == null) AdvanceRound(simulation);
                }
                if (recruit == null) throw new InvalidOperationException("Recruitment never became legal through real construction/turn progression.");
                float resourcesBeforeQuery = source.Perception.Capture(simulation.PlayerId).Global[24];
                var firstQuery = recruitment.Enumerate(simulation.PlayerId).Select(c => c.Id).ToArray();
                var secondQuery = recruitment.Enumerate(simulation.PlayerId).Select(c => c.Id).ToArray();
                if (!firstQuery.SequenceEqual(secondQuery)
                    || source.Perception.Capture(simulation.PlayerId).Global[24] != resourcesBeforeQuery)
                    throw new InvalidOperationException("Recruitment enumeration changed gameplay state or ordering.");
                if (recruitment.Execute(simulation.PlayerId, recruit, CancellationToken.None).GetAwaiter().GetResult().Status != BotExecutionStatus.Completed)
                    throw new InvalidOperationException("Authoritative recruitment enqueue failed.");
                BotCandidateAction deploy = null;
                for (int round = 0; round < 12 && deploy == null; round++)
                {
                    AdvanceRound(simulation);
                    deploy = recruitment.Enumerate(simulation.PlayerId).FirstOrDefault(c => c.ActorKey.StartsWith("queue:", StringComparison.Ordinal));
                }
                if (deploy == null || recruitment.Execute(simulation.PlayerId, deploy, CancellationToken.None).GetAwaiter().GetResult().Status != BotExecutionStatus.Completed
                    || recruitment.Validate(simulation.PlayerId, deploy, out _))
                    throw new InvalidOperationException("Recruitment deployment failed or accepted a stale queue action.");
                Debug.Log("MOYVA_FULLGAME_RECRUITMENT_OK: paid enqueue, real turn progress, deployment, stale rejection.");
                long turn = simulation.Turns.GlobalTurn;
                if (!simulation.Turns.TryEndTurn(simulation.PlayerId, out var turnReason)
                    || !simulation.Turns.TryEndTurn(simulation.Turns.ActiveOwnerId, out turnReason)
                    || simulation.Turns.GlobalTurn != turn + 2)
                    throw new InvalidOperationException("FullGame turn participants failed: " + turnReason);
                environment.EndEpisode(TrainingEpisodeResult.Timeout);
            }
            Debug.Log("MOYVA_FULLGAME_SCOPE_SANITY_OK: two owned worlds, economy, legal queries and turn participants. Complete FullGame readiness required.");
        }
        private static void AdvanceRound(ITrainingSimulation simulation)
        {
            for (int i = 0; i < 2; i++)
                if (!simulation.Turns.TryEndTurn(simulation.Turns.ActiveOwnerId, out var reason))
                    throw new InvalidOperationException("Turn progression failed: " + reason);
        }

        private static void AdvanceLegalSetupIfNeeded(TrainingEnvironment environment, ITrainingSimulation simulation)
        {
            var frame = environment.Bridge.Frame;
            if (frame == null || frame.Candidates.Count != 1 || frame.Candidates[0].Intent != BotIntentType.EndTurn)
                return;

            if (!environment.Step(0))
                throw new InvalidOperationException("Legal setup EndTurn was not accepted.");

            for (int i = 0; i < 60 && environment.LastCandidate == null; i++)
                environment.Tick(0.02f);

            if (!environment.IsReady)
                throw new InvalidOperationException(environment.Diagnostics.LastError ?? "Legal setup ended the episode.");

            if (!string.Equals(simulation.Turns.ActiveOwnerId, simulation.PlayerId, StringComparison.Ordinal)
                && !simulation.Turns.TryEndTurn(simulation.Turns.ActiveOwnerId, out var reason))
                throw new InvalidOperationException("Opponent legal setup turn failed: " + reason);

            environment.Tick(0.02f);

            Debug.Log("MOYVA_FULLGAME_LEGAL_SETUP_OK: initial legal-only frame advanced through EndTurn.");
        }

        public static void BuildPlayer(BuildTarget target, string output)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("Install Unity build support for " + target + " in Unity Hub.");
            if (!File.Exists(Scene)) throw new FileNotFoundException("Training scene is missing", Scene);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            // MOYVA_TRAINING_PREVIEW_WINDOW_SETTINGS
            // Training preview must behave like a normal desktop window: resizable/minimizable
            // and still running when the Control Center has focus. Runtime launch arguments decide
            // the actual monitor size; Control Center moves/maximizes it after startup.
            bool oldResizableWindow = PlayerSettings.resizableWindow;
            bool oldRunInBackground = PlayerSettings.runInBackground;
            bool oldAllowFullscreenSwitch = PlayerSettings.allowFullscreenSwitch;
            var oldFullScreenMode = PlayerSettings.fullScreenMode;
            int oldDefaultWidth = PlayerSettings.defaultScreenWidth;
            int oldDefaultHeight = PlayerSettings.defaultScreenHeight;
            BuildReport result;
            try
            {
                PlayerSettings.resizableWindow = true;
                PlayerSettings.runInBackground = true;
                PlayerSettings.allowFullscreenSwitch = true;
                PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
                PlayerSettings.defaultScreenWidth = 1280;
                PlayerSettings.defaultScreenHeight = 720;
                result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[] { Scene }, locationPathName = output, target = target,
                    options = BuildOptions.DetailedBuildReport });
            }
            finally
            {
                PlayerSettings.resizableWindow = oldResizableWindow;
                PlayerSettings.runInBackground = oldRunInBackground;
                PlayerSettings.allowFullscreenSwitch = oldAllowFullscreenSwitch;
                PlayerSettings.fullScreenMode = oldFullScreenMode;
                PlayerSettings.defaultScreenWidth = oldDefaultWidth;
                PlayerSettings.defaultScreenHeight = oldDefaultHeight;
            }
            if (result.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Training player build failed: " + result.summary.result + "; errors=" + result.summary.totalErrors);
            File.WriteAllText(output + ".contract.json", JsonUtility.ToJson(new ContractManifest(), true));
            Debug.Log("MOYVA_TRAINING_BUILD_OK " + output);
        }
        private static string Value(string[] args, string flag, string fallback)
        {
            int index = Array.IndexOf(args, flag);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
        }
        [Serializable] private sealed class ContractManifest
        {
            public string behavior = MoyvaStrategyAgent.BehaviorName;
            public int version = BotDecisionContract.ContractVersion;
            public string hash = BotDecisionContract.Hash;
            public int observations = BotDecisionContract.ObservationCount;
            public int candidateSlots = BotDecisionContract.MaxCandidateSlots;
        }
    }
}
