using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    NONE = -1, CAT_FOOD = 0, PROTEIN_SHAKE = 1, CHUG_JUG = 2, ENERGY_DRINK = 3
}

public enum ItemRarity
{
    COMMON, UNCOMMON, RARE, UNOBTAINABLE, PLACEHOLDER
}

public enum ItemUseCase
{
    ONLY_IN_BATTLE, IN_BATTLE_OR_MAP
}

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{

    [Header("Base Information")]
    public string itemName;
    public string itemDesc;
    public Sprite itemImage;
    public ItemType type;
    public ItemUseCase useCase;
    [Header("Variable Information")]
    public List<int> variables = new List<int>();

    [Header("Item Unlock Information")]
    public ItemRarity itemRarity;

}