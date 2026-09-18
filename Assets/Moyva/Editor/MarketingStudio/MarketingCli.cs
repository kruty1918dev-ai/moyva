using System;
using System.IO;
using Kruty1918.Moyva.Marketing.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.EditorTools
{
    /// <summary>
    /// Headless/batch entry points:
    ///   -executeMethod Kruty1918.Moyva.Marketing.EditorTools.MarketingCli.GenerateRecipe
    ///     -moyvaMarketingRecipe steam-screenshots [-moyvaMarketingOutput D:\out]
    ///   -executeMethod ...MarketingCli.BuildContentIndex
    /// </summary>
    public static class MarketingCli
    {
        public static void BuildContentIndex()
        {
            var snap = MarketingContentIndexBuilder.Build(saveSnapshot: true);
            Debug.Log($"[MarketingStudio] Content index built: {snap.entries.Count} entries, " +
                $"{snap.musicKeys.Count} music, {snap.sfxKeys.Count} sfx, fp={snap.fingerprint}");
            if (Application.isBatchMode)
                EditorApplication.Exit(0);
        }

        public static void GenerateRecipe()
        {
            string recipeId = Arg("-moyvaMarketingRecipe") ?? "steam-screenshots";
            string output = Arg("-moyvaMarketingOutput");
            string language = Arg("-moyvaMarketingLanguage");

            var snap = MarketingContentIndexBuilder.Build(saveSnapshot: true);
            Debug.Log($"[MarketingStudio] CLI run: recipe={recipeId}, index={snap.entries.Count} entries");

            MarketingRunMonitor.StartRun(new MarketingStudioSession.RunRequest
            {
                recipeId = recipeId,
                outputRoot = output ?? string.Empty,
                language = language ?? string.Empty,
                autoStart = true,
                exitPlayModeOnFinish = true,
                batch = true,
            }, batch: true);
        }

        public static void GenerateCampaign()
        {
            // Sequential batch pack: screenshots → trailer → shorts → stills.
            string output = Arg("-moyvaMarketingOutput");
            MarketingContentIndexBuilder.Build(saveSnapshot: true);
            MarketingRunMonitor.StartCampaign(new[]
            {
                "steam-screenshots", "gameplay-screenshots", "hero-stills",
                "trailer-60", "trailer-30", "teaser-15", "shorts-30",
                "youtube-thumbnail",
            }, new MarketingStudioSession.RunRequest
            {
                outputRoot = output ?? string.Empty,
                autoStart = true,
                exitPlayModeOnFinish = true,
                batch = true,
            }, batch: true);
        }

        private static string Arg(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return null;
        }
    }
}
