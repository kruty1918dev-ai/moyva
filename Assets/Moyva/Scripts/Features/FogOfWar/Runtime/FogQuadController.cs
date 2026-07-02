using System;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    [Obsolete("Use FogOfWarVolumeController for 3D prefab-based fog volume.")]
    /// <summary>
    /// Legacy MonoBehaviour host для старого quad-based fog presentation.
    /// Новий gameplay/runtime path не повинен покладатися на цей компонент.
    /// </summary>
    public sealed class FogQuadController : MonoBehaviour
    {
        /// <summary>
        /// Вимикає legacy quad path одразу після створення компонента.
        /// </summary>
        private void Awake() => DisableLegacyQuad();

        /// <summary>
        /// Додатково вимикає legacy quad path під час повторного ввімкнення об'єкта.
        /// </summary>
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
