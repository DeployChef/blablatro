using System;
using System.Collections.Generic;
using System.Linq;
using CardScripts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class Deck : MonoBehaviour
{
    [SerializeField] private List<CardData> initDeck;
    [SerializeField] private TextMeshProUGUI cardCountText;

    public List<CardData> gameDeck { get; private set; } = new();
    public Queue<CardData> roundDeck { get; private set; } = new();

    private void Awake()
    {
        gameDeck = new List<CardData>(initDeck);
        var roundList = gameDeck.ToList();
        roundList.Shuffle();
        roundDeck = new Queue<CardData>(roundList);
    }

    public IEnumerable<CardData> Draw(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (roundDeck.Any())
            {
                var item = roundDeck.Dequeue();
                    UpdateCardCount();
                yield return item;
            }
        }
    }

    private void UpdateCardCount()
    {
        if (cardCountText)
            cardCountText.text = $"{roundDeck.Count}/{gameDeck.Count}";
    }
}
