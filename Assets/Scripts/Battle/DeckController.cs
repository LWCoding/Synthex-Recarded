using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckController : MonoBehaviour
{

    public static DeckController Instance;
    [Header("Object Assignments")]
    [SerializeField] private TextMeshProUGUI _drawText;
    [SerializeField] private TextMeshProUGUI _discardText;
    [SerializeField] private Transform _deckParentTransform;
    [SerializeField] private Button _drawPileButton;
    [SerializeField] private Button _discardPileButton;

    public List<Card> CardsInDiscard = new List<Card>();
    public List<Card> CardsInDrawPile = new List<Card>();
    public List<Card> CardsInHand = new List<Card>();
    public List<GameObject> CardObjectsInHand = new List<GameObject>();

    private const int MAX_ALLOWED_CARDS_IN_HAND = 8;

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
        // Initialize the buttons.
        _drawPileButton.onClick.AddListener(() => TopBarController.Instance.ToggleCardOverlay(CardsInDrawPile, _drawPileButton));
        _discardPileButton.onClick.AddListener(() => TopBarController.Instance.ToggleCardOverlay(CardsInDiscard, _discardPileButton));
    }

    // Allows the players to use cards in their hand.
    public void EnableDeckInteractions()
    {
        // Allow the user to mess with cards.
        for (int i = 0; i < CardObjectsInHand.Count; i++)
        {
            CardHandler cardHandler = CardObjectsInHand[i].GetComponent<CardHandler>();
            cardHandler.EnableInteractions();
        }
    }

    // Allows the players to use cards in their hand.
    public void DisableDeckInteractions()
    {
        // Allow the user to mess with cards.
        for (int i = 0; i < CardObjectsInHand.Count; i++)
        {
            CardHandler cardHandler = CardObjectsInHand[i].GetComponent<CardHandler>();
            cardHandler.DisableInteractions();
        }
    }

    // Inflict a random card in the player's hand with an effect.
    public void InflictRandomCardWithEffect(CardEffectType type)
    {
        switch (type)
        {
            case CardEffectType.POISON:
                BattleController.Instance.GetPlayer().FlashColor(new Color(0, 1, 0), true);
                BattleController.Instance.GetPlayer().DamageShake(1, 1);
                break;
        }
        BattleController.Instance.OnNextTurnStart.AddListener(() =>
        {
            // Find all cards that do not have the specified effect.
            List<GameObject> cardsWithoutEffect = CardObjectsInHand.FindAll((c) => !c.GetComponent<CardHandler>().HasCardEffect(type));
            // If no cards without the effect exist, stop here.
            if (cardsWithoutEffect.Count == 0) { return; }
            // Or else, get that card's CardHandler and inflict the status.
            CardHandler randomCardHandler = cardsWithoutEffect[Random.Range(0, cardsWithoutEffect.Count)].GetComponent<CardHandler>();
            randomCardHandler.InflictCardEffect(type);
        });
    }

    // Renders all cards in the hand based on the CardsInHand list.
    // Creates any new card objects for cards which don't have object representations.
    // If shouldBeInteractable is set to true, cards will immediately be useable.
    public void UpdateCardsInHand(bool shouldBeInteractable = true)
    {
        int totalCards = CardsInHand.Count;
        float rotationDifference = 7;
        float positionDifference = 140;
        float verticalDifference = 15;
        float canvasScale = GameObject.Find("Canvas").transform.localScale.x;
        float squeezeAmount = 6 - (18 * (canvasScale - 1)); // Squeeze factor. Cards are more close together with more cards.
        float startRotation = ((totalCards % 2 == 0) ? -Mathf.Abs(-(Mathf.Floor(totalCards / 2) - 0.5f)) : -Mathf.Abs(-Mathf.Floor(totalCards / 2))) * rotationDifference;
        float startPosition = ((totalCards % 2 == 0) ? -Mathf.Abs(-(Mathf.Floor(totalCards / 2) - 0.5f)) : -Mathf.Abs(-Mathf.Floor(totalCards / 2))) * positionDifference;
        // Initialize every card.
        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardObject = CardObjectsInHand[i];
            CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
            cardObject.GetComponent<Canvas>().sortingOrder = i;
            cardObject.transform.localScale = new Vector2(0.5f, 0.5f);
            cardObject.transform.rotation = Quaternion.identity;
            cardObject.transform.Rotate(0, 0, -1 * (startRotation + rotationDifference * i));
            float squeezeTogether = (totalCards % 2 == 0) ? (i - (Mathf.Floor(totalCards / 2) - 0.5f)) * (squeezeAmount * totalCards) : (i - Mathf.Floor(totalCards / 2)) * (squeezeAmount * totalCards);
            cardObject.transform.position = _deckParentTransform.position;
            cardObject.transform.position += new Vector3(startPosition + i * positionDifference - squeezeTogether, ((totalCards % 2 == 0) ? -Mathf.Abs(-(Mathf.Floor(totalCards / 2) - 0.5f) + i) : -Mathf.Abs(-Mathf.Floor(totalCards / 2) + i)) * verticalDifference, 0);
            if (shouldBeInteractable)
            {
                cardHandler.EnableInteractions();
            }
            else
            {
                cardHandler.DisableInteractions();
            }
            cardHandler.InitializeStartingData();
        }
    }

    // Create a new card object and add it to the hand.
    // Does NOT re-render cards in the hand.
    public void AddCardToHand(Card card)
    {
        // If the player's hand is full, skip drawing a card.
        if (CardsInHand.Count >= MAX_ALLOWED_CARDS_IN_HAND)
        {
            ObjectPooler.Instance.SpawnPopup("Hand is full!", 6, Vector3.zero, new Color(1, 0.1f, 0.1f), 1, 2f, false);
            return;
        }
        CardsInHand.Add(card);
        InitializeNewCardObject(card);
    }

    // Adds most recent card into the player's draw pile into their hand.
    // (The draw pile should already be shuffled, so this is okay.)
    // Does NOT re-render cards in the hand.
    public void ShuffleRandomCardToHand()
    {
        // If the player's hand is full, skip drawing a card.
        if (CardsInHand.Count >= MAX_ALLOWED_CARDS_IN_HAND)
        {
            ObjectPooler.Instance.SpawnPopup("Hand is full!", 6, Vector3.zero, new Color(1, 0.1f, 0.1f), 1, 2f, false);
            return;
        }
        // If there are no cards to draw, shuffle discard into draw!
        if (CardsInDrawPile.Count == 0)
        {
            if (CardsInDiscard.Count > 0)
            {
                ShuffleDiscardIntoDraw();
            }
            else
            {
                // If there are no cards to draw OR shuffle, skip.
                return;
            }
        }
        Card cardToAdd = CardsInDrawPile[0];
        AddCardToHand(cardToAdd);
        CardsInDrawPile.RemoveAt(0);
    }

    // Removes all cards from the hand and brings them into the discard pile.
    public void EmptyHand()
    {
        for (int i = CardsInHand.Count - 1; i >= 0; i--)
        {
            CardsInDiscard.Add(CardsInHand[i]);
            GameObject cardObject = CardObjectsInHand[i];
            CardsInHand.RemoveAt(i);
            CardObjectsInHand.RemoveAt(i);
            BattlePooler.Instance.ReturnCardObjectToPool(cardObject);
            UpdateDrawDiscardTexts();
        }
    }

    // Shuffles all cards in the discard pile back into the draw pile.
    public void ShuffleDiscardIntoDraw()
    {
        int discardSize = CardsInDiscard.Count;
        for (int j = 0; j < discardSize; j++)
        {
            // Get a random index in the discard pile and add it to the draw pile.
            int randomDiscIdx = Random.Range(0, CardsInDiscard.Count);
            CardsInDrawPile.Add(CardsInDiscard[randomDiscIdx]);
            CardsInDiscard.RemoveAt(randomDiscIdx);
        }
    }

    // Updates the counter text for both the draw and discard piles.
    public void UpdateDrawDiscardTexts()
    {
        _drawText.text = CardsInDrawPile.Count.ToString();
        _discardText.text = CardsInDiscard.Count.ToString();
    }

    // Initializes a card object for battle.
    // Sets card visuals based on energy and attack and defense modifiers.
    private void InitializeNewCardObject(Card card)
    {
        GameObject cardObject = BattlePooler.Instance.GetCardObjectFromPool(_deckParentTransform);
        CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
        cardHandler.Initialize(card);
        cardHandler.ShowCardInstantly();
        cardHandler.UpdateCardVisuals(BattleController.Instance.GetPlayer().CalculateDamageModifiers(card), BattleController.Instance.GetPlayer().CalculateDefenseModifiers());
        cardHandler.UpdateColorBasedOnPlayability();
        cardHandler.ShouldScaleOnHover = true;
        cardHandler.ShouldSortToTopOnHover = true;
        cardHandler.ShouldTranslateUpOnHover = true;
        cardHandler.IsDraggable = true;
        CardObjectsInHand.Add(cardObject);
    }

}
