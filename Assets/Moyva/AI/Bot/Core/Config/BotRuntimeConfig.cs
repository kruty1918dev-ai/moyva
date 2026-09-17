using System;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    [Serializable]
    public sealed class BotModelProfile
    {
        public bool enabled;
        public string modelName = "Unassigned";
        public string modelResourcePath;
        public string trainingRunId;
        public string notes;
        public int curriculumStage = 8;
        public int contractVersion = BotDecisionContract.ContractVersion;
        public string contractHash;
    }

    [Serializable]
    public sealed class BotDifficultyProfile
    {
        public string id = "normal";
        public string displayName = "Normal";
        public string description;
        public string policyModeName;
        public BotPolicyMode policyMode = BotPolicyMode.Heuristic;
        public BotModelProfile modelProfile = new BotModelProfile();
        public float visibleDelay = -1f;

        public bool IsValid
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(policyModeName)
                    && Enum.TryParse(policyModeName.Trim(), true, out BotPolicyMode parsed))
                    policyMode = parsed;
                return !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(displayName)
                    && modelProfile != null && Enum.IsDefined(typeof(BotPolicyMode), policyMode);
            }
        }
    }

    [Serializable]
    public sealed class BotDifficultyRegistry
    {
        public string defaultId = "normal";
        public BotDifficultyProfile[] difficulties = Array.Empty<BotDifficultyProfile>();

        public static BotDifficultyRegistry Load(string resourceId = "MoyvaBotDifficultyRegistry")
        {
            if (string.IsNullOrWhiteSpace(resourceId))
                resourceId = "MoyvaBotDifficultyRegistry";
            var asset = Resources.Load<TextAsset>(resourceId);
            var registry = asset != null ? JsonUtility.FromJson<BotDifficultyRegistry>(asset.text) : null;
            if (registry == null || registry.difficulties == null || registry.difficulties.Length == 0)
                registry = Fallback();
            registry.Validate();
            return registry;
        }

        public BotDifficultyProfile Select(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
                foreach (var difficulty in difficulties)
                    if (difficulty != null && string.Equals(difficulty.id, id.Trim(), StringComparison.OrdinalIgnoreCase))
                        return difficulty;
            foreach (var difficulty in difficulties)
                if (difficulty != null && string.Equals(difficulty.id, defaultId, StringComparison.OrdinalIgnoreCase))
                    return difficulty;
            return difficulties[0];
        }

        public void Validate()
        {
            if (difficulties == null || difficulties.Length == 0)
                throw new ArgumentException("Bot difficulty registry must contain at least one difficulty.");
            foreach (var difficulty in difficulties)
                if (difficulty == null || !difficulty.IsValid)
                    throw new ArgumentException("Invalid bot difficulty profile.");
        }

        private static BotDifficultyRegistry Fallback() => new BotDifficultyRegistry
        {
            defaultId = "normal",
            difficulties = new[]
            {
                new BotDifficultyProfile
                {
                    id = "normal",
                    displayName = "Normal",
                    description = "Built-in heuristic bot.",
                    policyMode = BotPolicyMode.Heuristic,
                    modelProfile = new BotModelProfile
                    {
                        enabled = false,
                        modelName = "Heuristic",
                        notes = "No model registry was found; using the built-in heuristic bot.",
                        contractVersion = BotDecisionContract.ContractVersion,
                        contractHash = BotDecisionContract.Hash
                    }
                }
            }
        };
    }

    [Serializable]
    public sealed class BotRuntimeConfig
    {
        public BotPolicyMode policyMode = BotPolicyMode.Heuristic;
        public string modelProfileResourceId = "MoyvaBotModelProfile";
        public string difficultyRegistryResourceId = "MoyvaBotDifficultyRegistry";
        public string selectedDifficultyId = "normal";
        public string selectedDifficultyName = "Normal";
        public BotModelProfile modelProfile = new BotModelProfile();
        public int maxDecisionsPerTurn = 32, maxInvalidDecisions = 3, maxStaleDecisions = 5;
        public float decisionTimeout = 10, executionTimeout = 30, visibleDelay = 0.35f;
        public bool autoEndTurn = true, telemetryEnabled = true, detailedTelemetry;
        public int telemetryCapacity = 128;
        public int curriculumStage = 8;
        // Bitmask over BotCapabilityId; -1 enables every capability.
        // Bit 0 (Turn) is always treated as enabled: EndTurn must remain legal.
        public int capabilityMask = -1;
        public BotRuntimeConfig Snapshot()
        {
            var copy = JsonUtility.FromJson<BotRuntimeConfig>(JsonUtility.ToJson(this));
            copy.Validate(); return copy;
        }
        public void Validate()
        {
            if (maxDecisionsPerTurn < 1 || maxInvalidDecisions < 1 || maxStaleDecisions < 1
                || telemetryCapacity < 1 || telemetryCapacity > 4096 || curriculumStage < 0 || curriculumStage > 8
                || !Finite(decisionTimeout) || decisionTimeout <= 0 || !Finite(executionTimeout) || executionTimeout <= 0
                || !Finite(visibleDelay) || visibleDelay < 0 || modelProfile == null
                || !Enum.IsDefined(typeof(BotPolicyMode), policyMode))
                throw new ArgumentException("Invalid bot runtime config.");
        }
        public static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
