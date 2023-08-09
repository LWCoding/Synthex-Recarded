using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTurn : State
{
    public PlayerTurn(BattleController battleController) : base(battleController) { }

    public override IEnumerator Start()
    {
        BattleController.TurnNumber++;
        // Sets energy to max energy.
        EnergyController.Instance.RestoreEnergy();
        EnergyController.Instance.EnergyGlow();
        // Run the turn start logic function for player and enemies.
        BattleController.playerBCC.TurnStartLogic(BattleController.TurnNumber);
        foreach (BattleEnemyController bec in BattleController.enemyBCCs)
        {
            if (!bec.IsAlive()) { continue; }
            bec.TurnStartLogic(BattleController.TurnNumber);
            bec.GenerateNextMove(BattleController.TurnNumber);
        }
        // Update energy and health text values.
        EnergyController.Instance.UpdateEnergyText();
        // Draw random cards from the draw pile.
        // Potentially draw more from Lucky Draw effect.
        StatusEffect luckyDraw = BattleController.playerBCC.GetStatusEffect(Effect.LUCKY_DRAW);
        int cardsToDraw = 5 + ((luckyDraw != null) ? luckyDraw.amplifier : 0);
        // If it's the first turn, check if the player has any Cheat Cards.
        // If yes, then decrement the cards to draw and draw one of these at random.
        if (BattleController.TurnNumber == 1)
        {
            List<Card> cheatCards = GameManager.GetHeroCards().FindAll((card) =>
            {
                return card.HasTrait(Trait.CHEAT_CARD);
            });
            // If we have any cheat cards, draw them into the hand one at a time.
            // Don't let the player draw more than five cards.
            while (cardsToDraw > 0 && cheatCards.Count > 0)
            {
                int randomIdx = Random.Range(0, cheatCards.Count);
                Card cheatCard = cheatCards[randomIdx];
                cheatCards.RemoveAt(randomIdx);
                BattleController.DrawCards(new List<Card> { cheatCard });
                cardsToDraw--;
            }
        }
        yield return DrawCards(cardsToDraw);
        // Run after additional code.
        BattleController.OnNextTurnStart.Invoke();
        BattleController.OnNextTurnStart = new UnityEvent();
        // Allow the player to use the end turn button again.
        BattleController.SetEndTurnButtonInteractability(true);
    }

    public override IEnumerator OnScreenResize()
    {
        BattleController.UpdateCardsInHand();
        yield break;
    }

    public override IEnumerator DrawCards(int count, List<Card> cardsToDraw = null)
    {
        for (int i = 0; i < count; i++)
        {
            if (cardsToDraw == null)
            {
                // Get a card in the draw pile and add it to the hand.
                BattleController.ShuffleRandomCardToHand();
            }
            else
            {
                // If there are cards to draw, draw from those instead.
                BattleController.AddCardToHand(cardsToDraw[i]);
            }
            // Render the cards on the screen, and update the numbers.
            BattleController.UpdateCardsInHand(false);
            BattleController.UpdateDrawDiscardTexts();
            yield return new WaitForSeconds(0.1f);
        }
        // Allow the user to mess with cards.
        BattleController.EnableInteractionsForCardsInHand();
    }

    public override IEnumerator PlayCard(Card c, List<BattleCharacterController> collidingBCCs)
    {
        // Update energy cost after using card.
        EnergyController.Instance.ChangeEnergy(-c.GetCardStats().cardCost);
        // Play animations and perform actions specified on card.
        BattleController.playerBCC.PlayCard(c, collidingBCCs);
        // Find card and move it to the discard pile.
        int idx = BattleController.CardsInHand.IndexOf(c);
        // If it can't find the object, that means that the
        // card was removed during its effects. Ignore deleting
        // it from the hand, then.
        if (idx == -1)
        {
            yield break;
        }
        GameObject cardObject = BattleController.CardObjectsInHand[idx];
        // If the card shouldn't exhaust, add it to the discard pile.
        if (!c.HasTrait(Trait.EXHAUST))
        {
            BattleController.CardsInDiscard.Add(BattleController.CardsInHand[idx]);
        }
        BattleController.CardsInHand.RemoveAt(idx);
        BattleController.CardObjectsInHand.RemoveAt(idx);
        BattleController.UpdateDrawDiscardTexts();
        BattlePooler.Instance.ReturnCardObjectToPool(cardObject);
        BattleController.UpdateCardsInHand();
        yield break;
    }

}
