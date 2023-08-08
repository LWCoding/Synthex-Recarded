using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurn : State
{

    public EnemyTurn(BattleController battleController) : base(battleController) { }

    public override IEnumerator Start()
    {
        // Animate cards in hand and move them to the discard.
        // Remove all cards in hand, and add them to the discard.
        BattleController.SetEndTurnButtonInteractability(false);
        int numCardsInHand = BattleController.CardsInHand.Count;
        for (int i = numCardsInHand - 1; i >= 0; i--)
        {
            BattleController.CardsInDiscard.Add(BattleController.CardsInHand[i]);
            int cardIdx = i; // This is necessary because the coroutine doesn't save the current index
            BattleController.CardObjectsInHand[cardIdx].GetComponent<CardHandler>().CardDisappear(0.25f, CardAnimation.TRANSLATE_DOWN, () =>
            {
                BattlePooler.Instance.ReturnCardObjectToPool(BattleController.CardObjectsInHand[cardIdx]);
                BattleController.CardsInHand.RemoveAt(cardIdx);
                BattleController.CardObjectsInHand.RemoveAt(cardIdx);
            });
            BattleController.UpdateDrawDiscardTexts();
            yield return new WaitForSeconds(0.04f);
        }
        // Return game state back to enemy.
        yield return new WaitForSeconds(0.5f);
        // Make the enemy make a move based on the selected algorithm.
        // This is handled in the partial class `BattleController_AI`.
        // Loop ONLY through original enemies. Not summoned ones mid-way.
        List<BattleEnemyController> originalEnemyBCCs = new List<BattleEnemyController>(BattleController.enemyBCCs);
        foreach (BattleEnemyController bec in originalEnemyBCCs)
        {
            if (!bec.IsAlive()) { continue; }
            yield return bec.PlayCardCoroutine(bec.GetStoredCard(), new List<BattleCharacterController>() { BattleController.playerBCC });
        }
        // Then, switch the logic back to the enemy.
        // Run the turn end logic for both the player and the enemy.
        yield return new WaitForSeconds(0.25f);
        BattleController.playerBCC.TurnEndLogic();
        foreach (BattleEnemyController bec in BattleController.enemyBCCs)
        {
            if (!bec.IsAlive()) { continue; }
            bec.TurnEndLogic();
        }
        yield return new WaitForSeconds(0.25f);
        if (!BattleController.playerBCC.IsAlive())
        {
            BattleController.SetState(new Lost(BattleController));
        }
        else
        {
            BattleController.SetState(new PlayerTurn(BattleController));
        }
    }

}
