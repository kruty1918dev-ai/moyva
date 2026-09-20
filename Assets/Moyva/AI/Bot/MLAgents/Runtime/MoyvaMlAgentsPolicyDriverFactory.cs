using Unity.InferenceEngine;
using Unity.MLAgents.Policies;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class MoyvaMlAgentsPolicyDriverFactory : IBotPolicyDriverFactory
    {
        private GameObject _agentObject;

        public IBotPolicyDriver Create(BotRuntimeConfig config, BotTelemetryHub telemetry)
        {
            IBotPolicyDriver driver = CreateCore(config, telemetry);
            // Difficulty dial: blends in uniformly random legal actions.
            // Weakens the policy without touching rules, fog or resources.
            return config != null && config.explorationRate > 0f
                ? new EpsilonBlendPolicyDriver(driver, config.explorationRate)
                : driver;
        }

        private IBotPolicyDriver CreateCore(BotRuntimeConfig config, BotTelemetryHub telemetry)
        {
            if (config == null || config.policyMode != BotPolicyMode.MLAgentsInference)
                return new HeuristicBotPolicyDriver();

            var profile = config.modelProfile;
            var model = LoadModel(profile);
            if (!BotPolicyContractValidator.Validate(profile, model != null, out string reason))
            {
                if (telemetry != null)
                    telemetry.FallbackReason = reason;
                Debug.LogWarning("Bot policy fallback: " + reason);
                return new HeuristicBotPolicyDriver();
            }

            _agentObject = new GameObject("Moyva Bot Policy - " + profile.modelName);
            Object.DontDestroyOnLoad(_agentObject);
            _agentObject.SetActive(false);
            BotMlFrameWriter.Configure(_agentObject.AddComponent<BehaviorParameters>(), BehaviorType.InferenceOnly, model);
            var agent = _agentObject.AddComponent<MoyvaBotPolicyAgent>();
            agent.Configure(BotPolicyMode.MLAgentsInference);
            _agentObject.SetActive(true);
            return agent;
        }

        private static ModelAsset LoadModel(BotModelProfile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.modelResourcePath))
                return null;
            return Resources.Load<ModelAsset>(profile.modelResourcePath.Trim());
        }
    }
}
