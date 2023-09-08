using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ItemHandler))]
public class ItemBuyable : BuyableObject
{

    private ItemHandler _parentItemHandler;

    public Item Item => _parentItemHandler.itemInfo;

    private void Awake()
    {
        _parentItemHandler = GetComponent<ItemHandler>();
    }

    public override Sprite GetSprite() => Item.itemImage;
    public override bool CanPlayerBuyPrereq() => !GameManager.IsItemBagFull();

    public override int GetCost() {
        return Item.itemRarity switch
        {
            ItemRarity.COMMON => 60,
            ItemRarity.UNCOMMON => 90,
            ItemRarity.RARE => 120,
            _ => 99999,// Unobtainable!
        };
    }

    public override void OnBuyObject()
    {
        GameManager.AddItemToInventory(Item);
        TopBarController.Instance.RenderItems();
        TopBarController.Instance.FlashItemObject(GameManager.GetItems().Count - 1);
    }
}
