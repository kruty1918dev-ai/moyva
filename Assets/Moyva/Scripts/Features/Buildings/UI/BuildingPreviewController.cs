using Kruty1918.Moyva.Buildings.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.UI
{
    /// <summary>
    /// Контролер попереднього перегляду будівлі.
    /// Показує привид (ghost) будівлі на тайлі під курсором під час режиму розміщення.
    /// Білий/напівпрозорий спрайт — місце вільне; червоний — місце зайняте або недійсне.
    ///
    /// Використання: Додайте компонент до порожнього GameObject з SpriteRenderer у сцені.
    /// </summary>
    public class BuildingPreviewController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _previewRenderer;

        [Tooltip("Колір при допустимому розміщенні (напівпрозорий білий)")]
        [SerializeField] private Color _validColor = new Color(1f, 1f, 1f, 0.7f);

        [Tooltip("Колір при недопустимому розміщенні (напівпрозорий червоний)")]
        [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.7f);

        [Inject] private IBuildingPlacementService _placementService;
        [Inject] private BuildingRegistrySO _buildingRegistry;
        [Inject] private SignalBus _signalBus;

        private void Start()
        {
            _signalBus.Subscribe<TileHoveredSignal>(OnTileHovered);
            _signalBus.Subscribe<BuildingConstructionConfirmedSignal>(OnPlacementEnded);
            _signalBus.Subscribe<BuildingConstructionCanceledSignal>(OnPlacementEnded);
            HidePreview();
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<TileHoveredSignal>(OnTileHovered);
            _signalBus.Unsubscribe<BuildingConstructionConfirmedSignal>(OnPlacementEnded);
            _signalBus.Unsubscribe<BuildingConstructionCanceledSignal>(OnPlacementEnded);
        }

        private void OnTileHovered(TileHoveredSignal signal)
        {
            if (!_placementService.IsPlacementModeActive)
            {
                HidePreview();
                return;
            }

            var config = _buildingRegistry.GetConfig(_placementService.ActiveBuildingTypeId);
            if (config == null || config.PreviewSprite == null)
            {
                HidePreview();
                return;
            }

            _previewRenderer.sprite = config.PreviewSprite;
            transform.position = new Vector3(signal.Position.x, signal.Position.y, -0.1f);
            _previewRenderer.color = _placementService.CanPlace(signal.Position) ? _validColor : _invalidColor;
            _previewRenderer.enabled = true;
        }

        private void OnPlacementEnded(BuildingConstructionConfirmedSignal _) => HidePreview();
        private void OnPlacementEnded(BuildingConstructionCanceledSignal _) => HidePreview();

        private void HidePreview()
        {
            if (_previewRenderer != null)
                _previewRenderer.enabled = false;
        }
    }
}
