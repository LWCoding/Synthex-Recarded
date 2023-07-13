using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TopBarItemController : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private Transform _itemContainerParentTransform;
    [SerializeField] private GameObject _itemTooltipContainer;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _itemDescText;
    [SerializeField] private TextMeshProUGUI _itemVerifyText;
    private List<GameObject> _itemReferences = new List<GameObject>();
    [Header("Sprite Assignments")]
    [SerializeField] private Sprite _placeholderItemSprite;

    public void RenderItems()
    {
        // Hide the tooltip as we re-render.
        HideTopBarItemTooltip();
        // Return all current relics to the pool.
        foreach (GameObject obj in _itemReferences)
        {
            ObjectPooler.Instance.ReturnObjectToPool(PoolableType.ITEM, obj);
        }
        _itemReferences.Clear();
        List<Item> itemList = GameController.GetItems();
        // Spawn all item objects at top.
        for (int i = 0; i < GameController.GetHeroData().maxItemStorageSpace; i++)
        {
            GameObject itemObj = ObjectPooler.Instance.GetObjectFromPool(PoolableType.ITEM);
            itemObj.transform.SetParent(_itemContainerParentTransform, false);
            // Set items to placeholders if they're not actually present in the list.
            Item item = (i >= itemList.Count) ? GetPlaceholderItem() : itemList[i];
            itemObj.GetComponent<ItemHandler>().Initialize(item, false, true, i);
            _itemReferences.Add(itemObj);
        }
    }

    private Item GetPlaceholderItem()
    {
        Item itemToReturn = ScriptableObject.CreateInstance<Item>();
        itemToReturn.type = ItemType.NONE;
        itemToReturn.itemImage = _placeholderItemSprite;
        return itemToReturn;
    }

    public void ShowTopBarItemTooltip(Item item)
    {
        _itemNameText.text = item.itemName;
        string desc = item.itemDesc;
        for (int i = 0; i < item.variables.Count; i++)
        {
            desc = desc.Replace("[" + i.ToString() + "]", item.variables[i].ToString());
        }
        _itemDescText.text = GameController.GetDescriptionWithIcons(desc);
        _itemTooltipContainer.SetActive(true);
    }

    public void HideTopBarItemTooltip()
    {
        _itemTooltipContainer.SetActive(false);
    }

    // Prompt the user to either click twice or click again to use the item,
    // depending on its state.
    public void UpdateItemVerifyText(bool isClickable = true, bool hasBeenClicked = false)
    {
        if (!isClickable)
        {
            _itemVerifyText.text = "Cannot be used now.";
            return;
        }
        if (!hasBeenClicked)
        {
            _itemVerifyText.text = "Click twice to use.";
        }
        else
        {
            _itemVerifyText.text = "Click again to use.";
        }
    }

    public void FlashItemObject(int itemIdx)
    {
        if (itemIdx < 0 || itemIdx >= GameController.GetItems().Count)
        {
            Debug.Log("ERROR IN TOPBARITEMCONTROLLER.CS > FLASHITEMOBJECT! COULD NOT FIND ITEM OF INDEX (" + itemIdx + ")!");
            return;
        }
        _itemReferences[itemIdx].GetComponent<ItemHandler>().FlashItem();
        return;
    }

}
