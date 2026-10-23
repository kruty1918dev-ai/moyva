using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Kruty1918.Moyva.Editor.Shared
{
    /// <summary>
    /// Автоматична перевірка редакторського оточення після відкриття проєкту.
    /// Виявляє типові кросплатформні збої (Windows ↔ Linux ↔ macOS): відсутні
    /// embedded-пакети, git-залежності без встановленого git, шляхи, що ламаються
    /// на case-sensitive файлових системах, відсутні робочі директорії та
    /// завислі scene-backup, які блокують запуск модальним діалогом.
    /// </summary>
    [InitializeOnLoad]
    internal static class EditorPlatformResolver
    {
        private const string ResolveTriedKey = "Moyva.EditorPlatformResolver.ResolveTried";
        private const string ManifestPath = "Packages/manifest.json";
        private const string ReportPath = "Temp/ai/editor-platform-report.json";

        private static readonly string[] RequiredAssetPaths =
        {
            "Assets/Moyva/Scenes/Gamplay_Scene.unity",
            "Assets/Moyva/Scenes/HomeMenu.unity",
            "Assets/Moyva/Presets",
        };

        static EditorPlatformResolver()
        {
            EditorApplication.delayCall += Run;
        }

        [MenuItem("Moyva/Platform/Run Editor Platform Check")]
        private static void RunFromMenu()
        {
            Run();
        }

        private static void Run()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += Run;
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var errors = new List<string>();
            var warnings = new List<string>();

            Try(warnings, "working directories", EnsureWorkingDirectories);
            Try(warnings, "scene backups", CleanSceneBackups);
            Try(warnings, "manifest", () => CheckManifest(errors, warnings));
            Try(errors, "asset paths", () => CheckRequiredAssetPaths(errors));
            Try(warnings, "serialization mode", () => CheckSerializationMode(warnings));
            Try(errors, "package resolution", () => CheckPackageResolution(warnings, errors));

            WriteReport(errors, warnings);

            foreach (string warning in warnings)
                Debug.LogWarning($"[EditorPlatformResolver] {warning}");
            foreach (string error in errors)
                Debug.LogError($"[EditorPlatformResolver] {error}");
            if (errors.Count == 0 && warnings.Count == 0)
                Debug.Log(
                    $"[EditorPlatformResolver] OK — {Application.platform}, " +
                    $"Unity {Application.unityVersion}.");
        }

        private static void EnsureWorkingDirectories()
        {
            Directory.CreateDirectory("Temp/ai");
            Directory.CreateDirectory("Library/ai");
        }

        // Backup scenes left by an interrupted play-mode exit trigger a
        // "Scene Backup Detected" modal that blocks headless/remote startups.
        private static void CleanSceneBackups()
        {
            if (Directory.Exists("Temp/__Backupscenes"))
                Directory.Delete("Temp/__Backupscenes", true);
        }

        private static void CheckManifest(
            ICollection<string> errors,
            ICollection<string> warnings)
        {
            IDictionary<string, string> dependencies = DeclaredDependencies();

            foreach (KeyValuePair<string, string> dependency in dependencies)
            {
                if (!dependency.Value.StartsWith("file:", StringComparison.Ordinal))
                    continue;

                string packageDir = Path.Combine(
                    "Packages",
                    dependency.Value.Substring("file:".Length));
                if (!File.Exists(Path.Combine(packageDir, "package.json")))
                    errors.Add(
                        $"Embedded package '{dependency.Value.Substring(5)}' is declared " +
                        $"but its folder is missing at '{packageDir}' — it was probably " +
                        "not cloned or committed.");
            }

            if (dependencies.Values.Any(IsGitDependency) && !GitAvailable())
                warnings.Add(
                    "manifest.json declares git-based packages but 'git' is not on " +
                    "PATH — package resolution will fail until git is installed.");
        }

        private static void CheckRequiredAssetPaths(ICollection<string> errors)
        {
            foreach (string path in RequiredAssetPaths)
            {
                if (File.Exists(path) || Directory.Exists(path))
                    continue;
                errors.Add(
                    $"Required asset path '{path}' is missing — on case-sensitive file " +
                    "systems (Linux) check the path spelling matches the repository exactly.");
            }
        }

        private static void CheckSerializationMode(ICollection<string> warnings)
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
                warnings.Add(
                    "EditorSettings.serializationMode is not ForceText; serialized " +
                    "assets do not transfer cleanly between platforms.");
        }

        private static void CheckPackageResolution(
            ICollection<string> warnings,
            ICollection<string> errors)
        {
            PackageInfo[] resolved = PackageInfo.GetAllRegisteredPackages();
            var resolvedNames = new HashSet<string>(
                resolved.Select(package => package.name), StringComparer.Ordinal);

            var missing = new List<string>();
            foreach (KeyValuePair<string, string> dependency in DeclaredDependencies())
            {
                if (dependency.Key.StartsWith("com.unity.modules.", StringComparison.Ordinal))
                    continue;

                string expected = dependency.Value.StartsWith("file:", StringComparison.Ordinal)
                    ? EmbeddedPackageName(dependency.Value) ?? dependency.Value.Substring(5)
                    : dependency.Key;

                if (!resolvedNames.Contains(expected))
                    missing.Add(expected);
            }

            if (missing.Count == 0)
                return;

            if (!SessionState.GetBool(ResolveTriedKey, false))
            {
                SessionState.SetBool(ResolveTriedKey, true);
                Client.Resolve();
                warnings.Add(
                    $"Unresolved packages ({string.Join(", ", missing)}) — " +
                    "requested a manifest re-resolve.");
                return;
            }

            errors.Add(
                $"Packages unresolved after re-resolve: {string.Join(", ", missing)} " +
                "— check Window > Package Manager.");
        }

        private static IDictionary<string, string> DeclaredDependencies()
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!File.Exists(ManifestPath))
                return result;

            Match body = Regex.Match(
                File.ReadAllText(ManifestPath),
                "\"dependencies\"\\s*:\\s*\\{(?<deps>.*?)\\}",
                RegexOptions.Singleline);
            if (!body.Success)
                return result;

            foreach (Match pair in Regex.Matches(
                         body.Groups["deps"].Value,
                         "\"(?<name>[^\"]+)\"\\s*:\\s*\"(?<value>[^\"]*)\""))
                result[pair.Groups["name"].Value] = pair.Groups["value"].Value;
            return result;
        }

        private static string EmbeddedPackageName(string spec)
        {
            string packageJson = Path.Combine(
                "Packages",
                spec.Substring("file:".Length),
                "package.json");
            if (!File.Exists(packageJson))
                return null;

            Match match = Regex.Match(
                File.ReadAllText(packageJson),
                "\"name\"\\s*:\\s*\"(?<name>[^\"]+)\"");
            return match.Success ? match.Groups["name"].Value : null;
        }

        private static bool IsGitDependency(string spec)
            => spec.IndexOf(".git", StringComparison.OrdinalIgnoreCase) >= 0
               || spec.StartsWith("git@", StringComparison.Ordinal)
               || spec.StartsWith("git+", StringComparison.Ordinal);

        private static bool GitAvailable()
        {
            try
            {
                var info = new ProcessStartInfo("git", "--version")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };
                using (Process process = Process.Start(info))
                    return process != null
                           && process.WaitForExit(5000)
                           && process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private static void WriteReport(
            IReadOnlyList<string> errors,
            IReadOnlyList<string> warnings)
        {
            try
            {
                string parent = Path.GetDirectoryName(ReportPath);
                if (!string.IsNullOrEmpty(parent))
                    Directory.CreateDirectory(parent);

                var json = new StringBuilder();
                json.Append("{\n");
                json.Append($"  \"platform\": \"{Application.platform}\",\n");
                json.Append($"  \"unityVersion\": \"{Application.unityVersion}\",\n");
                json.Append($"  \"checkedUtc\": \"{DateTime.UtcNow:O}\",\n");
                json.Append($"  \"errors\": [");
                json.Append(string.Join(", ", errors.Select(JsonLiteral)));
                json.Append("],\n");
                json.Append($"  \"warnings\": [");
                json.Append(string.Join(", ", warnings.Select(JsonLiteral)));
                json.Append("]\n}\n");
                File.WriteAllText(ReportPath, json.ToString());
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[EditorPlatformResolver] Report write failed: {exception.Message}");
            }
        }

        private static string JsonLiteral(string value)
            => "\"" + value
                   .Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("\n", "\\n")
               + "\"";

        private static void Try(ICollection<string> sink, string label, Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                sink.Add($"{label}: {exception.Message}");
            }
        }
    }
}
