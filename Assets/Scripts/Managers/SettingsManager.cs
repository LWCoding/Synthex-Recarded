using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Settings
{
    public float desiredVolume;
}

public class SettingsManager : MonoBehaviour
{

    public static SettingsManager Instance;
    [Header("Object Assignments")]
    [SerializeField] private CanvasGroup _pauseUICanvasGroup;
    [SerializeField] private GameObject _pauseUIGroup;
    [SerializeField] private Image _fadeOverlayImage;
    [SerializeField] private Button _settingsBackButton;
    [Header("Panel Object Assignments")]
    [SerializeField] private GameObject _settingsScreenObject;
    [SerializeField] private GameObject _optionsScreenObject;
    [SerializeField] private GameObject _exitScreenObject;
    [Header("Options Assignments")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI _volumeText;
    private void SetVolumeText(float volume) => _volumeText.text = "Volume (" + Mathf.RoundToInt(volume * 100) + ")";
    [Header("Audio Assignments")]
    public AudioClip buttonHoverSFX;
    public AudioClip buttonSelectSFX;
    private bool _isGamePaused = false;
    public bool IsGamePaused() => _isGamePaused;
    private bool _isUIAnimating = false;
    private bool _isOptionChosen = false;

    // This Awake function runs on the first time the bar is instantiated.
    private void Awake()
    {
        // Set this to the Instance if it is the first one.
        // Or else, destroy this.
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Make sure this object isn't destroyed.
        DontDestroyOnLoad(this);
        _pauseUIGroup.SetActive(false);
        _fadeOverlayImage.gameObject.SetActive(false);
    }

    // Update the text after we're sure that the volume has been properly set
    // in the Awake() function of our SoundManager.
    private void Start()
    {
        SetVolumeText(SoundManager.Instance.GetDesiredVolume());
    }

    private void Update()
    {
        // If the player presses ESCAPE, toggle the pause screen.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(0.1f);
        }
    }

    private void EnableButtonPresses()
    {
        _isOptionChosen = false;
    }

    private void DisableButtonPresses()
    {
        _isOptionChosen = true;
    }

    // Title: Settings button should close the menu
    // Game: Settings button should re-open pause menu
    public void SetSettingsBackBehaviour(string sceneName)
    {
        // If we've already chosen an option, don't let the user select.
        if (_isOptionChosen) { return; }
        if (sceneName != "Title")
        {
            // If not title, bring back to the rest of the pause menu
            _settingsBackButton.onClick.RemoveAllListeners();
            _settingsBackButton.onClick.AddListener(() =>
            {
                ToggleScreen("Main");
            });
        }
        else
        {
            // If title, close the menu
            _settingsBackButton.onClick.RemoveAllListeners();
            _settingsBackButton.onClick.AddListener(() =>
            {
                TogglePause(0.1f);
            });
        }
    }

    // Plays the button hover sound.
    public void PlayButtonHoverSFX()
    {
        if (_isOptionChosen) { return; }
        SoundManager.Instance.PlayOneShot(buttonHoverSFX);
    }

    // Plays the button select sound.
    // If an option has already been selected, let's not let the sound play,
    // unless we are calling that as we've selected the option.
    public void PlayButtonSelectSFX(bool canPlayIfOptionSelected = false)
    {
        if (!canPlayIfOptionSelected && _isOptionChosen) { return; }
        SoundManager.Instance.PlayOneShot(buttonSelectSFX);
    }

    // Instantly hides the pause menu and returns to the title screen.
    // This should be called by the UI button that handles this.
    public void ExitToMenu(Button button)
    {
        // If we've already chosen an option, don't let the user select.
        if (_isOptionChosen) { return; }
        button.enabled = false;
        Image buttonImage = button.GetComponent<Image>();
        StartCoroutine(AnimateButtonBeforeRunningCode(buttonImage, button.spriteState.disabledSprite, button.spriteState.highlightedSprite, () =>
        {
            StartCoroutine(LoadSceneWithUnscaledTimeCoroutine("Title"));
            button.enabled = true;
        }));
    }

    // Shows the quit game screen.
    // This should be called by the quit button.
    public void ShowMainScreen(bool shouldHappenInstantly = false)
    {
        // If we've already chosen an option, don't let the user select.
        if (_isOptionChosen) { return; }
        ToggleScreen("Main", shouldHappenInstantly);
    }

    // CALLED BY RESUME UI BUTTON.
    public void ResumeGame(Button button)
    {
        // If we've already chosen an option, don't let the user select.
        if (_isOptionChosen) { return; }
        button.enabled = false;
        Image buttonImage = button.GetComponent<Image>();
        StartCoroutine(AnimateButtonBeforeRunningCode(buttonImage, button.spriteState.disabledSprite, button.spriteState.highlightedSprite, () =>
        {
            TogglePause(0.5f);
            button.enabled = true;
        }));
    }

    // Shows the settings screen.
    // CALLED BY SETTINGS UI BUTTON.
    public void ShowOptionsScreen(Button button)
    {
        // If we've already chosen an option, don't let the user select.
        if (_isOptionChosen) { return; }
        button.enabled = false;
        Image buttonImage = button.GetComponent<Image>();
        StartCoroutine(AnimateButtonBeforeRunningCode(buttonImage, button.spriteState.disabledSprite, button.spriteState.highlightedSprite, () =>
        {
            ToggleScreen("Options");
            _masterVolumeSlider.value = SoundManager.Instance.GetDesiredVolume();
            button.enabled = true;
        }));
    }

    // Shows the quit game screen.
    // CALLED BY QUIT UI BUTTON.
    public void ShowQuitScreen(Button button)
    {
        // If we've already chosen an option, don't let the user select.
        if (_isOptionChosen) { return; }
        button.enabled = false;
        Image buttonImage = button.GetComponent<Image>();
        StartCoroutine(AnimateButtonBeforeRunningCode(buttonImage, button.spriteState.disabledSprite, button.spriteState.highlightedSprite, () =>
        {
            ToggleScreen("Exit");
            button.enabled = true;
        }));
    }

    // CALLED BY EXIT "NO" BUTTON.
    public void ShowMainScreen(Button button)
    {
        // If we've already chosen an option, don't let the user select.
        if (_isOptionChosen) { return; }
        button.enabled = false;
        Image buttonImage = button.GetComponent<Image>();
        StartCoroutine(AnimateButtonBeforeRunningCode(buttonImage, button.spriteState.disabledSprite, button.spriteState.highlightedSprite, () =>
        {
            ToggleScreen("Main");
            button.enabled = true;
        }));
    }

    // Adjusts the master volume.
    // This should be called by the volume slider.
    public void AdjustMasterVolume(Slider slider)
    {
        SoundManager.Instance.SetDesiredVolume(slider.value);
        SetVolumeText(slider.value);
    }

    // Toggle the pause screen from true to false, or false to true.
    public void TogglePause(float animationTime = 0.5f)
    {
        // // If we're on the title screen, also don't let us pause.
        // if (SceneManager.GetActiveScene().name == "Title") { return; }
        // If the UI is currently animating, don't let the user toggle.
        if (_isUIAnimating) { return; }
        SetSettingsBackBehaviour(SceneManager.GetActiveScene().name);
        // Toggle the pause state and then animate the UI in.
        _isGamePaused = !_isGamePaused;
        if (_isGamePaused)
        {
            StartCoroutine(TogglePauseUI(true, animationTime));
            if (SceneManager.GetActiveScene().name == "Title")
            {
                ToggleScreen("Options");
            }
        }
        else
        {
            StartCoroutine(TogglePauseUI(false, animationTime));
        }
    }

    private IEnumerator AnimateButtonBeforeRunningCode(Image buttonImage, Sprite buttonDefaultSprite, Sprite buttonAltSprite, Action codeToRunAfter)
    {
        // We're animating, so let's make the user not able to select another option.
        DisableButtonPresses();
        PlayButtonSelectSFX(true);
        float buttonFlashDelay = 0.085f;
        for (int i = 0; i < 3; i++)
        {
            buttonImage.sprite = buttonAltSprite;
            yield return new WaitForSecondsRealtime(buttonFlashDelay);
            buttonImage.sprite = buttonDefaultSprite;
            yield return new WaitForSecondsRealtime(buttonFlashDelay);
        }
        codeToRunAfter.Invoke();
    }

    private IEnumerator TogglePauseUI(bool shouldPauseUIShow, float animationTime, Action codeToRunAfter = null)
    {
        // Tell this class we're animating so we can't toggle during the animation.
        _isUIAnimating = true;
        // Make all of the game assets pause.
        if (shouldPauseUIShow)
        {
            Time.timeScale = 0;
            ShowMainScreen(true);
            _pauseUIGroup.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
        }
        // Animate the canvas group to toggle.
        float currTime = 0;
        float timeToWait = animationTime;
        int currAlpha = (shouldPauseUIShow) ? 0 : 1;
        float targetAlpha = (shouldPauseUIShow) ? 1 : 0;
        while (currTime < timeToWait)
        {
            currTime += Time.unscaledDeltaTime;
            _pauseUICanvasGroup.alpha = Mathf.Lerp(currAlpha, targetAlpha, currTime / timeToWait);
            yield return null;
        }
        // Hide the pause UI group if we've made it hidden.
        if (!shouldPauseUIShow)
        {
            _pauseUIGroup.SetActive(false);
        }
        // Tell the class we're not animating anymore.
        EnableButtonPresses();
        _isUIAnimating = false;
        // Run the code afterwards, if there is any.
        codeToRunAfter?.Invoke();
    }

    private void ToggleScreen(string name, bool shouldHappenInstantly = false)
    {
        if (name != "Main" && name != "Options" && name != "Exit")
        {
            Debug.Log("Error! SettingsManager.cs couldn't find valid option in ToggleScreen() function.");
            return;
        }
        GameObject screenObject = null;
        _settingsScreenObject.SetActive(false);
        _optionsScreenObject.SetActive(false);
        _exitScreenObject.SetActive(false);
        switch (name)
        {
            case "Main":
                screenObject = _settingsScreenObject;
                break;
            case "Options":
                screenObject = _optionsScreenObject;
                break;
            case "Exit":
                screenObject = _exitScreenObject;
                break;
        }
        screenObject.SetActive(true);
        // We've switched the scene, so set the option chosen to nothing.
        EnableButtonPresses();
        if (!shouldHappenInstantly)
        {
            StartCoroutine(AnimateScreenObjectInCoroutine(screenObject));
        }
    }

    private IEnumerator AnimateScreenObjectInCoroutine(GameObject screenObject)
    {
        float currTime = 0;
        float timeToWait = 0.1f;
        Vector3 initialScale = screenObject.transform.localScale + new Vector3(0.07f, 0.07f, 0);
        Vector3 targetScale = screenObject.transform.localScale;
        while (currTime < timeToWait)
        {
            currTime += Time.unscaledDeltaTime;
            screenObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, currTime / timeToWait);
            yield return null;
        }
    }

    private IEnumerator LoadSceneWithUnscaledTimeCoroutine(string sceneName)
    {
        _fadeOverlayImage.gameObject.SetActive(true);
        _fadeOverlayImage.color = new Color(0, 0, 0, 0);
        Color initialColor = _fadeOverlayImage.color;
        Color targetColor = new Color(0, 0, 0, 1);
        float currTime = 0;
        float timeToWait = 1;
        while (currTime < timeToWait)
        {
            currTime += Time.unscaledDeltaTime;
            _fadeOverlayImage.color = Color.Lerp(initialColor, targetColor, currTime / timeToWait);
            yield return null;
        }
        // Load the next scene.
        SceneManager.LoadScene(sceneName);
        yield return new WaitForEndOfFrame();
        TogglePause(0);
        _fadeOverlayImage.gameObject.SetActive(false);
    }

}
