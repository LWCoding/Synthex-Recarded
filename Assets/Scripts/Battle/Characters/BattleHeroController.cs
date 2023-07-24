using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleHeroController : BattleCharacterController
{

    public override void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        OnDeath.AddListener(HandlePlayerDeath);
        // Update the actual stored health (affects UI) after the health of the hero updates.
        OnUpdateHealthText.AddListener(() => GameController.SetHeroHealth(GetHealth()));
    }

    public void Initialize(HeroData heroData, int currHP, int maxHP)
    {
        base.InitializeHealthData(currHP, maxHP);
        base.Initialize(heroData);
    }

    // Handles logic when this specific character dies.
    private void HandlePlayerDeath()
    {
        // Play Jack's death animation.
        SetCharacterSprite(CharacterState.DEATH, true);
        // Play the death sound effect.
        SoundManager.Instance.PlaySFX(SoundEffect.GAME_OVER);
        // Set the game state and transition back to the title.
        BattleController.Instance.ChangeGameState(GameState.GAME_OVER);
        FadeTransitionController.Instance.HideScreen("Title", 2.5f);
    }

}
