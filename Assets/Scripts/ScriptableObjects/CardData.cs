using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Target
{
    NONE = 0, OTHER = 1, SELF = 2, PLAYER_AND_ENEMY = 3, OTHER_ALL = 4
}

public enum Trait
{
    DAMAGE_IGNORES_BLOCK = 0, DRAW_ONE_CARD = 1, DRAW_TWO_CARDS = 2,
    CLEANSE_ALL_DEBUFFS = 3, SECOND_WIND = 4, MATERIALIZE_TWO_CARDS = 5,
    DEAL_DAMAGE_EQ_TO_BLOCK = 7,
    _EXHAUST = 12, DRAW_THREE_CARDS = 13,
    GAIN_TWO_ENERGY = 18, HEAL_SIX_HEALTH = 19, TURN_ENEMY_BLEED_TO_BLOCK = 20,
    DOUBLE_ENEMY_BLEED = 21, SALVAGE_NEW_CARDS = 26,
    POISON_TWO_CARDS = 29,
    POISON_THREE_CARDS = 30,
    DRAIN_CHARGE = 33, HEAL_SIXTEEN_HEALTH = 34
}

public enum Movement
{
    NO_MOVEMENT = 0, SHORT_DASH_FORWARD = 1, SHOOT_PROJECTILE = 2
}

public enum CardRarity
{
    COMMON, UNCOMMON, RARE, UNOBTAINABLE
}

public enum CardType
{
    ATTACKER, BLOCKER, SPECIAL_ATTACKER, SPECIAL_BLOCKER, SPECIAL_MISC
}

[System.Serializable]
public class Card
{
    public CardData cardData;
    public int level = 1;

    public Card(CardData cd, int lvl = 1)
    {
        cardData = cd;
        level = lvl;
    }

    // Returns the CardStats that correspond to the card's
    // current level. We subtract by one because level one
    // technically means the zero-th indexed CardStats.
    public CardStats GetCardStats()
    {
        return cardData.GetCardStats(level - 1);
    }

    // Upgrades the card, if possible. Returns a boolean
    // representing whether the operation was successful or not.
    public bool Upgrade()
    {
        if (level < cardData.GetMaxLevel())
        {
            level++;
            return true;
        }
        return false;
    }

    public bool HasTrait(Trait traitEnum)
    {
        return GetCardStats().traits.Contains(traitEnum);
    }

    public Target GetTarget()
    {
        Target damageTarget = GetCardStats().damageTarget;
        Target blockTarget = GetCardStats().blockTarget;
        int damageValue = GetCardStats().damageValue;
        int blockValue = GetCardStats().blockValue;
        // If the card is targeting other all, return OTHER ALL.
        if (damageTarget == Target.OTHER_ALL)
        {
            return Target.OTHER_ALL;
        }
        // If either target is targeting all, return ALL.
        if (damageTarget == Target.PLAYER_AND_ENEMY || blockTarget == Target.PLAYER_AND_ENEMY)
        {
            return Target.PLAYER_AND_ENEMY;
        }
        // If targets are targeting separate characters, return ALL.
        if (damageTarget != blockTarget && damageTarget != Target.NONE && blockTarget != Target.NONE)
        {
            return Target.PLAYER_AND_ENEMY;
        }
        // Or else, return the selected target.
        if (damageTarget != Target.NONE)
        {
            return damageTarget;
        }
        if (blockTarget != Target.NONE)
        {
            return blockTarget;
        }
        return Target.NONE;
    }
}

public enum EffectRenderOrder
{
    RENDER_EFFECT_BEFORE_ATTACK,
    RENDER_EFFECT_AFTER_ATTACK
}

[System.Serializable]
public class CardInflict
{
    public Target target = Target.NONE;
    public Effect effect = Effect.NONE;
    public int amplifier = 0;
    public EffectRenderOrder renderBehavior = EffectRenderOrder.RENDER_EFFECT_BEFORE_ATTACK;
}

[System.Serializable]
public class CardStats
{
    public string cardDesc = "Test description";
    public int cardCost;
    public Target damageTarget;
    public int damageValue;
    public Target blockTarget;
    public int blockValue;
    public int attackRepeatCount = 1;
    public List<CardInflict> inflictedEffects = new List<CardInflict>();
    public List<Trait> traits = new List<Trait>();
}

[CreateAssetMenu(fileName = "Card", menuName = "ScriptableObjects/Card")]
public class CardData : ScriptableObject
{

    [Header("Base Information")]
    [SerializeField] private string cardName;
    public string cardDisplayName;
    public Sprite cardImage;
    public CardType cardType;
    [Header("Character Movement")]
    public Movement characterMovement;
    [Header("Projectile Information (opt)")]
    public Sprite projectileSprite;
    public Vector2 projectileOffset;
    [Tooltip("The particles that play when the projectile is spawned")]
    public ParticleInfo sourceParticleInfo;
    [Tooltip("The particles that play when the projectile hits a target")]
    public ParticleInfo destinationParticleInfo;
    [Header("Card Unlock Information")]
    public CardRarity cardRarity;
    public HeroTag cardExclusivity;
    [Header("Card Audio Assignments")]
    public AudioClip onPlaySFX = null;
    [Range(0.0f, 2.0f)]
    public float onPlaySFXVolume = 1;
    [Header("Card Stats")]
    [SerializeField] private List<CardStats> cardStats = new List<CardStats>();
    public CardStats GetCardStats(int level) => cardStats[level];
    public int GetMaxLevel() => cardStats.Count;
    public string GetCardUniqueName() => cardName;
    public string GetCardDisplayName() => cardDisplayName == "" ? GetCardUniqueName() : cardDisplayName;

}
