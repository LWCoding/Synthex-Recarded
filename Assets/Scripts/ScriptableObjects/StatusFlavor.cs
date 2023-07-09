using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectFlavor
{
    NONE = 0,
    ENERGY = 1,
    MATERIALIZE = 2
}

[CreateAssetMenu(fileName = "Status", menuName = "ScriptableObjects/StatusFlavor")]
public class StatusFlavor : ScriptableObject
{

    public string statusName;
    public string statusDescription;
    public EffectFlavor type;
    public Sprite statusIcon;
    public Vector2 iconSpriteScale = new Vector2(0.5f, 0.5f);

}
