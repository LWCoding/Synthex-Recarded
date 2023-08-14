using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public struct LocationType
{
    public LocationChoice locationType;
    public Sprite sprite;
    public float weightedChance;
    public float iconScale;
}

public enum LocationChoice
{
    NONE = 0, BASIC_ENCOUNTER = 1, MINIBOSS_ENCOUNTER = 2, BOSS_ENCOUNTER = 3,
    SHOP = 4, TREASURE = 5, UPGRADE_MACHINE = 6
}

public enum CampaignScene
{
    NONE = 0, FOREST = 1, AERICHO = 2, SECRET = 99
}

[System.Serializable]
public class CampaignSave
{
    public CampaignScene currCampaign; // Current type of map the player is on.
    public GlobalObjectId currLevelID; // Player's current location on the map.
    public List<GlobalObjectId> visitedLevels; // Player's visited levels.
}

[CreateAssetMenu(fileName = "CampaignInfo", menuName = "ScriptableObjects/CampaignInfo")]
public class CampaignInfo : ScriptableObject
{

    [Header("Base Information")]
    public CampaignScene campaignType;
    [Header("Campaign Location Info")]
    public List<LocationType> campaignLocations;

}