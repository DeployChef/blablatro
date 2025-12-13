using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CardScripts
{
    public class Card : MonoBehaviour
    {

        [Header("Scale Parameters")]
        [SerializeField] private bool scaleAnimations = true;
        [SerializeField] private float scaleOnHover = 1.15f;
        [SerializeField] private float scaleOnSelect = 1.25f;
        [SerializeField] private float scaleTransition = .15f;
        [SerializeField] private Ease scaleEase = Ease.OutBack;

        [Header("Select Parameters")]
        [SerializeField] private float selectPunchAmount = 120;

        [Header("Hober Parameters")]
        [SerializeField] private float hoverPunchAngle = 5;
        [SerializeField] private float hoverTransition = .15f;

        [SerializeField] private Image bodyImage;
        [SerializeField] private Image valueImage;
        [SerializeField] private Image sealImage;

        public bool IsDragged { get; set; }
        public Vector3 HolderPosition { get; set; }
        public bool IsSelected { get; private set; }

        public Rank Rank { get; set; }
        public Suit Suit { get; set; }
        public int Value { get; set; }


        Canvas _canvas;

        void Awake()
        {
            HolderPosition = transform.localPosition;
            _canvas = GetComponentInParent<Canvas>();
        }

        public void Initialize(CardData cardData)
        {
            valueImage.sprite = cardData.artwork;
            Rank = cardData.rank;
            Suit = cardData.suit;
            Value = cardData.rank.ToGameValue();
        }

        public void Select()
        {
            IsSelected = true;
            AnimateSelect();
        }
        public void UnSelect()
        {
            IsSelected = false;
            AnimateSelect();
        }

        private void AnimateSelect()
        {
            DOTween.Kill(2, true);

            float targetY = HolderPosition.y + (IsSelected ? selectPunchAmount : 0f);

            transform.DOLocalMoveY(targetY, scaleTransition)
                .SetEase(scaleEase);

            transform.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);
            
            transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);
        }

        public void Discard()
        {
            // Отключаем взаимодействие
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            transform.DOKill(); // убить старые твины

            // 1. Вычисляем направление "вылета" (например, вверх-вправо относительно экрана)
            Vector2 centerScreen = new(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 offscreenPos = new(Screen.width * 1.5f, Screen.height * 1.5f); // за пределами
            Vector2 dir = (offscreenPos - centerScreen).normalized;

            // Берём текущую позицию карты в экранных координатах
            Vector2 cardScreenPos = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, transform.position);

            // Цель в экранных координатах
            Vector2 targetScreenPos = cardScreenPos + dir * Screen.height * 1.2f;

            // Конвертим цель обратно в локальные координаты Canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                targetScreenPos,
                _canvas.worldCamera,
                out Vector2 targetLocal
            );

            float duration = 0.5f;

            // 2. Анимация полёта
            transform.DOLocalMove(targetLocal, duration).SetEase(Ease.InQuad);

            // 3. Анимация вращения и лёгкого уменьшения
            transform
                .DORotate(new Vector3(0, 0, 360f), duration, RotateMode.FastBeyond360)
                .SetEase(Ease.InCubic);

            transform
                .DOScale(0.7f, duration)
                .SetEase(Ease.InCubic);

            // 4. Уничтожение по завершении
            DOVirtual.DelayedCall(duration, () =>
            {
                Destroy(gameObject);
            });
        }
    }
}
