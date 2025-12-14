using System.Collections;
using System.Collections.Generic;
using CardScripts;
using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class PlayProcessor : MonoBehaviour
{
    [SerializeField] private GameObject cardSlotPrefab;
    [SerializeField] private Transform playPanel;
    [SerializeField] private ComboProcessor comboProcessor;
    [SerializeField] private ScoreCalculator scoreCalculator;
    [SerializeField] private TextMeshProUGUI scoreText;
    private List<GameObject> slots = new List<GameObject>();
    private List<Card> _cards = new List<Card>();
    float duration = 0.3f;
    private int _score = 0;

    public IEnumerator Play(List<Card> cards)
    {
        _cards = new List<Card>(cards);
        Clear();

        var tween = GetCardsToCenter(cards);

        yield return tween?.WaitForCompletion();

        yield return StartCoroutine(CalcRoutine());
    }

    private IEnumerator CalcRoutine()
    {
        print("Start Calc");
        var playableCards = comboProcessor.GetComboCards(_cards);
        foreach (var card in playableCards)
        {
            card.Punch();
            scoreCalculator.AppendChips(card.Value);
            yield return new WaitForSeconds(.4f);
        }
        yield return new WaitForSeconds(2);
        print("End calc");
    }

    private Tween GetCardsToCenter(List<Card> cards)
    {
        Tween lastTween = null;
        foreach (var card in cards)
        {
            var slotObj = Instantiate(cardSlotPrefab, playPanel);
            var slotRect = slotObj.GetComponent<RectTransform>();
            slots.Add(slotObj);

            var cardRect = card.GetComponent<RectTransform>();

            cardRect.SetParent(slotRect, worldPositionStays: true);

            // гарантируем те же настройки, что и в дизайнере
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);

            // центр слота
          
            cardRect.localScale = Vector3.one;
            cardRect.localRotation = Quaternion.identity;

            // Анимация в центр слота
            lastTween = cardRect.DOAnchorPos(Vector2.zero, duration)
                .SetEase(Ease.OutQuad);
        }

        return lastTween;
    }

    private void Clear()
    {
        foreach (var slot in slots)
        {
            Destroy(slot);
        }
        slots.Clear();
    }
}
