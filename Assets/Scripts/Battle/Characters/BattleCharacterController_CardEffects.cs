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
        RenderTraits(targetBCC);
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

    private void RenderTraits(BattleCharacterController targetBCC)
    {
        foreach (CardModifier modifier in _storedCard.GetCardStats().modifiers)
        {
            switch (modifier.trait)
            {
                case Trait.DRAW_CARDS:
                    if (_characterAlignment == Alignment.HERO)
                    {
                        BattleController.Instance.DrawCards(modifier.amplifier);
                    }
                    break;
                case Trait.MATERIALIZE_CARDS:
                    if (_characterAlignment == Alignment.HERO)
                    {
                        List<Card> cardsToAdd = new List<Card>();
                        for (int i = 0; i < modifier.amplifier; i++)
                        {
                            cardsToAdd.Add(GameController.GetTrulyRandomCard(new List<Card>()));
                        }
                        BattleController.Instance.DrawCards(cardsToAdd);
                    }
                    break;
                case Trait.GAIN_ENERGY:
                    if (_characterAlignment == Alignment.HERO)
                    {
                        EnergyController.Instance.ChangeEnergy(modifier.amplifier);
                    }
                    break;
                case Trait.CLEAR_CARDS_IN_HAND:
                    if (_characterAlignment == Alignment.HERO)
                    {
                        BattleController.Instance.EmptyHand();
                    }
                    break;
                case Trait.POISON_CARDS:
                    if (_characterAlignment == Alignment.ENEMY)
                    {
                        for (int i = 0; i < modifier.amplifier; i++)
                        {
                            BattleController.Instance.InflictRandomCardWithEffect(CardEffectType.POISON);
                        }
                    }
                    break;
                case Trait.SUMMON_ENEMY:
                    if (_characterAlignment == Alignment.ENEMY)
                    {
                        if (BattleController.Instance.enemiesStillAlive < 2)
                        {
                            BattleController.Instance.SpawnEnemy(Globals.GetEnemy(modifier.special));
                        }
                    }
                    break;
                case Trait.ADDITIONAL_LUCK_DAMAGE:
                    if (statusHandler.GetStatusEffect(Effect.LUCK) != null)
                    {
                        targetBCC.ChangeHealth(-(modifier.amplifier + CalculateDamageModifiers(_storedCard)));
                    }
                    break;
                case Trait.GAIN_HEALTH:
                    ChangeHealth(modifier.amplifier);
                    break;
                case Trait.HEAL_ALL_ENEMIES:
                    foreach (BattleEnemyController enemyBCC in BattleController.Instance.enemyBCCs)
                    {
                        enemyBCC.ChangeHealth(modifier.amplifier);
                    }
                    break;
                case Trait.CLEANSE_ALL_DEBUFFS:
                    for (int i = statusHandler.statusEffects.Count - 1; i >= 0; i--)
                    {
                        StatusEffect effect = statusHandler.statusEffects[i];
                        if (effect.statusInfo.effectFaction == EffectFaction.DEBUFF)
                        {
                            statusHandler.RemoveStatusEffect(effect.statusInfo.type);
                        }
                    }
                    break;
                case Trait.DEAL_DAMAGE_EQ_TO_BLOCK:
                    targetBCC.ChangeHealth(-GetBlock());
                    break;
                case Trait.TURN_ENEMY_BLEED_TO_BLOCK:
                    StatusEffect enemyBleedToBlock = targetBCC.statusHandler.GetStatusEffect(Effect.BLEED);
                    if (enemyBleedToBlock != null) { ChangeBlock(enemyBleedToBlock.amplifier); }
                    break;
                case Trait.DOUBLE_ENEMY_BLEED:
                    StatusEffect enemyBleedToDouble = targetBCC.statusHandler.GetStatusEffect(Effect.BLEED);
                    if (enemyBleedToDouble != null) { targetBCC.statusHandler.AddStatusEffect(Globals.GetStatus(Effect.BLEED, enemyBleedToDouble.amplifier)); }
                    break;
                case Trait.DRAIN_CHARGE:
                    statusHandler.RemoveStatusEffect(Effect.CHARGE);
                    break;
            }
        }
    }

    private void RenderInflictions(BattleCharacterController targetBCC, EffectRenderOrder currentRenderTime)
    {
        foreach (CardInflict infliction in _storedCard.GetCardStats().inflictions)
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
