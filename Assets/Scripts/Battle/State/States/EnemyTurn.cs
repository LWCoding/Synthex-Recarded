using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurn : State
{

    public EnemyTurn() : base() { }

    public override IEnumerator Start()
    {
        // Animate cards in hand and move them to the discard.
        // Remove all cards in hand, and add them to the discard.
        BattleController.SetEndTurnButtonInteractability(false);
        int numCardsInHand = DeckController.CardsInHand.Count;
        for (int i = numCardsInHand - 1; i >= 0; i--)
        {
            DeckController.CardsInDiscard.Add(DeckController.CardsInHand[i]);
            int cardIdx = i; // This is necessary because the coroutine doesn't save the current index
            DeckController.CardObjectsInHand[cardIdx].GetComponent<CardHandler>().CardDisappear(0.25f, CardAnimation.TRANSLATE_DOWN, () =>
            {
                BattlePooler.Instance.ReturnCardObjectToPool(DeckController.CardObjectsInHand[cardIdx]);
                DeckController.CardsInHand.RemoveAt(cardIdx);
                DeckController.CardObjectsInHand.RemoveAt(cardIdx);
            });
            DeckController.UpdateDrawDiscardTexts();
            yield return new WaitForSeconds(0.04f);
        }
        // Return game state back to enemy.
        yield return new WaitForSeconds(0.5f);
        // Make the enemy make a move based on the selected algorithm.
        // This is handled in the partial class `BattleController_AI`.
        // Loop ONLY through original enemies. Not summoned ones mid-way.
        List<BattleCharacterController> originalEnemyBCCs = new List<BattleCharacterController>(BattleController.GetAliveEnemies());
        foreach (BattleEnemyController bec in originalEnemyBCCs)
        {
            yield return bec.PlayCardCoroutine(bec.GetStoredCard(), new List<BattleCharacterController>() { BattleController.GetPlayer() });
            yield return new WaitForSeconds(0.4f);
        }
        // Run the turn end logic for both the player and the enemy.
        BattleController.GetPlayer().TurnEndLogic();
        foreach (BattleEnemyController bec in BattleController.GetAliveEnemies())
        {
            bec.TurnEndLogic();
        }
        yield return new WaitForSeconds(0.25f);
        if (!BattleController.GetPlayer().IsAlive())
        {
            BattleController.SetState(new Lost());
        }
        else
        {
            BattleController.SetState(new PlayerTurn());
        }
    }

}
