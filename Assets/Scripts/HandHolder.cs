using CardScripts;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HandHolder : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private int handSize = 8;

    [SerializeField] Button playButton;
    [SerializeField] Button discardButton;

    bool _isCrossing;

    private List<Card> _cards = new();
    private List<Card> _selectedCards = new();

    private void Awake()
    {
        discardButton.onClick.AddListener(OnDiscardButtonPressed);

        UpdateButtonsState();
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
    }

    void UpdateButtonsState()
    {
        playButton.interactable = _selectedCards.Count > 0;
        discardButton.interactable = _selectedCards.Count > 0;
    }

    public void OnDiscardButtonPressed()
    {
        print("Discard");
        foreach (var selectedCard in _selectedCards)
        {
            selectedCard.Discard();
            _cards.Remove(selectedCard);
        }
        _selectedCards.Clear();

        UpdateButtonsState();
        DOVirtual.DelayedCall(0.5f, DrawCards);
    }

    private void DrawCards()
    {
        var count = handSize - _cards.Count;
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(cardPrefab, transform);
            var card = obj.GetComponent<Card>();
            var nameC = i.ToString();
            card.Initialize(nameC);
            _cards.Add(card);
        }
    }
}
