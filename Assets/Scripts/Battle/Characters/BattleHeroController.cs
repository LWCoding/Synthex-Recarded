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
        OnPlayedCard.AddListener((c) => HandleRelicsOnPlay(c));
        OnPlayedCard.AddListener((e) => BattleController.Instance.UpdateHandDescriptionInfo());
        OnDeath.AddListener(HandlePlayerDeath);
        OnUpdateHealthText.AddListener(UpdateUIHealthAfterDamaged);
    }

    public void Initialize(HeroData heroData, int currHP, int maxHP)
    {
        base.InitializeHealthData(currHP, maxHP);
        base.Initialize(heroData);
    }

    // Renders the effects of specific relics after the animation for an attack has played.
    private void HandleRelicsOnPlay(Card c)
    {
        // If the player has the Green Scarf relic, build up combos for every attack card.
        if (GameController.HasRelic(RelicType.GREEN_SCARF))
        {
            StatusEffect combo = statusHandler.GetStatusEffect(Effect.COMBO);
            if (c.cardData.cardType == CardType.ATTACKER || c.cardData.cardType == CardType.SPECIAL_ATTACKER)
            {
                if (combo == null || c.cardData.GetCardUniqueName() == combo.specialValue)
                {
                    statusHandler.AddStatusEffect(Globals.GetStatus(Effect.COMBO, 2, c.cardData.GetCardUniqueName()));
                    TopBarController.Instance.FlashRelicObject(RelicType.GREEN_SCARF);
                }
            }
            statusHandler.UpdateStatusIcons();
        }
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

    // Updates the health text at the top bar, because this is the main hero.
    private void UpdateUIHealthAfterDamaged()
    {
        GameController.SetHeroHealth(GetHealth());
    }

}
