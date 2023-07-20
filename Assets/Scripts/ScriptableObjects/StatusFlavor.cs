using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Status", menuName = "ScriptableObjects/StatusFlavor")]
public class StatusFlavor : ScriptableObject
{

    public string statusName;
    public string statusDescription;
    public Sprite statusIcon;

}
