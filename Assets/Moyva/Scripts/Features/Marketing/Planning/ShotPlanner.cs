using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Planning
{
    /// <summary>
    /// Turns live-world subjects + the content index into ordered shot plans.
    /// Enforces diversity (category share caps, subject repetition penalty,
    /// no consecutive identical rigs) and restraint (min shot length, mostly
    /// cuts). Pure logic — deterministic for a given seed.
    /// </summary>
    public sealed class ShotPlanner
    {
        /// <summary>Screenshot batch: plan a diverse still set.</summary>
        public ShotSequence PlanScreenshots(
            WorldSubjects world,
            ContentIndexSnapshot index,
            MarketingCaptureRecipe recipe,
            int seed)
        {
            var rng = new MarketingRng(seed);
            var sequence = new ShotSequence { seed = seed, recipeId = recipe.id };
            var categories = BuildCategoryQuota(recipe.shotCount, recipe.uiVisibility != MarketingUiVisibility.Hidden);
            var usage = new Dictionary<string, int>(StringComparer.Ordinal);
            CameraRigType lastRig = (CameraRigType)(-1);

            for (int i = 0; i < categories.Count; i++)
            {
                ShotCategory cat = categories[i];
                var shot = new ShotPlan
                {
                    shotId = $"shot{i + 1:00}",
                    category = cat,
                    durationSec = 1.6f, // golden-frame sampling window
                    seed = rng.Next(1, int.MaxValue),
                    aspectIntent = AspectOf(recipe),
                };
                shot.rig = PickRig(cat, rng, lastRig, recipe.cameraStyle);
                lastRig = shot.rig;
                shot.motion = DefaultMotion(shot.rig);
                shot.subject = PickSubject(world, index, cat, usage, rng, closeUp: IsCloseRig(shot.rig));
                sequence.shots.Add(shot);
            }
            return sequence;
        }

        /// <summary>Trailer: beats → shots, restraint rules applied.</summary>
        public ShotSequence PlanTrailer(
            WorldSubjects world,
            ContentIndexSnapshot index,
            MarketingCaptureRecipe recipe,
            int seed)
        {
            var rng = new MarketingRng(seed);
            var sequence = new ShotSequence { seed = seed, recipeId = recipe.id };
            var beats = TrailerBeatLibrary.BeatsFor(recipe.durationSec);
            float minLen = TrailerBeatLibrary.MinShotLength(recipe.durationSec);
            var usage = new Dictionary<string, int>(StringComparer.Ordinal);
            CameraRigType lastRig = (CameraRigType)(-1);
            int shotIndex = 0;

            foreach (var beat in beats)
            {
                int parts = Mathf.Clamp(Mathf.FloorToInt(beat.Duration / (minLen * 1.9f)), 1, 3);
                float partLen = beat.Duration / parts;
                for (int p = 0; p < parts; p++)
                {
                    shotIndex++;
                    var shot = new ShotPlan
                    {
                        shotId = $"beat-{beat.beat.ToString().ToLowerInvariant()}-{p + 1}",
                        beat = beat.beat.ToString(),
                        category = beat.preferredCategory,
                        durationSec = partLen,
                        seed = rng.Next(1, int.MaxValue),
                        transitionIn = TrailerBeatLibrary.TransitionFor(beat.beat, shotIndex),
                        aspectIntent = AspectOf(recipe),
                        textClaimId = beat.textClaimId,
                    };
                    shot.rig = beat.beat == TrailerBeat.End
                        ? CameraRigType.Static
                        : PickRig(beat.preferredCategory, rng, lastRig, recipe.cameraStyle);
                    lastRig = shot.rig;
                    shot.motion = beat.beat == TrailerBeat.End ? ShotMotion.Hold : DefaultMotion(shot.rig);
                    shot.subject = PickSubject(world, index, beat.preferredCategory, usage, rng,
                        closeUp: IsCloseRig(shot.rig));
                    sequence.shots.Add(shot);
                }
            }
            return sequence;
        }

        private static float AspectOf(MarketingCaptureRecipe recipe)
            => recipe.resolutionHeight > 0 ? (float)recipe.resolutionWidth / recipe.resolutionHeight : 16f / 9f;

        private static List<ShotCategory> BuildCategoryQuota(int shotCount, bool includeUiShot)
        {
            var quota = new List<ShotCategory>(shotCount);
            var weights = new (ShotCategory cat, float w)[]
            {
                (ShotCategory.WorldBeauty, 2f),
                (ShotCategory.Settlement, 2f),
                (ShotCategory.Economy, 1f),
                (ShotCategory.Building, 1.5f),
                (ShotCategory.Army, 2f),
                (ShotCategory.Tactical, 1f),
                (ShotCategory.Combat, 2f),
                (ShotCategory.Exploration, 1f),
                (ShotCategory.Atmosphere, 1f),
            };
            int uiSlot = includeUiShot ? 1 : 0;
            var counts = new Dictionary<ShotCategory, int>();
            int cap = Mathf.Max(1, Mathf.CeilToInt(shotCount * 0.3f)); // diversity rule

            var rng = new System.Random(shotCount * 7919);
            while (quota.Count < shotCount - uiSlot)
            {
                float total = 0f;
                foreach (var (c, w) in weights)
                {
                    int n = counts.TryGetValue(c, out int v) ? v : 0;
                    if (n < cap) total += w / (1f + n);
                }
                if (total <= 0f) break;
                double roll = rng.NextDouble() * total;
                ShotCategory picked = ShotCategory.WorldBeauty;
                foreach (var (c, w) in weights)
                {
                    int n = counts.TryGetValue(c, out int v) ? v : 0;
                    if (n >= cap) continue;
                    roll -= w / (1f + n);
                    if (roll <= 0) { picked = c; break; }
                }
                counts[picked] = (counts.TryGetValue(picked, out int cur) ? cur : 0) + 1;
                quota.Add(picked);
            }
            if (uiSlot == 1)
                quota.Add(ShotCategory.UiGameplay);
            return quota;
        }

        private static ShotSubject PickSubject(
            WorldSubjects world,
            ContentIndexSnapshot index,
            ShotCategory category,
            Dictionary<string, int> usage,
            MarketingRng rng,
            bool closeUp)
        {
            var pool = world != null ? world.For(category) : null;
            if (pool == null || pool.Count == 0)
                return new ShotSubject { contentId = "world", worldX = world?.worldCenterX ?? 0f, worldZ = world?.worldCenterZ ?? 0f, approximateRadius = world?.worldRadius ?? 20f };

            // Score world subjects: prefer entries whose contentId scores well in
            // the index and that were not already used.
            var weights = new float[pool.Count];
            var ctx = new ContentScorer.Context
            {
                shotCategory = category,
                needsAnimation = closeUp && (category == ShotCategory.Army || category == ShotCategory.Combat || category == ShotCategory.Tactical),
                usageCounts = usage,
                aspectIntent = 16f / 9f,
            };
            for (int i = 0; i < pool.Count; i++)
            {
                var subject = pool[i];
                float w = 1f;
                var entry = index?.entries?.Find(e => e.id == subject.contentId);
                if (entry != null)
                {
                    float s = ContentScorer.Score(entry, ctx);
                    if (float.IsNegativeInfinity(s)) { weights[i] = 0f; continue; }
                    if (float.IsPositiveInfinity(s)) { weights[i] = 1000f; continue; }
                    w = Mathf.Max(0.05f, 1f + s * 0.25f);
                    if (closeUp && !entry.suitsClose) w *= 0.25f;
                }
                int used = usage.TryGetValue(subject.instanceId, out int n) ? n : 0;
                w /= (1f + used * 2f);
                weights[i] = w;
            }

            int pick = rng.WeightedPick(weights);
            if (pick < 0) pick = rng.Next(0, pool.Count);
            var chosen = pool[pick];
            usage[chosen.instanceId] = (usage.TryGetValue(chosen.instanceId, out int c) ? c : 0) + 1;
            if (!string.IsNullOrEmpty(chosen.contentId))
                usage[chosen.contentId] = (usage.TryGetValue(chosen.contentId, out int cc) ? cc : 0) + 1;
            return chosen;
        }

        private static bool IsCloseRig(CameraRigType rig)
            => rig == CameraRigType.UnitHero || rig == CameraRigType.BuildingHero || rig == CameraRigType.BattleClose;

        private static CameraRigType PickRig(ShotCategory cat, MarketingRng rng, CameraRigType last, MarketingCameraStyle style)
        {
            if (style == MarketingCameraStyle.Gameplay)
                return cat == ShotCategory.WorldBeauty ? CameraRigType.TacticalOverview : CameraRigType.TopDown;

            CameraRigType[] options = cat switch
            {
                ShotCategory.WorldBeauty => new[] { CameraRigType.WorldReveal, CameraRigType.Orbit, CameraRigType.Dolly },
                ShotCategory.Settlement => new[] { CameraRigType.SettlementWide, CameraRigType.SettlementMedium, CameraRigType.Orbit },
                ShotCategory.Economy => new[] { CameraRigType.SettlementMedium, CameraRigType.Dolly },
                ShotCategory.Building => new[] { CameraRigType.BuildingHero, CameraRigType.Orbit, CameraRigType.SettlementMedium },
                ShotCategory.Army => new[] { CameraRigType.ArmyTrack, CameraRigType.ArmySideTrack, CameraRigType.BattleWide },
                ShotCategory.Tactical => new[] { CameraRigType.TacticalOverview, CameraRigType.TopDown },
                ShotCategory.Combat => new[] { CameraRigType.BattleWide, CameraRigType.BattleMedium, CameraRigType.BattleClose },
                ShotCategory.Exploration => new[] { CameraRigType.Dolly, CameraRigType.WorldReveal },
                ShotCategory.Atmosphere => new[] { CameraRigType.LowAngle, CameraRigType.Orbit, CameraRigType.Static },
                ShotCategory.UiGameplay => new[] { CameraRigType.TacticalOverview, CameraRigType.TopDown },
                ShotCategory.EndCard => new[] { CameraRigType.Static },
                _ => new[] { CameraRigType.SettlementWide },
            };

            CameraRigType pick = options[rng.Next(0, options.Length)];
            if (pick == last && options.Length > 1)
                pick = options[(Array.IndexOf(options, pick) + 1) % options.Length];
            return pick;
        }

        private static ShotMotion DefaultMotion(CameraRigType rig) => rig switch
        {
            CameraRigType.WorldReveal => ShotMotion.SlowPushIn,
            CameraRigType.Dolly => ShotMotion.SlowPushIn,
            CameraRigType.Orbit => ShotMotion.Orbit,
            CameraRigType.ArmyTrack => ShotMotion.LateralTrack,
            CameraRigType.ArmySideTrack => ShotMotion.LateralTrack,
            CameraRigType.Follow => ShotMotion.LateralTrack,
            CameraRigType.SettlementWide => ShotMotion.SlowPushIn,
            CameraRigType.TacticalOverview => ShotMotion.Crane,
            CameraRigType.Aftermath => ShotMotion.SlowPullOut,
            _ => ShotMotion.Hold,
        };
    }
}
