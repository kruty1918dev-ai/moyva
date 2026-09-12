using System;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    [Serializable]
    public sealed class BotModelProfile
    {
        public bool enabled;
        public string modelName = "Unassigned";
        public string trainingRunId;
        public string notes;
        public int curriculumStage = 8;
        public int contractVersion = BotDecisionContract.ContractVersion;
        public string contractHash;
    }
    [Serializable]
    public sealed class BotRuntimeConfig
    {
        public BotPolicyMode policyMode = BotPolicyMode.Heuristic;
        public string modelProfileResourceId = "MoyvaBotModelProfile";
        public BotModelProfile modelProfile = new BotModelProfile();
        public int maxDecisionsPerTurn = 32, maxInvalidDecisions = 3, maxStaleDecisions = 5;
        public float decisionTimeout = 10, executionTimeout = 30, visibleDelay = 0.35f;
        public bool autoEndTurn = true, telemetryEnabled = true, detailedTelemetry;
        public int telemetryCapacity = 128;
        public int curriculumStage = 8;
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
