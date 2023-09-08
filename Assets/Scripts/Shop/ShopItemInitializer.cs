using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemInitializer : MonoBehaviour
{

    [Header("Object Assignments")]
    public Transform scrollParentTransform;
    private List<ItemHandler> _itemPreviewControllers = new List<ItemHandler>();
    private List<Item> _currentItemsInShop = new List<Item>();

    private void Start()
    {
        _currentItemsInShop = GameManager.nextShopLoadout.items;
        InitializeShopItems();
    }

    private void InitializeShopItems()
    {
        Transform horizontalTransform = null;
        int currItemIdx = 0;
        // Recover a pooled object for each item.
        foreach (Item item in _currentItemsInShop)
        {
            // If divisible by 2, create a new row of items.
            // This number can be changed at any time to modify
            // the amount of items shown in one row.
            if (currItemIdx % 2 == 0)
            {
                GameObject newRow = CreateNewItemRow();
                horizontalTransform = newRow.transform;
                horizontalTransform.SetParent(scrollParentTransform, false);
            }
            // Set the basic information for the item.
            GameObject itemObject = ObjectPooler.Instance.GetObjectFromPool(PoolableType.ITEM);
            ItemHandler itemHandler = itemObject.GetComponent<ItemHandler>();
            itemHandler.Initialize(item, true, false);
            itemHandler.ToggleShopFunctionality(true);
            itemHandler.SetSortingOrder(1);
            itemHandler.SetItemImageScale(2, 1);
            itemObject.transform.SetParent(horizontalTransform, false);
            currItemIdx++;
            _itemPreviewControllers.Add(itemHandler);
        }
    }

    // Creates a new GameObject with a HorizontalLayoutGroup and returns
    // it. This is a helper function to organize objects in a layout.
    private GameObject CreateNewItemRow()
    {
        GameObject newRow = new GameObject("ItemRow", typeof(HorizontalLayoutGroup));
        HorizontalLayoutGroup newRowHLG = newRow.GetComponent<HorizontalLayoutGroup>();
        newRowHLG.childControlWidth = true;
        newRowHLG.childForceExpandWidth = true;
        newRowHLG.spacing = 20;
        newRow.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 0);
        return newRow;
    }

}
