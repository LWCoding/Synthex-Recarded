using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EnemyAI
{
    GARBITCH = 1, DUMMY = 2, LONE = 3,
    BUCKAROO = 4, HIVEMIND = 5, TREE = 6, MR_MUSHROOM = 7, TURTLEIST = 8,
    NUTS = 9, ROTTLE = 10, BOYKISSER = 99
}

public class BattleEnemyController : BattleCharacterController
{

    [SerializeField] private Transform _intentParentTransform;
    private EnemyAI enemyAI;
    private int _rewardAmount;
    public void SetRewardAmount(int amt) => _rewardAmount = amt;
    private int _xpRewardAmount;
    private EnemyIntentHandler _intentHandler;
    private Card _storedCardForNextTurn;
    private BattleController battleController = BattleController.Instance;

    public override void Awake()
    {
        base.Awake();
        _intentHandler = GetComponent<EnemyIntentHandler>();
    }

    public void Start()
    {
        statusHandler.OnGetStatusEffect.AddListener((e) => AdjustIntentForModifiers(e));
        OnDeath.AddListener(HandleEnemyDeath);
    }

    public void SetEnemyType(Enemy e)
    {
        enemyAI = e.enemyAI;
        _xpRewardAmount = e.enemyXPReward;
    }

    // Adjust the intent value depending on any modifiers they may have gained.
    // Ex: Gaining strength mid-fight means the intent should increase by 1 attack.
    private void AdjustIntentForModifiers(StatusEffect e = null)
    {
        int strengthBuff = CalculateDamageModifiers(_storedCardForNextTurn);
        foreach (BattleCharacterController targetBCC in targetBCCs)
        {
            strengthBuff += targetBCC.CalculateVulnerabilityModifiers();
        }
        _intentHandler.UpdateStrengthValue(strengthBuff);
    }

    // Handles logic when this specific character dies.
    private void HandleEnemyDeath()
    {
        // Play the enemy defeat animation.
        if (_flashColorCoroutine != null)
        {
            StopCoroutine(_flashColorCoroutine);
        }
        DamageShake(3, 1.7f);
        StartCoroutine(DisappearCoroutine());
        MakeUninteractable();
        battleController.enemiesStillAlive--;
        DisableCharacterUI();
        // Animate some coins going to the player's balance.
        if (GameController.HasRelic(RelicType.GOLDEN_PAW))
        {
            SetRewardAmount(Mathf.FloorToInt(_rewardAmount * 1.35f));
            TopBarController.Instance.FlashRelicObject(RelicType.GOLDEN_PAW);
        }
        TopBarController.Instance.AnimateCoinsToBalance(Camera.main.WorldToScreenPoint(gameObject.transform.position), _rewardAmount);
        TopBarController.Instance.AnimateXPToBalance(Camera.main.WorldToScreenPoint(gameObject.transform.position), _xpRewardAmount);
        // If the player died too, stop here.
        if (!BattleController.Instance.playerBCC.IsAlive()) { return; }
        // Only IF there are no remaining enemies, end the battle.
        if (battleController.enemiesStillAlive == 0)
        {
            battleController.HandleBattleWin();
            BattleController.Instance.ChangeGameState(GameState.GAME_OVER);
            // Render all of the available options unusable.
            foreach (GameObject cardObject in GameObject.FindGameObjectsWithTag("CardUI"))
            {
                cardObject.GetComponent<CardHandler>().DisableFunctionality();
            }
            Destroy(battleController.endTurnButton.GetComponent<Button>());
        }
    }

    private void DisableCharacterUI()
    {
        _barIconSprite.gameObject.SetActive(false);
        _healthBoxTransform.gameObject.SetActive(false);
        _intentParentTransform.gameObject.SetActive(false);
        _healthText.gameObject.SetActive(false);
        _blockText.gameObject.SetActive(false);
        statusHandler.statusParentTransform.gameObject.SetActive(false);
    }

    public Card GetStoredCard()
    {
        return _storedCardForNextTurn;
    }

    // Sets the next card that this enemy will play.
    // Also sets the intent preview based on the inputted card/behavior.
    public void GenerateNextMove(int _turnNumber)
    {
        _storedCardForNextTurn = RenderEnemyAI(_turnNumber);
        if (!IsAlive()) { return; }
        _intentHandler.ShowIntentIcon();
        int strengthBuff = CalculateDamageModifiers(_storedCardForNextTurn);
        foreach (BattleCharacterController targetBCC in targetBCCs)
        {
            strengthBuff += targetBCC.CalculateVulnerabilityModifiers();
        }
        _intentHandler.SetIntentType(_storedCardForNextTurn, strengthBuff);
    }

    // Renders the enemy AI after the character's turn.
    // This should return a card
    // depending on the moves it makes.
    private Card RenderEnemyAI(int _turnNumber)
    {
        float rng = Random.Range(0f, 1f);
        switch (enemyAI)
        {
            case EnemyAI.DUMMY:
                if (_turnNumber % 2 == 1)
                {
                    return Globals.GetCard("Dummy Swipe");
                }
                else
                {
                    return Globals.GetCard("Dummy Brace");
                }
            case EnemyAI.GARBITCH:
                if (rng < 0.5f)
                {
                    return Globals.GetCard("Garbitch Swipe");
                }
                else
                {
                    return Globals.GetCard("Garbitch Brace");
                }
            case EnemyAI.LONE:
                if (_turnNumber % 3 == 0)
                {
                    return Globals.GetCard("Lone Swipe");
                }
                else if (_turnNumber % 3 == 1)
                {
                    return Globals.GetCard("Lone Poison Bomb");
                }
                else
                {
                    return Globals.GetCard("Lone Leech");
                }
            case EnemyAI.BUCKAROO:
                if (_turnNumber % 2 == 1)
                {
                    return Globals.GetCard("Buckaroo Swipe");
                }
                else
                {
                    return Globals.GetCard("Buckaroo Disease");
                }
            case EnemyAI.HIVEMIND:
                if (_turnNumber % 3 == 1)
                {
                    return Globals.GetCard("Hivemind Block");
                }
                else if (_turnNumber % 3 == 2)
                {
                    return Globals.GetCard("Hivemind Sting");
                }
                else
                {
                    return Globals.GetCard("Hivemind Swipe");
                }
            case EnemyAI.TREE:
                if (statusHandler.GetStatusEffect(Effect.CHARGE) != null)
                {
                    return Globals.GetCard("Tree Heal");
                }
                if (health < maxHealth && rng < 0.4f)
                {
                    return Globals.GetCard("Tree Charge");
                }
                else
                {
                    return Globals.GetCard("Tree Swipe");
                }
            case EnemyAI.MR_MUSHROOM:
                if (_turnNumber % 3 == 1)
                {
                    return Globals.GetCard("MrMushroom Poison");
                }
                else if (_turnNumber % 2 == 1)
                {
                    return Globals.GetCard("MrMushroom Intoxicate");
                }
                else
                {
                    return Globals.GetCard("MrMushroom Swipe");
                }
            case EnemyAI.TURTLEIST:
                if (_turnNumber % 2 == 1)
                {
                    return Globals.GetCard("Turtleist Brace");
                }
                else
                {
                    return Globals.GetCard("Turtleist Swipe");
                }
            case EnemyAI.NUTS:
                if (statusHandler.GetStatusEffect(Effect.CHARGE) == null)
                {
                    return Globals.GetCard("Nuts Charge");
                }
                else
                {
                    return Globals.GetCard("Nuts Shoot");
                }
            case EnemyAI.ROTTLE:
                if (_turnNumber == 1)
                {
                    return Globals.GetCard("Rottle Cripple");
                }
                else
                {
                    return Globals.GetCard("Rottle Swipe");
                }
            case EnemyAI.BOYKISSER:
                if (statusHandler.GetStatusEffect(Effect.CHARGE) != null && statusHandler.GetStatusEffect(Effect.CHARGE).amplifier >= 2)
                {
                    return Globals.GetCard("Boykisser Blast");
                }
                if (rng < 0.3f || (statusHandler.GetStatusEffect(Effect.CHARGE) != null && statusHandler.GetStatusEffect(Effect.CHARGE).amplifier == 1))
                {
                    return Globals.GetCard("Boykisser Charge");
                }
                else
                {
                    return Globals.GetCard("Boykisser Swipe");
                }
        }
        return null;
    }

}
