using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ShopTab
{
    CARD, ITEM, RELIC
}

public class ShopController : MonoBehaviour
{

    public static ShopController Instance { get; private set; }
    [Header("Object Assignments")]
    public Animator shopDoorAnimator;
    public Animator shopkeeperAnimator;
    public CanvasGroup hologramCanvasGroup;
    public Transform rotatingObjectParentTransform; // Objects that change everytime shop is loaded.
    public ScrollRect holoScrollRect;
    public ShopIconHandler cardShopIconHandler;
    public ShopIconHandler itemShopIconHandler;
    public ShopIconHandler relicShopIconHandler;
    public Button exitShopButton;
    [Header("Audio Assignments")]
    public AudioClip shopDoorOpenSFX;
    public AudioClip shopDoorCloseSFX;

    private bool _isFirstTimeAtShop;
    private bool _isPlayerLeavingShop;

    private void Awake()
    {
        Instance = GetComponent<ShopController>();
        exitShopButton.interactable = false;
        _isPlayerLeavingShop = false;
    }

    private void Start()
    {
        // Initialize the UI.
        GlobalUIController.Instance.InitializeUI();
        // Randomize the shop BG objects.
        RandomizeShopBGObjects();
        // Make the game fade from black to clear.
        FadeTransitionController.Instance.ShowScreen(0.75f);
        // Play game music!
        SoundManager.Instance.PlayOnLoop(MusicType.SHOP_MUSIC);
        // Play the shop door animation and hide the hologram UI.
        OpenDoorAfterDelay();
        HideHologramUI(false);
    }

    public void CloseDoor()
    {
        StartCoroutine(CloseDoorAfterDelayCoroutine());
    }

    private IEnumerator CloseDoorAfterDelayCoroutine()
    {
        exitShopButton.interactable = false;
        _isPlayerLeavingShop = true;
        ShopDialogueHandler.Instance.ClearExistingDialogue();
        ShopDialogueHandler.Instance.HideDialogueBox();
        yield return new WaitForSeconds(0.4f);
        shopkeeperAnimator.Play("ShopkeepHide");
        yield return new WaitForSeconds(0.8f);
        shopDoorAnimator.gameObject.SetActive(true);
        shopDoorAnimator.Play("DoorClose");
        SoundManager.Instance.PlayOneShot(shopDoorCloseSFX, 1);
        yield return new WaitForSeconds(1f);
        FadeTransitionController.Instance.HideScreen("Map", 0.75f);
    }

    private void OpenDoorAfterDelay()
    {
        StartCoroutine(OpenDoorAfterDelayCoroutine());
    }

    private IEnumerator OpenDoorAfterDelayCoroutine()
    {
        yield return new WaitForEndOfFrame();
        // Enable the CARD shop tab
        SwitchShopTabTo(ShopTab.CARD);
        // Hide the UI so it's uninteractable
        hologramCanvasGroup.gameObject.SetActive(false);
        // Hide the shopkeeper so he can pop up later
        shopkeeperAnimator.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.8f);
        shopDoorAnimator.Play("DoorOpen");
        SoundManager.Instance.PlayOneShot(shopDoorOpenSFX, 1);
        yield return new WaitForSeconds(1.7f);
        shopDoorAnimator.gameObject.SetActive(false);
        shopkeeperAnimator.gameObject.SetActive(true);
        shopkeeperAnimator.Play("ShopkeepShow");
        yield return new WaitForSeconds(1f);
        _isFirstTimeAtShop = !GameController.visitedShopBefore;
        if (GameController.visitedShopBefore)
        {
            // If the player has visited a shop before, just show the UI.
            ShowHologramUI(true);
            // Allow player to click the back button to leave.
            exitShopButton.interactable = true;
        }
        else
        {
            // If the player hasn't, then play some dialogue.
            GameController.visitedShopBefore = true;
            ShopDialogueHandler.Instance.ShowDialogueBox();
            ShopDialogueHandler.Instance.QueueDialogueText("Scanning... facial identification.", "Scan");
            ShopDialogueHandler.Instance.QueueDialogueText("Ah! Howdy. You must be Jack.", "Neutral");
            ShopDialogueHandler.Instance.QueueDialogueText("I'm Shop-Bot. Please take a look at my wares.", "Neutral");
            ShopDialogueHandler.Instance.QueueDialogueText("I have cards, which perform actions during battle, and relics, which give you permanent buffs.", "Glance");
            ShopDialogueHandler.Instance.QueueDialogueText("I also have some items that may help you on your way to what I presume to be Aericho City.", "Glance");
            ShopDialogueHandler.Instance.QueueDialogueText("I have different materials at every location. So swing by at any time.", "Neutral");
            ShopDialogueHandler.Instance.RenderDialogueText(true, true, () => { ShowHologramUI(true); exitShopButton.interactable = true; });
        }
    }

    public void OnClickedCardsTab()
    {
        if (_isPlayerLeavingShop) { return; }
        if (_isFirstTimeAtShop)
        {
            ShopDialogueHandler.Instance.ShowDialogueBox();
            ShopDialogueHandler.Instance.ClearExistingDialogue();
            ShopDialogueHandler.Instance.QueueDialogueText("Cards. Assembling a good deck helps you develop a better strategy in battle!", "Neutral");
            ShopDialogueHandler.Instance.QueueDialogueText("If any have a special effect, hover over the card for a while to see what they do.", "Scan");
            ShopDialogueHandler.Instance.RenderDialogueText(true, false);
        }
    }

    public void OnClickedRelicTab()
    {
        if (_isPlayerLeavingShop) { return; }
        if (_isFirstTimeAtShop)
        {
            ShopDialogueHandler.Instance.ShowDialogueBox();
            ShopDialogueHandler.Instance.ClearExistingDialogue();
            ShopDialogueHandler.Instance.QueueDialogueText("Ah, relics. They'll grant you permanent, passive buffs both in and out of battle.", "Neutral");
            ShopDialogueHandler.Instance.QueueDialogueText("They range in rarity, so some are more expensive than others. If you break it, you buy it.", "Glance");
            ShopDialogueHandler.Instance.RenderDialogueText(true, false);
        }
    }

    public void OnClickedItemTab()
    {
        if (_isPlayerLeavingShop) { return; }
        if (_isFirstTimeAtShop)
        {
            ShopDialogueHandler.Instance.ShowDialogueBox();
            ShopDialogueHandler.Instance.ClearExistingDialogue();
            ShopDialogueHandler.Instance.QueueDialogueText("Items for sale! They're one-time-use, and you can only hold up to three at a time.", "Glance");
            ShopDialogueHandler.Instance.QueueDialogueText("You can double click an item in your inventory to use it. Some can only be used during battle, though.", "Neutral");
            ShopDialogueHandler.Instance.RenderDialogueText(true, false);
        }
    }

    // Randomizes whether or not the shop background objects are shown or hidden.
    // This is purely cosmetic.
    private void RandomizeShopBGObjects()
    {
        for (int i = 0; i < rotatingObjectParentTransform.childCount; i++)
        {
            GameObject obj = rotatingObjectParentTransform.GetChild(i).gameObject;
            float chance = Random.Range(0f, 1f);
            // 50% chance to not show the object. 50% chance to show the object.
            if (chance < 0.5f)
            {
                obj.SetActive(false);
            }
            else
            {
                obj.SetActive(true);
            }
        }
    }

    public void ShowHologramUI(bool shouldAnimate)
    {
        hologramCanvasGroup.gameObject.SetActive(true);
        if (!shouldAnimate)
        {
            hologramCanvasGroup.alpha = 1;
            return;
        }
        StartCoroutine(ToggleHologramUICoroutine(true));
    }

    public void HideHologramUI(bool shouldAnimate)
    {
        if (!shouldAnimate)
        {
            hologramCanvasGroup.alpha = 0;
            return;
        }
        StartCoroutine(ToggleHologramUICoroutine(false));
    }

    private IEnumerator ToggleHologramUICoroutine(bool shouldShow)
    {
        WaitForSeconds wfs = new WaitForSeconds(0.01f);
        float initialVal = (shouldShow) ? 0 : 1;
        float targetVal = (shouldShow) ? 1 : 0;
        for (int i = 0; i < 30; i++)
        {
            hologramCanvasGroup.alpha = Mathf.Lerp(initialVal, targetVal, (float)i / 30);
            yield return wfs;
        }
    }

    public void SwitchShopTabTo(ShopTab shopTab)
    {
        List<ShopIconHandler> allIconHandlers = new List<ShopIconHandler>() {
            cardShopIconHandler, itemShopIconHandler, relicShopIconHandler
        };
        ShopIconHandler chosenShopIconHandler = null;
        switch (shopTab)
        {
            case ShopTab.CARD:
                chosenShopIconHandler = cardShopIconHandler;
                break;
            case ShopTab.ITEM:
                chosenShopIconHandler = itemShopIconHandler;
                break;
            case ShopTab.RELIC:
                chosenShopIconHandler = relicShopIconHandler;
                break;
        }
        foreach (ShopIconHandler iconHandler in allIconHandlers)
        {
            if (chosenShopIconHandler == iconHandler)
            {
                iconHandler.ChooseButton();
                iconHandler.isInteractable = false;
            }
            else
            {
                iconHandler.UnchooseButton();
                iconHandler.isInteractable = true;
            }
        }
    }

}
