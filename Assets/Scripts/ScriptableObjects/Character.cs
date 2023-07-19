using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character : ScriptableObject
{

    [Header("Base Information")]
    public string characterName;
    [TextArea(3, 10)]
    public string characterDesc;
    public Sprite idleSprite;
    public Sprite damagedSprite;
    public Sprite deathSprite;
    public Vector2 spriteOffset = new Vector2(0, 0);
    public Vector2 spriteScale = new Vector2(1, 1);
    public Vector2 shadowScale = new Vector2(3.3f, 0.77f);

}
