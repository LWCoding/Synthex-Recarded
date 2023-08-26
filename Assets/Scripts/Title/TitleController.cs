using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{

    public static TitleController Instance;
    [Header("Object Assignments")]
    [SerializeField] private GameObject warningContainerObject;
    [SerializeField] private GameObject secretButtonObject;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject beatGameTrophyObject;
    [SerializeField] private GameObject secretTrophyObject;
    [Header("Audio Assignments")]
    [SerializeField] private AudioClip warningBeepsSFX;

    public bool CanSelectTitleButton = false;

    private List<UITitleButtonHandler> _allTitleButtons = new List<UITitleButtonHandler>();
    public List<UITitleButtonHandler> AllTitleButtons => _allTitleButtons;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        // Initialize all title buttons.
        _allTitleButtons = new List<UITitleButtonHandler>(GameObject.FindObjectsOfType<UITitleButtonHandler>());
        // Show trophies depending on playerprefs variables.
        beatGameTrophyObject?.SetActive(PlayerPrefs.GetInt("BeatGame") == 1);
        secretTrophyObject?.SetActive(PlayerPrefs.GetInt("BeatBoykisser") >= 1);
        // Set secret button state.
        secretButtonObject.GetComponent<UITitleButtonHandler>().SetIsClickable(PlayerPrefs.GetInt("BeatGame") == 1);
    }

    private void Start()
    {
        // Set target frame rate to 60.
        Application.targetFrameRate = 60;
        // Hide top bar stuff if it exists.
        if (TopBarController.Instance != null)
        {
            // If the deck is showing, hide it.
            TopBarController.Instance.HideDeckOverlay();
            // If the journal is showing, hide it.
            JournalManager.Instance.HidePopup();
            TopBarController.Instance.HideTopBar();
        }
#if !UNITY_WEBGL || UNITY_EDITOR
        // If we're not using the website version, just skip the warning screen.
        InitializeGame();
#else
// If we are, then show the warning screen if this is the first load.
        if (GameManager.wasTitleRendered == false) {
            InitializeWarningScreen();
            GameManager.wasTitleRendered = true;
        } else {
            InitializeGame();
        }
#endif
    }

    public void AnimateAllTitleButtonsIn()
    {
        StartCoroutine(AnimateAllTitleButtonsInCoroutine());
    }

    private IEnumerator AnimateAllTitleButtonsInCoroutine()
    {
        CanSelectTitleButton = true;
        foreach (UITitleButtonHandler titleButton in _allTitleButtons)
        {
            Animator anim = titleButton.GetComponent<Animator>();
            titleButton.PlayAnimation("Appear");
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !_allTitleButtons[0].IsButtonAnimating());
        CanSelectTitleButton = false;
    }

    private void InitializeWarningScreen()
    {
        SoundManager.Instance.PlayOneShot(warningBeepsSFX);
        warningContainerObject.SetActive(true);
        StartCoroutine(WarningScreenClickCoroutine());
    }

    private void InitializeGame()
    {
        warningContainerObject.SetActive(false);
        // Play music.
        SoundManager.Instance.PlayOnLoop(MusicType.TITLE_MUSIC);
        // Make the game fade from black to clear.
        TransitionManager.Instance.ShowScreen(1);
        // Modify the settings button for the title screen.
        settingsButton.onClick.AddListener(() => SettingsManager.Instance.TogglePause(0.1f));
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

    // Plays the button hover sound. Used by UI buttons.
    public void PlayButtonHoverSFX()
    {
        SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_BUTTON_HOVER);
    }

    // Opens the settings menu. Used by UI buttons.
    public void OpenSettingsMenu()
    {
        SettingsManager.Instance.TogglePause(0.2f);
    }

    // Quits the game. Used by UI buttons.
    public void QuitGame()
    {
        Application.Quit();
    }

    #region Secret code

    private void Update()
    {
        if (Input.GetKey(KeyCode.J) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.K))
        {
            PlayerPrefs.SetInt("BeatGame", 1);
            StartCoroutine(WaitUntilNoKeyCodesThenRefresh());
        }
        if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.N))
        {
            PlayerPrefs.SetInt("BeatGame", 0);
            StartCoroutine(WaitUntilNoKeyCodesThenRefresh());
        }
    }

    private IEnumerator WaitUntilNoKeyCodesThenRefresh()
    {
        yield return new WaitUntil(() => !Input.anyKey);
        SceneManager.LoadScene("Title");
    }

    #endregion

}
