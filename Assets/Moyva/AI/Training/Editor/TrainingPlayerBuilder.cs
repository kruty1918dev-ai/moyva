using System;
using System.IO;
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
        public static void BuildPlayer(BuildTarget target, string output)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("Install Unity build support for " + target + " in Unity Hub.");
            if (!File.Exists(Scene)) throw new FileNotFoundException("Training scene is missing", Scene);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { Scene }, locationPathName = output, target = target,
                options = BuildOptions.DetailedBuildReport });
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
