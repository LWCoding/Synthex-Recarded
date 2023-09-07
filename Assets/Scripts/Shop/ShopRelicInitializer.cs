using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopRelicInitializer : MonoBehaviour
{

    [Header("Object Assignments")]
    public Transform scrollParentTransform;
    private List<RelicHandler> _relicPreviewControllers = new List<RelicHandler>();
    private List<Relic> _currentRelicsInShop = new List<Relic>();

    private void Start()
    {
        _currentRelicsInShop = GameManager.nextShopLoadout.relics;
        InitializeShopRelics();
    }

    private void InitializeShopRelics()
    {
        Transform horizontalTransform = null;
        int currItemIdx = 0;
        // Recover a pooled object for each relic.
        foreach (Relic relic in _currentRelicsInShop)
        {
            // If divisible by 2, create a new row of relics.
            // This number can be changed at any time to modify
            // the amount of relics shown in one row.
            if (currItemIdx % 2 == 0)
            {
                GameObject newRow = CreateNewRelicRow();
                horizontalTransform = newRow.transform;
                horizontalTransform.SetParent(scrollParentTransform, false);
            }
            // Set the basic information for the relic.
            GameObject relicObject = ObjectPooler.Instance.GetObjectFromPool(PoolableType.RELIC);
            RelicHandler relicHandler = relicObject.GetComponent<RelicHandler>();
            relicHandler.Initialize(relic, true);
            relicHandler.EnableShopFunctionality();
            relicHandler.SetSortingOrder(1);
            relicHandler.SetRelicImageScale(2, 1);
            relicObject.transform.localPosition = new Vector3(relicObject.transform.localPosition.x, relicObject.transform.localPosition.y, 0);
            relicObject.transform.SetParent(horizontalTransform, false);
            currItemIdx++;
            _relicPreviewControllers.Add(relicHandler);
        }
    }

    // Creates a new GameObject with a HorizontalLayoutGroup and returns
    // it. This is a helper function to organize objects in a layout.
    private GameObject CreateNewRelicRow()
    {
        GameObject newRow = new GameObject("RelicRow", typeof(HorizontalLayoutGroup));
        HorizontalLayoutGroup newRowHLG = newRow.GetComponent<HorizontalLayoutGroup>();
        newRowHLG.childControlWidth = true;
        newRowHLG.childForceExpandWidth = true;
        newRowHLG.spacing = 20;
        newRow.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 0);
        return newRow;
    }

}
