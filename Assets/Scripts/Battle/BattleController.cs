using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

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
    [SerializeField] private GameObject _enemyPrefabObject;
    [Header("Object Assignments")]
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private Button _endTurnButton;
    public void SetEndTurnButtonInteractability(bool isInteractable)
    {
        if (_endTurnButton != null)
        {
            _endTurnButton.interactable = isInteractable;
        }
    }

    [Header("Spawnable Enemy Locations")]
    [SerializeField] private List<SpawnableEnemyLocation> _spawnableEnemyLocations = new List<SpawnableEnemyLocation>();
    public SpawnableEnemyLocation GetNextAvailableEnemyLocation() => _spawnableEnemyLocations.Find((loc) => !loc.isTaken);
    public void TakeUpEnemyLocation(Vector3 pos) => _spawnableEnemyLocations.Find((loc) => loc.position == pos).isTaken = true;
    public void FreeUpEnemyLocation(Vector3 pos) => _spawnableEnemyLocations.Find((loc) => loc.position == pos).isTaken = false;

    private BattleCharacterController _playerBCC;
    private List<BattleCharacterController> _enemyBCCs = new List<BattleCharacterController>();
    public BattleCharacterController GetPlayer() => _playerBCC;
    public List<BattleCharacterController> GetAliveEnemies() => _enemyBCCs.FindAll((bec) => bec.IsAlive());

    [HideInInspector] public UnityEvent OnNextTurnStart = new UnityEvent();
    public int TurnNumber; // To help with Enemy AI calculations

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        TurnNumber = 0;
        // Initialize some properties.
        _playerBCC = _playerObject.GetComponent<BattleHeroController>();
        // Make the screen refresh cards if the screen is resized.
        RefreshCardsOnScreenResize();
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
        // Initialize the tutorial if we need to.
        TryInitializeTutorial();
        // Make the game fade from black to clear.
        TransitionManager.Instance.ShowScreen(0.75f);
        // Initialize the beginning state.
        SetState(new Begin());
    }

    // Sets the state to be the enemy turn when the end turn button is pressed.
    public void OnEndTurnPressed()
    {
        StartCoroutine(State.EndTurn());
    }

    #region Initialization Scripts

    // Initializes the logic to handle card refreshing when the screen is resized.
    private void RefreshCardsOnScreenResize()
    {
        ResizeAspectRatioController.OnScreenResize -= () => StartCoroutine(State.OnScreenResize());
        ResizeAspectRatioController.OnScreenResize += () => StartCoroutine(State.OnScreenResize());
    }

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
            DeckController.Instance.CardsInDiscard.Add(cardCopy);
        }
        // Set the rest of the PlayerBCC values.
        _playerBCC.InitializeHealthData(GameManager.GetHeroHealth(), GameManager.GetHeroMaxHealth());
        _playerBCC.Initialize(GameManager.GetHeroData());
    }

    // Initializes an enemy object.
    public void SpawnEnemy(Enemy enemyData)
    {
        // Don't let the game spawn more than three enemies at a time.
        if (GetAliveEnemies().Count > 3) { return; }
        // Initialize this enemy based on the Enemy scriptable object data.
        GameObject enemyObject = Instantiate(_enemyPrefabObject);
        BattleEnemyController bec = enemyObject.GetComponent<BattleEnemyController>();
        _enemyBCCs.Add(bec);
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

    #region Drawing and playing cards

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

    ///<summary>
    /// Have the player play a card in their hand, along with any colliding BCCs.
    ///</summary>
    public void PlayCardInHand(Card c, List<BattleCharacterController> collidingBCCs)
    {
        StartCoroutine(State.PlayCard(c, collidingBCCs));
    }

    #endregion

}
