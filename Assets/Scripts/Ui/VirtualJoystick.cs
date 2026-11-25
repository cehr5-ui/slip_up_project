using UnityEngine;
using UnityEngine.EventSystems;

namespace SlipUp
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _handle;
        [SerializeField] private float _range = 90f;

        private Vector2 _value;

        public Vector2 Value => _value;

        internal void Bind(RectTransform root, RectTransform handle, float range)
        {
            _root = root;
            _handle = handle;
            _range = range;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _root,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);

            if (localPoint.sqrMagnitude > _range * _range)
            {
                localPoint = localPoint.normalized * _range;
            }

            _handle.anchoredPosition = localPoint;
            _value = localPoint / _range;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _handle.anchoredPosition = Vector2.zero;
            _value = Vector2.zero;
        }
    }
}
