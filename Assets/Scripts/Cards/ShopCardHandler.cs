using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CardHandler))]
[RequireComponent(typeof(UITooltipHandler))]
public class ShopCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
IPointerClickHandler
{

    [Header("Object Assignments")]
    public GameObject purchaseOverlayObject;
    public TextMeshProUGUI cardCostText;

    private CardHandler _parentCardHandler;
    private UITooltipHandler _uiTooltipHandler;
    private bool _isInteractable;
    private Transform _parentCardTransform;
    private Card _card;
    private int _cardCost;

    private void Awake()
    {
        _parentCardHandler = GetComponent<CardHandler>();
        _uiTooltipHandler = GetComponent<UITooltipHandler>();
        _parentCardTransform = _parentCardHandler.CardObject.transform;
    }

    private void Start()
    {
        _card = _parentCardHandler.card;
        _cardCost = GetRandomCost(_card.cardData.cardRarity);
        _isInteractable = true;
        _uiTooltipHandler.SetTooltipPosition(TooltipPosition.RIGHT);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        _parentCardHandler.SetSortingOrder(2);
        // Show price on mouse enter!
        cardCostText.text = "$" + _cardCost.ToString();
        purchaseOverlayObject.SetActive(true);
        // Set the color of the overlay text depending on if
        // the player can afford it or not.
        if (_cardCost <= GameManager.GetMoney())
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
        if (_cardCost <= GameManager.GetMoney())
        {
            // Subtract the GetMoney() and update the top bar.
            GameManager.SpendMoney(_cardCost);
            TopBarController.Instance.UpdateCurrencyText();
            TopBarController.Instance.AnimateCardsToDeck(transform.position, new List<Card> { _card }, _parentCardHandler.transform.localScale);
            // Make the card not interactable.
            _isInteractable = false;
            cardCostText.text = "";
            _parentCardHandler.SetSortingOrder(1);
            _parentCardHandler.DisableInteractions();
            // Play the card chosen SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.SHOP_PURCHASE);
            // Add the card to the deck.
            GameManager.AddCardToDeck(_card);
        }
    }

    private int GetRandomCost(CardRarity cardRarity)
    {
        switch (cardRarity)
        {
            case CardRarity.COMMON:
                return 100 + Random.Range(-20, 20); // 80-120 $
            case CardRarity.UNCOMMON:
                return 150 + Random.Range(-20, 20); // 130-170 $
            case CardRarity.RARE:
                return 200 + Random.Range(-20, 20); // 180-220 $
        }
        return 99999; // Unobtainable!
    }

}
