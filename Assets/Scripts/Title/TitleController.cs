using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TitleController : MonoBehaviour
{

    public static TitleController Instance;
    [Header("Object Assignments")]
    [SerializeField] private GameObject warningContainerObject;
    [SerializeField] private GameObject continueButtonObject;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject trophyObject;
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
        _allTitleButtons = new List<UITitleButtonHandler>(GameObject.FindObjectsOfType<UITitleButtonHandler>());
        SetContinueButtonState();
        trophyObject?.SetActive(PlayerPrefs.GetInt("BeatGame") == 1);
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
        TransitionManager.Instance.ShowScreen(1);
        // Modify the settings button for the title screen.
        settingsButton.onClick.AddListener(() => SettingsManager.Instance.TogglePause(0.1f));
    }

    // Checks to see if there is a save file already made.
    // If there is NO save file, don't make the Continue button clickable.
    private void SetContinueButtonState()
    {
        continueButtonObject.GetComponent<UITitleButtonHandler>().SetIsClickable(SaveLoadManager.DoesSaveExist("Save.ass"));
    }

    // Plays the button hover sound.
    public void PlayButtonHoverSFX()
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

    /*
        Returns a boolean representing whether or not the specified animator is
        playing an animation clip with the specified name.
    */
    public bool IsPlaying(Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

}
