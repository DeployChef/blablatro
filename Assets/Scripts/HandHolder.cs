using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class HandHolder : MonoBehaviour
{
    [SerializeField] private GameObject _cardPrefab;

    [SerializeField] private int _countToGenerate;

    bool _isCrossing = false;

    private List<Card> _cards = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < _countToGenerate; i++)
        {
            var obj = Instantiate(_cardPrefab, transform);
            var card = obj.GetComponent<Card>();
            var nameC = i.ToString();
            card.Initialize(nameC);
            _cards.Add(card);
        }
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
}
