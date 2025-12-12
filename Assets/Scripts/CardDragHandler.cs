using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform rectTransform;
    Canvas canvas;                    // Canvas, в котором лежит панель
    CanvasGroup canvasGroup;
    Transform originalParent;
    int originalSiblingIndex;

    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Hover Parameters")]
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Найти ближайший Canvas наверху
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Поднять над остальными
        transform.SetParent(canvas.transform, true);

        // Сделать полупрозрачной и не ловить лучи под собой
        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Двигаем карту по экранным координатам
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPoint);

        rectTransform.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Вернуть в панель
        transform.SetParent(originalParent, false);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Найдём новое место по X среди карт панели
        int newIndex = 0;
        for (int i = 0; i < originalParent.childCount; i++)
        {
            Transform child = originalParent.GetChild(i);
            if (child == transform) continue;

            if (rectTransform.position.x > child.position.x)
                newIndex = i + 1;
        }

      
        transform.SetSiblingIndex(newIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
        transform.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1, scaleTransition).SetEase(scaleEase);
    }
}
