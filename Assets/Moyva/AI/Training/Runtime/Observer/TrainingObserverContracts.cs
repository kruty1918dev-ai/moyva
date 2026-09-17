using System;

namespace Kruty1918.Moyva.AI.Training
{
    [Serializable]
    public sealed class ArenaSnapshot
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;
        public string sessionId;
        public int arenaId;
        public long episodeId;
        public long sequence;
        public int width, height;
        public string scenarioId;
        public int scenarioStep;
        public bool isComplete;
        public string status;
        public string[] cellLayers = Array.Empty<string>();
        public float[] heights = Array.Empty<float>();
        public string[] surfaces = Array.Empty<string>();
        public string[] staticObjects = Array.Empty<string>();
        public float[] projection = Array.Empty<float>();
    }

    [Serializable]
    public sealed class AgentDecisionEvent
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;
        public string sessionId;
        public int arenaId;
        public long episodeId;
        public long sequence;
        public string agentId;
        public string scenarioId;
        public int scenarioStep;
        public float scenarioProgress;
        // Real legal candidate count on the decided frame; never counts the
        // synthetic no-legal-action fallback.
        public int candidateCount;
        // True when the engine executed the only legal candidate without a
        // policy decision — the event is journal evidence, not a trainable step.
        public bool forcedAction;
        // True when the frame had zero real candidates and only held the
        // synthetic "wait" placeholder.
        public bool syntheticFallback;
        public string[] availableIntents = Array.Empty<string>();
        public string[] availableActions = Array.Empty<string>();
        public int chosenSlot = -1;
        public string chosenIntent;
        public string actorId;
        public string actionId;
        public string targetId;
        public int targetX;
        public int targetY;
        public string result;
        public string rejectionReason;
        public float rewardDelta;
    }

    public enum ObserverCommandKind { Subscribe, Unsubscribe, RequestFullSnapshot, SelectAgent }

    [Serializable]
    public sealed class ObserverCommand
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;
        public ObserverCommandKind kind;
        public string sessionId;
        public int arenaId = -1;
        public string agentId;
        public long lastSequence;
        public bool visuals;
    }
}
