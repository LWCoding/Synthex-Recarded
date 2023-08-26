using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSaveController : MonoBehaviour
{

    public static UISaveFileHandler CurrentlySelectedSave = null;
    public string GetCurrentlySelectedSaveFileName() => CurrentlySelectedSave.SaveFileName.Replace(" ", "") + ".ass";

    public static TitleSaveController Instance;
    [Header("Object Assignments")]
    [SerializeField] private CanvasGroup _saveFileContainerCanvasGroup;

    public bool IsUIAnimating { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        Debug.Assert(_saveFileContainerCanvasGroup != null, "SaveFileContainer is not assigned!", this);
    }

    // Toggles the visibility of the entire save panel. Can be called from buttons.
    public void ToggleSavePanel(bool shouldUIShow)
    {
        StartCoroutine(ToggleSavePanelCoroutine(shouldUIShow));
    }

    // Toggles the visibility of the entire save panel.
    private IEnumerator ToggleSavePanelCoroutine(bool shouldUIShow)
    {
        // Set the game object to initially enabled.
        _saveFileContainerCanvasGroup.gameObject.SetActive(true);
        // Select the current save file.
        CurrentlySelectedSave.SelectAsSaveFile();
        // Animate the canvas group to toggle.
        float currTime = 0;
        float timeToWait = 0.5f;
        int currAlpha = (shouldUIShow) ? 0 : 1;
        float targetAlpha = (shouldUIShow) ? 1 : 0;
        while (currTime < timeToWait)
        {
            currTime += Time.unscaledDeltaTime;
            _saveFileContainerCanvasGroup.alpha = Mathf.Lerp(currAlpha, targetAlpha, currTime / timeToWait);
            yield return null;
        }
        // If we're disabling the save panel, run special logic to
        // hide the panel and animate the buttons back in.
        if (!shouldUIShow)
        {
            _saveFileContainerCanvasGroup.gameObject.SetActive(false);
            TitleController.Instance.AnimateAllTitleButtonsIn();
        }
        // Tell the class we're not animating anymore.
        IsUIAnimating = false;
    }

    public void StartSecretGame()
    {
        // Initialize the hero with base information.
        GameManager.SetChosenHero(Globals.GetBaseHero(HeroTag.JACK));
        GameManager.SetSeenEnemies(new List<Encounter>());
        GameManager.SetGameScene(GameScene.FOREST);
        GameManager.SetMapObject(null);
        GameManager.SetMoney(150);
        GameManager.SetXP(15);
        GameManager.saveFileName = "Secret.ass";
        GameManager.alreadyPlayedTutorials.Add("Battle");
        GameManager.alreadyPlayedTutorials.Add("Shop");
        // Transition the game to the MAP.
        GameManager.IsInCampaign = false;
        TransitionManager.Instance.HideScreen("Map", 1.5f);
    }

    public void StartGameWithCurrentSaveFile()
    {
        // If the UI is currently animating, don't let the user select this.
        if (IsUIAnimating) { return; }
        string currentSaveFile = GetCurrentlySelectedSaveFileName();
        Debug.Log("Loading game with " + currentSaveFile + "...");
        // If we can't find a save, initialize new game information.
        // If we can, then just load the game.
        if (!SaveLoadManager.DoesSaveExist(currentSaveFile))
        {
            // Initialize the hero with base information.
            GameManager.SetChosenHero(Globals.GetBaseHero(HeroTag.JACK));
            GameManager.SetSeenEnemies(new List<Encounter>());
            GameManager.SetPlayedDialogues(new List<DialogueName>(), new List<string>());
            GameManager.SetGameScene(GameScene.FOREST);
            GameManager.SetCampaignSave(null);
            GameManager.SetMapObject(null);
            GameManager.SetMoney(150);
            GameManager.saveFileName = currentSaveFile;
        }
        else
        {
            SaveLoadManager.Load(currentSaveFile);
        }
        GameManager.IsInCampaign = true;
        TransitionManager.Instance.HideScreen("Campaign", 1.5f);
    }

}
