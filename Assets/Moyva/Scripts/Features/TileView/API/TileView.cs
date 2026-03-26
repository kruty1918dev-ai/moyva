using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tiles
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _occupiedColor;

        [Inject] private SignalBus _signalBus;

        public void Occupy()
        {
            _spriteRenderer.color = _occupiedColor;
        }

        public void Vacate()
        {
            _spriteRenderer.color = Color.white;
        }

        private void OnMouseDown()
        {
            _signalBus.Fire(new TileClickedSignal { Position = new Vector2Int((int)transform.position.x, (int)transform.position.y) });
        }
    }
}