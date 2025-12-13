using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace CardScripts
{
    public class CardDragHandler : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Card card;

        RectTransform _rectTransform;
        Canvas _canvas;
        CanvasGroup _canvasGroup;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            card.HolderPosition = _rectTransform.localPosition;

            _canvasGroup.alpha = 0.8f;
            _canvasGroup.blocksRaycasts = false;
            card.IsDragged = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform parentRect = _rectTransform.parent as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                _canvas.worldCamera,
                out Vector2 localPoint);

            _rectTransform.localPosition = localPoint;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            transform.DOLocalMove(card.HolderPosition, .3f).SetEase(Ease.OutBack);
            card.IsDragged = false;
        }
    }
}
