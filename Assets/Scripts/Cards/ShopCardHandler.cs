using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(CardHandler))]
public class ShopCardHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
IPointerClickHandler
{

    [Header("Object Assignments")]
    public GameObject shopOverlayObject;
    public TextMeshProUGUI cardCostText;

    public CardHandler _parentCardHandler;
    private bool _isInteractable;
    private Transform _parentCardTransform;
    private Card _card;
    private int _cardCost;

    private void Awake()
    {
        _parentCardHandler = GetComponent<CardHandler>();
        _parentCardTransform = _parentCardHandler.CardObject.transform;
    }

    private void Start()
    {
        _card = _parentCardHandler.card;
        _cardCost = GetRandomCost(_card.cardData.cardRarity);
        _isInteractable = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        _parentCardHandler.SetSortingOrder(2);
        // Show price on mouse enter!
        cardCostText.text = "$" + _cardCost.ToString();
        shopOverlayObject.SetActive(true);
        // Set the color of the overlay text depending on if
        // the player can afford it or not.
        if (_cardCost < GameController.GetMoney())
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
        shopOverlayObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // If you can buy the card, buy the card.
        if (_cardCost <= GameController.GetMoney())
        {
            // Subtract the GetMoney() and update the top bar.
            GameController.SpendMoney(_cardCost);
            TopBarController.Instance.UpdateCurrencyText();
            TopBarController.Instance.AnimateCardsToDeck(transform.position, new List<Card> { _card }, _parentCardHandler.transform.localScale);
            // Make the card not interactable.
            _isInteractable = false;
            cardCostText.text = "";
            _parentCardHandler.SetSortingOrder(1);
            _parentCardHandler.DisableFunctionality();
            // Play the card chosen SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.SHOP_PURCHASE);
            // Add the card to the deck.
            GameController.AddCardToDeck(_card);
        }
    }

    private int GetRandomCost(CardRarity cardRarity)
    {
        switch (cardRarity)
        {
            case CardRarity.COMMON:
                return 100 + Random.Range(-20, 20); // 80-120 $
            case CardRarity.UNCOMMON:
                return 170 + Random.Range(-20, 20); // 150-190 $
            case CardRarity.RARE:
                return 240 + Random.Range(-20, 20); // 220-260 $
        }
        return 99999; // Unobtainable!
    }

}
