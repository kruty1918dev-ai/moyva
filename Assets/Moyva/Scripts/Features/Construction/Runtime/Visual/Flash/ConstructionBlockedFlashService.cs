using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBlockedFlashService : IConstructionBlockedFlashService
    {
        private struct FlashRestore
        {
            public GameObject Target;
            public bool IsGhostPreview;
            public float RestoreAt;
        }

        private readonly IConstructionVisualStyleService _styleService;
        private readonly Dictionary<int, FlashRestore> _flashRestoresByTarget = new();
        private readonly List<int> _keysBuffer = new();
        private readonly float _blockedFlashDuration;

        public ConstructionBlockedFlashService(
            IConstructionVisualStyleService styleService,
            IConstructionVisualSettingsProvider visualSettingsProvider = null)
        {
            _styleService = styleService;
            _blockedFlashDuration = Mathf.Max(0f, visualSettingsProvider?.BlockedFlashDurationSeconds ?? 0.35f);
        }

        public void Flash(GameObject target, bool isGhostPreview)
        {
            if (target == null)
                return;

            _styleService.ApplyGhostStyle(target, false);
            _flashRestoresByTarget[target.GetInstanceID()] = new FlashRestore
            {
                Target = target,
                IsGhostPreview = isGhostPreview,
                RestoreAt = Time.time + _blockedFlashDuration,
            };
        }

        public void Tick()
        {
            if (_flashRestoresByTarget.Count <= 0)
                return;

            float now = Time.time;
            _keysBuffer.Clear();
            foreach (int key in _flashRestoresByTarget.Keys)
                _keysBuffer.Add(key);

            for (int i = 0; i < _keysBuffer.Count; i++)
            {
                int key = _keysBuffer[i];
                var entry = _flashRestoresByTarget[key];
                if (now < entry.RestoreAt)
                    continue;

                if (entry.Target != null)
                {
                    if (entry.IsGhostPreview)
                        _styleService.ApplyGhostStyle(entry.Target, true);
                    else
                        _styleService.ApplySolidStyle(entry.Target);
                }

                _flashRestoresByTarget.Remove(key);
            }
        }

        public void Clear()
        {
            _flashRestoresByTarget.Clear();
            _keysBuffer.Clear();
        }
    }
}
