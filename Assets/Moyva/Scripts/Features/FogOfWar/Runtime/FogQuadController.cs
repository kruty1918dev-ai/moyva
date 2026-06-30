using System;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    [Obsolete("Use FogOfWarVolumeController for 3D prefab-based fog volume.")]
    public sealed class FogQuadController : MonoBehaviour
    {
        private void Awake() => DisableLegacyQuad();

        private void OnEnable() => DisableLegacyQuad();

        private void DisableLegacyQuad()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = false;

            enabled = false;
        }
    }
}
