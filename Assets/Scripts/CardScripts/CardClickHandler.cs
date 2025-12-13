using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CardScripts
{
    public class CardClickHandler : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Card card;
        private HandHolder _hand;

        public void Awake()
        {
            _hand = GetComponentInParent<HandHolder>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(_hand)
                _hand.ToggleCard(card);
        }
    }
}
