using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Validation
{
    public sealed class ValidationIssue
    {
        public string code;
        public string message;
        public bool blocking;

        public ValidationIssue(string code, string message, bool blocking = true)
        {
            this.code = code;
            this.message = message;
            this.blocking = blocking;
        }
    }

    /// <summary>Recipe sanity validation — runs before any capture.</summary>
    public static class MarketingRecipeValidator
    {
        public static List<ValidationIssue> Validate(MarketingCaptureRecipe r)
        {
            var issues = new List<ValidationIssue>();
            if (r == null)
            {
                issues.Add(new ValidationIssue("recipe-null", "Recipe is null."));
                return issues;
            }
            if (string.IsNullOrWhiteSpace(r.id))
                issues.Add(new ValidationIssue("recipe-id", "Recipe id is empty."));
            if (r.resolutionWidth < 0 || r.resolutionHeight < 0)
                issues.Add(new ValidationIssue("resolution-negative", "Resolution cannot be negative."));
            if (r.resolutionWidth > 0 && r.resolutionWidth < 160)
                issues.Add(new ValidationIssue("resolution-low", $"Width {r.resolutionWidth} below 160px."));
            if (r.resolutionHeight > 0 && r.resolutionHeight < 160)
                issues.Add(new ValidationIssue("resolution-low", $"Height {r.resolutionHeight} below 160px."));
            bool video = r.outputKind >= MarketingOutputKind.Video;
            if (video)
            {
                if (r.durationSec < 2f || r.durationSec > 180f)
                    issues.Add(new ValidationIssue("duration", $"Duration {r.durationSec}s outside 2..180s."));
                if (r.frameRate < 12 || r.frameRate > 120)
                    issues.Add(new ValidationIssue("fps", $"Frame rate {r.frameRate} outside 12..120."));
            }
            else if (r.shotCount < 1 || r.shotCount > 64)
            {
                issues.Add(new ValidationIssue("shot-count", $"Shot count {r.shotCount} outside 1..64."));
            }
            if (r.contentType == MarketingContentType.Trailer && !video)
                issues.Add(new ValidationIssue("trailer-not-video", "Trailer recipe must output video.", false));
            return issues;
        }
    }

    /// <summary>Store compliance: recipe must obey the platform profile or the
    /// export is blocked / a compliant clean variant is generated.</summary>
    public static class PlatformCompliance
    {
        public static List<ValidationIssue> Check(MarketingCaptureRecipe r, PlatformCaptureProfile p)
        {
            var issues = new List<ValidationIssue>();
            if (p == null) return issues;

            if (p.gameplayOnly && r.includeMarketingText)
                issues.Add(new ValidationIssue("compliance-text",
                    $"Platform '{p.id}' is gameplay-only: marketing text must be off."));
            if (p.gameplayOnly && r.includeLogo)
                issues.Add(new ValidationIssue("compliance-logo",
                    $"Platform '{p.id}' is gameplay-only: logo overlay must be off."));
            if (p.gameplayOnly && !p.preRenderedAllowed && r.allowStagingSpawns
                && r.outputKind == MarketingOutputKind.Image)
                issues.Add(new ValidationIssue("compliance-staging",
                    $"Platform '{p.id}' forbids staged/prerendered stills: staging spawns must be off.",
                    false));
            if (!p.textAllowed && r.includeMarketingText)
                issues.Add(new ValidationIssue("compliance-text2",
                    $"Platform '{p.id}' does not allow text overlays."));
            if (!p.logoAllowed && r.includeLogo)
                issues.Add(new ValidationIssue("compliance-logo2",
                    $"Platform '{p.id}' does not allow logos."));
            if (p.minResolutionWidth > 0 && r.resolutionWidth > 0 && r.resolutionWidth < p.minResolutionWidth)
                issues.Add(new ValidationIssue("compliance-minres",
                    $"Platform '{p.id}' requires >= {p.minResolutionWidth}px wide."));
            if (p.maxDurationSec > 0 && r.outputKind >= MarketingOutputKind.Video && r.durationSec > p.maxDurationSec)
                issues.Add(new ValidationIssue("compliance-duration",
                    $"Platform '{p.id}' max duration {p.maxDurationSec}s exceeded ({r.durationSec}s)."));
            return issues;
        }

        /// <summary>Returns a cloned recipe forced into compliance (text/logo off,
        /// staging off) — used for automatic clean variants.</summary>
        public static MarketingCaptureRecipe ForceCompliant(MarketingCaptureRecipe r, PlatformCaptureProfile p)
        {
            var clone = JsonConvert.DeserializeObject<MarketingCaptureRecipe>(JsonConvert.SerializeObject(r));
            if (p == null) return clone;
            if (p.gameplayOnly || !p.textAllowed) clone.includeMarketingText = false;
            if (p.gameplayOnly || !p.logoAllowed) clone.includeLogo = false;
            if (p.gameplayOnly && !p.preRenderedAllowed) clone.allowStagingSpawns = false;
            if (p.maxDurationSec > 0 && clone.durationSec > p.maxDurationSec)
                clone.durationSec = p.maxDurationSec;
            return clone;
        }
    }

    /// <summary>Content validity gate for index entries.</summary>
    public static class MarketingContentValidator
    {
        public static List<ValidationIssue> Validate(ContentIndexEntry e)
        {
            var issues = new List<ValidationIssue>();
            if (string.IsNullOrWhiteSpace(e.id))
                issues.Add(new ValidationIssue("content-id", "Entry without id."));
            if (string.IsNullOrWhiteSpace(e.editorPath) && e.category != MarketingContentCategory.Audio)
                issues.Add(new ValidationIssue("content-path", $"Entry '{e.id}' has no asset path."));
            if (e.missingMaterial)
                issues.Add(new ValidationIssue("content-material", $"Entry '{e.id}' has renderer without material."));
            if (e.rendererCount == 0 && e.category != MarketingContentCategory.Audio)
                issues.Add(new ValidationIssue("content-renderer", $"Entry '{e.id}' has no renderers."));
            return issues;
        }
    }
}
