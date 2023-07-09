using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleController : MonoBehaviour
{

    public static TitleController Instance;
    [Header("Object Assignments")]
    public GameObject continueButtonObject;
    public Button settingsButton;
    public GameObject trophyObject;
    public List<Animator> allButtonAnimators = new List<Animator>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        SetContinueButtonState();
        trophyObject?.SetActive(PlayerPrefs.GetInt("BeatGame") == 1);
        // Hide the top bar.
        TopBarController.Instance?.HideTopBar();
    }

    private void Start()
    {
        // Set target frame rate to 60.
        Application.targetFrameRate = 60;
        // Play music.
        SoundManager.Instance.PlayOnLoop(MusicType.TITLE_MUSIC);
        // Make the game fade from black to clear.
        FadeTransitionController.Instance.ShowScreen(1);
        // Modify the settings button for the title screen.
        ModifySettingsButton();
    }

    // Checks to see if there is a save file already made.
    // If there is NO save file, don't make the Continue button clickable.
    private void SetContinueButtonState()
    {
        continueButtonObject.GetComponent<TitleUIButtonHandler>().SetIsClickable(SaveLoadManager.DoesSaveExist());
    }

    // Modify the settings so that it spawns the appropriate menus for the title.
    public void ModifySettingsButton()
    {
        settingsButton.onClick.AddListener(() => SettingsManager.Instance.TogglePause(0.1f));
    }

    // Starts a new game by setting all of the variables in GameController
    // and initializing a starting relic. Optionally, start in a different
    // scene by supplying the mapScene parameter.
    public void StartNewGame()
    {
        // Initialize the hero with base information.
        GameController.SetChosenHero(Globals.GetBaseHero(HeroTag.JACK));
        GameController.SetSeenEnemies(new List<Encounter>());
        GameController.SetPlayedDialogues(new List<DialogueName>(), new List<string>(), false);
        GameController.SetMapScene(MapScene.FOREST);
        GameController.SetMapObject(null);
        GameController.SetMoney(150);
        // Start the game.
        StartGame();
    }

    public void ContinueGame()
    {
        // Load the game. This will populate the GameController information.
        SaveLoadManager.Load();
        // Start the game.
        StartGame();
    }

    private void StartGame()
    {
        // Make sure the map starts in the forest.
        FadeTransitionController.Instance.HideScreen("Map", 1.5f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
