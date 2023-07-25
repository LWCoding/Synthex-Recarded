using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeController : MonoBehaviour
{

    public static UpgradeController Instance;
    [Header("Object Assignments")]
    [SerializeField] private Transform _cardVertLayoutTransform;
    [SerializeField] private Button _exitButton;

    private List<CardHandler> _cardPreviewHandlers = new List<CardHandler>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
    }

    private void Start()
    {
        // Initialize the buttons that should modify UI elements.
        _exitButton.onClick.AddListener(() => FadeTransitionController.Instance.HideScreen("Map", 0.75f));
        // Initialize the cards in the deck.
        StartCoroutine(InitializeDeckCardsCoroutine());
        // Make the game fade from black to clear.
        FadeTransitionController.Instance.ShowScreen(1.25f);
    }

    public void RefreshCardPreviews()
    {
        List<Card> heroCards = GameController.GetHeroCards();
        for (int i = 0; i < _cardPreviewHandlers.Count; i++)
        {
            _cardPreviewHandlers[i].UpdateCardVisuals();
        }
    }

    private IEnumerator InitializeDeckCardsCoroutine()
    {
        Transform horizontalTransform = null;
        int currCardIdx = 0;
        // Order the cards in alphabetical order, so the player
        // can't cheat and see the exact order.
        List<Card> cardsToShow = GameController.GetHeroCards();
        // Recover a pooled object for each card.
        foreach (Card card in cardsToShow)
        {
            // If divisible by 5, create a new row of cards.
            if (currCardIdx % 5 == 0)
            {
                GameObject newRow = CreateNewCardRow();
                horizontalTransform = newRow.transform;
                horizontalTransform.SetParent(_cardVertLayoutTransform, false);
            }
            // Set the basic information for the card.
            GameObject cardObject = GetCardObjectFromPool();
            CardHandler cardController = cardObject.GetComponent<CardHandler>();
            cardObject.transform.SetParent(horizontalTransform, false);
            // We want the card to appear from nothing, so set the
            // initial showing to false.
            cardController.Initialize(card, false, 0, 0, currCardIdx);
            currCardIdx++;
            _cardPreviewHandlers.Add(cardController);
        }
        // After all cards are created, animate them one-by-one.
        WaitForSeconds wfs = new WaitForSeconds(0.04f);
        foreach (CardHandler cc in _cardPreviewHandlers)
        {
            cc.CardAppear();
            yield return wfs;
        }
    }

    // Grabs a card from the pool but tweaks some basic properties of it to make
    // it look good.
    private GameObject GetCardObjectFromPool()
    {
        // Return an already created card object.
        GameObject cardObject = ObjectPooler.Instance.GetObjectFromPool(PoolableType.CARD);
        CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
        cardObject.transform.localPosition = new Vector3(cardObject.transform.localPosition.x, cardObject.transform.localPosition.y, 0);
        cardObject.transform.localScale = new Vector2(0.4f, 0.4f);
        cardHandler.ModifyHoverBehavior(true, false, false, false); // Modify to be static & unselectable.
        cardHandler.HideCardInstantly(); // Hide the card instantly so we can animate it after.
        cardHandler.EnableUpgradeFunctionality();
        return cardObject;
    }

    // Creates a new GameObject with a HorizontalLayoutGroup and returns
    // it. This is a helper function to organize objects in a layout.
    private GameObject CreateNewCardRow()
    {
        GameObject newRow = new GameObject("CardRow", typeof(HorizontalLayoutGroup));
        HorizontalLayoutGroup newRowHLG = newRow.GetComponent<HorizontalLayoutGroup>();
        newRowHLG.childControlWidth = true;
        newRowHLG.childForceExpandWidth = true;
        newRowHLG.spacing = 40;
        newRow.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 0);
        return newRow;
    }

}
