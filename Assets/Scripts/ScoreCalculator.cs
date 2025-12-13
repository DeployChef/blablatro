using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class CurrentScore : MonoBehaviour
{
    private int chips;
    [SerializeField] private TextMeshProUGUI chipsText;
    private int mult;
    [SerializeField] private TextMeshProUGUI multText;

    [Header("Shake settings")]
    [SerializeField] float shakeScale = 1.2f;
    [SerializeField] float shakeDuration = 0.2f;
    [SerializeField] float shakeMoveY = 15f;
    [SerializeField] float maxShakeAngle = 15f;

    void Start()
    {
        if (chipsText)
            chipsText.text = chips.ToString();

        if (multText)
            multText.text = mult.ToString();
    }

    public void SetBasicScore(int newChips, int newMult)
    {
        chips = newChips;
        mult = newMult;

        if (chipsText)
        {
            chipsText.text = chips.ToString();
            AnimateText(chipsText.rectTransform);
        }

        if (multText)
        {
            multText.text = mult.ToString();
            AnimateText(multText.rectTransform);
        }
    }

    public void AppendChips(int addChip)
    {
        chips += addChip;

        if (!chipsText)
            return;

        chipsText.text = chips.ToString();
        AnimateText(chipsText.rectTransform);
    }

    public void AppendMult(int addMult)
    {
        mult += addMult;

        if (!multText)
            return;

        multText.text = mult.ToString();
        AnimateText(multText.rectTransform);
    }

    void AnimateText(RectTransform rect)
    {
        rect.DOKill();

        Vector3 basePos = rect.anchoredPosition;
        Vector3 baseScale = rect.localScale;
        Quaternion baseRot = rect.localRotation;

        float duration = 0.25f;

        // Позиционный шейк (как будто тряхнули железку)
        rect.DOShakeAnchorPos(
            duration,
            strength: new Vector2(20f, 15f),   // мощность по X/Y
            vibrato: 25,                       // сколько «дрожаний»
            randomness: 90f,                   // хаотичность
            snapping: false,
            fadeOut: true
        );

        // Вращение туда‑сюда
        rect.DOShakeRotation(
            duration,
            strength: new Vector3(0f, 0f, 25f), // до ~25 градусов по Z
            vibrato: 30,
            randomness: 90f,
            fadeOut: true
        );

        // Лёгкий скейл с возвратом
        rect.DOScale(baseScale * 1.2f, duration * 0.5f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                rect.anchoredPosition = basePos;
                rect.localScale = baseScale;
                rect.localRotation = baseRot;
            });
    }
}
