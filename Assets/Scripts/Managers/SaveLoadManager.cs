using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveObject
{

    [Header("Game Information")]
    public Hero hero;
    public CampaignSave campaignSave;
    public SerializableMapObject mapObject;
    public int money;
    public int xp;
    [Header("Game Background Information")]
    public List<GameEvent> registeredEvents;
    public List<string> tutorialsPlayed;
    public List<string> mapDialoguesPlayed;
    public List<Encounter> loadedEncounters;

}

public static class SaveLoadManager
{

    private static string savePath = Application.persistentDataPath + "/SaveInfo/";

    public static void Save(SaveObject saveObject, string fileName)
    {

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log("Save directory not found! Creating new directory...");
        }

        File.WriteAllText(savePath + fileName, JsonUtility.ToJson(saveObject));
        Debug.Log("Slot saved into current file.");

    }

    public static void Load(string fileName)
    {
        Debug.Assert(DoesSaveExist(fileName), "Attempted to retrieve save object that didn't exist in SaveLoadManager.cs!");
        Debug.Log("Loading save slot (" + fileName + ").");

        SaveObject so = GetSaveObject(fileName);
        GameManager.SetChosenHero(so.hero);
        EventManager.RegisteredEvents = so.registeredEvents;
        GameManager.SetMoney(so.money);
        GameManager.SetXP(so.xp);
        GameManager.SetCampaignSave(so.campaignSave);
        if (so.mapObject != null) GameManager.SetGameScene(so.mapObject.currScene);
        if (so.campaignSave != null) GameManager.SetGameScene(so.campaignSave.currScene);
        GameManager.SetMapObject(so.mapObject);
        GameManager.SetPlayedDialogues(so.mapDialoguesPlayed, so.tutorialsPlayed);
        GameManager.SetSeenEnemies(so.loadedEncounters);
        GameManager.SaveFileName = fileName;

    }

    // Returns true if a save file already exists; else false.
    public static bool DoesSaveExist(string fileName)
    {
        // If we don't have a save file, we don't have a save.
        if (!File.Exists(savePath + fileName)) { return false; }
        // Check if the hero is valid. If it is invalid, we likely have a corrupted file.
        string savedText = File.ReadAllText(savePath + fileName);
        SaveObject so = JsonUtility.FromJson<SaveObject>(savedText);
        if (so.hero.heroData == null)
        {
            return false;
        }
        // If the checks pass, return true!
        return true;
    }

    public static SaveObject GetSaveObject(string fileName)
    {
        Debug.Assert(DoesSaveExist(fileName), "Attempted to retrieve save object that didn't exist in SaveLoadManager.cs!");

        string savedText = File.ReadAllText(savePath + fileName);
        SaveObject so = JsonUtility.FromJson<SaveObject>(savedText);
        return so;
    }

    public static void EraseSave(string fileName)
    {
        if (DoesSaveExist(fileName))
        {
            File.Delete(savePath + fileName);
        }
    }

}
