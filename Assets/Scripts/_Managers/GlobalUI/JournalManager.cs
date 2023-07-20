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

    private CanvasGroup _containerCanvasGroup;

    private bool _isJournalShowing = false;
    public bool IsJournalShowing() => _isJournalShowing;
    private bool _isUIAnimating = false;
    private int _initialButtonSortingOrder;

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
    }

    private void Start()
    {
        _initialButtonSortingOrder = _buttonToToggleJournal.GetComponent<Canvas>().sortingOrder;
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
        // Initialize the journal enemy controller, and set the preview to the first enemy.
        JournalEnemyTabController.Instance.InitializeEnemySelections(Globals.allEnemies);
        // Always show the first enemy by default when opened.
        if (!_isJournalShowing)
        {
            JournalEnemyTabController.Instance.SetEnemyInfo(Globals.allEnemies[0]);
        }
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

    // public void AnimateNewEnemyToJournal(BattleCharacterController enemyBCC, Enemy enemy)
    // {
    //     StartCoroutine(AnimateNewEnemyToJournalCoroutine(enemyBCC, enemy));
    // }

    // // Animates a single card going to the deck icon transform,
    // // given an initial position, card type, and a
    // // delay before the card appears.
    // private IEnumerator AnimateNewEnemyToJournalCoroutine(BattleCharacterController enemyBCC, Enemy enemy)
    // {
    //     yield return new WaitForSeconds(0.5f);
    //     // Flash the enemy after a delay.
    //     enemyBCC.FlashColor(new Color(0.2f, 0.2f, 1));
    //     yield return new WaitForSeconds(0.2f);
    //     // Create and initialize the object.
    //     GameObject obj = ObjectPooler.Instance.GetObjectFromPool(PoolableType.UI_IMAGE, GlobalUIController.Instance.GlobalCanvas.transform);
    //     CanvasGroup objCanvasGroup = obj.GetComponent<CanvasGroup>();
    //     obj.GetComponent<Image>().sprite = enemy.idleSprite;
    //     obj.GetComponent<Image>().color = new Color(0.5f, 0.5f, 1);
    //     obj.GetComponent<Image>().SetNativeSize();
    //     obj.GetComponent<Canvas>().sortingOrder = GetJournalButtonSortingOrder() + 1;
    //     // Animate the object towards the journal icon.
    //     Vector3 initialPosition = Camera.main.WorldToScreenPoint(enemyBCC.transform.position);
    //     Vector3 targetPosition = _buttonToToggleJournal.transform.position;
    //     Vector3 initialScale = enemy.spriteScale * new Vector3(0.7f, 0.7f, 1);
    //     Vector3 targetScale = enemy.spriteScale * new Vector3(0.1f, 0.1f, 1);
    //     float currTime = 0;
    //     float timeToWait = 0.5f;
    //     while (currTime < timeToWait)
    //     {
    //         currTime += Time.deltaTime;
    //         objCanvasGroup.alpha = Mathf.Lerp(0.5f, 0, currTime / timeToWait);
    //         obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, currTime / timeToWait);
    //         obj.transform.position = Vector3.Lerp(initialPosition, targetPosition, currTime / timeToWait);
    //         yield return null;
    //     }
    //     ObjectPooler.Instance.ReturnObjectToPool(PoolableType.UI_IMAGE, obj);
    // }

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
