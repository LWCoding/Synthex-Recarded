using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Effect
{
    NONE = -1,
    BLEED = 0, // Reduces HP equal to amount every turn. Lowers by one every turn. Ignores block.
    STRENGTH = 1, // Deals additional damage with each attack.
    DEFENSE = 2, // Improves block gained from direct blockers.
    PERSEVERE = 3, // This character does not lose block at start of turn. Lowers by one every turn.
    FOCUS = 4, // This character gets +1 energy at the start of each turn. Lowers by one every turn.
    DOUBLE_TAKE = 5, // The next card will play twice
    SHARPEN = 6, // All attacks inflict extra bleed each turn.
    LUCKY_DRAW = 7, // Draw extra cards at the start of each turn.
    CATASTROPHE = 8, // Deal X damage to enemy after playing a card.
    COMBO = 9, // Deal +3 damage per duplicated attack card
    DISEASE = 10, // Take X more damage from all sources,
    CRIPPLED = 11, // Deal X less damage with all direct attacks
    GROWTH = 12, // Gain X strength everytime this character takes damage
    VOLATILE = 13, // Deal 30 damage to the hero in X turns
    CHARGE = 14, // Do nothing this turn
    BARRIER = 15, // Gain X block when you take unblocked damage
    REFLECT = 16, // If attacked, deal X damage back to the aggressor
    LUCK = 17 // Allows Ryan to have special effects with cards
}

public enum EffectFaction
{
    NONE, BUFF, DEBUFF, CHARGE
}

[System.Serializable]
public class StatusEffect
{

    public Status statusInfo;
    public int amplifier { get; private set; }
    public bool shouldActivate = true;
    public string specialValue;

    public StatusEffect(Status s, int amp, string special = "na")
    {
        statusInfo = s;
        amplifier = amp;
        specialValue = special;
    }

    public void ChangeCount(int amp)
    {
        amplifier += amp;
        if (!statusInfo.canGoNegative && amplifier < 0)
        {
            amplifier = 0;
        }
    }

    public bool IsActive()
    {
        return amplifier != 0;
    }

}

[CreateAssetMenu(fileName = "Status", menuName = "ScriptableObjects/Status")]
public class Status : ScriptableObject
{

    public string statusName;
    public string statusDescription;
    public Effect type;
    public Sprite statusIcon;
    public Vector2 iconSpriteScale = new Vector2(0.5f, 0.5f);
    public EffectFaction effectFaction;
    public bool decrementEveryTurn = false;
    public bool canGoNegative = false;

}