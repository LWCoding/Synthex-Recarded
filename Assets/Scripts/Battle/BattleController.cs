using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class SpawnableEnemyLocation
{
    public Vector3 position;
    public bool isTaken;
}

public partial class BattleController : StateMachine
{

    public static BattleController Instance;
    [Header("Prefab Assignments")]
    [SerializeField] private GameObject enemyPrefabObject;
    [Header("Object Assignments")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Button endTurnButton;
    public void SetEndTurnButtonInteractability(bool isInteractable)
    {
        if (endTurnButton != null)
        {
            endTurnButton.interactable = isInteractable;
        }
    }
    [SerializeField] private TextMeshProUGUI drawText;
    [SerializeField] private TextMeshProUGUI discardText;
    [SerializeField] private Transform deckParentTransform;
    [SerializeField] private Button _drawPileButton;
    [SerializeField] private Button _discardPileButton;

    [Header("Spawnable Enemy Locations")]
    [SerializeField] private List<SpawnableEnemyLocation> _spawnableEnemyLocations = new List<SpawnableEnemyLocation>();
    public SpawnableEnemyLocation GetNextAvailableEnemyLocation() => _spawnableEnemyLocations.Find((loc) => !loc.isTaken);
    public void TakeUpEnemyLocation(Vector3 pos) => _spawnableEnemyLocations.Find((loc) => loc.position == pos).isTaken = true;
    public void FreeUpEnemyLocation(Vector3 pos) => _spawnableEnemyLocations.Find((loc) => loc.position == pos).isTaken = false;

    public UnityEvent OnNextTurnStart = new UnityEvent();
    [HideInInspector] public BattleHeroController playerBCC;
    [HideInInspector] public List<BattleEnemyController> enemyBCCs = new List<BattleEnemyController>();
    public List<BattleEnemyController> GetAliveEnemies() => enemyBCCs.FindAll((bec) => bec.IsAlive());

    public List<Card> CardsInDiscard = new List<Card>();
    public List<Card> CardsInDrawPile = new List<Card>();
    public List<Card> CardsInHand = new List<Card>();
    public List<GameObject> CardObjectsInHand = new List<GameObject>();
    public int TurnNumber; // To help with Enemy AI calculations

    private const int MAX_ALLOWED_CARDS_IN_HAND = 8;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        TurnNumber = 0;
        // Initialize some properties.
        playerBCC = playerObject.GetComponent<BattleHeroController>();
        // Make the screen refresh cards if the screen is resized.
        ResizeAspectRatioController.OnScreenResize -= () =>
        {
            StartCoroutine(State.OnScreenResize());
        };
        ResizeAspectRatioController.OnScreenResize += () =>
        {
            StartCoroutine(State.OnScreenResize());
        };
    }

    private void Start()
    {
        // Initialize the UI.
        GlobalUIController.Instance.InitializeUI();
        // Initialize the hero and all the enemies.
        InitializeHero();
        foreach (Enemy e in GameManager.nextBattleEnemies)
        {
            SpawnEnemy(e);
        }
        // Initialize the background!
        BackgroundController.Instance.InitializeBG();
        // Play game music!
        SoundManager.Instance.PlayOnLoop(MusicType.BATTLE_MUSIC);
        // Initialize the buttons.
        _drawPileButton.onClick.AddListener(() => TopBarController.Instance.ToggleCardOverlay(CardsInDrawPile, _drawPileButton));
        _discardPileButton.onClick.AddListener(() => TopBarController.Instance.ToggleCardOverlay(CardsInDiscard, _discardPileButton));
        // Initialize the tutorial if we need to.
        TryInitializeTutorial();
        // Make the game fade from black to clear.
        TransitionManager.Instance.ShowScreen(0.75f);
        // Initialize the beginning state.
        SetState(new Begin(this));
    }

    // Sets the state to be the enemy turn when the end turn button is pressed.
    public void OnEndTurnPressed()
    {
        SetState(new EnemyTurn(this));
    }

    #region Initialization Scripts

    // Initializes the battle tutorial if the player hasn't loaded it in yet.
    private void TryInitializeTutorial()
    {
#if !UNITY_EDITOR
        // If we haven't played the battle tutorial yet, do that.
        if (!GameManager.alreadyPlayedTutorials.Contains("Battle"))
        {
            TutorialController.Instance.StartTutorial();
            GameManager.alreadyPlayedTutorials.Add("Battle");
        }
#endif
    }

    // Initializes the hero's deck, sprite, and health.
    private void InitializeHero()
    {
        for (int i = 0; i < GameManager.GetHeroCards().Count; i++)
        {
            CardData cardDataCopy = Instantiate(GameManager.GetHeroCards()[i].cardData);
            Card cardCopy = new Card(cardDataCopy, GameManager.GetHeroCards()[i].level);
            CardsInDiscard.Add(cardCopy);
        }
        // Set the rest of the PlayerBCC values.
        playerBCC.Initialize(GameManager.GetHeroData(), GameManager.GetHeroHealth(), GameManager.GetHeroMaxHealth());
    }

    // Initializes an enemy object.
    public void SpawnEnemy(Enemy enemyData)
    {
        // Don't let the game spawn more than three enemies at a time.
        if (GetAliveEnemies().Count > 3) { return; }
        // Initialize this enemy based on the Enemy scriptable object data.
        GameObject enemyObject = Instantiate(enemyPrefabObject);
        BattleEnemyController bec = enemyObject.GetComponent<BattleEnemyController>();
        enemyBCCs.Add(bec);
        // Initialize the rest of the enemy's information.
        int generatedHealth = Random.Range(enemyData.enemyHealthMin, enemyData.enemyHealthMax + 1);
        if (enemyData.effectsToStartWith.Count > 0)
        {
            foreach (StatusEffect effect in enemyData.effectsToStartWith)
            {
                bec.AddStatusEffect(effect);
            }
        }
        // Spawn the enemy at the proper location.
        SpawnableEnemyLocation enemyLocationToSpawnAt = GetNextAvailableEnemyLocation();
        TakeUpEnemyLocation(enemyLocationToSpawnAt.position);
        bec.gameObject.transform.position = enemyLocationToSpawnAt.position;
        bec.InitializeHealthData(generatedHealth, generatedHealth);
        bec.Initialize(enemyData);
        bec.SetEnemyType(enemyData);
    }

    #endregion

    /// <summary>
    /// Shuffle cards from the draw pile into the player's hand, along with
    /// some animations.
    /// </summary>
    public void DrawCards(int cardCount)
    {
        StartCoroutine(State.DrawCards(cardCount));
    }

    /// <summary>
    /// Takes a list of cards and shuffles those cards into the player's hand,
    /// along with some animations.
    /// </summary>
    public void DrawCards(List<Card> cardsToDraw)
    {
        StartCoroutine(State.DrawCards(cardsToDraw.Count, cardsToDraw));
    }

    // Allows the players to use cards in their hand.
    public void EnableInteractionsForCardsInHand()
    {
        // Allow the user to mess with cards.
        for (int i = 0; i < CardObjectsInHand.Count; i++)
        {
            CardHandler cardHandler = CardObjectsInHand[i].GetComponent<CardHandler>();
            cardHandler.EnableInteractions();
        }
    }

    // Allows the players to use cards in their hand.
    public void DisableInteractionsForCardsInHand()
    {
        // Allow the user to mess with cards.
        for (int i = 0; i < CardObjectsInHand.Count; i++)
        {
            CardHandler cardHandler = CardObjectsInHand[i].GetComponent<CardHandler>();
            cardHandler.DisableInteractions();
        }
    }

    ///<summary>
    /// Have the player play a card in their hand, along with any colliding BCCs.
    ///</summary>
    public void UseCardInHand(Card c, List<BattleCharacterController> collidingBCCs)
    {
        StartCoroutine(State.PlayCard(c, collidingBCCs));
    }

    // Inflict a random card in the player's hand with an effect.
    public void InflictRandomCardWithEffect(CardEffectType type)
    {
        switch (type)
        {
            case CardEffectType.POISON:
                playerBCC.FlashColor(new Color(0, 1, 0), true);
                playerBCC.DamageShake(1, 1);
                break;
        }
        OnNextTurnStart.AddListener(() =>
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
    public void UpdateCardsInHand(bool shouldBeInteractable = false)
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
            cardObject.transform.position = deckParentTransform.position;
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
        drawText.text = CardsInDrawPile.Count.ToString();
        discardText.text = CardsInDiscard.Count.ToString();
    }

    // Initializes a card object for battle.
    // Sets card visuals based on energy and attack and defense modifiers.
    private void InitializeNewCardObject(Card card)
    {
        GameObject cardObject = BattlePooler.Instance.GetCardObjectFromPool(deckParentTransform);
        CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
        cardHandler.Initialize(card, true);
        cardHandler.UpdateCardVisuals(playerBCC.CalculateDamageModifiers(card), playerBCC.CalculateDefenseModifiers());
        cardHandler.UpdateColorBasedOnPlayability();
        cardHandler.ModifyHoverBehavior(true, true, true, true);
        CardObjectsInHand.Add(cardObject);
    }

}
