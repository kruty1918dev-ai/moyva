using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// MonoBehaviour controller for the FogOfWar quad overlay.
    /// Sizes the quad to match the map, initialises the service and texture updater.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class FogQuadController : MonoBehaviour
    {
        [SerializeField] private FogOfWarSettings _settings;

        [Inject] private IFogOfWarService  _fogService;
        [Inject] private IFogTextureUpdater _textureUpdater;
        [Inject] private IGridService       _gridService;

        private void Start()
        {
            int w = _gridService != null ? _gridService.GridWidth  : 10;
            int h = _gridService != null ? _gridService.GridHeight : 10;

            // Position and scale the quad to cover the entire map
            transform.localScale = new Vector3(w, h, 1f);
            transform.position   = new Vector3((w - 1) * 0.5f, (h - 1) * 0.5f, -0.5f);

            var mr = GetComponent<MeshRenderer>();

            _fogService.Initialize(w, h);
            _textureUpdater.Initialize(w, h, mr.material);

            if (_settings != null)
                ApplySettingsToMaterial(mr.material);
        }

        private void ApplySettingsToMaterial(Material mat)
        {
            if (mat == null) return;

            mat.SetColor("_UnexploredColor", _settings.UnexploredFogColor);
            mat.SetColor("_ExploredColor",   _settings.ExploredFogColor);

            mat.SetFloat("_NoiseScaleA",    _settings.NoiseScaleUnexplored);
            mat.SetFloat("_NoiseSpeedA",    _settings.NoiseSpeedUnexplored);
            mat.SetFloat("_NoiseStrengthA", _settings.NoiseStrengthUnexplored);

            mat.SetFloat("_NoiseScaleB",    _settings.NoiseScaleExplored);
            mat.SetFloat("_NoiseSpeedB",    _settings.NoiseSpeedExplored);
            mat.SetFloat("_NoiseStrengthB", _settings.NoiseStrengthExplored);

            mat.SetFloat("_EdgeBleedRadius",   _settings.EdgeBleedRadius);
            mat.SetFloat("_EdgeBleedStrength", _settings.EdgeBleedStrength);
            mat.SetFloat("_TransitionSoftness", _settings.TransitionSoftness);
        }
    }
}
