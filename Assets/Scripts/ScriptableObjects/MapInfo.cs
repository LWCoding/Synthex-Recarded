using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapScene
{
    NONE, FOREST, AERICHO, FACTORY, SECRET
}

public enum MapChoice
{
    NONE, BASIC_ENCOUNTER, SHOP, TREASURE, BOSS, MINIBOSS_ENCOUNTER, UPGRADE_MACHINE
}

[System.Serializable]
public class SerializableMapObject
{
    public MapScene currScene; // Current type of map the player is on.
    public SerializableMapLocation currLocation = new SerializableMapLocation(); // Player's current location on the map.
    public List<SerializableMapLocation> mapLocations = new List<SerializableMapLocation>(); // All locations on the map.
    public List<SerializableMapPath> mapPaths = new List<SerializableMapPath>(); // Paths between locations on the map.
}

[System.Serializable]
public class SerializableMapPath
{
    public Vector3 initialPosition;
    public Vector3 targetPosition;
}

[System.Serializable]
public class SerializableMapLocation
{
    public MapLocationType mapLocationType;
    public int floorNumber;
    public Vector3 position;
}


[System.Serializable]
public struct MapLocationType
{
    public string name;
    public MapChoice type;
    public Sprite sprite;
    public float weightedChance;
    public float iconScale; // 0.5f by default
    public bool isObstacle;
}

[System.Serializable]
public class Encounter
{

    public List<Enemy> enemies;

    [Header("Enemy Information")]
    public bool isMiniboss = false;
    public bool isBoss = false;
    public int minFloorRequired = 0;
    public int maxFloorLimit = 999;

    public override string ToString()
    {
        string enemyString = "";
        foreach (Enemy e in enemies)
        {
            enemyString += e.characterName + " ";
        }
        return enemyString;
    }

    // Two encounters equal each other if the enemies are the same.
    public override bool Equals(System.Object obj) => obj != null && obj as Encounter != null && Equals(obj as Encounter);
    public bool Equals(Encounter e) => enemies.All(e.enemies.Contains) && e.enemies.All(enemies.Contains);
    public override int GetHashCode() => base.GetHashCode();

}

[System.Serializable]
public class DialogueByFloor
{
    public int floorNumber;
    public Dialogue mapDialogue;
}

[CreateAssetMenu(fileName = "MapInfo", menuName = "ScriptableObjects/MapInfo")]
public class MapInfo : ScriptableObject
{

    [Header("Base Information")]
    public MapScene mapType;
    public Color mapBGColor;
    public int numFloors;
    [Header("Map Location Info")]
    public List<MapLocationType> mapLocations;
    public List<MapLocationType> mapObstacles;
    [Header("Encounter Info")]
    public List<Encounter> possibleEncounters;
    [Header("Map Events")]
    public List<DialogueByFloor> mapDialogues;

}
