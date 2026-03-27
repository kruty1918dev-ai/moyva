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

        private void Start()
        {
            _signalBus.Subscribe<OnTileChanged>(OnTileChanged);
        }

        private void OnTileChanged(OnTileChanged signal)
        {
            // Якщо позиція зміни належить цьому TileView
            // Ми можемо отримати інформацію про те, чи зайнята ця позиція, і оновити візуал відповідно
            // Для спрощення, припустимо, що ми просто змінюємо колір, якщо позиція зайнята
            // Якщо з тайл дата видно чи є окупант можна виконувати відповідно дію, якщо його немає відповідно також дія
        
            if (IsMinePosition(signal.Position)) 
            {
                Occupy();
            }
        }

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
            Debug.Log($"Tile clicked at position: {transform.position}");
            _signalBus.Fire(new TileClickedSignal { Position = new Vector2Int((int)transform.position.x, (int)transform.position.y) });
        }

        private bool IsMinePosition(Vector2Int position)
        {
            return position == new Vector2Int((int)transform.position.x, (int)transform.position.y);
        }
    }
}