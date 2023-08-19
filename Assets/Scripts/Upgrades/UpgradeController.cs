using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct CardAndCost
{
    public Card card;
    public int cardCost;
    public int cardIdx;
    public CardAndCost(Card c, int cc, int ci)
    {
        card = c;
        cardCost = cc;
        cardIdx = ci;
    }
}

public class UpgradeController : MonoBehaviour
{

    public static UpgradeController Instance;
    [Header("Prefab Assignments")]
    [SerializeField] private GameObject _upgradeInfoPrefab;
    [Header("Preview Object Assignments")]
    [SerializeField] private Transform _cardVertLayoutTransform;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _upgradeButton;
    [SerializeField] private Button _prevToSummButton;
    [SerializeField] private Button _summToPrevButton;
    [SerializeField] private TextMeshProUGUI _cardPreviewText;
    [SerializeField] private TextMeshProUGUI _totalCostText;
    [SerializeField] private TextMeshProUGUI _previewErrorMessageText;
    [SerializeField] private TextMeshProUGUI _summaryErrorMessageText;
    [Header("Summary Object Assignments")]
    [SerializeField] private Transform _cardInfoVertLayoutTransform;
    [SerializeField] private TextMeshProUGUI _cardSummaryText;
    [SerializeField] private Animator _screenSwitcherAnimator;
    [Header("Audio Assignments")]
    [SerializeField] private AudioClip _buttonSelectSFX;

    private List<CardHandler> _cardPreviewHandlers = new List<CardHandler>();
    private List<CardAndCost> _selectedCardsToUpgrade = new List<CardAndCost>();
    private bool _isOptionChosen = false;
    private int _totalCost = 0;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    private void Start()
    {
        // Initialize the buttons that should modify UI elements.
        _exitButton.onClick.AddListener(() =>
        {
            _isOptionChosen = true;
            TransitionManager.Instance.BackToMapOrCampaign(0.75f);
        });
        // If the player clicks the upgrade button, upgrade cards if cost allows
        // AND if the player has selected any cards to upgrade.
        _upgradeButton.onClick.AddListener(() =>
        {
            _isOptionChosen = true;
            GameManager.SpendXP(_totalCost);
            UpgradeSelectedCards();
            TransitionManager.Instance.BackToMapOrCampaign(1.25f);
        });
        // Initialize the cards in the deck.
        StartCoroutine(InitializeDeckCardsCoroutine());
        // Set the console text to have nothing at first.
        ResetConsolePreview();
        // Make the game fade from black to clear.
        TransitionManager.Instance.ShowScreen(0.75f);
    }

    public void ResetConsolePreview()
    {
        bool doesPlayerHaveEnoughXP = _totalCost <= GameManager.GetXP();
        _previewErrorMessageText.gameObject.SetActive(!doesPlayerHaveEnoughXP);
        _cardPreviewText.text = "Hover over a card to view its upgrade information.";
        _totalCostText.text = "TOTAL: <color=\"" + (doesPlayerHaveEnoughXP ? "green" : "red") + "\">" + _totalCost.ToString() + " XP</color>";
    }

    ///<summary>
    /// Updates the text on the preview console as a card is hovered over.
    /// Requires the card, cost of the card to upgrade, and whether it is selected/deselected.
    ///</summary>
    public void UpdatePreviewConsole(Card c, int upgradeCost, bool isSelected)
    {
        if (c.IsMaxLevel())
        {
            _cardPreviewText.text = "<color=#FB4BC7>" + c.GetCardDisplayName() + "</color> is already at max level. Please select another card.";
            return;
        }
        bool doesPlayerHaveEnoughXP = _totalCost <= GameManager.GetXP();
        _previewErrorMessageText.gameObject.SetActive(!doesPlayerHaveEnoughXP);
        Card cardAfterUpgrade = Globals.GetCard(c.GetCardUniqueName(), c.level + 1);
        _cardPreviewText.text = "Upgrade card from <color=#FB4BC7>" + c.GetCardDisplayName() + "</color> to <color=#FB4BC7>" + cardAfterUpgrade.GetCardDisplayName() + "</color>?\n\nCost: <color=\"green\">" + upgradeCost.ToString() + " XP</color>\n\nClick card to " + ((isSelected) ? "deselect" : "select") + ".";
        _totalCostText.text = "TOTAL: <color=\"" + (doesPlayerHaveEnoughXP ? "green" : "red") + "\">" + _totalCost.ToString() + " XP</color>";
    }

    ///<summary>
    /// Updates the text on the summary console.
    ///</summary>
    public void UpdateSummaryConsole()
    {
        bool doesPlayerHaveEnoughXP = _totalCost <= GameManager.GetXP();
        // If the player does not have enough XP to upgrade the selected cards, error.
        if (!doesPlayerHaveEnoughXP)
        {
            _summaryErrorMessageText.text = "(INSUFFICIENT FUNDS)";
            _summaryErrorMessageText.gameObject.SetActive(true);
        }
        // If the player did not select any cards to upgrade, error.
        else if (_selectedCardsToUpgrade.Count == 0)
        {
            _summaryErrorMessageText.text = "(NO CARDS SELECTED)";
            _summaryErrorMessageText.gameObject.SetActive(true);
        }
        // Or else, hide the error message.
        else
        {
            _summaryErrorMessageText.gameObject.SetActive(false);
        }
        // String together the first three selected cards and display it in the console.
        string listOfCardsString = _selectedCardsToUpgrade.Count == 0 ? "<color=#9F9F9F>None</color>\n" : "";
        for (int i = 0; i < Mathf.Min(3, _selectedCardsToUpgrade.Count); i++)
        {
            Card card = _selectedCardsToUpgrade[i].card;
            listOfCardsString += "<color=#FB4BC7>" + card.GetCardDisplayName() + "</color>\n";
        }
        if (_selectedCardsToUpgrade.Count > 3)
        {
            listOfCardsString += "<i><color=#9F9F9F>(...and " + (_selectedCardsToUpgrade.Count - 3).ToString() + " other" + (_selectedCardsToUpgrade.Count - 3 == 1 ? "" : "s") + ")</color></i>\n";
        }
        _cardSummaryText.text = "Upgrading cards:\n" + listOfCardsString + "\nTotal Cost: <color=\"" + (doesPlayerHaveEnoughXP ? "green" : "red") + "\">" + _totalCost.ToString() + " XP</color>";
    }

    ///<summary>
    /// Adds a card (given itself and its upgrade cost) to the list of cards
    /// the player is intending to buy. This does NOT update the console preview;
    /// that must be done separately.
    ///</summary>
    public void AddCardToUpgradeList(Card c, int upgradeCost, int cardIdx)
    {
        _selectedCardsToUpgrade.Add(new CardAndCost(c, upgradeCost, cardIdx));
        _totalCost += upgradeCost;
    }

    ///<summary>
    /// Removes a card (given itself and its upgrade cost) from the list of cards
    /// the player is intending to buy. This does NOT update the console preview;
    /// that must be done separately.
    ///</summary>
    public void RemoveCardFromUpgradeList(Card c, int upgradeCost, int cardIdx)
    {
        _selectedCardsToUpgrade.Remove(new CardAndCost(c, upgradeCost, cardIdx));
        _totalCost -= upgradeCost;
        _cardPreviewHandlers[cardIdx].GetComponent<UpgradeCardHandler>().SetIsSelected(false);
    }

    ///<summary>
    /// Upgrades all cards in the _selectedCardsToUpgrade list.
    /// Clears the list afterwards so this won't upgrade cards multiple times.
    ///</summary>
    public void UpgradeSelectedCards()
    {
        for (int i = 0; i < _selectedCardsToUpgrade.Count; i++)
        {
            GameManager.GetHeroCards()[_selectedCardsToUpgrade[i].cardIdx].UpgradeLevel();
        }
        _selectedCardsToUpgrade.Clear();
    }

    ///<summary>
    /// Erases all pre-existing card info in the summary screen before creating all 
    /// of the card info prefabs in the summary screen (using current list of cards).
    ///</summary>
    public void UpdateCardInfosInSummaryScreen()
    {
        // Destroy all pre-existing card infos.
        foreach (Transform t in _cardInfoVertLayoutTransform)
        {
            Destroy(t.gameObject);
        }
        // For every card in the current cards list, create a new prefab.
        foreach (CardAndCost cardAndCost in _selectedCardsToUpgrade)
        {
            GameObject cardInfo = Instantiate(_upgradeInfoPrefab, _cardInfoVertLayoutTransform);
            cardInfo.GetComponent<UpgradeInfoHandler>().Initialize(cardAndCost.card, cardAndCost.cardCost, cardAndCost.cardIdx);
        }
    }

    ///<summary>
    /// Switches the upgrade screen from the card preview to the order summary screen,
    /// or vice versa. Throws error if animation is not one of the expected results.
    ///</summary>
    public void SwitchUpgradeScreens(string animationName)
    {
        StartCoroutine(SwitchUpgradeScreensCoroutine(animationName));
    }

    ///<summary>
    /// Given two cards, determines the difference between the two cards and
    /// returns the description as a string. Shows difference FROM c1 TO c2.
    ///</summary>
    public string FindDifferenceInCards(Card c1, Card c2)
    {
        string changeInfo = "";
        CardStats currentStats = c1.GetCardStats();
        CardStats afterStats = c2.GetCardStats();
        // If the costs aren't the same, register that difference.
        if (currentStats.cardCost != afterStats.cardCost)
        {
            changeInfo += (currentStats.cardCost > afterStats.cardCost ? "-" : "+") + Mathf.Abs(afterStats.cardCost - currentStats.cardCost) + " <sprite name=\"energy\">\n";
        }
        // If the attack or block levels aren't the same, register that difference.
        if (currentStats.damageValue != afterStats.damageValue)
        {
            changeInfo += (currentStats.damageValue > afterStats.damageValue ? "-" : "+") + Mathf.Abs(afterStats.damageValue - currentStats.damageValue) + " <sprite name=\"damage\">\n";
        }
        if (currentStats.blockValue != afterStats.blockValue)
        {
            changeInfo += (currentStats.blockValue > afterStats.blockValue ? "-" : "+") + Mathf.Abs(afterStats.blockValue - currentStats.blockValue) + " <sprite name=\"block\">\n";
        }
        // If the attack repeat count isn't the same, register that difference.
        if (currentStats.attackRepeatCount != afterStats.attackRepeatCount)
        {
            changeInfo += (currentStats.attackRepeatCount > afterStats.attackRepeatCount ? "-" : "+") + Mathf.Abs(afterStats.attackRepeatCount - currentStats.attackRepeatCount) + " repeat count\n";
        }
        // Register differences regarding status effects.
        foreach (CardInflict infliction in afterStats.inflictions)
        {
            string statusName = Globals.GetStatus(infliction.effect).statusInfo.statusName;
            // If there is a new status effect, register that difference.
            if (currentStats.inflictions.Find(i => i.effect == infliction.effect) == null)
            {
                changeInfo += "+" + (statusName + (infliction.amplifier == 0 ? "" : " " + infliction.amplifier) + "\n");
            }
            // If there are any existing status effects that were changed, register those differences.
            CardInflict currentInfliction = currentStats.inflictions.Find(i => i.effect == infliction.effect);
            if (currentInfliction != null && currentInfliction.amplifier != infliction.amplifier)
            {
                changeInfo += (currentInfliction.amplifier > infliction.amplifier ? "-" : "+") + Mathf.Abs(currentInfliction.amplifier - infliction.amplifier) + " <sprite name=\"" + statusName.ToLower() + "\">";
            }
        }
        // Register differences regarding new modifiers.
        foreach (CardModifier modifier in afterStats.modifiers)
        {
            // If there is a new modifier, register that difference.
            if (currentStats.modifiers.Find(m => m.trait == modifier.trait) == null)
            {
                changeInfo += "+ Gains <color=\"yellow\">" + (modifier.trait + "</color>\n");
            }
            // If there are any existing status effects that were changed, register those differences.
            CardModifier currentModifier = currentStats.modifiers.Find(m => m.trait == modifier.trait);
            if (currentModifier != null && currentModifier.amplifier != modifier.amplifier)
            {
                changeInfo += (currentModifier.amplifier > modifier.amplifier ? "-" : "+") + Mathf.Abs(currentModifier.amplifier - modifier.amplifier) + " <color=\"yellow\">amplifier</color>";
            }
        }
        // Register differences regarding removing modifiers.
        foreach (CardModifier modifier in currentStats.modifiers)
        {
            // If there is a new modifier, register that difference.
            if (afterStats.modifiers.Find(m => m.trait == modifier.trait) == null)
            {
                changeInfo += "- Loses <color=\"yellow\">" + (modifier.trait + "</color>\n");
            }
        }
        return changeInfo;
    }

    // Plays the button hover sound.
    public void PlayButtonHoverSFX(Button button)
    {
        if (_isOptionChosen) { return; }
        if (!button.interactable) { return; }
        SoundManager.Instance.PlaySFX(SoundEffect.GENERIC_BUTTON_HOVER);
    }

    // Plays the button select sound.
    // If an option has already been selected, let's not let the sound play,
    // unless we are calling that as we've selected the option.
    public void PlayButtonSelectSFX(Button button)
    {
        if (!button.interactable || _isOptionChosen) { return; }
        SoundManager.Instance.PlayOneShot(_buttonSelectSFX);
    }

    private IEnumerator SwitchUpgradeScreensCoroutine(string animationName)
    {
        yield return new WaitForEndOfFrame();
        _isOptionChosen = true;
        if (animationName != "Switch1To2" && animationName != "Switch2To1")
        {
            Debug.LogError("Didn't provide proper animation name to SwitchUpgradeScreens() in UpgradeController.cs!");
            yield break;
        }
        // IF WE ARE GOING FROM PREVIEW TO THE SUMMARY SCREEN...
        if (animationName == "Switch1To2")
        {
            _prevToSummButton.interactable = false;
            _exitButton.interactable = false;
            // Make all cards uninteractable.
            foreach (CardHandler cardHandler in _cardPreviewHandlers)
            {
                cardHandler?.GetComponent<UpgradeCardHandler>()?.SetInteractable(false);
            }
            // Update card infos on the next screen.
            UpdateCardInfosInSummaryScreen();
            // Update console on the next screen.
            UpdateSummaryConsole();
        }
        // IF WE ARE GOING FROM SUMMARY TO THE PREVIEW SCREEN...
        else if (animationName == "Switch2To1")
        {
            _summToPrevButton.interactable = false;
            _upgradeButton.interactable = false;
            ResetConsolePreview();
            // Make cards interactable again.
            foreach (CardHandler cardHandler in _cardPreviewHandlers)
            {
                cardHandler?.GetComponent<UpgradeCardHandler>()?.SetInteractable(true);
            }
        }
        // Play the animation and wait for it to finish.
        _screenSwitcherAnimator.Play(animationName);
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => _screenSwitcherAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        // Reenable the buttons after the animation is done playing.
        _prevToSummButton.interactable = true;
        _summToPrevButton.interactable = true;
        _upgradeButton.interactable = _totalCost <= GameManager.GetXP() && _selectedCardsToUpgrade.Count > 0;
        _exitButton.interactable = true;
        _isOptionChosen = false;
    }

    // Initializes all of the cards in the player's deck on the preview screen.
    // We only need to do this once because the player's deck won't change.
    private IEnumerator InitializeDeckCardsCoroutine()
    {
        Transform horizontalTransform = null;
        int currCardIdx = 0;
        // Order the cards in alphabetical order, so the player
        // can't cheat and see the exact order.
        List<Card> cardsToShow = GameManager.GetHeroCards();
        // Recover a pooled object for each card.
        foreach (Card card in cardsToShow)
        {
            // If divisible by 3, create a new row of cards.
            if (currCardIdx % 3 == 0)
            {
                GameObject newRow = CreateNewCardRow();
                horizontalTransform = newRow.transform;
                horizontalTransform.SetParent(_cardVertLayoutTransform, false);
            }
            // Set the basic information for the card.
            GameObject cardObject = GetCardObjectFromPool();
            CardHandler cardController = cardObject.GetComponent<CardHandler>();
            cardObject.transform.SetParent(horizontalTransform, false);
            // We want the card to appear from nothing, so set the
            // initial showing to false.
            cardController.Initialize(card, false);
            cardController.SetCardIdx(currCardIdx);
            currCardIdx++;
            _cardPreviewHandlers.Add(cardController);
        }
        // After all cards are created, animate them one-by-one.
        WaitForSeconds wfs = new WaitForSeconds(0.04f);
        foreach (CardHandler cc in _cardPreviewHandlers)
        {
            cc.CardAppear();
            yield return wfs;
        }
    }

    // Grabs a card from the pool but tweaks some basic properties of it to make
    // it look good.
    private GameObject GetCardObjectFromPool()
    {
        // Return an already created card object.
        GameObject cardObject = ObjectPooler.Instance.GetObjectFromPool(PoolableType.CARD);
        CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
        cardObject.transform.localPosition = new Vector3(cardObject.transform.localPosition.x, cardObject.transform.localPosition.y, 0);
        cardObject.transform.localScale = new Vector2(0.5f, 0.5f);
        cardHandler.ModifyHoverBehavior(true, false, false, false); // Modify to be static & unselectable.
        cardHandler.HideCardInstantly(); // Hide the card instantly so we can animate it after.
        cardHandler.EnableUpgradeFunctionality();
        return cardObject;
    }

    // Creates a new GameObject with a HorizontalLayoutGroup and returns
    // it. This is a helper function to organize objects in a layout.
    private GameObject CreateNewCardRow()
    {
        GameObject newRow = new GameObject("CardRow", typeof(HorizontalLayoutGroup));
        HorizontalLayoutGroup newRowHLG = newRow.GetComponent<HorizontalLayoutGroup>();
        newRowHLG.childControlWidth = true;
        newRowHLG.childForceExpandWidth = true;
        newRowHLG.spacing = 25;
        newRow.GetComponent<RectTransform>().sizeDelta = new Vector2(680, 0);
        return newRow;
    }

}
