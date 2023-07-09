using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffectType
{
    POISON
}

[CreateAssetMenu(fileName = "CardEffect", menuName = "ScriptableObjects/CardEffect")]
public class CardEffect : ScriptableObject
{

    [Header("Base Information")]
    public CardEffectType type;
    public Sprite sprite;

}
