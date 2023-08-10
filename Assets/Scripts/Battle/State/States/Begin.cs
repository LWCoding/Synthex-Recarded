using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Begin : State
{

    public Begin() : base() { }

    public override IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        InitializePlayCardListeners();
        InitializeRelicListeners();
        InitializeEnergyListeners();
        // After initializing, make it the player's turn.
        BattleController.SetState(new PlayerTurn());
    }

    // Add logic that makes different relics perform their functions by modifying
    // delegates that were pre-defined.
    private void InitializeRelicListeners()
    {
        // If the character has the SCALE OF JUSTICE relic, they can play their first card twice.
        if (GameManager.HasRelic(RelicType.SCALE_OF_JUSTICE))
        {
            BattleController.GetPlayer().AddStatusEffect(Globals.GetStatus(Effect.DOUBLE_TAKE, 1));
            TopBarController.Instance.FlashRelicObject(RelicType.SCALE_OF_JUSTICE);
        }
        // If the character has the PLASMA CORE relic, they gain one additional max energy.
        if (GameManager.HasRelic(RelicType.PLASMA_CORE))
        {
            EnergyController.Instance.UpdateMaxEnergy(1);
            TopBarController.Instance.FlashRelicObject(RelicType.PLASMA_CORE);
        }
        // If the character has the DUMBELL relic, they gain one additional Strength.
        if (GameManager.HasRelic(RelicType.DUMBELL))
        {
            BattleController.GetPlayer().AddStatusEffect(Globals.GetStatus(Effect.STRENGTH, 1));
            TopBarController.Instance.FlashRelicObject(RelicType.DUMBELL);
        }
        // If the character has the DUMBELL relic, they gain one additional Defense.
        if (GameManager.HasRelic(RelicType.KEVLAR_VEST))
        {
            BattleController.GetPlayer().AddStatusEffect(Globals.GetStatus(Effect.DEFENSE, 1));
            TopBarController.Instance.FlashRelicObject(RelicType.KEVLAR_VEST);
        }
        // If the character has the GRAPPLING HOOK relic, they draw one additional card per turn.
        if (GameManager.HasRelic(RelicType.GRAPPLING_HOOK))
        {
            BattleController.GetPlayer().AddStatusEffect(Globals.GetStatus(Effect.LUCKY_DRAW, 1));
            TopBarController.Instance.FlashRelicObject(RelicType.GRAPPLING_HOOK);
        }

        // If the player has the Green Scarf relic, remove combo if the card isn't identical.
        if (GameManager.HasRelic(RelicType.GREEN_SCARF))
        {
            BattleController.GetPlayer().OnPlayCard.AddListener((c) =>
            {
                StatusEffect combo = BattleController.GetPlayer().GetStatusEffect(Effect.COMBO);
                if (combo != null && c.GetCardDisplayName() != combo.specialValue)
                {
                    BattleController.GetPlayer().RemoveStatusEffect(Effect.COMBO);
                }
            });
        }
        // If the player has the Green Scarf relic, build up combos for every attack card.
        if (GameManager.HasRelic(RelicType.GREEN_SCARF))
        {
            BattleController.GetPlayer().OnPlayedCard.AddListener((c) =>
            {
                StatusEffect combo = BattleController.GetPlayer().GetStatusEffect(Effect.COMBO);
                if (c.cardData.cardType == CardType.ATTACKER || c.cardData.cardType == CardType.SPECIAL_ATTACKER)
                {
                    if (combo == null || c.GetCardDisplayName() == combo.specialValue)
                    {
                        BattleController.GetPlayer().AddStatusEffect(Globals.GetStatus(Effect.COMBO, 2, c.GetCardDisplayName()));
                        TopBarController.Instance.FlashRelicObject(RelicType.GREEN_SCARF);
                    }
                }
            });
        }
        // If the player has the The Thinker relic, deal 1 damage to enemies for every card.
        if (GameManager.HasRelic(RelicType.THE_THINKER))
        {
            BattleController.GetPlayer().OnPlayCard.AddListener((c) =>
            {
                foreach (BattleCharacterController bcc in BattleController.GetAliveEnemies())
                {
                    bcc.ChangeHealth(-1);
                }
                TopBarController.Instance.FlashRelicObject(RelicType.THE_THINKER);
            });
        }
        // If the player has the Vampire Teeth relic, killing an enemy should heal 3 health.
        if (GameManager.HasRelic(RelicType.VAMPIRE_FANGS))
        {
            foreach (BattleEnemyController bec in BattleController.GetAliveEnemies())
            {
                bec.OnDeath.AddListener(() =>
                {
                    BattleController.GetPlayer().ChangeHealth(3);
                    TopBarController.Instance.FlashRelicObject(RelicType.VAMPIRE_FANGS);
                });
            }
        }
        // If the player has the Airhorn relic, all enemies start with 1 crippled.
        if (GameManager.HasRelic(RelicType.AIRHORN))
        {
            foreach (BattleEnemyController bec in BattleController.GetAliveEnemies())
            {
                bec.AddStatusEffect(Globals.GetStatus(Effect.CRIPPLED, 1));
                TopBarController.Instance.FlashRelicObject(RelicType.AIRHORN);
            }
        }
    }

    // Add logic that should render whenever the player plays a specific card,
    // (e.g. Catastrophe).
    private void InitializePlayCardListeners()
    {
        // If the player has the Catastrophe status effect, deal X damage to all enemies
        // when a card is played.
        BattleController.GetPlayer().OnPlayCard.AddListener((card) =>
        {
            StatusEffect catastropheEffect = BattleController.GetPlayer().GetStatusEffect(Effect.CATASTROPHE);
            if (catastropheEffect != null)
            {
                foreach (BattleEnemyController bec in BattleController.GetAliveEnemies())
                {
                    bec.ChangeHealth(-catastropheEffect.amplifier);
                }
            }
        });
        BattleController.GetPlayer().OnPlayedCard.AddListener((e) =>
        {
            foreach (GameObject obj in DeckController.CardObjectsInHand)
            {
                CardHandler cardHandler = obj.GetComponent<CardHandler>();
                cardHandler.UpdateCardDescription(BattleController.GetPlayer().CalculateDamageModifiers(cardHandler.card), BattleController.GetPlayer().CalculateDefenseModifiers());
            }
        });
    }

    // Add logic that pertains to the battle actually functioning properly, outside
    // of player/enemy logic.
    private void InitializeEnergyListeners()
    {
        // Make energy updates change the displays of cards in the player's hand.
        EnergyController.Instance.OnEnergyChanged.AddListener((energy) =>
        {
            foreach (GameObject cardObject in DeckController.CardObjectsInHand)
            {
                CardHandler cardHandler = cardObject.GetComponent<CardHandler>();
                cardHandler.UpdateColorBasedOnPlayability();
            }
        });
    }

}
