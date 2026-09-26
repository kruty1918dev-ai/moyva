using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Floating
{
    /// <summary>
    /// Registry and per-frame updater for objects floating on water.
    /// Batched, physics-free buoyancy: each registered transform samples the
    /// replicated wave field and is eased toward the surface height and wave
    /// normal. One authority owns all float motion so spawning code only
    /// registers entries.
    /// </summary>
    internal interface IWaterFloatService
    {
        bool Enabled { get; }
        int Count { get; }

        /// <summary>
        /// Starts floating the transform around <paramref name="restWorldY"/>.
        /// <paramref name="sampleRadius"/> is the horizontal half-extent used
        /// for wave-normal sampling; larger objects tilt less.
        /// </summary>
        void Register(Transform transform, float restWorldY, float sampleRadius);

        void Tick(float time, float deltaTime);
        void Clear();
    }
}
