#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kruty1918.Moyva.EditorTools
{
    /// <summary>
    /// Command-line Android APK build entry point.
    /// unity build . --target Android --execute-method Kruty1918.Moyva.EditorTools.AndroidPlayerBuild.BuildApk -o Build/Android/Moyva.apk
    /// </summary>
    public static class AndroidPlayerBuild
    {
        private const string DefaultOutputPath = "Build/Android/Moyva.apk";

        public static void BuildApk()
        {
            string outputPath = ResolveOutputPath();
            ConfigurePlayerSettings();

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                {
                    Fail("SwitchActiveBuildTarget(Android) failed — is Android Build Support installed?");
                    return;
                }
            }

            string[] scenes = CollectEnabledScenes();
            if (scenes.Length == 0)
            {
                Fail("No enabled scenes in EditorBuildSettings.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.Android, BuildOptions.None);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AndroidBuild] APK built: {outputPath} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime:g})");
                EditorApplication.Exit(0);
            }
            else
            {
                Fail($"[AndroidBuild] Build failed: {summary.result}, errors={summary.totalErrors}");
            }
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.optimizedFramePacing = true;
            PlayerSettings.Android.useCustomKeystore = false;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        }

        private static string[] CollectEnabledScenes()
        {
            var scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && !string.IsNullOrEmpty(scene.path))
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }

        private static string ResolveOutputPath()
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-buildOutput" || args[i] == "-o")
                    return Path.GetFullPath(args[i + 1]);
            }
            return Path.GetFullPath(DefaultOutputPath);
        }

        private static void Fail(string message)
        {
            Debug.LogError(message);
            EditorApplication.Exit(1);
        }
    }
}
#endif
