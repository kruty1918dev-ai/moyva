using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Floating
{
    /// <summary>
    /// Thin MonoBehaviour pump for <see cref="WaterFloatService"/>; the service
    /// creates one lazily when the first floater registers. All motion logic
    /// lives in the service so it stays unit-testable.
    /// </summary>
    internal sealed class WaterFloatDriver : MonoBehaviour
    {
        private WaterFloatService _service;

        public void Initialize(WaterFloatService service)
        {
            _service = service;
        }

        private void Update()
        {
            _service?.Tick(Time.time, Time.deltaTime);
        }
    }
}
