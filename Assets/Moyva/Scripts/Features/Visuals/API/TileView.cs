using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Visuals
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _occupiedColor;

        [Inject] private SignalBus _signalBus;
        [Inject] private IObjectsMapService _objectsMapService;

        private void Start()
        {
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(OnObjectsMapChanged);
        }

        private void OnObjectsMapChanged(OnObjectsMapChangedSignal signal)
        {
            if (!IsMinePosition(signal.Position)) return;

            if (signal.OccupantId != null)
                Occupy();
            else
                Vacate();
        }

        public void Occupy()
        {
            _spriteRenderer.color = _occupiedColor;
        }

        public void Vacate()
        {
            _spriteRenderer.color = Color.white;
        }

        private bool IsMinePosition(Vector2Int position)
        {
            return position == new Vector2Int((int)transform.position.x, (int)transform.position.y);
        }

        public void Setup(Vector2Int position)
        {
            transform.position = new Vector3(position.x, position.y, 0);
        }
    }
}