using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RelicHandler))]
public class RelicBuyable : BuyableObject
{

    private RelicHandler _parentRelicHandler;

    public Relic Relic => _parentRelicHandler.relicInfo;

    private void Awake()
    {
        _parentRelicHandler = GetComponent<RelicHandler>();
    }

    public override Sprite GetSprite() => Relic.relicImage;
    public override bool CanPlayerBuyPrereq() => true;

    public override int GetCost() {
        return Relic.relicRarity switch
        {
            RelicRarity.COMMON => 160,
            RelicRarity.UNCOMMON => 200,
            RelicRarity.RARE => 250,
            _ => 99999,// Unobtainable!
        };
    }

    public override void OnBuyObject()
    {
        GameManager.AddRelicToInventory(Relic);
        TopBarController.Instance.RenderRelics();
    }
}
