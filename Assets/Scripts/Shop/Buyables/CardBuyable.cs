using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CardHandler))]
public class CardBuyable : BuyableObject
{

    private CardHandler _parentCardHandler;

    public Card Card => _parentCardHandler.card;

    private void Awake()
    {
        _parentCardHandler = GetComponent<CardHandler>();
        GetComponent<UITooltipHandler>().SetTooltipPosition(TooltipPosition.RIGHT);
    }

    public override Sprite GetSprite() => null;
    public override bool CanPlayerBuyPrereq() => true;

    public override int GetCost() {
        return Card.cardData.cardRarity switch
        {
            CardRarity.COMMON => 100,
            CardRarity.UNCOMMON => 150,
            CardRarity.RARE => 200,
            _ => 99999,// Unobtainable!
        };
    }

    public override void OnBuyObject()
    {
        TopBarController.Instance.AnimateCardsToDeck(transform.position, new List<Card> { Card }, _parentCardHandler.transform.localScale);
        _parentCardHandler.DisableInteractions();
        GameManager.AddCardToDeck(Card);
    }
}
