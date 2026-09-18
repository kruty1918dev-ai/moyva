using System;

namespace Kruty1918.Moyva.AI.Training
{
    [Serializable]
    public sealed class TrainingRewardConfig
    {
        public float victory = 1f;
        public float defeat = -1f;
        public float draw = -0.10f;
        public float timeout = -0.15f;
        public float objectiveCaptured = 0.02f;
        public float objectiveLost = -0.02f;
        public float enemyUnitDestroyed = 0.01f;
        public float ownUnitLost = -0.01f;
        public float resourceMilestone = 0.005f;
        public float buildOrRecruit = 0.002f;
        public float perDecision = -0.0002f;
        public float perTurn = -0.001f;
        public float stagnantDecision = -0.002f;
        public float isolatedSettlement = -0.01f;
        public float resourcePotential = 0.004f;
        public bool penalizePerDecision = true;
        public float invalidAction = -0.005f;
        public int invalidActionLimit = 20;
        public float shapingCap = 0.25f;

        public void Validate()
        {
            float[] values = { victory, defeat, draw, timeout, objectiveCaptured, objectiveLost,
                enemyUnitDestroyed, ownUnitLost, resourceMilestone, buildOrRecruit, perDecision,
                perTurn, stagnantDecision, isolatedSettlement, resourcePotential, invalidAction, shapingCap };
            foreach (float value in values)
                if (!TrainingConfig.Finite(value)) throw new ArgumentException("Non-finite reward.");
            if (victory < 1 || defeat > -1 || draw > 0 || timeout > 0
                || shapingCap < 0 || shapingCap > 0.25f || resourceMilestone < 0 || resourceMilestone > 0.005f
                || buildOrRecruit < 0 || buildOrRecruit > 0.002f || invalidAction < -0.005f || invalidAction > 0
                || perDecision > 0 || perTurn > 0
                || stagnantDecision > 0 || stagnantDecision < -0.01f
                || isolatedSettlement > 0 || isolatedSettlement < -0.05f
                || resourcePotential < 0 || resourcePotential > 0.01f
                || invalidActionLimit < 1
                || objectiveCaptured < 0 || objectiveLost > 0 || enemyUnitDestroyed < 0 || ownUnitLost > 0)
                throw new ArgumentException("Unsafe training reward configuration.");
        }
    }
}
