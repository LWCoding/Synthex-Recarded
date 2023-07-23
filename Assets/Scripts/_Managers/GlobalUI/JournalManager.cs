using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(JournalEnemyTabController))]
public class JournalManager : MonoBehaviour
{

    public static JournalManager Instance;
    [Header("Object Assignments")]
    [SerializeField] private GameObject _journalContainer;
    [SerializeField] private GameObject _overlayContainer;
    [SerializeField] private Button _buttonToToggleJournal;
    public int GetJournalButtonSortingOrder() => _buttonToToggleJournal.GetComponent<Canvas>().sortingOrder;
    [Header("Sound Assignments")]
    [SerializeField] private AudioClip bookmarkShowSFX;

    private CanvasGroup _containerCanvasGroup;

    private bool _isJournalShowing = false;
    public bool IsJournalShowing() => _isJournalShowing;
    private bool _isUIAnimating = false;
    private Animator _journalIconAnimator;
    private int _initialButtonSortingOrder;

    private JournalEnemyTabController _journalEnemyTabController;
    public void UnlockNewEnemy(Enemy e) => _journalEnemyTabController.UnlockNewEnemy(e);

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
        _overlayContainer.SetActive(true);
        _journalContainer.SetActive(false);
        _containerCanvasGroup = _journalContainer.GetComponent<CanvasGroup>();
        _journalIconAnimator = _buttonToToggleJournal.GetComponent<Animator>();
        _journalEnemyTabController = GetComponent<JournalEnemyTabController>();
    }

    private void Start()
    {
        _initialButtonSortingOrder = _buttonToToggleJournal.GetComponent<Canvas>().sortingOrder;
    }

    public void Initialize()
    {
        UpdateAnimationStatus();
    }

    public void UpdateAnimationStatus()
    {
        // Check if any enemies have been discovered but not checked
        // in the journal. If yes, then play the alert idle animation.
        bool anyEnemyNotCheckedInJournal = false;
        foreach (Enemy e in Globals.allEnemies)
        {
            if (PlayerPrefs.GetInt(e.characterName) == 1)
            {
                anyEnemyNotCheckedInJournal = true;
                break;
            }
        }
        // Or else, just have the journal icon remain idle.
        _journalIconAnimator.Play((anyEnemyNotCheckedInJournal) ? "IdleAlert" : "Idle");
    }

    // Play the alert animation IF the journal tab isn't currently showing.
    // Or else, just go to the idle alert.
    public void PlayAlertAnimation()
    {
        if (!IsJournalShowing())
        {
            _journalIconAnimator.Play("Alert");
            SoundManager.Instance.PlayOneShot(bookmarkShowSFX, 2);
        }
        else
        {
            _journalIconAnimator.Play("IdleAlert");
        }
    }

    // Toggle the journal popup.
    public void TogglePopup(Button button)
    {
        if (_isUIAnimating) { return; }
        // Make sure the container is active to begin with.
        _journalContainer.SetActive(true);
        // Make sure the game knows we're animating and then animate the UI in.
        _isUIAnimating = true;
        StartCoroutine(TogglePopupCoroutine(button, !_isJournalShowing));
        // Always show the first enemy by default when opened.
        if (!_isJournalShowing)
        {
            _journalEnemyTabController.SetEnemyInfo(Globals.allEnemies[0]);
        }
        // Initialize the journal enemy controller, and set the preview to the first enemy.
        _journalEnemyTabController.InitializeEnemySelections(Globals.allEnemies);
    }

    // Immediately hide the journal popup.
    public void HidePopup()
    {
        // Simply hide the UI and the container.
        _isUIAnimating = false;
        _journalContainer.SetActive(false);
        _isJournalShowing = false;
        _buttonToToggleJournal.GetComponent<Canvas>().sortingOrder = _initialButtonSortingOrder;
    }

    private IEnumerator TogglePopupCoroutine(Button buttonClicked, bool shouldUIShow)
    {
        // Tell this class we're animating so we can't toggle during the animation.
        _isUIAnimating = true;
        buttonClicked.interactable = false;
        // Push this button's sorting order to the front or send it back to the regular order.
        if (shouldUIShow)
        {
            _initialButtonSortingOrder = buttonClicked.GetComponent<Canvas>().sortingOrder;
            buttonClicked.GetComponent<Canvas>().sortingOrder = _overlayContainer.GetComponent<Canvas>().sortingOrder + 1;
        }
        else
        {
            buttonClicked.GetComponent<Canvas>().sortingOrder = _initialButtonSortingOrder;
        }
        // Animate the canvas group to toggle.
        float currTime = 0;
        float timeToWait = 0.3f;
        int currAlpha = (shouldUIShow) ? 0 : 1;
        float targetAlpha = (shouldUIShow) ? 1 : 0;
        while (currTime < timeToWait)
        {
            currTime += Time.unscaledDeltaTime;
            _containerCanvasGroup.alpha = Mathf.Lerp(currAlpha, targetAlpha, currTime / timeToWait);
            yield return null;
        }
        // Hide the pause UI group if we've made it hidden.
        if (!shouldUIShow)
        {
            _journalContainer.SetActive(false);
        }
        // Tell the class we're not animating anymore.
        _isUIAnimating = false;
        _isJournalShowing = !_isJournalShowing;
        buttonClicked.interactable = true;
    }

}
