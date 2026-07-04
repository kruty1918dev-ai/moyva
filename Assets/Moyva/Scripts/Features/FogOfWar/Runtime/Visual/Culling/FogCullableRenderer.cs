using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Stores renderer visibility state while fog culling temporarily hides it.
    /// </summary>
    internal sealed class FogCullableRenderer
    {
        public FogCullableRenderer(Renderer renderer)
        {
            Renderer = renderer;
            _enabledBeforeFog = renderer != null && renderer.enabled;
        }

        public Renderer Renderer { get; }

        private bool _hiddenByFog;
        private bool _enabledBeforeFog;

        public void SetHiddenByFog(bool hidden)
        {
            if (Renderer == null)
                return;

            if (hidden)
            {
                if (!_hiddenByFog)
                {
                    _enabledBeforeFog = Renderer.enabled;
                    _hiddenByFog = true;
                }

                Renderer.enabled = false;
                return;
            }

            Restore();
        }

        public void Restore()
        {
            if (!_hiddenByFog || Renderer == null)
                return;

            Renderer.enabled = _enabledBeforeFog;
            _hiddenByFog = false;
        }
    }
}
