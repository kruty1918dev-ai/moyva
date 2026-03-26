using UnityEngine;

namespace Kruty1918.Moyva.DTO
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _occupiedColor;

        public void Occupy()
        {
            _spriteRenderer.color = _occupiedColor;
        }

        public void Vacate()
        {
            _spriteRenderer.color = Color.white;
        }
    }
}