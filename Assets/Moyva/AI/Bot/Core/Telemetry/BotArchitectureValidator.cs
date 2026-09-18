using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.AI.Bot
{
    public static class BotArchitectureValidator
    {
        public static void ValidateFrame(BotDecisionFrame frame, BotCapabilityRegistry registry)
        {
            if (registry.Get(BotCapabilityId.Turn) == null) throw new InvalidOperationException("Missing Turn capability.");
            if (frame.Observations.Count != BotObservationSchema.Size || frame.Candidates.Count < 1
                || frame.Candidates.Count > BotDecisionContract.MaxCandidateSlots)
                throw new InvalidOperationException("Invalid bot frame dimensions or all actions masked.");
            var keys = new HashSet<string>();
            for (int slot = 0; slot < frame.Candidates.Count; slot++)
            {
                var candidate = frame.Candidates[slot];
                if (!keys.Add(candidate.Capability + ":" + candidate.Id)) throw new InvalidOperationException("Duplicate candidate.");
            }
            foreach (float value in frame.Observations)
                if (!BotRuntimeConfig.Finite(value)) throw new InvalidOperationException("Non-finite observation.");
        }
    }
}

