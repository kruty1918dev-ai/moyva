using UnityEngine;

namespace Kruty1918.Moyva.GameMode.API
{
    /// <summary>ScriptableObject що описує умову перемоги в сесії.</summary>
    [CreateAssetMenu(menuName = "Moyva/Session/Win Condition", fileName = "WinCondition")]
    public sealed class WinConditionSO : ScriptableObject
    {
        public enum ConditionType { EliminateAllEnemies, ControlPoints, Survival, Custom }

        [SerializeField] private ConditionType _condition = ConditionType.EliminateAllEnemies;
        [SerializeField] private int _controlPointsRequired = 3;
        [SerializeField] private float _survivalTimeSeconds = 300f;

        public ConditionType Condition => _condition;
        public int ControlPointsRequired => _controlPointsRequired;
        public float SurvivalTimeSeconds => _survivalTimeSeconds;
    }
}
