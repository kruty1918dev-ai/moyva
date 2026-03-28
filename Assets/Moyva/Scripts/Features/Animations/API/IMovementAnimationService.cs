using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.API
{
    public interface IMovementAnimationService
    {
        /// <summary>
        /// Асинхронно переміщує об'єкт по заданому шляху.
        /// </summary>
        Task MoveAlongPathAsync(
            Transform target, 
            IReadOnlyList<Vector2Int> path, 
            PathAnimationSettings settings, 
            CancellationToken cancellationToken = default);
    }
}