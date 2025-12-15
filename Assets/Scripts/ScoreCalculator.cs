using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    private int chips;
    [SerializeField] private TextMeshProUGUI chipsText;
    private int mult;
    [SerializeField] private TextMeshProUGUI multText;
    private int score;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;

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

    public IEnumerator CalculateScore()
    {
        int result = chips * mult;
        score += result;

        if (!comboText || !scoreText)
            yield break;

        // 1. Показываем результат умножения в comboText
        comboText.DOKill();
        scoreText.DOKill();

        RectTransform comboRect = comboText.rectTransform;
        RectTransform scoreRect = scoreText.rectTransform;

        string finalCombo = result.ToString();
        string finalScore = score.ToString();

        comboText.text = finalCombo;

        // небольшое подпрыгивание цифры в combo
        Vector3 comboBasePos = comboRect.anchoredPosition;
        Vector3 comboBaseScale = comboRect.localScale;

        Tween comboJump = comboRect
            .DOAnchorPosY(comboBasePos.y + 30f, 0.2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                comboRect.DOAnchorPosY(comboBasePos.y, 0.2f)
                         .SetEase(Ease.InQuad);
            });

        comboRect.DOScale(comboBaseScale * 1.2f, 0.2f)
                 .SetLoops(2, LoopType.Yoyo)
                 .SetEase(Ease.OutQuad);

        yield return comboJump.WaitForCompletion(); // ждём «подпрыгивания»

        // 2. «Переливаем» число из combo в score:
        // делаем клон текста, который летит в score
        GameObject flyObj = new GameObject("ScoreFly");
        flyObj.transform.SetParent(comboRect.parent, worldPositionStays: false);

        var flyRect = flyObj.AddComponent<RectTransform>();
        flyRect.anchorMin = flyRect.anchorMax = comboRect.anchorMin;
        flyRect.pivot = comboRect.pivot;
        flyRect.position = comboRect.position;

        var flyTMP = flyObj.AddComponent<TextMeshProUGUI>();
        flyTMP.font = comboText.font;
        flyTMP.fontSize = comboText.fontSize;
        flyTMP.alignment = comboText.alignment;
        flyTMP.text = finalCombo;

        Tween flyTween = flyRect
            .DOMove(scoreRect.position, 0.4f)
            .SetEase(Ease.InQuad);

        flyTMP.DOFade(0f, 0.4f);

        yield return flyTween.WaitForCompletion();

        Destroy(flyObj);

        // 3. Обновляем scoreText и анимируем его
        scoreText.text = finalScore;
        AnimateText(scoreRect);
    }

}
