using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ItemHandler))]
public class ShopItemHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public TextMeshProUGUI itemCostText;
    public GameObject frameOverlayObject;
    private ItemHandler _parentItemHandler;
    private Item _itemInfo;
    private bool _isInteractable;
    private int _itemCost;

    private void Awake()
    {
        _parentItemHandler = GetComponent<ItemHandler>();
    }

    private void Start()
    {
        _itemInfo = _parentItemHandler.itemInfo;
        _itemCost = GetRandomCost(_itemInfo.itemRarity);
        _isInteractable = true;
        frameOverlayObject.GetComponent<Image>().sprite = _itemInfo.itemImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // Show price on mouse enter!
        itemCostText.text = "$" + _itemCost.ToString();
        frameOverlayObject.SetActive(true);
        // Set the color of the overlay text depending on if
        // the player can afford it or not.
        if (_itemCost < GameManager.GetMoney() && !GameManager.IsItemBagFull())
        {
            // Can afford the relic!
            itemCostText.color = new Color(0.3f, 1, 0);
        }
        else
        {
            // Cannot afford the relic.
            itemCostText.color = new Color(1, 0.15f, 0.15f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // Hide tooltip and price on mouse exit!
        frameOverlayObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isInteractable) { return; }
        // If you can buy the relic, buy the relic.
        if (_itemCost <= GameManager.GetMoney())
        {
            // Attempt to add the item to the inventory.
            // This won't work if, say, the player has reached the item limit.
            if (GameManager.IsItemBagFull()) { return; }
            // Subtract the money and update the top bar.
            GameManager.SpendMoney(_itemCost);
            TopBarController.Instance.UpdateCurrencyText();
            // Add the item to the bag.
            GameManager.AddItemToInventory(_itemInfo);
            // Make the item not interactable.
            _isInteractable = false;
            itemCostText.text = "";
            // Make the tooltip not show anymore.
            _parentItemHandler.DisableTooltip();
            // Play the item chosen SFX.
            SoundManager.Instance.PlaySFX(SoundEffect.SHOP_PURCHASE);
            TopBarController.Instance.RenderItems();
            TopBarController.Instance.FlashItemObject(GameManager.GetItems().Count - 1);
        }
    }

    private int GetRandomCost(ItemRarity itemRarity)
    {
        switch (itemRarity)
        {
            case ItemRarity.COMMON:
                return 60 + Random.Range(-10, 10); // 50-70 $
            case ItemRarity.UNCOMMON:
                return 100 + Random.Range(-10, 10); // 90-110 $
            case ItemRarity.RARE:
                return 120 + Random.Range(-10, 10); // 110-130 $
        }
        return 99999; // Unobtainable!
    }

}
