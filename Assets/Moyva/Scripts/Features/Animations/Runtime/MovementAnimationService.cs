using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Animations.API;
using UnityEngine;

namespace Kruty1918.Moyva.Animations.Runtime
{
    internal sealed class MovementAnimationService : IMovementAnimationService
    {
        public async Task MoveAlongPathAsync(
            Transform target, 
            IReadOnlyList<Vector2Int> path, 
            PathAnimationSettings settings, 
            CancellationToken cancellationToken = default)
        {
            if (target == null || path == null || path.Count <= 1) return;
    
            // Починаємо з індексу 1, бо індекс 0 — це тайл, на якому об'єкт вже стоїть
            for (int i = 1; i < path.Count; i++)
            {
                // Перевіряємо, чи не надійшла команда зупинити рух
                cancellationToken.ThrowIfCancellationRequested();

                Vector2Int nextGridPos = path[i];
                Vector3 startPos = target.position;
                
                // Переводимо 2D координати гріда у 3D простір Unity (Y - залишаємо як висоту об'єкта)
                Vector3 endPos = new Vector3(nextGridPos.x, nextGridPos.y);

                float elapsed = 0f;

                // Цикл плавного переміщення (Lerp)
                while (elapsed < settings.MoveDurationPerTile)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / settings.MoveDurationPerTile);
                    
                    target.position = Vector3.Lerp(startPos, endPos, t);

                    // Чекаємо наступного кадру Unity
                    await Task.Yield(); 
                }

                // Гарантуємо точне прибуття в центр тайла
                target.position = endPos;

                // Обробляємо затримку на тайлі, якщо вона є
                if (settings.DelayOnTile > 0)
                {
                    // Task.Delay приймає мілісекунди
                    await Task.Delay(Mathf.RoundToInt(settings.DelayOnTile * 1000), cancellationToken);
                }

                // Викликаємо колбек після завершення кроку
                settings.OnStepCompleted?.Invoke(nextGridPos);
            }
        }
    }
}