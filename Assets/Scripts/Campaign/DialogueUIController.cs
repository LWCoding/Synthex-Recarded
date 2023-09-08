using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class DialogueUIController : MonoBehaviour
{

    [HideInInspector] public static DialogueUIController Instance;
    [Header("Object Assignments")]
    [SerializeField] private GameObject dialogueContainerObject;
    [SerializeField] private Animator leftSpriteAnimator;
    [SerializeField] private Animator rightSpriteAnimator;
    [SerializeField] private GameObject leftSpriteObject;
    [SerializeField] private GameObject rightSpriteObject;
    [SerializeField] private TextMeshProUGUI dialogueNameText;
    [SerializeField] private TextMeshProUGUI dialogueContentsText;

    private Animator _dialogueBoxAnimator;
    private Image _leftSpriteImage;
    private Image _rightSpriteImage;
    private RectTransform _leftSpriteRectTransform;
    private RectTransform _rightSpriteRectTransform;
    private Dialogue _storedDialogue;
    private Queue<DialogueLine> _dialogueStringQueue = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        // Set some variables that will be reused later.
        _dialogueBoxAnimator = dialogueContainerObject.GetComponent<Animator>();
        _leftSpriteImage = leftSpriteObject.GetComponent<Image>();
        _rightSpriteImage = rightSpriteObject.GetComponent<Image>();
        _leftSpriteRectTransform = leftSpriteObject.GetComponent<RectTransform>();
        _rightSpriteRectTransform = rightSpriteObject.GetComponent<RectTransform>();
    }

    // Queues multiple lines of text to be said.
    public void PrequeueDialogueText(Dialogue dialogues)
    {
        // If the dialogue has already played before, don't play it.
        if (GameManager.alreadyPlayedMapDialogues.Contains(dialogues.dialogueName))
        {
            return;
        }
        _storedDialogue = dialogues;
        foreach (DialogueLine dl in dialogues.dialogueLines)
        {
            // Add the text to the queue.
            _dialogueStringQueue.Enqueue(dl);
        }
    }

    // Set both dialogue sprites to use Native Size (in the image component).
    // This makes it so every sprite used defaults to the correct size.
    private IEnumerator SetSpriteNativeSizesCoroutine()
    {
        yield return new WaitForEndOfFrame();
        _leftSpriteImage.SetNativeSize();
        _rightSpriteImage.SetNativeSize();
        _leftSpriteRectTransform.pivot = CalculateUIPivot(DialogueDirection.LEFT);
        _rightSpriteRectTransform.pivot = CalculateUIPivot(DialogueDirection.RIGHT);
    }

    // Calculate the UI Pivot because it takes in a percentage as opposed to the positions that the
    // regular pivot returns (from Image component).
    private Vector2 CalculateUIPivot(DialogueDirection dir)
    {
        Vector2 size = (dir == DialogueDirection.LEFT ? leftSpriteObject : rightSpriteObject).GetComponent<RectTransform>().sizeDelta;
        Vector2 pixelPivot = (dir == DialogueDirection.LEFT ? leftSpriteObject : rightSpriteObject).GetComponent<Image>().sprite.pivot;
        Vector2 percentPivot = new Vector2(pixelPivot.x / size.x, pixelPivot.y / size.y);
        return percentPivot;
    }

    // Initializes the start of a dialogue line by setting the animations and names.
    // Does not set the content of the dialogue box, as that should be rendered in with the
    // RenderDialogueTextCoroutine() coroutine.
    public void ChangeDialogueAnimations(DialogueLine dl, bool forceIdleSprites)
    {
        // If we have to force the idle sprites, make sure that the strings contain the word
        // "Idle" after them.
        string leftAnimationName = (forceIdleSprites && dl.leftAnimationName.Substring(dl.leftAnimationName.Length - 4) != "Idle") ? dl.leftAnimationName + "Idle" : dl.leftAnimationName;
        string rightAnimationName = (forceIdleSprites && dl.rightAnimationName.Substring(dl.rightAnimationName.Length - 4) != "Idle") ? dl.rightAnimationName + "Idle" : dl.rightAnimationName;
        leftSpriteAnimator.Play(leftAnimationName);
        rightSpriteAnimator.Play(rightAnimationName);
        // Change the focus.
        if (dl.focusDirection == DialogueDirection.LEFT)
        {
            _leftSpriteImage.color = new Color(1, 1, 1);
            _rightSpriteImage.color = new Color(0.8f, 0.8f, 0.8f);
        }
        else
        {
            _leftSpriteImage.color = new Color(0.8f, 0.8f, 0.8f);
            _rightSpriteImage.color = new Color(1, 1, 1);
        }
    }

    /*
        Render the dialogue rendering coroutine with a pre-set dialogue
        object.
    */
    public void QueueRenderedDialogue(Dialogue dialogue)
    {
        PrequeueDialogueText(dialogue);
        CampaignEventController.Instance.QueuedEvents.Enqueue(() =>
        {
            StartCoroutine(RenderDialogueCoroutine(() =>
            {
                switch (dialogue.actionToPlayAfterDialogue)
                {
                    case DialogueAction.HEAL_TO_FULL_HP:
                        GameManager.SetHeroHealth(GameManager.GetHeroMaxHealth());
                        SoundManager.Instance.PlaySFX(SoundEffect.HEAL_HEALTH);
                        break;
                    case DialogueAction.SECRET_WIN_SEND_TO_TITLE:
                        PlayerPrefs.SetInt("BeatBoykisser", 1);
                        TransitionManager.Instance.HideScreen("Title", 2);
                        break;
                    case DialogueAction.WON_GAME_SEND_TO_TITLE:
                        PlayerPrefs.SetInt("BeatGame", 1);
                        TransitionManager.Instance.HideScreen("Title", 2);
                        break;
                }
            }));
        });
    }

    /*
        This function shows the dialogue and starts to render it to the screen.
    */
    public IEnumerator RenderDialogueCoroutine(Action codeToRunAfter)
    {
        if (CampaignEventController.Instance != null) CampaignEventController.Instance.IsPlayingEvent = true;
        // If there is no dialogue to play (likely it was skipped), then
        // just run the code afterwards.
        if (_dialogueStringQueue.Count == 0)
        {
            codeToRunAfter.Invoke();
            yield break;
        }
        // Show the dialogue container.
        dialogueContainerObject.SetActive(true);
        // Get the most recent DialogueLine and pre-set values as the DBox is animating in.
        DialogueLine dl = _dialogueStringQueue.Peek();
        SetNameText(dl.speakerName);
        SetContentsText("");
        StartCoroutine(SetSpriteNativeSizesCoroutine());
        ChangeDialogueAnimations(dl, true);
        // Start playing the animation for the DBox.
        _dialogueBoxAnimator.Play("Show");
        _dialogueBoxAnimator.Play(dl.focusDirection == DialogueDirection.LEFT ? "NameFrameSlideLeft" : "NameFrameSlideRight");
        yield return new WaitUntil(() => !IsPlaying());
        StartCoroutine(RenderDialogueTextCoroutine(codeToRunAfter));
    }

    /*
        This function should hide the dialogue UI when called.
    */
    public IEnumerator HideDialogueBoxCoroutine(Action codeToRunAfter)
    {
        _dialogueBoxAnimator.Play("Hide");
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => !IsPlaying());
        dialogueContainerObject.SetActive(false);
        codeToRunAfter?.Invoke();
        if (CampaignEventController.Instance != null) CampaignEventController.Instance.IsPlayingEvent = false;
    }

    private IEnumerator RenderDialogueTextCoroutine(Action codeToRunAfter)
    {
        DialogueLine dl = _dialogueStringQueue.Dequeue();
        // Initialize dialogue box with starting info according to DialogueLine.
        string currentDialogueText = dl.speakerText;
        SetNameText(dl.speakerName);
        SetContentsText("");
        StartCoroutine(SetSpriteNativeSizesCoroutine());
        ChangeDialogueAnimations(dl, false);
        _dialogueBoxAnimator.Play(dl.focusDirection == DialogueDirection.LEFT ? "NameFrameSlideLeft" : "NameFrameSlideRight", -1, 1);
        // If the dialogue box is animating in, wait for it to finish.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => { return !IsPlaying(); });
        // Start rendering the current dialogue text character-by-character.
        float wfs = 0.03f;
        float punctWfs = 0.14f;
        float startTime, timeToWait;
        bool skippedDialogue = false;
        for (int i = 1; i < currentDialogueText.Length + 1; i++)
        {
            SetContentsText(currentDialogueText.Substring(0, i));
            // Wait for an amount of time depending on the current character.
            startTime = Time.time;
            char currentChar = currentDialogueText[i - 1];
            if (currentChar == '?' || currentChar == '!' || currentChar == '.' || currentChar == ',')
            {
                timeToWait = punctWfs;
            }
            else
            {
                SoundManager.Instance.PlayBlip(dl.characterBlipName);
                timeToWait = wfs;
            }
            while (Time.time - startTime < timeToWait)
            {
                // If the player clicks left click, skip the wait times!
                if (!TopBarController.Instance.IsPlayerInteractingWithTopBar() && !TopBarController.Instance.IsCardPreviewShowing() && !JournalManager.Instance.IsJournalShowing() && !SettingsManager.Instance.IsGamePaused() && Input.GetMouseButtonDown(0)) { skippedDialogue = true; break; }
                yield return null;
            }
        }
        // Stop the animators.
        ChangeDialogueAnimations(dl, true);
        // Wait until the user left-clicks in the next frame.
        if (skippedDialogue) { yield return new WaitUntil(() => { return Input.GetMouseButtonUp(0); }); }
        yield return new WaitUntil(() => { return !TopBarController.Instance.IsPlayerInteractingWithTopBar() && !TopBarController.Instance.IsCardPreviewShowing() && !JournalManager.Instance.IsJournalShowing() && !SettingsManager.Instance.IsGamePaused() && Input.GetMouseButtonDown(0); });
        // If there are more dialogue strings to render, render those!
        if (_dialogueStringQueue.Count != 0)
        {
            StartCoroutine(RenderDialogueTextCoroutine(codeToRunAfter));
        }
        else
        {
            // Or else, make sure this dialogue doesn't play again and hide the dialogue box.
            GameManager.alreadyPlayedMapDialogues.Add(_storedDialogue.dialogueName);
            StartCoroutine(HideDialogueBoxCoroutine(codeToRunAfter));
        }
    }

    public void SetNameText(string nameText)
    {
        dialogueNameText.text = nameText;
    }

    public void SetContentsText(string dialogueText)
    {
        dialogueContentsText.text = dialogueText;
    }

    /*
        Returns a boolean representing whether or not the specified animator is
        playing an animation clip with the specified name.
    */
    private bool IsPlaying()
    {
        return _dialogueBoxAnimator.GetCurrentAnimatorStateInfo(0).length > _dialogueBoxAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

}
