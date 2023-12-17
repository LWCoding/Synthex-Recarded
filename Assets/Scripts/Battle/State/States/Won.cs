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
            BattleController.GetPlayer().ChangeHealth(4);
        }
        // Allot some time to animate the coins going to the player's balance.
        yield return new WaitForSeconds(2.5f);
        /*
        // Let the player add a new card to their deck (out of 3).
        CardChoiceController.Instance.ShowCardChoices(3, () =>
        {
            TransitionManager.Instance.BackToMapOrCampaign(0.75f);
        });
        */
        // Transition the battle back to the map/campaign scene.
        TransitionManager.Instance.BackToMapOrCampaign(0.75f);
    }

}
