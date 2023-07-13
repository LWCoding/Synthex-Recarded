using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BattleCharacterController : MonoBehaviour
{

    // This function should be called (with a stored Card
    // variable under `_storedCard`) when the card's effects
    // should be played during the animation.
    private void RenderCardEffects(BattleCharacterController targetBCC)
    {
        // RENDER ALL CARD INFLICTIONS BEFORE THE ATTACK.
        RenderInflictions(targetBCC, EffectRenderOrder.RENDER_EFFECT_BEFORE_ATTACK);
        // RENDER TRAITS.
        if (_characterAlignment == Alignment.HERO && _storedCard.HasTrait(Trait.DRAW_ONE_CARD))
        {
            BattleController.Instance.DrawCards(1);
            BattleController.Instance.UpdateCardsInHand();
        }
        if (_characterAlignment == Alignment.HERO && _storedCard.HasTrait(Trait.DRAW_TWO_CARDS))
        {
            BattleController.Instance.DrawCards(2);
            BattleController.Instance.UpdateCardsInHand();
        }
        if (_characterAlignment == Alignment.HERO && _storedCard.HasTrait(Trait.MATERIALIZE_TWO_CARDS))
        {
            List<Card> cardsToAdd = new List<Card>();
            cardsToAdd.Add(GameController.GetTrulyRandomCard(new List<Card>()));
            cardsToAdd.Add(GameController.GetTrulyRandomCard(new List<Card>()));
            BattleController.Instance.DrawCards(cardsToAdd);
            BattleController.Instance.UpdateCardsInHand();
        }
        if (_characterAlignment == Alignment.HERO && _storedCard.HasTrait(Trait.DRAW_THREE_CARDS))
        {
            BattleController.Instance.DrawCards(3);
            BattleController.Instance.UpdateCardsInHand();
        }
        if (_characterAlignment == Alignment.HERO && _storedCard.HasTrait(Trait.GAIN_TWO_ENERGY))
        {
            EnergyController.Instance.ChangeEnergy(2);
        }
        if (_characterAlignment == Alignment.HERO && _storedCard.HasTrait(Trait.SALVAGE_NEW_CARDS))
        {
            BattleController.Instance.EmptyHand();
            BattleController.Instance.DrawCards(3);
            BattleController.Instance.UpdateCardsInHand();
        }
        if (_storedCard.HasTrait(Trait.CLEANSE_ALL_DEBUFFS))
        {
            for (int i = statusHandler.statusEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = statusHandler.statusEffects[i];
                if (effect.statusInfo.effectFaction == EffectFaction.DEBUFF)
                {
                    statusHandler.RemoveStatusEffect(effect);
                }
            }
        }
        if (_storedCard.HasTrait(Trait.DEAL_DAMAGE_EQ_TO_BLOCK))
        {
            targetBCC.ChangeHealth(-block);
        }
        if (_storedCard.HasTrait(Trait.HEAL_SIX_HEALTH))
        {
            ChangeHealth(6);
        }
        if (_storedCard.HasTrait(Trait.HEAL_SIXTEEN_HEALTH))
        {
            ChangeHealth(16);
        }
        if (_storedCard.HasTrait(Trait.POISON_TWO_CARDS))
        {
            for (int i = 0; i < 2; i++)
            {
                BattleController.Instance.InflictRandomCardWithEffect(CardEffectType.POISON);
            }
        }
        if (_storedCard.HasTrait(Trait.POISON_THREE_CARDS))
        {
            for (int i = 0; i < 3; i++)
            {
                BattleController.Instance.InflictRandomCardWithEffect(CardEffectType.POISON);
            }
        }
        if (_storedCard.HasTrait(Trait.TURN_ENEMY_BLEED_TO_BLOCK))
        {
            StatusEffect enemyBleed = targetBCC.statusHandler.GetStatusEffect(Effect.BLEED);
            if (enemyBleed != null)
            {
                ChangeBlock(enemyBleed.amplifier);
            }
        }
        if (_storedCard.HasTrait(Trait.DOUBLE_ENEMY_BLEED))
        {
            StatusEffect enemyBleed = targetBCC.statusHandler.GetStatusEffect(Effect.BLEED);
            if (enemyBleed != null)
            {
                targetBCC.statusHandler.AddStatusEffect(Globals.GetStatus(Effect.BLEED, enemyBleed.amplifier));
            }
        }
        if (_storedCard.HasTrait(Trait.DRAIN_CHARGE))
        {
            statusHandler.RemoveStatusEffect(Globals.GetStatus(Effect.CHARGE));
        }
        // Render attack and block values.
        RenderAttackAndBlock(_storedCard, targetBCC);
        // Render traits that should happen after attack/block is calculated
        if (_storedCard.HasTrait(Trait.SECOND_WIND))
        {
            if (!targetBCC.IsAlive())
            {
                EnergyController.Instance.ChangeEnergy(2);
            }
        }
        // RENDER ALL CARD INFLICTIONS AFTER THE ATTACK.
        RenderInflictions(targetBCC, EffectRenderOrder.RENDER_EFFECT_AFTER_ATTACK);
    }

    private void RenderInflictions(BattleCharacterController targetBCC, EffectRenderOrder currentRenderTime)
    {
        foreach (CardInflict infliction in _storedCard.GetCardStats().inflictedEffects)
        {
            if (infliction.renderBehavior == currentRenderTime)
            {
                if (infliction.target == Target.SELF || infliction.target == Target.PLAYER_AND_ENEMY)
                {
                    statusHandler.AddStatusEffect(Globals.GetStatus(infliction.effect, infliction.amplifier));
                }
                if (infliction.target == Target.OTHER || infliction.target == Target.PLAYER_AND_ENEMY)
                {
                    targetBCC.statusHandler.AddStatusEffect(Globals.GetStatus(infliction.effect, infliction.amplifier));
                }
            }
        }
    }

}
