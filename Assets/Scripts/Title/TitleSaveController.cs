using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleSaveController : MonoBehaviour
{

    public static UISaveFileHandler CurrentlySelectedSave = null;
    public string GetCurrentlySelectedSaveFileName() => CurrentlySelectedSave.SaveFileName.Replace(" ", "") + ".ass";

    public static TitleSaveController Instance;
    [Header("Object Assignments")]
    [SerializeField] private CanvasGroup _saveFileContainerCanvasGroup;
    [SerializeField] private UISaveFileHandler _initiallySelectedSaveOption;
    [SerializeField] private TextMeshProUGUI saveContentText;
    [Header("Audio Assignments")]
    [SerializeField] private AudioClip _buttonSelectSFX;

    public bool IsUIAnimating { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        Debug.Assert(_saveFileContainerCanvasGroup != null, "SaveFileContainer is not assigned!", this);
        // Set currently selected save.
        TitleSaveController.CurrentlySelectedSave = _initiallySelectedSaveOption;
    }

    // Toggles the visibility of the entire save panel. Can be called from buttons.
    public void ToggleSavePanel(bool shouldUIShow)
    {
        StartCoroutine(ToggleSavePanelCoroutine(shouldUIShow));
    }

    // Toggles the visibility of the entire save panel.
    private IEnumerator ToggleSavePanelCoroutine(bool shouldUIShow)
    {
        if (IsUIAnimating) { yield break; }
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
            GameManager.SetPlayedDialogues(new List<string>(), new List<string>());
            GameManager.SetGameScene(GameScene.FOREST);
            GameManager.SetCampaignSave(null);
            GameManager.SetMapObject(null);
            GameManager.SetMoney(150);
            GameManager.SetXP(0);
            GameManager.saveFileName = currentSaveFile;
        }
        else
        {
            SaveLoadManager.Load(currentSaveFile);
        }
        GameManager.IsInCampaign = true;
        TransitionManager.Instance.HideScreen("Campaign", 1.5f);
    }

    private int _numTimesEraseButtonClicked = 0;

    // Erases the current save file. Called by UI buttons.
    public void EraseCurrentSaveFile(Button buttonToAnimate)
    {
        // If the UI is currently animating, don't let the user select this.
        if (IsUIAnimating) { return; }
        _numTimesEraseButtonClicked++;
        if (_numTimesEraseButtonClicked < 2)
        {
            saveContentText.text = "To delete this save, click button again to confirm.";
            StartCoroutine(WaitBeforeResettingCount());
            return;
        }
        FlashButton(buttonToAnimate);
        StartCoroutine(WaitSecondBeforeErasing());
    }

    private IEnumerator WaitBeforeResettingCount()
    {
        yield return new WaitForSeconds(2);
        if (_numTimesEraseButtonClicked == 1) CurrentlySelectedSave.SelectAsSaveFile();
        _numTimesEraseButtonClicked = 0;
    }

    private IEnumerator WaitSecondBeforeErasing()
    {
        IsUIAnimating = true;
        saveContentText.text = "Deleting save information... please wait. :)";
        yield return new WaitForSeconds(2);
        _numTimesEraseButtonClicked = 0;
        SaveLoadManager.EraseSave(GetCurrentlySelectedSaveFileName());
        CurrentlySelectedSave.SelectAsSaveFile();
        IsUIAnimating = false;
    }

    // Flashes a specific button. Called by UI buttons.
    public void FlashButton(Button buttonToAnimate)
    {
        if (TitleSaveController.Instance.IsUIAnimating) { return; }
        SoundManager.Instance.PlayOneShot(_buttonSelectSFX);
        StartCoroutine(FlashButtonCoroutine(buttonToAnimate));
    }

    private IEnumerator FlashButtonCoroutine(Button buttonToAnimate)
    {
        // We're animating, so let's make the user not able to select another option.
        float buttonFlashDelay = 0.095f;
        Sprite highlightedSprite = buttonToAnimate.spriteState.highlightedSprite;
        Sprite disabledSprite = buttonToAnimate.spriteState.disabledSprite;
        Image buttonImage = buttonToAnimate.GetComponent<Image>();
        buttonToAnimate.enabled = false;
        for (int i = 0; i < 3; i++)
        {
            buttonImage.sprite = highlightedSprite;
            yield return new WaitForSecondsRealtime(buttonFlashDelay);
            buttonImage.sprite = disabledSprite;
            yield return new WaitForSecondsRealtime(buttonFlashDelay);
        }
        buttonToAnimate.enabled = true;
    }

}
