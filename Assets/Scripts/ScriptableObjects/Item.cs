using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    NONE = -1, CAT_FOOD = 0, PROTEIN_SHAKE = 1, CHUG_JUG = 2, ELIXIR = 3
}

public enum ItemRarity
{
    COMMON, UNCOMMON, RARE, UNOBTAINABLE, PLACEHOLDER
}

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item")]
public class Item : ScriptableObject
{

    [Header("Base Information")]
    public string itemName;
    public string itemDesc;
    public Sprite itemImage;
    public ItemType type;
    [Header("Item Unlock Information")]
    public ItemRarity itemRarity;

}