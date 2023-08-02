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

    ///<summary>
    /// Initialize the information of this current card.
    ///</summary>
    public void Initialize(Card c, int cost)
    {
        _card = c;
        _cardCost = cost;
        // We need to manually call awake because these prefabs were NOT instantiated.
        _cardBeforeUpgradeHandler.Awake();
        _cardAfterUpgradeHandler.Awake();
        // Initialize both cards.
        _cardBeforeUpgradeHandler.Initialize(c);
        _cardAfterUpgradeHandler.Initialize(c);
        _cardBeforeUpgradeHandler.DisableInteractions();
        _cardAfterUpgradeHandler.DisableInteractions();
        _upgradeCostText.text = cost.ToString() + " XP TO UPGRADE";
    }

    // Delete the current card (that this UpgradeInfoHandler is representing)
    // from the overall UpgradeController list. Then, update the summary console
    // and destroy the current object.
    public void DeleteCardFromList()
    {
        UpgradeController.Instance.RemoveCardFromUpgradeList(_card, _cardCost);
        UpgradeController.Instance.UpdateSummaryConsole();
        Destroy(gameObject);
    }

}
