using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CardScripts
{
    public class CardHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Card card;

        [Header("Scale Parameters")]
        [SerializeField] private bool scaleAnimations = true;
        [SerializeField] private float scaleOnHover = 1.15f;
        [SerializeField] private float scaleOnSelect = 1.25f;
        [SerializeField] private float scaleTransition = .15f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        [Header("Hover Parameters")]
        [SerializeField] private float hoverPunchAngle = 5;
        [SerializeField] private float hoverTransition = .15f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

            DOTween.Kill(2, true);
            transform.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(!card.IsSelected)
                transform.DOScale(1, scaleTransition).SetEase(scaleEase);
        }
    }
}
