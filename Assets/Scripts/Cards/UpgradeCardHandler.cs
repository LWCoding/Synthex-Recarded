using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CardHandler))]
public class UpgradeCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
IPointerClickHandler
{

    [Header("Object Assignments")]
    public GameObject purchaseOverlayObject;
    public TextMeshProUGUI cardCostText;

    private CardHandler _parentCardHandler;
    private bool _isInteractable;
    private Transform _parentCardTransform;
    private Card _card;
    private int _cardIdx;
    private int _cardCost;

    private void Awake()
    {
        _parentCardHandler = GetComponent<CardHandler>();
        _parentCardTransform = _parentCardHandler.CardObject.transform;
    }

    private void Start()
    {
        _card = _parentCardHandler.card;
        _cardIdx = _parentCardHandler.cardIdx;
        _isInteractable = true;
    }

    private void SetPreviewInfo()
    {
        _cardCost = _card.level * 2;
        if (_card.IsMaxLevel())
        {
            cardCostText.text = "MAX";
            cardCostText.color = new Color(0, 0.8f, 1);
        }
        else
        {
            cardCostText.text = _cardCost.ToString() + " XP";
            if (_cardCost <= GameController.GetXP())
            {
                // Can afford the card!
                cardCostText.color = new Color(0.3f, 1, 0);
            }
            else
            {
                // Cannot afford the card.
                cardCostText.color = new Color(1, 0.15f, 0.15f);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        _parentCardHandler.SetSortingOrder(2);
        // Set the text it should show based on the card's info.
        SetPreviewInfo();
        // Show price on mouse enter!
        purchaseOverlayObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        _parentCardHandler.SetSortingOrder(1);
        // Hide price on mouse exit!
        purchaseOverlayObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // If you can buy the card, buy the card.
        if (!_card.IsMaxLevel() && _cardCost <= GameController.GetXP())
        {
            // Subtract the GetMoney() and update the top bar.
            GameController.SpendXP(_cardCost);
            TopBarController.Instance.UpdateCurrencyText();
            // Play the card chosen SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.SHOP_PURCHASE);
            // Upgrade the card in the deck.
            GameController.UpgradeCardInDeck(_cardIdx);
            UpgradeController.Instance.RefreshCardPreviews();
            // Set the information for this card after the upgrade.
            // This will run for other cards, only when they're hovered.
            SetPreviewInfo();
        }
    }

}
