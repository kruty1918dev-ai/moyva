using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Architecture.Tests
{
    /// <summary>
    /// Architecture guard: `Container.Resolve`/`TryResolve` is a composition-root
    /// privilege. Runtime code must receive dependencies through constructor
    /// injection. Service-locator lookups are allowed only inside installer /
    /// *Bindings composition files and the explicitly allow-listed legacy or
    /// boundary call sites below, each with a hard cap so existing debt cannot
    /// grow. New call sites fail this test — inject the dependency instead.
    /// </summary>
    public sealed class DiContainerResolveGuardTests
    {
        private static readonly Regex CompositionRootFileName = new Regex(
            @"(Installer|Installers|Bindings|BindingGroups|CompositionRoot)\.cs$",
            RegexOptions.Compiled);

        private static readonly Regex ResolveCall = new Regex(
            @"([A-Za-z_][A-Za-z0-9_.]*)\s*\.\s*(?:TryResolve|ResolveAll|Resolve)\s*[<(]",
            RegexOptions.Compiled);

        private static readonly HashSet<string> SkippedDirectories = new HashSet<string>(
            StringComparer.Ordinal)
        {
            "Tests", "Editor", "EditorShared", "Development",
        };

        /// <summary>
        /// Grandfathered non-composition-root call sites. Value is the maximum
        /// allowed number of resolve calls in that file. Reduce the cap when
        /// shrinking debt; remove the entry when a file is clean.
        /// </summary>
        private static readonly Dictionary<string, int> Allowance =
            new Dictionary<string, int>(StringComparer.Ordinal)
            {
                // Boot scene boundary: optional project services during scene bring-up.
                ["Assets/Moyva/Scripts/Bootstrap/Runtime/Boot/BootSceneController.cs"] = 2,
                // Owned-map boundary for headless/editor world builds.
                ["Assets/Moyva/Scripts/Features/Generator/Runtime/OwnedGameplayMap.cs"] = 1,
                // Marketing studio boundary: optional gameplay services.
                ["Assets/Moyva/Scripts/Features/Marketing/Runtime/MarketingGameplayContext.cs"] = 1,
                // Headless training composition root: builds and owns its container.
                ["Assets/Moyva/AI/Training/Runtime/Integration/GameplayTrainingEpisode.cs"] = 38,
                // Training bootstrap: creates the headless episode container.
                ["Assets/Moyva/AI/Training/Runtime/Environment/TrainingBootstrap.cs"] = 1,
                // Training scenario assembly: receives the episode-owned container.
                ["Assets/Moyva/AI/Training/Runtime/Integration/TrainingScenarioScaffolder.cs"] = 16,
            };

        [Test]
        public void NoContainerResolveOutsideCompositionRoots()
        {
            var violations = new List<string>();
            var moyvaRoot = Path.Combine(Application.dataPath, "Moyva");

            foreach (var file in EnumerateProductionSources(moyvaRoot))
            {
                var relativePath = NormalizePath(Path.GetRelativePath(
                    Directory.GetParent(Application.dataPath).FullName, file));

                int hits = CountResolveCalls(file);
                if (hits == 0)
                    continue;

                if (CompositionRootFileName.IsMatch(Path.GetFileName(file)))
                    continue;

                if (Allowance.TryGetValue(relativePath, out int allowed))
                {
                    if (hits > allowed)
                    {
                        violations.Add(
                            $"{relativePath}: {hits} resolve call(s), cap is {allowed} — "
                            + "do not grow service-locator debt; use constructor injection");
                    }
                    continue;
                }

                violations.Add(
                    $"{relativePath}: {hits} resolve call(s) — Container.Resolve outside a "
                    + "composition root is forbidden; use constructor injection");
            }

            Assert.IsEmpty(violations,
                "Container.Resolve/TryResolve outside composition roots:\n"
                + string.Join("\n", violations));
        }

        [Test]
        public void AllowlistedResolveFilesStillExist()
        {
            var missing = new List<string>();
            foreach (var path in Allowance.Keys)
            {
                var absolute = Path.Combine(
                    Directory.GetParent(Application.dataPath).FullName,
                    path.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(absolute))
                    missing.Add(path);
            }

            Assert.IsEmpty(missing,
                "Resolve-guard allowance entries point to removed files — drop them: "
                + string.Join(", ", missing));
        }

        private static IEnumerable<string> EnumerateProductionSources(string root)
        {
            if (!Directory.Exists(root))
                yield break;

            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var dir = pending.Pop();
                foreach (var file in Directory.GetFiles(dir, "*.cs"))
                    yield return file;
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    if (!SkippedDirectories.Contains(Path.GetFileName(sub)))
                        pending.Push(sub);
                }
            }
        }

        private static int CountResolveCalls(string file)
        {
            var code = StripCommentsAndStrings(File.ReadAllText(file));
            int count = 0;
            foreach (Match match in ResolveCall.Matches(code))
            {
                if (IsContainerReceiver(match.Groups[1].Value))
                    count++;
            }
            return count;
        }

        private static bool IsContainerReceiver(string receiver)
        {
            var last = receiver.Substring(receiver.LastIndexOf('.') + 1);
            if (last == "Container" || last == "DiContainer")
                return true;
            if (receiver.IndexOf('.') < 0
                && (char.IsLower(receiver[0]) || receiver[0] == '_')
                && (last.EndsWith("Container", StringComparison.Ordinal)
                    || last.EndsWith("container", StringComparison.Ordinal)))
                return true;
            return false;
        }

        private static string NormalizePath(string path)
            => path.Replace('\\', '/');

        private static string StripCommentsAndStrings(string source)
        {
            var builder = new StringBuilder(source.Length);
            bool inLineComment = false, inBlockComment = false;
            bool inString = false, inChar = false, inVerbatim = false;

            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                char next = i + 1 < source.Length ? source[i + 1] : '\0';

                if (inLineComment)
                {
                    if (c == '\n') { inLineComment = false; builder.Append(c); }
                    continue;
                }
                if (inBlockComment)
                {
                    if (c == '*' && next == '/') { inBlockComment = false; i++; }
                    else if (c == '\n') builder.Append(c);
                    continue;
                }
                if (inString)
                {
                    if (inVerbatim)
                    {
                        if (c == '"' && next == '"') { i++; continue; }
                        if (c == '"') inString = inVerbatim = false;
                    }
                    else
                    {
                        if (c == '\\') { i++; continue; }
                        if (c == '"') inString = false;
                    }
                    if (c == '\n') builder.Append(c);
                    continue;
                }
                if (inChar)
                {
                    if (c == '\\') { i++; continue; }
                    if (c == '\'') inChar = false;
                    continue;
                }

                if (c == '/' && next == '/') { inLineComment = true; i++; continue; }
                if (c == '/' && next == '*') { inBlockComment = true; i++; continue; }
                if (c == '@' && next == '"') { inString = inVerbatim = true; i++; continue; }
                if (c == '$' && next == '"') { inString = true; i++; continue; }
                if (c == '"') { inString = true; continue; }
                if (c == '\'') { inChar = true; continue; }

                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
