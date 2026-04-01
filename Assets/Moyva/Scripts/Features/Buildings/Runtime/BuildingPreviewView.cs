using Kruty1918.Moyva.Buildings.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// MonoBehaviour-компонент попереднього перегляду будівлі на тайлі.
    /// Відображає спрайт будівлі із нормальним кольором (можна розмістити)
    /// або червоним підсвічуванням (позиція заблокована).
    /// Підписується на TileClickedSignal для ініціювання розміщення.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BuildingPreviewView : MonoBehaviour
    {
        [SerializeField] private Color _validColor   = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0.2f, 0.2f, 0.6f);

        private SpriteRenderer _spriteRenderer;

        [Inject] private IBuildingPlacementService _placementService;
        [Inject] private SignalBus _signalBus;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            gameObject.SetActive(false);
        }

        private void Start()
        {
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<TileClickedSignal>(OnTileClicked);
        }

        private void OnTileClicked(TileClickedSignal signal)
        {
            if (!_placementService.IsPlacingMode) return;
            _placementService.PlaceBuilding(signal.Position);
        }

        /// <summary>
        /// Оновлює позицію та колір попереднього перегляду.
        /// Викликається зовнішньою системою (наприклад, при наведенні курсору / дотику).
        /// </summary>
        public void UpdatePreview(Vector2Int hoveredPosition, Sprite buildingSprite)
        {
            if (!_placementService.IsPlacingMode)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            transform.position = new Vector3(hoveredPosition.x, hoveredPosition.y, -0.1f);

            _spriteRenderer.sprite = buildingSprite;
            _spriteRenderer.color = _placementService.CanPlaceAt(hoveredPosition)
                ? _validColor
                : _invalidColor;
        }

        /// <summary>Сховати попередній перегляд.</summary>
        public void HidePreview()
        {
            gameObject.SetActive(false);
        }
    }
}
