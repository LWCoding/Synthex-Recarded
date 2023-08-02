using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeInfoHandler : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private CardHandler _cardBeforeUpgradeHandler;
    [SerializeField] private CardHandler _cardAfterUpgradeHandler;
    [SerializeField] private TextMeshProUGUI _upgradeInfoText;
    [SerializeField] private TextMeshProUGUI _upgradeCostText;

    private Card _card;
    private int _cardCost;
    private int _cardIdx;

    ///<summary>
    /// Initialize the information of this current card.
    ///</summary>
    public void Initialize(Card c, int cost, int ci)
    {
        _card = c;
        _cardCost = cost;
        _cardIdx = ci;
        // We need to manually call awake because these prefabs were NOT instantiated.
        _cardBeforeUpgradeHandler.Awake();
        _cardAfterUpgradeHandler.Awake();
        // Initialize both cards.
        _cardBeforeUpgradeHandler.Initialize(c);
        _cardBeforeUpgradeHandler.DisableInteractions();
        Card cardAfterUpgrade = Globals.GetCard(c.GetCardUniqueName(), c.level + 1);
        _cardAfterUpgradeHandler.Initialize(cardAfterUpgrade);
        _cardAfterUpgradeHandler.DisableInteractions();
        _upgradeInfoText.text = "Upgrade Details:\n" + UpgradeController.Instance.FindDifferenceInCards(c, cardAfterUpgrade);
        _upgradeCostText.text = "Cost: " + cost.ToString() + " XP";
    }

    // Delete the current card (that this UpgradeInfoHandler is representing)
    // from the overall UpgradeController list. Then, update the summary console
    // and destroy the current object.
    public void DeleteCardFromList()
    {
        UpgradeController.Instance.RemoveCardFromUpgradeList(_card, _cardCost, _cardIdx);
        UpgradeController.Instance.UpdateSummaryConsole();
        Destroy(gameObject);
    }

}
