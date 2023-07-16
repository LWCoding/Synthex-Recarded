using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeroTag
{
    ANY_HERO, JACK, RENO, RYAN
}

[System.Serializable]
public class Hero
{

    [Header("Hero Information")]
    public HeroData heroData;
    public int currentHealth;
    public int maxHealth;
    [Header("Inventory")]
    public List<Card> currentDeck = new List<Card>();
    public List<Relic> currentRelics = new List<Relic>();
    public List<Item> currentItems = new List<Item>();

    // Initialize a hero from scratch from its data.
    public void Initialize(HeroData data)
    {
        heroData = data;
        maxHealth = heroData.baseHealth;
        currentHealth = maxHealth;
        // Create a Card object for all cardData in the starting deck.
        currentDeck = new List<Card>();
        foreach (CardData cardData in heroData.startingDeck)
        {
            currentDeck.Add(new Card(cardData));
        }
        // Create a new Relic object for all relics in the starting relics.
        currentRelics = new List<Relic>();
        foreach (Relic relic in heroData.startingRelics)
        {
            currentRelics.Add(relic);
        }
        // Create a new Item object for all items in the starting items.
        currentItems = new List<Item>();
        foreach (Item item in heroData.startingItems)
        {
            currentItems.Add(item);
        }
    }

}

[System.Serializable]
[CreateAssetMenu(fileName = "Hero", menuName = "ScriptableObjects/HeroData")]
public class HeroData : Character
{

    [Header("Base Information")]
    public int baseHealth;
    public HeroTag heroTag;
    public int maxItemStorageSpace;
    [Header("UI Information")]
    public Sprite uiHeadshotSprite;
    public Color heroUIColor;
    [Header("Other Sprite Assignments")]
    public Sprite mapHeadshotSprite;
    [Header("Deck")]
    public List<CardData> startingDeck = new List<CardData>();
    [Header("Relics")]
    public List<Relic> startingRelics = new List<Relic>();
    [Header("Items")]
    public List<Item> startingItems = new List<Item>();

}
