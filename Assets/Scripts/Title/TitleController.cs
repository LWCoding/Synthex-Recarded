using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleController : MonoBehaviour
{

    public static TitleController Instance;
    [Header("Object Assignments")]
    public GameObject warningContainerObject;
    public GameObject continueButtonObject;
    public Button settingsButton;
    public GameObject trophyObject;
    public List<Animator> allButtonAnimators = new List<Animator>();
    [Header("Audio Assignments")]
    public AudioClip warningBeepsSFX;

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
    }

    private void Start()
    {
        // Set target frame rate to 60.
        Application.targetFrameRate = 60;
        // Hide the top bar.
        TopBarController.Instance?.HideTopBar();
#if !UNITY_WEBGL || UNITY_EDITOR
        // If we're not using the website version, just skip the warning screen.
        InitializeGame();
#else
// If we are, then show the warning screen if this is the first load.
        if (GameController.wasTitleRendered == false) {
            InitializeWarningScreen();
            GameController.wasTitleRendered = true;
        } else {
            InitializeGame();
        }
#endif
    }

    private void InitializeWarningScreen()
    {
        SoundManager.Instance.PlayOneShot(warningBeepsSFX);
        warningContainerObject.SetActive(true);
        StartCoroutine(WarningScreenClickCoroutine());
    }

    private IEnumerator WarningScreenClickCoroutine()
    {
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        Animator anim = warningContainerObject.GetComponent<Animator>();
        anim.Play("Hide");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        InitializeGame();
    }

    public void InitializeGame()
    {
        warningContainerObject.SetActive(false);
        // Play music.
        SoundManager.Instance.PlayOnLoop(MusicType.TITLE_MUSIC);
        // Make the game fade from black to clear.
        FadeTransitionController.Instance.ShowScreen(1);
        // Modify the settings button for the title screen.
        settingsButton.onClick.AddListener(() => SettingsManager.Instance.TogglePause(0.1f));
    }

    // Checks to see if there is a save file already made.
    // If there is NO save file, don't make the Continue button clickable.
    private void SetContinueButtonState()
    {
        continueButtonObject.GetComponent<TitleUIButtonHandler>().SetIsClickable(SaveLoadManager.DoesSaveExist("Save.ass"));
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
        GameController.saveFileName = "Save.ass"; // TODO: Make this vary!
        // Start the game.
        StartGame();
    }

    public void ContinueGame()
    {
        // Load the game. This will populate the GameController information.
        SaveLoadManager.Load("Save.ass"); // TODO: Make this vary!
        // Start the game.
        StartGame();
    }

    private void StartGame()
    {
        // Make sure the map starts in the forest.
        FadeTransitionController.Instance.HideScreen("Map", 1.5f);
    }

    // Plays the button hover sound.
    public void PlaySettingsButtonHoverSFX()
    {
        SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_BUTTON_HOVER);
    }

    // Plays the button hover sound.
    public void OpenSettingsMenu()
    {
        SettingsManager.Instance.TogglePause(0.2f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
