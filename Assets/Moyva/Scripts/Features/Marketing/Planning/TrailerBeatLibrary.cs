using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;

namespace Kruty1918.Moyva.Marketing.Planning
{
    /// <summary>
    /// Narrative beat structure for trailers. The 45–60s template is scaled
    /// down for shorter cuts (15s/30s collapse beats instead of shrinking
    /// every segment to noise). Deterministic: same duration ⇒ same beats.
    /// </summary>
    public static class TrailerBeatLibrary
    {
        private static readonly (TrailerBeat beat, float start, float end, ShotCategory cat)[] FullTemplate =
        {
            (TrailerBeat.Hook,     0.00f, 0.05f, ShotCategory.Combat),
            (TrailerBeat.World,    0.05f, 0.17f, ShotCategory.WorldBeauty),
            (TrailerBeat.Build,    0.17f, 0.30f, ShotCategory.Settlement),
            (TrailerBeat.Expand,   0.30f, 0.45f, ShotCategory.Army),
            (TrailerBeat.Tension,  0.45f, 0.58f, ShotCategory.Tactical),
            (TrailerBeat.Action,   0.58f, 0.80f, ShotCategory.Combat),
            (TrailerBeat.Climax,   0.80f, 0.92f, ShotCategory.WorldBeauty),
            (TrailerBeat.End,      0.92f, 1.00f, ShotCategory.EndCard),
        };

        private static readonly (TrailerBeat beat, float start, float end, ShotCategory cat)[] MidTemplate =
        {
            (TrailerBeat.Hook,     0.00f, 0.08f, ShotCategory.Combat),
            (TrailerBeat.World,    0.08f, 0.24f, ShotCategory.WorldBeauty),
            (TrailerBeat.Build,    0.24f, 0.44f, ShotCategory.Settlement),
            (TrailerBeat.Tension,  0.44f, 0.60f, ShotCategory.Tactical),
            (TrailerBeat.Action,   0.60f, 0.84f, ShotCategory.Combat),
            (TrailerBeat.End,      0.84f, 1.00f, ShotCategory.EndCard),
        };

        private static readonly (TrailerBeat beat, float start, float end, ShotCategory cat)[] ShortTemplate =
        {
            (TrailerBeat.Hook,     0.00f, 0.14f, ShotCategory.Combat),
            (TrailerBeat.Build,    0.14f, 0.40f, ShotCategory.Settlement),
            (TrailerBeat.Action,   0.40f, 0.78f, ShotCategory.Combat),
            (TrailerBeat.End,      0.78f, 1.00f, ShotCategory.EndCard),
        };

        public static List<TrailerBeatPlan> BeatsFor(float durationSec)
        {
            var template = durationSec >= 40f ? FullTemplate
                : durationSec >= 22f ? MidTemplate
                : ShortTemplate;

            var beats = new List<TrailerBeatPlan>(template.Length);
            foreach (var (beat, s, e, cat) in template)
            {
                beats.Add(new TrailerBeatPlan
                {
                    beat = beat,
                    startSec = s * durationSec,
                    endSec = e * durationSec,
                    preferredCategory = cat,
                });
            }
            return beats;
        }

        /// <summary>Restraint rule: minimum on-screen shot length for a given
        /// trailer duration. Prevents per-second random cutting.</summary>
        public static float MinShotLength(float durationSec)
            => durationSec >= 40f ? 2.4f : durationSec >= 22f ? 2.0f : 1.6f;

        /// <summary>Restraint rule: only every Nth cut may use a non-cut
        /// transition, and End/Hook beats always prefer fades.</summary>
        public static ShotTransition TransitionFor(TrailerBeat beat, int shotIndex)
        {
            if (beat == TrailerBeat.End) return ShotTransition.FadeThroughBlack;
            if (beat == TrailerBeat.Hook) return ShotTransition.Fade;
            return shotIndex % 5 == 4 ? ShotTransition.Dissolve : ShotTransition.Cut;
        }
    }
}
