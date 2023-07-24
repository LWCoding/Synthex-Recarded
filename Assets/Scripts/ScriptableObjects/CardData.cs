using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Target
{
    NONE = 0, OTHER = 1, SELF = 2, PLAYER_AND_ENEMY = 3, ENEMY_ALL = 4
}

public enum Trait
{
    DAMAGE_IGNORES_BLOCK = 0, DRAW_CARDS = 1, MATERIALIZE_CARDS = 2,
    CLEANSE_ALL_DEBUFFS = 3, SECOND_WIND = 4, GAIN_ENERGY = 5, GAIN_HEALTH = 6,
    DEAL_DAMAGE_EQ_TO_BLOCK = 7, HEAL_ALL_ENEMIES = 8, SUMMON_ENEMY = 9,
    CLEAR_CARDS_IN_HAND = 10, POISON_CARDS = 11,
    EXHAUST = 12, ADDITIONAL_LUCK_DAMAGE = 13, TURN_ENEMY_BLEED_TO_BLOCK = 20,
    DOUBLE_ENEMY_BLEED = 21, CHEAT_CARD = 22,
    DRAIN_CHARGE = 33
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
    public bool UpgradeLevel()
    {
        if (level < cardData.GetMaxLevel())
        {
            level++;
            return true;
        }
        return false;
    }

    public bool IsMaxLevel() => level == cardData.GetMaxLevel();

    public bool HasTrait(Trait traitEnum)
    {
        return GetCardStats().modifiers.Find((m) => m.trait == traitEnum) != null;
    }

    public Target GetTarget()
    {
        Target damageTarget = GetCardStats().damageTarget;
        Target blockTarget = GetCardStats().blockTarget;
        int damageValue = GetCardStats().damageValue;
        int blockValue = GetCardStats().blockValue;
        // If the card is targeting enemy all, return ENEMY_ALL.
        if (damageTarget == Target.ENEMY_ALL) { return Target.ENEMY_ALL; }
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
        if (damageTarget != Target.NONE) { return damageTarget; }
        if (blockTarget != Target.NONE) { return blockTarget; }
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
public class CardModifier
{
    public Trait trait;
    public int amplifier = 0;
    public string special = "";
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
    public List<CardInflict> inflictions = new List<CardInflict>();
    public List<CardModifier> modifiers = new List<CardModifier>();
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
