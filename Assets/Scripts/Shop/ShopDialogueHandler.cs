using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct CharacterEmotion
{
    public string emotionName;
    public Sprite sprite;
}

public struct ShopDialogueLine
{
    public string textToSay;
    public Sprite spriteToTransitionTo;
}

public class ShopDialogueHandler : MonoBehaviour
{

    public static ShopDialogueHandler Instance { get; private set; }
    [Header("Object Assignments")]
    public SpriteRenderer shopkeeperSpriteRenderer;
    public TextMeshPro dialogueText;
    public List<CharacterEmotion> shopkeeperDialogueSprites = new List<CharacterEmotion>();
    private Animator _dialogueBoxAnimator;
    private Animator _shopkeeperAnimator;
    private Queue<ShopDialogueLine> _dialogueStringQueue = new Queue<ShopDialogueLine>();
    private Action _funcToRun;

    private void Awake()
    {
        Instance = this;
        _dialogueBoxAnimator = GetComponent<Animator>();
        _shopkeeperAnimator = ShopController.Instance.shopkeeperAnimator;
    }

    // Activate the dialogue box, but hide it at the start.
    private void Start()
    {
        gameObject.SetActive(false);
    }

    // Queues a line of text to be said in the dialogue box.
    public void QueueDialogueText(string text, string dialogueName = null)
    {
        ShopDialogueLine dl = new ShopDialogueLine();
        dl.textToSay = text;
        // Try to find the sprite, if one was provided.
        if (dialogueName != null)
        {
            Sprite foundSprite = FindSpriteByName(dialogueName);
            if (foundSprite != null)
            {
                dl.spriteToTransitionTo = foundSprite;
            }
        }
        // Add the text to the queue.
        _dialogueStringQueue.Enqueue(dl);
    }

    // Animates all text in the current queue to show in the dialogue box.
    // Will wait for the dialogue box to finish animating, if it is.
    // Has an optional parameter to hide the dialogue box after all dialogue is printed.
    public void RenderDialogueText(bool shouldHideWhenFinished, Action functionToRunAfterwards = null)
    {
        if (functionToRunAfterwards != null)
        {
            _funcToRun = functionToRunAfterwards;
        }
        StartCoroutine(RenderDialogueTextCoroutine(shouldHideWhenFinished));
    }


    // Plays an animation to show the dialogue box.
    // Hide any pre-existing text when playing animation.
    public void ShowDialogueBox()
    {
        dialogueText.text = "";
        gameObject.SetActive(true);
        _dialogueBoxAnimator.Play("Show");
    }

    // Plays an animation to hide the dialogue box.
    private void HideDialogueBox()
    {
        _dialogueBoxAnimator.Play("Hide");
    }

    private IEnumerator RenderDialogueTextCoroutine(bool shouldHideWhenFinished)
    {
        ShopDialogueLine dl = _dialogueStringQueue.Dequeue();
        string text = dl.textToSay;
        // If the dialogue box is animating in, wait for it to finish.
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() =>
        {
            return !IsAnimatorPlaying();
        });
        // Set the dialogue sprite, if applicable.
        if (dl.spriteToTransitionTo != null)
        {
            shopkeeperSpriteRenderer.sprite = dl.spriteToTransitionTo;
        }
        // Make the dialogue sprite animate while talking.
        _shopkeeperAnimator.Play("ShopkeepTalk");
        float wfs = 0.03f;
        float punctWfs = 0.14f;
        float startTime, timeToWait;
        for (int i = 1; i < text.Length + 1; i++)
        {
            dialogueText.text = text.Substring(0, i);
            // Wait for an amount of time depending on the current character.
            startTime = Time.time;
            char currentChar = text[i - 1];
            if (currentChar == '?' || currentChar == '!' || currentChar == '.' || currentChar == ',')
            {
                timeToWait = punctWfs;
            }
            else
            {
                SoundManager.Instance.PlayBlip(CharacterBlipName.SHOPKEEPER);
                timeToWait = wfs;
            }
            while (Time.time - startTime < timeToWait)
            {
                // If the player clicks left click, skip the wait times!
                if (Input.GetMouseButtonDown(0)) { break; }
                yield return null;
            }
        }
        // Make the dialogue sprite go back to normal.
        _shopkeeperAnimator.Play("ShopkeepIdle");
        // Wait for a certain time after the message is done.
        yield return new WaitForEndOfFrame();
        startTime = Time.time;
        timeToWait = 1.2f;
        yield return new WaitUntil(() => { return !SettingsManager.Instance.IsGamePaused() && !Input.GetMouseButton(0); });
        while (Time.time - startTime < timeToWait)
        {
            // If the player clicks left click, skip this wait time!
            if (Input.GetMouseButtonDown(0))
            {
                timeToWait = 0;
            }
            yield return null;
        }
        // If there are more dialogue strings to render, render those!
        if (_dialogueStringQueue.Count != 0)
        {
            StartCoroutine(RenderDialogueTextCoroutine(shouldHideWhenFinished));
        }
        else if (shouldHideWhenFinished)
        {
            HideDialogueBox();
            _funcToRun();
        }
    }

    // Returns a sprite by the assigned name.
    // If the sprite isn't found, returns null.
    public Sprite FindSpriteByName(string name)
    {
        foreach (CharacterEmotion ce in shopkeeperDialogueSprites)
        {
            if (ce.emotionName == name)
            {
                return ce.sprite;
            }
        }
        Debug.Log("DIALOGUESPRITES.CS: Couldn't find sprite " + name + " for Shopkeeper!");
        return null;
    }

    private bool IsAnimatorPlaying()
    {
        return _dialogueBoxAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }
}
