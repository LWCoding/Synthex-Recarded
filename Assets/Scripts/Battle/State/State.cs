using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{

    protected BattleController BattleController;

    public State(BattleController battleController)
    {
        BattleController = battleController;
    }

    public virtual IEnumerator Start()
    {
        yield break;
    }

    public virtual IEnumerator OnScreenResize()
    {
        yield break;
    }

    public virtual IEnumerator DrawCards(int cardCount, List<Card> cardsToDraw = null)
    {
        yield break;
    }

    public virtual IEnumerator PlayCard(Card card, List<BattleCharacterController> collidingBCCs)
    {
        yield break;
    }

}
