using CardScripts;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HandHolder : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private int handSize = 8;

    [SerializeField] private int hands = 4;
    [SerializeField] private int discards = 3;

    [SerializeField] Button playButton;
    [SerializeField] Button discardButton;

    [SerializeField] ComboProcessor comboProcessor;
    [SerializeField] Deck deck;

    [SerializeField] TextMeshProUGUI handsText;
    [SerializeField] TextMeshProUGUI discardsText;

    bool _isCrossing;

    private List<Card> _cards = new();
    private List<Card> _selectedCards = new();

    private void Awake()
    {
        discardButton.onClick.AddListener(OnDiscardButtonPressed);

        UpdateButtonsState();

        handsText.text = hands.ToString();
        discardsText.text = discards.ToString();
    }

    void OnDestroy()
    {
        if (discardButton)
            discardButton.onClick.RemoveListener(OnDiscardButtonPressed);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrawCards();
    }

    // Update is called once per frame
    void Update()
    {
        if(_isCrossing)
            return;

        var selectedCard = _cards.FirstOrDefault(c => c.IsDragged);
        if (!selectedCard)
            return;

        for (int i = 0; i < _cards.Count; i++)
        {

            if (selectedCard.transform.position.x > _cards[i].transform.position.x)
            {
                if (selectedCard.transform.GetSiblingIndex() < _cards[i].transform.GetSiblingIndex())
                {
                    Swap(selectedCard, _cards[i]);
                    break;
                }
            }

            if (selectedCard.transform.position.x < _cards[i].transform.position.x)
            {
                if (selectedCard.transform.GetSiblingIndex() > _cards[i].transform.GetSiblingIndex())
                {
                    Swap(selectedCard, _cards[i]);
                    break;
                }
            }
        }
    }

    void Swap(Card from, Card to)
    {
        _isCrossing = true;

        var index = to.transform.GetSiblingIndex();
        var position = to.transform.localPosition;
        from.transform.SetSiblingIndex(index);
        from.HolderPosition = position;

        _isCrossing = false;
    }

    public void ToggleCard(Card card)
    {
        print($"Toggle {card.name}");

        if (card.IsSelected)
        {
            card.UnSelect();
            _selectedCards.Remove(card);
        }
        else
        {
            if(_selectedCards.Count >= 5)
                return;

            card.Select();
            _selectedCards.Add(card);
        }

        UpdateButtonsState();
        comboProcessor.SelectedChanged(_selectedCards);
    }

    void UpdateButtonsState()
    {
        playButton.interactable = _selectedCards.Count > 0;
        discardButton.interactable = _selectedCards.Count > 0 && discards > 0;
    }

    public void OnDiscardButtonPressed()
    {

        print("Discard");

        discards--;

        AnimateDiscardsChange(-1);

        foreach (var selectedCard in _selectedCards)
        {
            selectedCard.Discard();
            _cards.Remove(selectedCard);
        }
        _selectedCards.Clear();

        UpdateButtonsState();
        comboProcessor.SelectedChanged(_selectedCards);
        DOVirtual.DelayedCall(0.5f, DrawCards);
    }

    private void DrawCards()
    {
        var count = handSize - _cards.Count;
        var cards = deck.Draw(count);
        foreach (var cardData in cards)
        {
            var obj = Instantiate(cardPrefab, transform);
            var card = obj.GetComponent<Card>();
            card.Initialize(cardData);
            _cards.Add(card);
        }
    }

    void AnimateDiscardsChange(int delta)
    {
        if (!discardsText)
            return;

        RectTransform rect = discardsText.rectTransform;

        // текущий реальный текст (после изменения счётчика)
        string finalText = discards.ToString();

        // сначала показываем "-1"
        discardsText.text = delta.ToString();

        rect.DOKill();
        Vector3 basePos = rect.anchoredPosition;
        Vector3 baseScale = rect.localScale;
        Quaternion baseRot = rect.localRotation;

        float duration = 0.25f;

        // жёсткий “металлический” shake
        rect.DOShakeAnchorPos(duration,
            strength: new Vector2(15f, 10f),
            vibrato: 25,
            randomness: 90f,
            fadeOut: true);

        rect.DOShakeRotation(duration,
            strength: new Vector3(0f, 0f, 25f),
            vibrato: 30,
            randomness: 90f,
            fadeOut: true);

        rect.DOScale(baseScale * 1.25f, duration * 0.5f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutQuad);

        // по окончании шейка вернуть текст и трансформ
        DOVirtual.DelayedCall(duration, () =>
        {
            discardsText.text = finalText;
            rect.anchoredPosition = basePos;
            rect.localScale = baseScale;
            rect.localRotation = baseRot;
        });
    }
}
