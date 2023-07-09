using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RelicType
{
    GOLDEN_PAW = 0, SCALE_OF_JUSTICE = 1, THE_THINKER = 2, GRAPPLING_HOOK = 3,
    PLASMA_CORE = 4, GREEN_SCARF = 5, QUESTION_MARK = 6, DUMBELL = 7, KEVLAR_VEST = 8,
    MEDKIT = 9, VAMPIRE_TEETH = 10, POWERBANK = 11, LOUD_MEGAPHONE = 12
}

public enum RelicRarity
{
    COMMON, UNCOMMON, RARE, UNOBTAINABLE, PLACEHOLDER
}


[CreateAssetMenu(fileName = "Relic", menuName = "ScriptableObjects/Relic")]
public class Relic : ScriptableObject
{

    [Header("Base Information")]
    public string relicName;
    public string relicDesc;
    public Sprite relicImage;
    public RelicType type;
    [Header("Relic Unlock Information")]
    public RelicRarity relicRarity;

}