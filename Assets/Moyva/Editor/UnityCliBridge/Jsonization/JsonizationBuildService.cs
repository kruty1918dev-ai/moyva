using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kruty1918.Moyva.Jsonization.Editor
{
    public static class JsonizationBuildService
    {
        public static string DevelopmentPlayerSmoke(string reportPath, string outputDirectory)
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled && !string.IsNullOrWhiteSpace(s.path))
                .Select(s => s.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                string gameplay = "Assets/Moyva/Scenes/Gamplay_Scene.unity";
                if (File.Exists(gameplay)) scenes = new[] { gameplay };
            }
            if (scenes.Length == 0)
                throw new InvalidOperationException("No buildable scenes found.");

            Directory.CreateDirectory(outputDirectory);
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
            string location = ResolveOutput(outputDirectory, target);
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                target = target,
                targetGroup = group,
                locationPathName = location,
                options = BuildOptions.Development
            };

            BuildReport buildReport = BuildPipeline.BuildPlayer(options);
            var summary = buildReport.summary;
            var report = new
            {
                result = summary.result.ToString(),
                totalErrors = summary.totalErrors,
                totalWarnings = summary.totalWarnings,
                totalSize = summary.totalSize,
                totalTimeSeconds = summary.totalTime.TotalSeconds,
                target = target.ToString(),
                location,
                scenes
            };
            JsonizationEditorUtil.WriteJson(reportPath, report);
            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Player build failed: {summary.result}, errors={summary.totalErrors}");
            return $"MOYVA_JSON_PLAYER_BUILD PASS target={target} size={summary.totalSize}";
        }

        private static string ResolveOutput(string root, BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return Path.Combine(root, "MoyvaJsonSmoke.exe");
                case BuildTarget.StandaloneOSX:
                    return Path.Combine(root, "MoyvaJsonSmoke.app");
                case BuildTarget.Android:
                    return Path.Combine(root, "MoyvaJsonSmoke.apk");
                default:
                    return Path.Combine(root, "MoyvaJsonSmoke");
            }
        }
    }
}
