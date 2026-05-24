using System;
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
    
            for (int i = 1; i < path.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Vector2Int nextGridPos = path[i];

                // --- ПЕРЕВІРКА МОЖЛИВОСТІ КРОКУ ---
                // Викликаємо делегат перед початком руху до наступного тайла
                if (settings.CanPerformStep != null && !settings.CanPerformStep.Invoke(nextGridPos))
                {
                    Debug.Log($"[MovementAnimation] Рух зупинено: крок до {nextGridPos} відхилено логікою.");
                    break; // Виходимо з циклу, юніт залишається на поточному місці
                }

                Vector3 startPos = target.position;
                Vector3 endPos = settings.ResolveWorldPosition != null
                    ? settings.ResolveWorldPosition(nextGridPos)
                    : new Vector3(nextGridPos.x, nextGridPos.y, startPos.z);

                float elapsed = 0f;

                // Анімація переміщення до наступного тайла
                while (elapsed < settings.MoveDurationPerTile)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / settings.MoveDurationPerTile);
                    
                    target.position = Vector3.Lerp(startPos, endPos, t);

                    await Task.Yield(); 
                }

                target.position = endPos;

                if (settings.DelayOnTile > 0)
                {
                    await Task.Delay(Mathf.RoundToInt(settings.DelayOnTile * 1000), cancellationToken);
                }

                // Фіксуємо завершення кроку (тут UnitService спише стаміну)
                settings.OnStepCompleted?.Invoke(nextGridPos);
            }
        }
    }
}