using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopLoadout
{
    
    public List<Card> cards;
    public List<Relic> relics;
    public List<Item> items;

    public ShopLoadout() {
        cards = new List<Card>();
        relics = new List<Relic>();
        items = new List<Item>();
    }

}
