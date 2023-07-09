using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemInitializer : MonoBehaviour
{

    [Header("Object Assignments")]
    public GameObject cardPrefab;
    public Transform scrollParentTransform;
    private List<CardHandler> _cardPreviewControllers = new List<CardHandler>();
    private Stack<GameObject> _inactiveCardObjects = new Stack<GameObject>();
    private List<Card> _currentCardsInShop = new List<Card>();

    private void Start()
    {
        InitializeShopCards();
    }

    private void InitializeShopCards()
    {
        Transform horizontalTransform = null;
        int currCardIdx = 0;
        PopulateShopCards();
        // Recover a pooled object for each card.
        foreach (Card card in _currentCardsInShop)
        {
            // If divisible by 2, create a new row of cards.
            // This number can be changed at any time to modify
            // the amount of cards shown in one row.
            if (currCardIdx % 2 == 0)
            {
                GameObject newRow = CreateNewCardRow();
                horizontalTransform = newRow.transform;
                horizontalTransform.SetParent(scrollParentTransform, false);
            }
            // Set the basic information for the card.
            GameObject cardObject = GetCardObjectFromPool();
            CardHandler cardController = cardObject.GetComponent<CardHandler>();
            cardObject.transform.SetParent(horizontalTransform, false);
            // We want the card to appear instantly.
            cardController.Initialize(card, true);
            currCardIdx++;
            _cardPreviewControllers.Add(cardController);
        }
    }

    private void PopulateShopCards()
    {
        for (int i = 0; i < 10; i++)
        {
            Card randomCard = GameController.GetRandomCard(_currentCardsInShop);
            // If there are no new cards, stop here.
            if (randomCard == null)
            {
                continue;
            }
            _currentCardsInShop.Add(randomCard);
        }
    }

    // Creates a new GameObject with a HorizontalLayoutGroup and returns
    // it. This is a helper function to organize objects in a layout.
    private GameObject CreateNewCardRow()
    {
        GameObject newRow = new GameObject("CardRow", typeof(HorizontalLayoutGroup));
        HorizontalLayoutGroup newRowHLG = newRow.GetComponent<HorizontalLayoutGroup>();
        newRowHLG.childControlWidth = true;
        newRowHLG.childForceExpandWidth = true;
        newRowHLG.spacing = 80;
        newRow.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 0);
        return newRow;
    }

    private GameObject GetCardObjectFromPool()
    {
        // Return an already created card object.
        GameObject cardObject = ObjectPooler.Instance.GetObjectFromPool(PoolableType.CARD);
        CardHandler cardController = cardObject.GetComponent<CardHandler>();
        cardController.EnableFunctionality();
        cardController.EnableShopFunctionality();
        cardObject.GetComponent<Canvas>().sortingOrder = 1;
        cardObject.transform.localScale = new Vector2(0.4f, 0.4f);
        cardController.HideCardInstantly(); // Hide the card instantly so we can animate it after.
        return cardObject;
    }

}
