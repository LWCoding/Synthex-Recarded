using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public abstract class BuyableObject : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    
    [Header("Object Assignments")]
    public GameObject purchaseOverlayObject;
    public TextMeshProUGUI costText;

    private bool _isInteractable = true;

    public abstract int GetCost();
    public abstract void OnBuyObject();
    /// <summary>
    /// Include a sprite here if the overlay image should change.
    /// Return null if should keep the overlay sprite the same.
    /// </summary>
    public abstract Sprite GetSprite();
    /// <summary>
    /// Write logic for prerequisites to check if player can buy object.
    /// Return true if the player can buy. Return false if player cannot.
    /// </summary>
    public abstract bool CanPlayerBuyPrereq();

    private void Start()
    {
        if (GetSprite() != null) purchaseOverlayObject.GetComponent<Image>().sprite = GetSprite();
        _isInteractable = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // Show price on mouse enter!
        ShowPriceOverlay();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // Hide price on mouse exit!
        HidePriceOverlay();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // If you can buy the object, buy it.
        if (CanPlayerBuyPrereq() && GetCost() <= GameManager.GetMoney())
        {
            // Subtract the GetMoney() and update the top bar.
            GameManager.SpendMoney(GetCost());
            TopBarController.Instance.UpdateCurrencyText();
            // Disable interactability.
            _isInteractable = false;
            costText.text = "";
            // Play the card chosen SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.SHOP_PURCHASE);
            // Run code to buy the object.
            OnBuyObject();
        }
    }

    private void ShowPriceOverlay() { 
        // Show price on mouse enter!
        costText.text = "$" + GetCost().ToString();
        purchaseOverlayObject.SetActive(true);
        // Set the color of the overlay text depending on if
        // the player can afford it or not.
        bool canAfford = GetCost() <= GameManager.GetMoney();
        costText.color = canAfford ? new Color(0.3f, 1, 0) : new Color(1, 0.15f, 0.15f);
    }

    private void HidePriceOverlay() {
        purchaseOverlayObject.SetActive(false);
    }
    
}
