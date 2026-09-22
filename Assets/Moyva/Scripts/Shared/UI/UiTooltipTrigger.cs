using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using Kruty1918.UiFoundation;

namespace Kruty1918.Moyva.Shared.UI
{
    [DisallowMultipleComponent]
    public sealed class UiTooltipTrigger : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler
    {
        [SerializeField, TextArea] private string _text;
        [SerializeField] private RectTransform _anchor;

        private IUiTooltipService _service;

        [Inject]
        public void Construct(IUiTooltipService service)
        {
            _service = service;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _service?.Request(new UiTooltipRequest(
                this,
                _text,
                eventData.position,
                _anchor != null ? _anchor : transform as RectTransform));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _service?.Hide(this);
        }

        public void OnSelect(BaseEventData eventData)
        {
            _service?.Request(new UiTooltipRequest(
                this,
                _text,
                Vector2.zero,
                _anchor != null ? _anchor : transform as RectTransform,
                immediate: true));
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _service?.Hide(this);
        }
    }

}
