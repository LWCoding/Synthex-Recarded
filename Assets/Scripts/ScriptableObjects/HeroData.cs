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
        // Create a new Relic object for all relicData in the starting relics.
        currentRelics = new List<Relic>();
        foreach (Relic relic in heroData.startingRelics)
        {
            currentRelics.Add(relic);
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
    [Header("Cosmetic Information")]
    public Sprite heroHeadshotSprite;
    public Color heroUIColor;
    [Header("Death Animation")]
    public Sprite deathSprite;
    [Header("Deck")]
    public List<CardData> startingDeck = new List<CardData>();
    [Header("Relics")]
    public List<Relic> startingRelics = new List<Relic>();

}
