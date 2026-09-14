using UnityEngine;
using Unity.InferenceEngine;
using Unity.MLAgents.Policies;
using Zenject;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class MoyvaBotPolicyBinding : MonoBehaviour
    {
        [SerializeField] private ModelAsset model;
        private GameObject _agentObject;
        private IBotDecisionOrchestrator _orchestrator;
        [Inject]
        public void Connect(IBotDecisionOrchestrator orchestrator, BotRuntimeConfig config)
        {
            _orchestrator = orchestrator;
            if (config.policyMode != BotPolicyMode.MLAgentsInference) return;
            var profile = config.modelProfile;
            var selectedModel = LoadModel(profile) ?? model;
            if (!BotPolicyContractValidator.Validate(profile, selectedModel != null, out string reason))
            {
                orchestrator.SetPolicy(new HeuristicBotPolicyDriver(), profile?.modelName ?? "Unassigned", reason);
                Debug.LogWarning("Bot policy fallback: " + reason, this);
                return;
            }
            _agentObject = new GameObject("Moyva Bot Policy");
            _agentObject.transform.SetParent(transform, false);
            _agentObject.SetActive(false);
            BotMlFrameWriter.Configure(_agentObject.AddComponent<BehaviorParameters>(), BehaviorType.InferenceOnly, selectedModel);
            var agent = _agentObject.AddComponent<MoyvaBotPolicyAgent>();
            agent.Configure(BotPolicyMode.MLAgentsInference);
            _agentObject.SetActive(true);
            orchestrator.SetPolicy(agent, profile.modelName);
        }

        private static ModelAsset LoadModel(BotModelProfile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.modelResourcePath))
                return null;
            return Resources.Load<ModelAsset>(profile.modelResourcePath.Trim());
        }

        private void OnDestroy()
        {
            _orchestrator?.Cancel();
            if (_agentObject != null) Destroy(_agentObject);
        }
    }
}
