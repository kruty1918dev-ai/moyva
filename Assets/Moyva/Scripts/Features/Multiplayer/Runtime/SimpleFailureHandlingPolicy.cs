using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Централізує реакцію на помилки сесії без аварійного завершення гри.
    /// </summary>
    public sealed class SimpleFailureHandlingPolicy : IFailureHandlingPolicy
    {
        /// <summary>Позначає відновлювану помилку як невиконану операцію.</summary>
        public bool HandleRecoverable(FailureCategory category, string details)
        {
            return false;
        }

        /// <summary>Фіксує критичну помилку, після якої поточна сесія не може продовжитися.</summary>
        public void HandleNonRecoverable(FailureCategory category, string details)
        {
            Debug.LogError($"Multiplayer failure [{category}]: {details}");
        }
    }
}
