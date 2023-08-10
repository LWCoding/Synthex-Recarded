using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Won : State
{

    public Won() : base() { }

    public override IEnumerator Start()
    {
        if (GameManager.HasRelic(RelicType.MEDKIT))
        {
            TopBarController.Instance.FlashRelicObject(RelicType.MEDKIT);
            BattleController.playerBCC.ChangeHealth(4);
        }
        // Allot some time to animate the coins going to the player's balance.
        yield return new WaitForSeconds(1.4f);
        // Let the player add a new card to their deck (out of 3).
        CardChoiceController.Instance.ShowCardChoices(3, () =>
        {
            TransitionManager.Instance.HideScreen("Map", 0.75f);
        });
    }

}
