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
        InitializeShopItems();
    }

    private void InitializeShopItems()
    {
        Transform horizontalTransform = null;
        int currItemIdx = 0;
        PopulateShopItems();
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
            itemHandler.EnableShopFunctionality();
            itemHandler.SetSortingOrder(1);
            itemHandler.SetItemImageScale(2, 1);
            itemObject.transform.SetParent(horizontalTransform, false);
            currItemIdx++;
            _itemPreviewControllers.Add(itemHandler);
        }
    }

    private void PopulateShopItems()
    {
        for (int i = 0; i < 4; i++)
        {
            Item randomItem = GameController.GetRandomItem(_currentItemsInShop);
            // If there are no new items, stop here.
            if (randomItem == null)
            {
                continue;
            }
            _currentItemsInShop.Add(randomItem);
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
