using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Перемикає візуал воріт між зачиненим і відчиненим станом.
    /// Логічний стан зберігає <see cref="WallTopologyService"/>; цей компонент
    /// лише активує відповідний меш у префабі воріт.
    /// </summary>
    public sealed class GateVisualStateView : MonoBehaviour
    {
        [SerializeField] private GameObject _closedRoot;
        [SerializeField] private GameObject _openRoot;

        public void SetOpen(bool isOpen)
        {
            if (_closedRoot != null)
                _closedRoot.SetActive(!isOpen);
            if (_openRoot != null)
                _openRoot.SetActive(isOpen);
        }
    }
}
